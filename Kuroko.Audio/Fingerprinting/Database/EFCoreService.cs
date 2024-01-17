using Kuroko.Database;
using Kuroko.Database.Entities.Audio;
using Microsoft.Extensions.DependencyInjection;
using SoundFingerprinting;
using SoundFingerprinting.Configuration;
using SoundFingerprinting.DAO;
using SoundFingerprinting.DAO.Data;
using SoundFingerprinting.Data;
using System.Diagnostics;

namespace Kuroko.Audio.Fingerprinting.Database
{
    internal class EFCoreService : IModelService
    {
        private const string VIDEO_NOT_SUPPORTED_MESSAGE = "This storage is designed to handle only audio media type of tracks";
        private const string Id = "efcore-model-service";

        private readonly IServiceProvider _services;

        private SubFingerprintDao SubFingerprintDao { get; }

        public EFCoreService(IServiceProvider service)
        {
            _services = service;
            SubFingerprintDao = new SubFingerprintDao();
        }

        public IEnumerable<ModelServiceInfo> Info
        {
            get
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                return new[] { new ModelServiceInfo(Id, db.SFPTrackData.Count(), SubFingerprintDao.GetSubFingerprintCounts(db), SubFingerprintDao.GetHashCountsPerTable(db).ToArray()) };
            }
        }

        public void Insert(TrackInfo trackInfo, AVHashes hashes)
        {
            if (trackInfo.MediaType.HasFlag(MediaType.Video) || hashes.Audio is null)
                throw new NotSupportedException(VIDEO_NOT_SUPPORTED_MESSAGE);

            using var scope = _services.CreateScope();

            var audioHashes = hashes.Audio;
            var fingerprints = audioHashes.ToList();
            if (fingerprints.Count == 0)
                return;

            // We don't store media type
            var trackEntity = new TrackDataEntity(trackInfo.Id, audioHashes.DurationInSeconds);
            SubFingerprintDao.InsertHashDataForTrack(audioHashes, trackEntity);

            var db = scope.ServiceProvider.GetService<DatabaseContext>();
            db.SFPTrackData.Add(trackEntity);
            db.SaveChanges();
        }

        public void UpdateTrack(TrackInfo trackInfo)
        {
            if (trackInfo.MediaType.HasFlag(MediaType.Video))
                throw new NotSupportedException(VIDEO_NOT_SUPPORTED_MESSAGE);

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetService<DatabaseContext>();

            var track = ReadTrackEntityById(db, trackInfo.Id);
            if (track == null)
                throw new ArgumentException($"Could not find track {trackInfo.Id} to update", nameof(trackInfo.Id));

            // We don't store media type
            track.SongId = trackInfo.Id;

            db.SaveChanges();
        }

        public void DeleteTrack(string trackId)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetService<DatabaseContext>();

            var track = ReadTrackEntityById(db, trackId);
            if (track == null)
                return;

            db.Remove(track);
            db.SaveChanges();
        }

        public IEnumerable<SubFingerprintData> Query(Hashes hashes, QueryConfiguration config)
        {
            using var scope = _services.CreateScope();

            Stopwatch sw = Stopwatch.StartNew();
            if (hashes.MediaType != MediaType.Audio)
                throw new NotSupportedException(VIDEO_NOT_SUPPORTED_MESSAGE);

            var queryHashes = hashes.Select(hashedFingerprint => hashedFingerprint.HashBins);

            var ret = hashes.Count > 0 ? SubFingerprintDao.ReadSubFingerprints(scope, queryHashes, config).Select(RepackSubFingerprint).ToList() : Enumerable.Empty<SubFingerprintData>();
            sw.Stop();
            Console.WriteLine($"Query Took {sw.ElapsedMilliseconds}ms");
            return ret;
        }

        public AVHashes ReadHashesByTrackId(string trackId)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var track = ReadTrackEntityById(db, trackId);
            if (track == null)
                return AVHashes.Empty;

            var fingerprints = SubFingerprintDao
                .ReadHashedFingerprintsByTrackReference(db, track.Id)
                .Select(subFingerprint => new HashedFingerprint(GetHashes(subFingerprint), subFingerprint.SequenceNumber, subFingerprint.SequenceAt, subFingerprint.OriginalPoint));
            return new AVHashes(new Hashes(fingerprints, track.Length, MediaType.Audio), Hashes.GetEmpty(MediaType.Video));
        }

        public IEnumerable<TrackData> ReadTracksByReferences(IEnumerable<IModelReference> references)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            foreach (var trackReference in references)
            {
                int reference = trackReference.Get<int>();
                yield return RepackTrackData(db.SFPTrackData.FirstOrDefault(x => x.Id == reference));
            }
        }

        public TrackInfo ReadTrackById(string trackId)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var trackData = ReadTrackEntityById(db, trackId);
            if (trackData == null)
                return null;

            var metaFields = CopyMetaFields(/*trackData.MetaFields*/ null);
            metaFields.Add("TrackLength", $"{trackData.Length: 0.000}");
            return new TrackInfo(trackData.SongId, "", "", metaFields, MediaType.Audio);
        }

        public IEnumerable<string> GetTrackIds()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            return db.SFPTrackData.Select(x => x.SongId);
        }

        public TrackDataEntity ReadTrackEntityById(DatabaseContext ctx, string id) => ctx.SFPTrackData.FirstOrDefault(x => x.SongId == id);

        private static IDictionary<string, string> CopyMetaFields(IDictionary<string, string> metaFields)
        {
            return metaFields == null ? new Dictionary<string, string>() : metaFields.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static TrackData RepackTrackData(TrackDataEntity entity)
        {
            return new TrackData(entity.SongId, "", "", entity.Length, new ModelReference<int>(entity.Id), /*entity.MetaFields*/ new Dictionary<string, string>(), MediaType.Audio);
        }

        private static SubFingerprintData RepackSubFingerprint(SubFingerprintEntity entity)
        {
            var ret = new SubFingerprintData(GetHashes(entity), entity.SequenceNumber, entity.SequenceAt, new ModelReference<long>(entity.Id), new ModelReference<int>(entity.TrackDataId), entity.OriginalPoint);
            return ret;
        }

        private static int[] GetHashes(SubFingerprintEntity entity)
        {
            return entity.Hashes.OrderBy(x => x.IndexedHash).Select(x => (int)x.IndexedHash).ToArray();
        }
    }
}
