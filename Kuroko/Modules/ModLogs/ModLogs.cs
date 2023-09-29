using Discord;
using Discord.Interactions;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class ModLogs : ModLogBase
    {
        private StringBuilder OutputMsg
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

        [ComponentInteraction($"{CommandIdMap.ModLogMenu}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{CommandIdMap.ModLogChannelDelete}:*")]
        public async Task UnsetAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var properties = await GetPropertiesAsync();

            properties.LogChannelId = 0;

            await DeferAsync();
            await ExecuteAsync(true, properties);
        }

        [ComponentInteraction($"{CommandIdMap.ModLogChannelIgnoreReset}:*")]
        public async Task ResetIgnoreAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var properties = await GetPropertiesAsync();

            properties.IgnoredChannelIds.Clear(Context.Database);

            await DeferAsync();
            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(true, properties);
        }

        private async Task ExecuteAsync(bool isReturning = false, ModLogEntity propParam = null)
        {
            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync();
            var msgComponents = MLMenu.BuildMenu(user, properties);
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
