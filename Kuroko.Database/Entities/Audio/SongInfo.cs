using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Audio
{
    public class SongInfo
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public string Title { get; set; } = null;
        public string Artist { get; set; } = null;
        public string Album { get; set; } = null;

        public TimeSpan Length { get; set; } = new TimeSpan(0);

        public long FileSize { get; set; } = 0;

        public SongInfo(string title, string artist, string album, TimeSpan length, long fileSize)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Length = length;
            FileSize = fileSize;
        }
    }
}
