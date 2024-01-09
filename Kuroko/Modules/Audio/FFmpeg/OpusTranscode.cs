using System;
using System.Diagnostics;

namespace Kuroko.Modules.Audio.FFmpeg
{
    internal class OpusTranscode
    {
        public static async Task<Stream> TranscodeAudio(Stream data, CancellationToken cancellationToken)
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
    }
}
