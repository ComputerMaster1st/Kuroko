using Kuroko.Database.Entities.Guild;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Message
{
    public class MessageEntity : IDiscordEntity, IPropertyEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; private set; } = 0;

        public ulong GuildId { get; private set; } = 0;
        public virtual GuildEntity Guild { get; private set; } = null;
        public ulong ChannelId { get; private set; } = 0;
        public ulong UserId { get; private set; } = 0;

        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeletedAt { get; set; } = null;

        public string Content { get; private set; } = string.Empty;

        public virtual List<EditedMessageEntity> EditedMessages { get; private set; } = new();

        public virtual List<AttachmentEntity> Attachments { get; private set; } = new();

        public MessageEntity(ulong id, ulong channelId, ulong userId, string content)
        {
            Id = id;
            ChannelId = channelId;
            UserId = userId;
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
