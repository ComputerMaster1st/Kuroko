using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Audio
{
    public class SubFingerprintEntity : IEquatable<SubFingerprintEntity>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; private set; } = 0;

        public int TrackDataId { get; private set; } = 0;
        public virtual TrackDataEntity TrackData { get; private set; } = null;

        public virtual List<HashEntity> Hashes { get; private set; } = new();

        public uint SequenceNumber { get; set; }

        public float SequenceAt { get; set; }

        public byte[] OriginalPoint { get; set; }

        public SubFingerprintEntity(uint sequenceNumber, float sequenceAt, byte[] originalPoint)
        {
            SequenceNumber = sequenceNumber;
            SequenceAt = sequenceAt;
            OriginalPoint = originalPoint;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubFingerprintEntity);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(SubFingerprintEntity? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }
    }
}
