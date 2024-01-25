using Kuroko.Shared;
using System;
using System.Diagnostics;

namespace Kuroko.Audio.FFmpeg
{
    public class OpusTranscode
    {
        public static async Task<Stream> TranscodeAudio(Stream data, SongMetadata metadata, CancellationToken cancellationToken)
        {
            if (!metadata.TranscodeNeedsFile)
            {
                var output = await TranscodeStream(data, cancellationToken);

                if (output.Length < 1024)
                {
                    output.Dispose();
                    Console.WriteLine($"Failed tor transcode from stream, retrying from file");
                }
                else
                    return output;
            }

            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await data.CopyToAsync(fileStream);

                return await TranscodeFile(tempFile.FilePath, cancellationToken);
            }
        }

        private static async Task<Stream> TranscodeStream(Stream data, CancellationToken cancellationToken)
        {
            MemoryStream output = new MemoryStream();
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i pipe: -hide_banner -v quiet -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus -map_metadata -1 pipe:",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => { awaitExitSource.SetResult(process.ExitCode); };
                process.Start();

                Task outTask = process.StandardOutput.BaseStream.CopyToAsync(output);
                await data.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
                process.StandardInput.Close();
                await outTask;

                await awaitExitSource.Task;

                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            output.Position = 0;
            return output;
        }

        private static async Task<Stream> TranscodeFile(string file, CancellationToken cancellationToken)
        {
            MemoryStream output = new MemoryStream();
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {file} -hide_banner -v quiet -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus -map_metadata -1 pipe:",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => { awaitExitSource.SetResult(process.ExitCode); };
                process.Start();

                await process.StandardOutput.BaseStream.CopyToAsync(output);

                await awaitExitSource.Task;

                if (cancellationToken.IsCancellationRequested)
                {
                    process.Kill();
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            output.Position = 0;
            return output;
        }
    }
}
