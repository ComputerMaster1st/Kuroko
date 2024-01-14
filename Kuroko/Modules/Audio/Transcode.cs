using Discord;
using Discord.Interactions;
using Kuroko.Audio.FFmpeg;
using Kuroko.Core;
using Kuroko.Shared;
using System.Diagnostics;

namespace Kuroko.Audio
{
    public class Transcode : KurokoModuleBase
    {
        private readonly HttpClient _httpClient = new();

        [SlashCommand("transcode", "Transcode file to opus")]
        public async Task ExecuteAsync(IAttachment attachment)
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, "Got transcode command"));
            await DeferAsync(true);

            Stopwatch sw = new Stopwatch();

            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, "Downloading file"));
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

            if (!match)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Adding file to fingerprint database"));
                sw.Start();
                await Fingerprinting.Fingerprinting.AddTrack(sourceAudio, metadata);
                sw.Stop();
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Added (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
                sourceAudio.Position = 0;
                sw.Reset();
            }

            // TODO: Ask to fill in missing info
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Transcoding file"));
            sw.Start();
            var tokenSourceTranscode = new CancellationTokenSource();
            Stream transcodedAudio = await OpusTranscode.TranscodeAudio(sourceAudio, tokenSourceTranscode.Token);
            sw.Stop();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SLASHCMD, $"Transcoded (Elapsed = {sw.ElapsedMilliseconds / 1000d})"));
            sw.Reset();

            // Return file
            await FollowupWithFileAsync(transcodedAudio, $"{filename}.ogg", $"Title: {metadata.Title}\nArtist: {metadata.Artist}\nLength: {metadata.Duration:m\\:ss}\nSize: {transcodedAudio.Length / 1024}KiB", ephemeral: true);
            return;
        }
    }
}
