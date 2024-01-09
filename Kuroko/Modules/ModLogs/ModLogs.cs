using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ViewAuditLog)]
    public class ModLogs : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Moderation Logging");
            }
        }

        [SlashCommand("modlogs", "Configure moderation logging")]
        public async Task EntryAsync()
            => await ExecuteAsync();

        [ComponentInteraction($"{ModLogCommandMap.MENU}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_DELETE}:*")]
        public async Task UnsetAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            properties.LogChannelId = 0;

            await DeferAsync();
            await ExecuteAsync(true, properties);
        }

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_IGNORE_RESET}:*")]
        public async Task ResetIgnoreAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            properties.IgnoredChannelIds.Clear(Context.Database);

            await DeferAsync();
            await ExecuteAsync(true, properties);
        }
        
        private async Task ExecuteAsync(bool isReturning = false, ModLogEntity propParam = null)
        {
            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var mainRow = 0;
            var exitRow = 1;
            var componentBuilder = new ComponentBuilder()
                .WithButton("Configure Log Channel", $"{ModLogCommandMap.CHANNEL}:{user.Id},0", ButtonStyle.Primary, row: mainRow);

            if (properties.LogChannelId != 0)
            {
                componentBuilder
                    .WithButton("Logging Options", $"{ModLogCommandMap.ENTRIES_MENU}:{user.Id}", ButtonStyle.Primary, row: mainRow)
                    .WithButton("Ignore Channels", $"{ModLogCommandMap.CHANNEL_IGNORE}:{user.Id},0", ButtonStyle.Primary, row: mainRow);
                
                if (properties.IgnoredChannelIds.Count > 0)
                    componentBuilder
                        .WithButton("Resume Channels", $"{ModLogCommandMap.CHANNEL_RESUME}:{user.Id},0", ButtonStyle.Primary, row: mainRow)
                        .WithButton("Monitor All Channels", $"{ModLogCommandMap.CHANNEL_IGNORE_RESET}:{user.Id}", ButtonStyle.Success, row: mainRow);
            }

            componentBuilder.WithButton("Exit", $"{GlobalCommandMap.EXIT}:{user.Id}", ButtonStyle.Secondary, row: exitRow);

            var msgComponents = componentBuilder.Build();
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var channelTag = (logChannel is null) ? "**Not Set**" : $"<#{logChannel.Id}>";
            var output = OutputMsg
                .AppendLine($"Logging Channel: {channelTag}")
                .AppendLine("## Ignored Channels");
            var temp = new List<ulong>();

            if (properties.IgnoredChannelIds.Count > 0)
                foreach (var channelId in properties.IgnoredChannelIds)
                {
                    var channel = Context.Guild.GetChannel(channelId.Value);

                    if (channel is null)
                    {
                        temp.Add(channelId.Value);
                        continue;
                    }

                    output.AppendLine($"* <#{channel.Id}>");
                }
            else
                output.AppendLine($"* **None set**");

            if (!isReturning)
            {
                await RespondAsync(output.ToString(), components: msgComponents);
                (await Context.Interaction.GetOriginalResponseAsync()).SetTimeout(1);
            }
            else
                (await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = msgComponents;
                })).ResetTimeout();
        }
    }
}
