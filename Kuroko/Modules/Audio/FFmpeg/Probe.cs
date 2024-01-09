using System;
using System.Diagnostics;
using System.Text.Json;

namespace Kuroko.Modules.Audio.FFmpeg
{
    internal class Probe
    {
        public static async Task<SongMetadata> GetMetadataAsync(Stream data)
        {
            // Need to use a file to read duration
            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await data.CopyToAsync(fileStream);

                return await GetMetadataAsync(tempFile.FilePath);
            }
        }

        public static async Task<SongMetadata> GetMetadataAsync(string url)
        {
            Metadata root = await ProbeFileAsync(url);
            SongMetadata metadata = new SongMetadata();

            if (root == null || root.format == null)
                throw new ArgumentException("FFprobe gave back unreadable information.");

            metadata.Duration = TimeSpan.FromSeconds(double.Parse(root.format.duration));

            if (root.format.tags != null)
            {
                var tags = root.format.tags;
                if (tags.title != null)
                    metadata.Title = tags.title;

                if (tags.artist != null)
                    metadata.Artist = tags.artist;

                if (tags.album != null)
                    metadata.Album = tags.album;
            }

            return metadata;
        }

        private static async Task<Metadata> ProbeFileAsync(string path)
        {
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();
            string json;

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-i {path} -hide_banner -show_format -print_format json -v quiet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => { awaitExitSource.SetResult(process.ExitCode); };
                process.Start();

                json = await process.StandardOutput.ReadToEndAsync();
                await awaitExitSource.Task;
            }

            if (await awaitExitSource.Task != 0)
                throw new Exception("FFprobe closed with a non-0 exit code");

            return JsonSerializer.Deserialize<Metadata>(json);
        }
    }
}
