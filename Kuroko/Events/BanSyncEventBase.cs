using Discord;

namespace Kuroko.Events;

public abstract class BanSyncEventBase
{
    protected static Embed CreateEmbed(string title, string username, ulong uid, string description,
        string thumbnailUrl, string reason)
        => new EmbedBuilder
        {
            Title = title,
            Timestamp = DateTimeOffset.Now,
            ThumbnailUrl = thumbnailUrl,
            Color = Color.Orange,
            Description = description,
            Fields = [
                new EmbedFieldBuilder
                {
                    Name = "Username",
                    Value = username,
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "UID",
                    Value = uid.ToString(),
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = reason
                }
            ]
        }.Build();
}