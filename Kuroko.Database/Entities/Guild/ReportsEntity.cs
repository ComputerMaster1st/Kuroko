using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Guild
{
    public class ReportsEntity : IPropertyEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public ulong GuildId { get; private set; } = 0;
        public virtual GuildEntity Guild { get; private set; } = null;

        public ulong ReportCategoryId { get; set; } = 0;
        public ulong TranscriptsChannelId { get; set; } = 0;

        public bool RecordMessages { get; set; } = false;

        public virtual List<ReportHandler> ReportHandlers { get; private set; } = new();



        // TODO: List of tickets


        // ticket will need:
        // id, header/subject, rules violated, description, reporter, reported, created at, closed at, threat level, messages, attachments

        // messages will need:
        // msg id, created at, edited list, deleted at (null), content

        // edited msg will need:
        // id, edited at, content
    }

    public class ReportHandler : ITypeEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public string Name { get; set; } = string.Empty;

        public ulong RoleId { get; set; } = 0;

        public int Level { get; set; } = 0;
    }
}
