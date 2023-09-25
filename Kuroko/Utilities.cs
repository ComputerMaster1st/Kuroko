using Discord;

namespace Kuroko
{
    public class Utilities
    {
        private static readonly SemaphoreSlim _logLock = new(1);

        public const string SeparatorCharacter = "⬤";

        // TODO: Write anything out to console & output logs

        public static async Task WriteLogAsync(LogMessage message)
        {
            await _logLock.WaitAsync();

            try
            {
                await File.AppendAllTextAsync(string.Format("{0}/{1}.log",
                    DataDirectories.LOG,
                    DateTime.Today.ToString("yyyy_MM_dd")),
                    message + Environment.NewLine);
            }
            finally
            {
                _logLock.Release();
            }

            Console.WriteLine(message);
        }
    }
}
