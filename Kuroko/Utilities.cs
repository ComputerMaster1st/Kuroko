using Discord;
using Ionic.Zip;
using Ionic.Zlib;
using Kuroko.Database.Entities.Guild;
using Kuroko.Shared;
using System.Text;

namespace Kuroko
{
    public static class Utilities
    {
        private static readonly SemaphoreSlim _logLock = new(1);

        public const string SeparatorCharacter = "⬤";

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

        private static DirectoryInfo CreateZip(string zipName, DirectoryInfo directory, out int segmentsMade)
        {
            var dir = Directory.CreateDirectory(Path.Combine(DataDirectories.TEMPZIPS, zipName));
            var file = zipName + ".zip";
            var filePath = Path.Combine(dir.ToString(), file);

            if (File.Exists(filePath))
                File.Delete(filePath);

            using var zip = new ZipFile(filePath, Encoding.UTF8);
            zip.AddDirectory(directory.FullName);
            zip.CompressionLevel = CompressionLevel.BestCompression;
            zip.MaxOutputSegmentSize = 20 * 1024 * 1024;
            zip.Save();

            segmentsMade = zip.NumberOfSegmentsForMostRecentSave;

            return dir;
        }

        public static async Task<(DirectoryInfo ZipDir, int Segments, IUserMessage Message)> ZipAndUploadAsync(
            TicketEntity ticket, DirectoryInfo ticketDir, ITextChannel ticketChannel)
        {
            var zipLocation = CreateZip($"ticket_{ticket.Id}", ticketDir, out int segments);
            var discordAttachments = new List<FileAttachment>();
            void clearAttachments()
            {
                foreach (var file in discordAttachments)
                    file.Dispose();
            }

            IUserMessage sentMessage;
            if (segments < 10)
            {
                foreach (var file in zipLocation.GetFiles())
                    discordAttachments.Add(new FileAttachment(file.FullName));

                sentMessage = await ticketChannel.SendFilesAsync(discordAttachments);
                clearAttachments();
            }
            else
            {
                foreach (var file in zipLocation.GetFiles())
                {
                    discordAttachments.Add(new FileAttachment(file.FullName));

                    if (!(discordAttachments.Count < 10))
                    {
                        sentMessage = await ticketChannel.SendFilesAsync(discordAttachments);

                        clearAttachments();
                        discordAttachments.Clear();
                    }
                }

                sentMessage = await ticketChannel.SendFilesAsync(discordAttachments);
            }

            return (zipLocation, segments, sentMessage);
        }
    }
}
