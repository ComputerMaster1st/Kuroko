using Discord;
using Ionic.Zip;
using Ionic.Zlib;
using Kuroko.Shared;
using System.Text;

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

        public static DirectoryInfo CreateZip(string zipName, DirectoryInfo directory, out int segmentsMade)
        {
            var dir = Directory.CreateDirectory(Path.Combine(DataDirectories.TEMPZIPS, zipName));
            var file = zipName + ".zip";

            using var zip = new ZipFile(file, Encoding.UTF8);
            zip.AddDirectory(directory.ToString());
            zip.CompressionLevel = CompressionLevel.BestCompression;
            zip.MaxOutputSegmentSize = 20 * 1024 * 1024;
            zip.Save();

            segmentsMade = zip.NumberOfSegmentsForMostRecentSave;

            return dir;
        }
    }
}
