using System;
using System.Diagnostics;
using System.Text.Json;
using Kuroko.Shared;

namespace Kuroko.Audio.FFmpeg
{
    public class Probe
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
            (Metadata root, bool needsFile) = await ProbeFileAsync(url);
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

            metadata.TranscodeNeedsFile = needsFile;

            return metadata;
        }

        private static async Task<(Metadata Metadata, bool NeedsFileTranscode)> ProbeFileAsync(string path)
        {
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();
            string json;
            string stats;

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-i \"{path}\" -hide_banner -show_format -print_format json -v quiet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => awaitExitSource.SetResult(process.ExitCode);
                process.Start();

                // FFprobe outputs in UTF8
                using StreamReader streamReader = new StreamReader(process.StandardOutput.BaseStream, System.Text.Encoding.UTF8);

                json = await streamReader.ReadToEndAsync();
                await awaitExitSource.Task;
            }

            if (await awaitExitSource.Task != 0)
                throw new Exception("FFprobe closed with a non-0 exit code");

            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Metadata metadata = JsonSerializer.Deserialize<Metadata>(json, options);

            // m4a audio files can only be piped into ffmpeg if the file
            // is encoded with faststart. we can infer this from trace logs
            bool isMOV = metadata?.format?.format_long_name == "QuickTime / MOV";
            bool fastStart = true;

            // FFprobe can give incorrect duration on some files
            // FFmpeg is able to get accurate duration when decoding
            awaitExitSource = new TaskCompletionSource<int>();
            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{path}\" -hide_banner -v {(isMOV ? "trace" : "quiet")} -vn -progress pipe:1 -f null -",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = isMOV,
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => awaitExitSource.SetResult(process.ExitCode);
                process.Start();

                var fastStartTask = new Task<bool>(() =>
                {
                    bool faststart = false;
                    string line;
                    while ((line = process.StandardError.ReadLine()) != null)
                    {
                        // If moov appeares first, the file is encoded for faststart
                        // we can pipe into ffmpeg
                        if (line.Contains("type:'moov'"))
                        {
                            faststart = true;
                            break;
                        }
                        // If mdat appeares first, we need to provide ffmpeg a file
                        if (line.Contains("type:'mdat'"))
                        {
                            faststart = false;
                            break;
                        }
                    }

                    // Discard rest of the logs
                    _ = process.StandardError.ReadToEnd();
                    return faststart;
                });

                if (isMOV)
                    fastStartTask.Start();

                stats = await Task.Run<string>(() =>
                {
                    List<string> output = new List<string>();
                    string line;
                    while ((line = process.StandardOutput.ReadLine()) != null)
                        output.Add(line);
                    return output.FindLast(x => x.StartsWith("out_time_us=")).Substring("out_time_us=".Length);
                });

                if (isMOV)
                    fastStart = fastStartTask.Result;

                await awaitExitSource.Task;
            }

            if (double.TryParse(stats, out double time_us))
                metadata.format.duration = (time_us / 1000000d).ToString();
            else
                throw new ArgumentException("FFmpeg (probe) gave back unreadable information.");

            if (await awaitExitSource.Task != 0)
                throw new Exception("FFmpeg (probe) closed with a non-0 exit code");

            return (metadata, !fastStart);
        }
    }
}
