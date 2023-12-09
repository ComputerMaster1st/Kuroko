using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize, KurokoEvent]
    public class ModLogMessageDeletedEvent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordShardedClient _client;

        public ModLogMessageDeletedEvent(DiscordShardedClient client, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _client = client;

            _client.MessageDeleted += MessageDeleted;
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> msg, Cacheable<IMessageChannel, ulong> channel)
        {
            var chn = await channel.GetOrDownloadAsync();
            var channelType = chn.GetChannelType().GetValueOrDefault();

            if (channelType == ChannelType.DM)
                return;

            var message = msg.HasValue ? msg.Value : null;

            if (message != null && message.Author.Id == _client.CurrentUser.Id)
                return;

            var guildChannel = chn as IGuildChannel;

            ModLogEntity properties;
            using (var db = _serviceProvider.GetRequiredService<DatabaseContext>())
            {
                properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == guildChannel.Guild.Id);

                if (properties is null || !(properties.LogChannelId != 0 && properties.EditedMessages))
                    return;
                if (properties.IgnoredChannelIds.Any(x => x.Value == guildChannel.Id))
                    return;
            }

            var logChannel = await guildChannel.Guild.GetTextChannelAsync(properties.LogChannelId);
            var embedBuilder = new EmbedBuilder()
            {
                Author = new()
                {
                    Name = message is null ? "( INFO: Unknown User )" : message.Author.Username,
                    IconUrl = message?.Author.GetAvatarUrl()
                },
                Color = Color.Red,
                Title = "Message Deleted!",
                Timestamp = DateTime.UtcNow,
                Footer = new()
                {
                    Text = $"UID: {(message is null ? "( Unknown )" : message.Author.Id)}"
                },
                Description = message is null ? "_**(INFO:** Original message not available due to restart after message creation**)**_" : message.Content
            };

            await logChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
