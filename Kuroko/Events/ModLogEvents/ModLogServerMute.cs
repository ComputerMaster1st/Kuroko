using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize, KurokoEvent]
    public class ModLogServerMute
    {
        private readonly IServiceProvider _services;

        public ModLogServerMute(IServiceProvider services, DiscordShardedClient client)
        {
            _services = services;

            client.UserBanned += (user, guild) => Task.Factory.StartNew(() => ExecuteAsync(user, guild));
        }

        private async Task ExecuteAsync(SocketUser user, SocketGuild guildParam)
        {
            using var db = _services.GetRequiredService<DatabaseContext>();
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == guildParam.Id);

            if (properties is null || properties.LogChannelId == 0 || !properties.Ban)
                return;

            var guild = guildParam as IGuild;
            var logChannel = await guild.GetTextChannelAsync(properties.LogChannelId);

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
                Title = $"{user.Username} Banned!",
                Timestamp = DateTime.UtcNow,
                ThumbnailUrl = user.GetAvatarUrl(),
                Fields = embedFields
            };

            await logChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}