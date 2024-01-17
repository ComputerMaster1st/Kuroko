using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Audio
{

    public class TrackDataEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public string SongId { get; set; }

        public double Length { get; set; } = 0;

        public virtual List<SubFingerprintEntity> SubFingerprints { get; private set; } = new();

        public TrackDataEntity(string songId, double length)
        {
            SongId = songId;
            Length = length;
        }
    }
}
