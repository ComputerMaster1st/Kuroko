using Kuroko.Database;
using Kuroko.Database.Entities.Audio;
using Kuroko.Shared;
using Microsoft.Extensions.DependencyInjection;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
using SoundFingerprinting.Query;

namespace Kuroko.Audio.Fingerprinting
{
    public class FingerprintingCache
    {
        private readonly IServiceProvider _services;
        private readonly IModelService modelService;
        private readonly IAudioService audioService = new FFmpegAudioService();

        public FingerprintingCache(IServiceProvider serviceProvider)
        {
            _services = serviceProvider;
            modelService = new Database.EFCoreService(serviceProvider);
        }

        public async Task<SongInfo> Match(Stream audioStream, double fullLength)
        {
            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await audioStream.CopyToAsync(fileStream);

                return await Match(tempFile.FilePath, fullLength);
            }
        }

        public string GetFilePath(SongInfo songInfo)
        {
            return Path.Combine(DataDirectories.TRANSCODE, $"{songInfo.Id}.ogg");
        }

        public async Task<SongInfo> Match(string file, double fullLength)
        {
            int secondsToAnalyze = 15; // Number of seconds to analyze
            int startAtSecond = 20; // Start 20 seconds in

            // Adjust for short songs
            if (fullLength < (2 * startAtSecond + secondsToAnalyze) & (fullLength >= secondsToAnalyze))
            {
                startAtSecond = (int)((fullLength - secondsToAnalyze) / 2);
            }
            else if (fullLength < secondsToAnalyze)
            {
                secondsToAnalyze = (int)fullLength;
                startAtSecond = 0;
            }

            // Query the underlying database for similar audio sub-fingerprints
            var queryResult = await QueryCommandBuilder.Instance.BuildQueryCommand()
                .From(file, secondsToAnalyze, startAtSecond)
                .UsingServices(modelService, audioService)
                .Query();

            // Iterate over the results if any
            // Results are ordered best to worst
            List<ResultEntry> result = new();
            foreach (var (entry, _) in queryResult.ResultEntries)
            {
                // Output only those tracks that matched enough of the stored track.
                // Also check if matched song length matches
                if ((entry.TrackCoverageWithPermittedGapsLength >= 0.65 * secondsToAnalyze) &&
                    (fullLength < entry.Track.Length + 20d) && (fullLength > entry.Track.Length - 20d))
                {
                    result.Add(entry);
                }
            }

            var resultTracks = result.GroupBy(x => x.Track.Id).ToList();

            if (resultTracks.Count == 0)
            {
                Console.WriteLine($"Matched no tracks");
                return null;
            }

            if (resultTracks.Count > 1)
            {
                Console.WriteLine($"Matched multiple tracks");
                foreach (var tr in resultTracks)
                    Console.WriteLine($"{tr.Key} With {tr.Count()} matches. Coverage[0]: {tr.First().TrackCoverageWithPermittedGapsLength:0.00} seconds. Confidence[0]: {tr.First().Coverage.Confidence}");
                Console.WriteLine($"Best match was {result[0].Track.Id}");
            }

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            return db.SongInfo.Where(x => x.Id == int.Parse(result[0].Track.Id)).First();
        }

        public async Task AddTrack(Stream originalStream, Stream transcodedStream, SongMetadata metadata)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            // We assume that the stream is at position 0

            // Store metadata into database
            SongInfo songInfo = new(metadata.Title, metadata.Artist, metadata.Album, metadata.Duration, transcodedStream.Length);
            db.SongInfo.Add(songInfo);
            await db.SaveChangesAsync();

            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await originalStream.CopyToAsync(fileStream);

                // Store fingerprint into database
                await FingerprintTrack(tempFile.FilePath, songInfo.Id);
            }

            // Store file
            using (var outFile = new FileStream(Path.Combine(DataDirectories.TRANSCODE, $"{songInfo.Id}.ogg"), FileMode.CreateNew))
                await transcodedStream.CopyToAsync(outFile);
        }

        //public async Task AddTrack(string originalFile, string transcodedFile, SongMetadata metadata)
        //{
        //    using var scope = _services.CreateScope();
        //    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        //    // We assume that the stream is at position 0

        //    // Store metadata into database
        //    SongInfo songInfo = new(metadata.Title, metadata.Artist, metadata.Album, metadata.Duration, new FileInfo(transcodedFile).Length);
        //    db.SongInfo.Add(songInfo);
        // await db.SaveChangesAsync();

        //    // fingerprint into database
        //    await FingerprintTrack(originalFile, songInfo.Id);

        //    File.Copy(originalFile, Path.Combine(DataDirectories.TRANSCODE, $"{songInfo.Id}.ogg"));  
        //}

        private async Task FingerprintTrack(string file, int id)
        {
            // Metadata stored seperatly
            var track = new TrackInfo(id.ToString(), "", "");

            // Fingerprint audio
            var fingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(file)
                .UsingServices(audioService)
                .Hash();

            // Store in database
            await Task.Run(() => modelService.Insert(track, fingerprints));
        }
    }
}
