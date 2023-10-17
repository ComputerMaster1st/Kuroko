﻿using Kuroko.Database.Entities.Message;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Guild
{
    public enum Severity
    {
        Critical,
        Major,
        Minor,
        Unassigned
    }

    public enum TicketType
    {
        ReportMessage,
        ReportUser,
        Unspecified
    }

    public class TicketEntity : IPropertyEntity, ITypeEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;
        public ulong GuildId { get; private set; } = 0;
        public virtual GuildEntity Guild { get; private set; } = null;
        public ulong ChannelId { get; private set; } = 0;
        public ulong SummaryMessageId { get; set; } = 0;
        public int ReportHandlerId { get; set; } = -1;

        public TicketType Type { get; private set; } = TicketType.Unspecified;
        public string Subject { get; private set; } = string.Empty;
        public string RulesViolated { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public ulong SubmitterId { get; private set; } = 0;
        public ulong ReportedUserId { get; private set; } = 0;
        public ulong? ReportedMessageId { get; private set; } = null;

        public DateTimeOffset CreatedAt { get; private set; } = DateTime.UtcNow;

        public Severity Severity { get; set; } = Severity.Unassigned;

        public virtual List<MessageEntity> Messages { get; private set; } = new();

        [NotMapped]
        public long CreatedAtEpoch
        {
            get
            {
                return CreatedAt.ToUnixTimeSeconds();
            }
        }

        public TicketEntity(TicketType type, ulong channelId, string subject, string rulesViolated, string description,
            ulong submitterId, ulong reportedUserId, ulong? reportedMessageId = null)
        {
            Type = type;
            ChannelId = channelId;
            Subject = subject;
            RulesViolated = rulesViolated;
            Description = description;
            SubmitterId = submitterId;
            ReportedUserId = reportedUserId;
            ReportedMessageId = reportedMessageId;
        }
    }
}
