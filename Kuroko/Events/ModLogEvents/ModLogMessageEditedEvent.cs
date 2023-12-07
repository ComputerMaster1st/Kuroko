using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize, KurokoEvent]
    public class ModLogMessageEditedEvent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;

        public ModLogMessageEditedEvent(DiscordShardedClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = client;

            _client.MessageUpdated += MessageUpdated;
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var channelType = channel.GetChannelType().GetValueOrDefault();

            if (channelType == ChannelType.DM ||
               (before.HasValue && before.Value.Content == after.Content))
                return;
            if (after.Author.Id == _client.CurrentUser.Id)
                return;

            var db = _serviceProvider.GetRequiredService<DatabaseContext>();
            var guildChannel = channel as IGuildChannel;
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == guildChannel.Guild.Id);

            if (properties is null || !(properties.LogChannelId != 0 && properties.EditedMessages))
                return;
            if (properties.IgnoredChannelIds.Any(x => x.Value == guildChannel.Id))
                return;

            var logChannel = await guildChannel.Guild.GetTextChannelAsync(properties.LogChannelId);
            var originalText = "_**(INFO:** Original message not available due to restart after message creation**)**_";
            var afterText = "_**(INFO:** Edited message is shown as \"Original\" due to not being available in cache.**)**_";

            if (before.HasValue && before.Value.Content.Length <= 1024)
                originalText = before.Value.Content;
            else if (before.HasValue && before.Value.Content.Length > 1024)
                originalText = "_(INFO:**Original message exceeds 1024 character length.**)_";

            if (after.Content.Length <= 1024)
                afterText = after.Content;
            else if (after.Content.Length > 1024)
                afterText = "_(INFO:**Edited message exceeds 1024 character length.**)_";

            var embedFieldBuilders = new List<EmbedFieldBuilder>()
            {
                new()
                {
                    IsInline = false,
                    Name = "Original Message",
                    Value = originalText
                },
                new()
                {
                    IsInline = false,
                    Name = "Edited Message",
                    Value = afterText
                }
            };

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Orange,
                Title = "Message Edited!",
                Url = after.GetJumpUrl(),
                Timestamp = DateTime.UtcNow,
                Fields = embedFieldBuilders,
                Author = new()
                {
                    Name = after.Author.Username,
                    IconUrl = after.Author.GetAvatarUrl()
                },
                Footer = new()
                {
                    Text = $"UID: {after.Author.Id}"
                }
            };

            await logChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
