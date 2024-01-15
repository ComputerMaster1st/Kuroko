using Discord;
using Discord.Interactions;
using FluentScheduler;
using Kuroko.Audio.FFmpeg;
using Kuroko.Core;
using Kuroko.Modules.Audio;
using Kuroko.Modules.Audio.Modal;
using Kuroko.Services;
using Kuroko.Shared;
using System.Diagnostics;
using System.Text;

namespace Kuroko.Audio
{
    public class Transcode : KurokoModuleBase
    {
        private class TranscodeState
        {
            public Stream Stream { get; set; }
            public string FileName { get; set; }
            public SongMetadata Meta { get; set; }
        }

        private readonly HttpClient _httpClient = new();

        static Dictionary<ulong, TranscodeState> activeCommands = [];

        [SlashCommand("transcode", "Transcode file to opus")]
        public async Task ExecuteAsync(IAttachment attachment)
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, "Got transcode command"));
            await DeferAsync(true);

            Stopwatch sw = new Stopwatch();

            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Downloading file (Id: {attachment.Id})"));
            string filename = Path.GetFileNameWithoutExtension(attachment.Filename);
            sw.Start();
            byte[] bytes = await _httpClient.GetByteArrayAsync(attachment.Url ?? attachment.ProxyUrl);
            sw.Stop();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Downloading file (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
            sw.Reset();

            bool isAudio = FileMimeType.GetFromBytes(bytes).Any(x => x.MimeType.StartsWith("audio/"));
            if (!isAudio)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, "File MIME type is not audio"));
                await FollowupWithFileAsync("Not an audio file", ephemeral: true);
                return;
            }

            MemoryStream sourceAudio = new MemoryStream(bytes);

            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Probing File"));
            sw.Start();
            SongMetadata metadata = await Probe.GetMetadataAsync(sourceAudio);
            sw.Stop();
            sourceAudio.Position = 0;
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Probed File (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
            sw.Reset();

            if (metadata.Duration.TotalMinutes > 30)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, "File too long"));
                await FollowupAsync("Audio file too long", ephemeral: true);
                return;
            }

            // Check if in database
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Checking if already transcoded"));
            sw.Start();
            bool match = await Fingerprinting.Fingerprinting.Match(sourceAudio, metadata.Duration.TotalSeconds);
            sw.Stop();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"{(match ? "File already transcoded" : "File needs transcoding")} (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
            sourceAudio.Position = 0;
            sw.Reset();

            var output = new StringBuilder()
                .AppendLine(filename)
                .AppendLine($"Title: {metadata.Title}")
                .AppendLine($"Artist: {metadata.Artist}")
                .AppendLine($"Album: {metadata.Album}")
                .AppendLine($"Length: {metadata.Duration:m\\:ss}")
                .AppendLine();

            var componentBuilder = new ComponentBuilder()
                .WithButton("Edit", $"{TranscodeCommandMap.EDIT_META}:{attachment.Id}", ButtonStyle.Secondary, row: 0)
                .WithButton("Accept", $"{TranscodeCommandMap.ACCEPT_META}:{attachment.Id}", ButtonStyle.Primary, row: 0);

            // We don't have a way to check if ephemeral responses are dismissed
            // If the metadata prompt is dismissed without starting the transcode, we will leak the TranscodeState
            // So setup a job to timeout if transcoding hasn't been started, forcing a cleanup
            JobManager.AddJob(() =>
            {
                try
                {
                    ModifyOriginalResponseAsync(x =>
                    {
                        x.Content = "Command timed out";
                        x.Components = null;
                    }).Result.ResetTimeout();
                }
                finally
                {
                    activeCommands.Remove(attachment.Id);
                }
            }, (s) => s.WithName($"{TranscodeCommandMap.JOB_TIMEOUT}:{attachment.Id}").ToRunOnceIn(10).Minutes());

            activeCommands.Add(attachment.Id, new TranscodeState() { Stream = sourceAudio, FileName = filename, Meta = metadata });

            try
            {
                (await ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = componentBuilder.Build();
                })).ResetTimeout();
            }
            catch
            {
                activeCommands.Remove(attachment.Id);
                throw;
            }
        }

        [ComponentInteraction($"{TranscodeCommandMap.EDIT_META}:*")]
        public async Task EditMetadata(ulong id)
        {
            try
            {
                TranscodeState state = activeCommands[id];

                await RespondWithModalAsync<MetadataModal>($"{TranscodeCommandMap.META_MODAL}:{id}", modifyModal: (mb) =>
                {
                    if (state.Meta.Title != "Unknown")
                        mb.UpdateTextInput(TranscodeCommandMap.META_MODAL_TITLE, state.Meta.Title);
                    if (state.Meta.Artist != "Unknown")
                        mb.UpdateTextInput(TranscodeCommandMap.META_MODAL_ARTIST, state.Meta.Artist);
                    if (!string.IsNullOrEmpty(state.Meta.Album))
                        mb.UpdateTextInput(TranscodeCommandMap.META_MODAL_ALBUM, state.Meta.Album);

                    // It seems all fields of IModal are marked required
                    // Override that for Albums
                    mb.UpdateTextInput(TranscodeCommandMap.META_MODAL_ALBUM, x => x.Required = false);
                });
            }
            catch
            {
                activeCommands.Remove(id);
                throw;
            }
        }

        [ComponentInteraction($"{TranscodeCommandMap.ACCEPT_META}:*")]
        public async Task AcceptMetadata(ulong id) => await ContinueTranscode(id);

        [ModalInteraction($"{TranscodeCommandMap.META_MODAL}:*")]
        public async Task SaveMetadata(ulong id, MetadataModal modal)
        {
            if (!activeCommands.TryGetValue(id, out TranscodeState state))
            {
                // Assume we have timeout
                await DeferAsync();
                return;
            }
            state.Meta.Title = modal.SongTitle;
            state.Meta.Artist = modal.SongArtist;
            state.Meta.Album = modal.SongAlbum;

            await ContinueTranscode(id);
        }

        private async Task ContinueTranscode(ulong id)
        {
            TranscodeState state = activeCommands[id];
            activeCommands.Remove(id);

            // Transcode started, remove timeout job
            JobManager.RemoveJob($"{TranscodeCommandMap.JOB_TIMEOUT}:{id}");

            await DeferAsync();

            var metadata = state.Meta;
            var output = new StringBuilder()
                .AppendLine(state.FileName)
                .AppendLine($"Title: {metadata.Title}")
                .AppendLine($"Artist: {metadata.Artist}")
                .AppendLine($"Album: {metadata.Album}")
                .AppendLine($"Length: {metadata.Duration:m\\:ss}")
                .AppendLine()
                .AppendLine("Transcoding...");

            (await ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = null;
            })).ResetTimeout();

            Stopwatch sw = new Stopwatch();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Transcoding file"));
            sw.Start();
            var tokenSourceTranscode = new CancellationTokenSource();
            Stream transcodedAudio = await OpusTranscode.TranscodeAudio(state.Stream, tokenSourceTranscode.Token);
            state.Stream.Position = 0;
            sw.Stop();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Transcoded (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
            sw.Reset();

            if (false)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Adding file to fingerprint database"));
                sw.Start();
                await Fingerprinting.Fingerprinting.AddTrack(state.Stream, metadata);
                sw.Stop();
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Added (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
                state.Stream.Position = 0;
                sw.Reset();
            }

            output = new StringBuilder()
                .AppendLine(state.FileName)
                .AppendLine($"Title: {metadata.Title}")
                .AppendLine($"Artist: {metadata.Artist}")
                .AppendLine($"Album: {metadata.Album}")
                .AppendLine($"Length: {metadata.Duration:m\\:ss}")
                .AppendLine($"Size: {transcodedAudio.Length / 1024} KiB");

            // Return File
            (await ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = null;
                x.Attachments = new Optional<IEnumerable<FileAttachment>>(new FileAttachment[] { new FileAttachment(transcodedAudio, $"{state.FileName}.ogg") });
            })).ResetTimeout();
        }
    }
}
