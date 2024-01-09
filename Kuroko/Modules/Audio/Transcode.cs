using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Modules.Audio.FFmpeg;
using Kuroko.Shared;
using System.Security.Cryptography;

namespace Kuroko.Modules.Audio
{
    public class Transcode : KurokoModuleBase
    {
        private readonly HttpClient _httpClient = new();

        [SlashCommand("transcode", "Transcode file to opus")]
        public async Task ExecuteAsync(IAttachment attachment)
        {
            string filename = Path.GetFileNameWithoutExtension(attachment.Filename);
            byte[] bytes = await _httpClient.GetByteArrayAsync(attachment.Url ?? attachment.ProxyUrl);

            bool isAudio = FileMimeType.GetFromBytes(bytes).Any(x => x.MimeType.StartsWith("audio/"));
            if (!isAudio)
            {
                await RespondAsync("Not an audio file", ephemeral: true);
                return;
            }

            await DeferAsync(true);

            MemoryStream sourceAudio = new MemoryStream(bytes);
            SongMetadata metadata = await Probe.GetMetadataAsync(sourceAudio);
            sourceAudio.Position = 0;

            if (metadata.Duration.TotalMinutes > 30)
            {
                await FollowupAsync("Audio file too long", ephemeral: true);
                return;
            }

            // TODO: Ask to fill in missing info

            var tokenSourceTranscode = new CancellationTokenSource();
            Stream transcodedAudio = await OpusTranscode.TranscodeAudio(sourceAudio, tokenSourceTranscode.Token);

            // Return file
            await FollowupWithFileAsync(transcodedAudio, $"{filename}.ogg", $"Title: {metadata.Title}\nArtist: {metadata.Artist}\nLength: {metadata.Duration:m\\:ss}\nSize: {transcodedAudio.Length / 1024}KiB", ephemeral: true);
            return;
        }
    }
}
