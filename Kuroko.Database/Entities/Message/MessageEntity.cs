﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Message
{
    public class MessageEntity : IDiscordEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; private set; } = 0;

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeletedAt { get; set; } = null;

        public string Content { get; private set; } = string.Empty;

        public virtual List<EditedMessageEntity> EditedMessages { get; private set; } = new();

        public virtual List<AttachmentEntity> Attachments { get; private set; } = new();

        public MessageEntity(ulong messageId, string content)
        {
            Id = messageId;
            Content = content;
        }
    }

    public class EditedMessageEntity : ITypeEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public DateTimeOffset EditedAt { get; private set; } = DateTimeOffset.UtcNow;

        public string Content { get; private set; } = string.Empty;

        public EditedMessageEntity(string content)
            => Content = content;
    }
}
