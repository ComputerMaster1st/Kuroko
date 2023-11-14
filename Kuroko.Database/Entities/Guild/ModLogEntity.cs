using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Guild
{
    public class ModLogEntity : IPropertyEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public ulong GuildId { get; private set; } = 0;
        public virtual GuildEntity Guild { get; private set; } = null;

        public ulong LogChannelId { get; set; } = 0;
        public virtual List<UlongEntity> IgnoredChannelIds { get; private set; } = new();

        #region Flags

        public bool DeletedMessages { get; set; } = false;
        public bool EditedMessages { get; set; } = false;
        public bool Join { get; set; } = false;
        public bool Leave { get; set; } = false;

        #endregion

        #region Specials

        public bool SaveAttachments { get; set; } = false;
        public bool EnableBlackbox { get; set; } = false;

        #endregion
    }
}
