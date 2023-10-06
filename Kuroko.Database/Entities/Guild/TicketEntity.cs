using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Guild
{
    public enum Severity
    {
        Critical,
        Major,
        Minor,
        Unknown
    }

    public class TicketEntity : IPropertyEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;
        public ulong GuildId { get; private set; } = 0;
        public virtual GuildEntity Guild { get; private set; } = null;

        public string Subject { get; private set; } = string.Empty;
        public string RulesViolated { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public ulong SubmitterId { get; private set; } = 0;
        public ulong ReportedUserId { get; private set; } = 0;

        public DateTimeOffset CreatedAt { get; private set; } = DateTime.UtcNow;

        public Severity Severity { get; set; } = Severity.Unknown;

        [NotMapped]
        public long CreatedAtEpoch
        {
            get
            {
                return CreatedAt.ToUnixTimeSeconds();
            }
        }

        public TicketEntity(string subject, string rulesViolated, string description,
            ulong submitterId, ulong reportedUserId)
        {
            Subject = subject;
            RulesViolated = rulesViolated;
            Description = description;
            SubmitterId = submitterId;
            ReportedUserId = reportedUserId;
        }


        // TODO: List of tickets

        // messages, attachments

        // messages will need:
        // msg id, created at, edited list, deleted at (null), content

        // edited msg will need:
        // id, edited at, content
    }
}
