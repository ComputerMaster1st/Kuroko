using Kuroko.Database.Entities.Guild;
using Kuroko.Database.Entities.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Audio
{

    public class TrackDataEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public string TrackInfoId { get; set; } = "";

        public string Artist { get; set; } = "";
        public string Title { get; set; } = "";
        public double Length { get; set; } = 0;

        //public IDictionary<string, string> MetaFields { get; set; }

        public virtual List<SubFingerprintEntity> SubFingerprints { get; private set; } = new();

        public TrackDataEntity(string trackInfoId, string artist, string title, double length)
        {
            TrackInfoId = trackInfoId;
            Artist = artist;
            Title = title;
            Length = length;
        }
    }
}
