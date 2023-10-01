using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize]
    public class ModLogUserJoinLeaveEvent
    {
        private enum JoinType
        {
            Joined,
            Left
        }

        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _serviceProvider;

        public ModLogUserJoinLeaveEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _serviceProvider = services;

            _client.UserJoined += UserJoinedAsync;
            _client.UserLeft += UserLeftAsync;
        }

        private async Task UserJoinedAsync(SocketGuildUser arg)
        {
            using var db = _serviceProvider.GetRequiredService<DatabaseContext>();

            var user = arg as IGuildUser;
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == user.Guild.Id);

            if (properties is null || !(properties.LogChannelId != 0 && properties.Join))
                return;

            var textChannel = await user.Guild.GetTextChannelAsync(properties.LogChannelId);

            await ExecuteAsync(textChannel, user, properties, JoinType.Joined);
        }

        private async Task UserLeftAsync(SocketGuild arg1, SocketUser arg2)
        {
            using var db = _serviceProvider.GetRequiredService<DatabaseContext>();

            var guild = arg1 as IGuild;
            var user = arg2 as IUser;
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == arg1.Id);

            if (properties is null || !(properties.LogChannelId != 0 && properties.Leave))
                return;

            var textChannel = await guild.GetTextChannelAsync(properties.LogChannelId);

            await ExecuteAsync(textChannel, user, properties, JoinType.Left);
        }

        private static async Task ExecuteAsync(ITextChannel textChannel, IUser user, ModLogEntity properties, JoinType joinType)
        {
            var embedFields = new List<EmbedFieldBuilder>();
            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Green,
                Title = $"{user.Mention} {joinType}!",
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = user.GetAvatarUrl(),
                Footer = new()
                {
                    Text = "Time represented as UTC 0"
                }
            };

            var accountCreated = new EmbedFieldBuilder()
                .WithName("Account Created")
                .WithValue(user.CreatedAt.ToString("dd/MM/yyyy : hh:mm"))
                .WithIsInline(true);
            embedFields.Add(accountCreated);

            var accountId = new EmbedFieldBuilder()
                .WithName("Account Id")
                .WithValue(user.Id)
                .WithIsInline(true);
            embedFields.Add(accountId);

            embedBuilder.WithFields(embedFields);

            await textChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
