using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize]
    public class ModLogMessageEditedEvent
    {
        private readonly IServiceProvider _serviceProvider;

        public ModLogMessageEditedEvent(DiscordShardedClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            client.MessageUpdated += MessageUpdated;
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            var channelType = channel.GetChannelType().GetValueOrDefault();

            if (channelType == ChannelType.DM)
                return;

            using var db = _serviceProvider.GetRequiredService<DatabaseContext>();
            var guildChannel = channel as IGuildChannel;
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == guildChannel.Guild.Id);

            if (properties is null || !(properties.LogChannelId != 0 && properties.EditedMessages))
                return;

            var logChannel = await guildChannel.Guild.GetTextChannelAsync(properties.LogChannelId);

            var embedFieldBuilders = new List<EmbedFieldBuilder>()
            {
                new()
                {
                    IsInline = false,
                    Name = "Original Message",
                    Value = before.HasValue ? before.Value.Content : "_**(INFO:** Original message not available due to restart after message creation**)**_"
                },
                new()
                {
                    IsInline = false,
                    Name = "Edited Message",
                    Value = after.Content ?? "_**(INFO:** Edited msg is shown as \"Original\" due to internal cache issue. Contact Bot Dev.**)**_"
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
