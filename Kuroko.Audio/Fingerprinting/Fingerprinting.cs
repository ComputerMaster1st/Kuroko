using Kuroko.Shared;
using SoundFingerprinting;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.Emy;
using SoundFingerprinting.Query;

namespace Kuroko.Audio.Fingerprinting
{
    public class Fingerprinting
    {
        private static IModelService modelService;
        private static readonly IAudioService audioService = new FFmpegAudioService();

        public static void InitService(IServiceProvider serviceProvider)
        {
            if (modelService == null)
                modelService = new Database.EFCoreService(serviceProvider);
        }

        public static async Task<bool> Match(Stream audioStream, double fullLength)
        {
            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await audioStream.CopyToAsync(fileStream);

                return await Match(tempFile.FilePath, fullLength);
            }
        }

        public static async Task<bool> Match(string file, double fullLength)
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
                Console.WriteLine($"Matched no tracks");

            if (resultTracks.Count > 1)
            {
                Console.WriteLine($"Matched multiple tracks");
                foreach (var tr in resultTracks)
                    Console.WriteLine($"{tr.Key} With {tr.Count()} matches. Coverage[0]: {tr.First().TrackCoverageWithPermittedGapsLength:0.00} seconds. Confidence[0]: {tr.First().Coverage.Confidence}");
                Console.WriteLine($"Best match was {result[0].Track.Id}");
            }

            return resultTracks.Count >= 1;
        }

        //TODO, Passthough ID
        public static async Task<string> AddTrack(Stream audioStream, SongMetadata metadata)
        {
            using (var tempFile = new TempFileInstance())
            {
                using (var fileStream = new FileStream(tempFile.FilePath, FileMode.Truncate))
                    await audioStream.CopyToAsync(fileStream);

                return await AddTrack(tempFile.FilePath, metadata);
            }
        }

        public static async Task<string> AddTrack(string file, SongMetadata metadata)
        {
            var track = new TrackInfo(Guid.NewGuid().ToString(), metadata.Title, metadata.Artist);
            var fingerprints = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(file)
                .UsingServices(audioService)
                .Hash();

            // Store hashes in the database for later retrieval
            modelService.Insert(track, fingerprints);

            // Save file somewhere
            // Add to database
            return track.Id;
        }
    }
}
