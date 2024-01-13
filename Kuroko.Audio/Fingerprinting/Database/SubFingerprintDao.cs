using Kuroko.Database;
using Kuroko.Database.Entities.Audio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.Data;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Kuroko.Audio.Fingerprinting.Database
{
    internal class SubFingerprintDao
    {
        private readonly IMetaFieldsFilter metaFieldsFilter = new MetaFieldsFilter();

        internal SubFingerprintDao() { }

        public void InsertHashDataForTrack(IEnumerable<HashedFingerprint> hashedFingerprints, TrackDataEntity trackData)
        {
            foreach (var hashedFingerprint in hashedFingerprints)
            {
                var entity = new SubFingerprintEntity(hashedFingerprint.SequenceNumber, hashedFingerprint.StartsAt, hashedFingerprint.OriginalPoint);

                // Insert hashes to hashTable
                for (var i = 0; i < hashedFingerprint.HashBins.Length; i++)
                {
                    var hashEntity = new HashEntity((long)i << 32 | (uint)hashedFingerprint.HashBins[i]);
                    entity.Hashes.Add(hashEntity);
                }
                trackData.SubFingerprints.Add(entity);
            }
        }

        public IEnumerable<SubFingerprintEntity> ReadHashedFingerprintsByTrackReference(DatabaseContext databaseContext, int trackReference) => databaseContext.SubFingerprints.Where(x => x.TrackDataId == trackReference);

        public IEnumerable<SubFingerprintEntity> ReadSubFingerprints(IServiceScope scope, IEnumerable<int[]> hashes, QueryConfiguration queryConfiguration)
        {
            Stopwatch fnSW = Stopwatch.StartNew();
            var allSubs = new ConcurrentBag<SubFingerprintEntity>();
            var times = new ConcurrentBag<long>();

            // MaxDegreeOfParallelism to be adjusted based on performance testing
            Parallel.ForEach(hashes, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, hashedFingerprint =>
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                Stopwatch stopwatch = Stopwatch.StartNew();
                foreach (var subFingerprint in ReadSubFingerprints(
                    db,
                    hashedFingerprint,
                    queryConfiguration.ThresholdVotes,
                    queryConfiguration.YesMetaFieldsFilters,
                    queryConfiguration.NoMetaFieldsFilters
                ))
                {
                    // Force load Hashes so subFingerprint can cross DatabaseContext boundary
                    _ = subFingerprint.Hashes.AsEnumerable();
                    allSubs.Add(subFingerprint);
                }
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedMilliseconds);
            });

            Console.WriteLine($"Inner ReadSubFingerprints() Took avg {times.Average()}ms");
            Console.WriteLine($"Inner ReadSubFingerprints() Took max {times.Max()}ms");
            var b = allSubs.OrderBy(x => x.Id);
            var ret = allSubs.Distinct();
            fnSW.Stop();
            Console.WriteLine($"Outer ReadSubFingerprints() Took {fnSW.ElapsedMilliseconds}ms");
            return ret;
        }

        private IEnumerable<SubFingerprintEntity> ReadSubFingerprints(DatabaseContext ctx, int[] hashes, int thresholdVotes,
            IDictionary<string, string> yesMetaFieldsFilters,
            IDictionary<string, string> noMetaFieldsFilters)
        {
            var subFingeprintIds = GetSubFingerprintMatches(ctx, hashes, thresholdVotes);
            var subFingerprints = subFingeprintIds.Select(x => ctx.SubFingerprints.AsNoTracking().FirstOrDefault(y => y.Id == x));

            if (yesMetaFieldsFilters.Count > 0 || noMetaFieldsFilters.Count > 0)
            {
                return subFingerprints
                    .GroupBy(subFingerprint => subFingerprint.TrackDataId)
                    .Where(group =>
                    {
                        var trackData = ctx.TrackData.AsNoTracking().FirstOrDefault(x => x.Id == group.Key);
                        var result = metaFieldsFilter.PassesFilters(/*trackData.MetaFields*/
                            new Dictionary<string, string>(), yesMetaFieldsFilters, noMetaFieldsFilters);
                        return result;
                    })
                    .SelectMany(x => x.ToList());
            }

            return subFingerprints;
        }

        private IEnumerable<long> GetSubFingerprintMatches(DatabaseContext ctx, IReadOnlyList<int> hashes, int thresholdVotes)
        {
            var compositHashes = hashes.Select((hash, table) => (long)table << 32 | (uint)hash);

            return ctx.Hashs.AsNoTracking()
                .Where(x => compositHashes.Contains(x.IndexedHash))
                .Select(x => x.SubFingerprintId)
                .GroupBy(x => x)
                .Where(x => x.Count() >= thresholdVotes)
                .Select(x => x.Key).ToList();
        }

        public int GetSubFingerprintCounts(DatabaseContext databaseContext) => databaseContext.SubFingerprints.Count();

        public IEnumerable<int> GetHashCountsPerTable(DatabaseContext databaseContext)
        {
            if (databaseContext.Hashs.Count() == 0)
                yield break;

            int tables = (int)(databaseContext.Hashs.Max(x => x.IndexedHash) >> 32) + 1;

            for (int i = 0; i < tables; i++)
            {
                long lower = (long)i << 32;
                long upper = (long)i << 32 | uint.MaxValue; ;
                yield return databaseContext.Hashs.Where(x => x.IndexedHash >= lower & x.IndexedHash <= upper).Count();
            }
        }
    }
}
