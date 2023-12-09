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
    public class ModLogUserJoinLeaveEvent
    {
        private enum JoinType
        {
            Joined,
            Left
        }

        private readonly IServiceProvider _serviceProvider;

        public ModLogUserJoinLeaveEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _serviceProvider = services;

            client.UserJoined += UserJoinedAsync;
            client.UserLeft += UserLeftAsync;
        }

        private async Task UserJoinedAsync(SocketGuildUser arg)
        {
            var user = arg as IGuildUser;

            ModLogEntity properties;
            using (var db = _serviceProvider.GetRequiredService<DatabaseContext>())
            {
                properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == user.Guild.Id);

                if (properties is null || !(properties.LogChannelId != 0 && properties.Join))
                    return;
            }

            var textChannel = await user.Guild.GetTextChannelAsync(properties.LogChannelId);
            await ExecuteAsync(textChannel, user, JoinType.Joined);
        }

        private async Task UserLeftAsync(SocketGuild arg1, SocketUser arg2)
        {
            ModLogEntity properties;
            using (var db = _serviceProvider.GetRequiredService<DatabaseContext>())
            {
                properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == arg1.Id);

                if (properties is null || !(properties.LogChannelId != 0 && properties.Leave))
                    return;
            }

            var guild = arg1 as IGuild;
            var user = arg2 as IUser;
            var textChannel = await guild.GetTextChannelAsync(properties.LogChannelId);

            await ExecuteAsync(textChannel, user, JoinType.Left);
        }

        private static async Task ExecuteAsync(ITextChannel textChannel, IUser user, JoinType joinType)
        {
            var embedFields = new List<EmbedFieldBuilder>
            {
                new()
                {
                    Name = "Account Created",
                    Value = user.CreatedAt.ToString("dd/MM/yyyy : hh:mm"),
                    IsInline = true
                },
                new()
                {
                    Name = "Account Id",
                    Value = user.Id,
                    IsInline = true
                }
            };

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Green,
                Title = $"{user.Username} {joinType}!",
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = user.GetAvatarUrl(),
                Fields = embedFields
            };

            await textChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
