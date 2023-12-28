using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Events.ModLogEvents
{
    [PreInitialize, KurokoEvent]
    public class ModLogAudit
    {
        private readonly IServiceProvider _services;

        public ModLogAudit(DiscordShardedClient client, IServiceProvider services)
        {
            _services = services;

            client.AuditLogCreated += (x, y) => Task.Factory.StartNew(() => ExecuteAsync(x, y));
        }

        private static Embed CreateLogEmbed(string title, SocketAuditLogEntry entry)
            => new EmbedBuilder
            {
                Title = title,
                Author = new()
                {
                    IconUrl = entry.User.GetDisplayAvatarUrl(),
                    Name = entry.User.GlobalName,
                },
                Color = Color.Blue,
                Description = entry.Reason,
                Timestamp = entry.CreatedAt
            }.Build();

        public async Task ExecuteAsync(SocketAuditLogEntry entry, SocketGuild guildParam)
        {
            using var db = _services.GetRequiredService<DatabaseContext>();
            var properties = await db.GuildModLogs.FirstOrDefaultAsync(x => x.GuildId == guildParam.Id);

            if (properties.LogChannelId == 0)
                return;

            var guild = guildParam as IGuild;
            var user = entry.User as IGuildUser;
            var logChannel = await guild.GetTextChannelAsync(properties.LogChannelId);

            if (properties.AuditLog)
                switch (entry.Action)
                {
                    case ActionType.Ban:
                    case ActionType.MessageDeleted:
                        return;
                    default:
                        await logChannel.SendMessageAsync(embed: CreateLogEmbed($"Audit Log: {entry.Action}", entry));
                        return;
                }

            switch (entry.Action)
            {
                case ActionType.Kick:
                    if (properties.Kick)
                        await logChannel.SendMessageAsync(embed: CreateLogEmbed("Member Kicked!", entry));
                    break;
                default:
                    return;
            }
        }
    }
}