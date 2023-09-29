using Discord;
using Discord.Interactions;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class IgnoreChannelComponent : ModLogBase
    {
        [ComponentInteraction($"{CommandIdMap.ModLogChannelIgnore}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(index);
        }

        [ComponentInteraction($"{CommandIdMap.ModLogChannelIgnoreSave}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] channelIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var selectedChannelIds = channelIds.Select(ulong.Parse);
            var properties = await GetPropertiesAsync();

            foreach (var channelId in selectedChannelIds)
            {
                var channel = Context.Guild.GetChannel(channelId);

                if (properties.IgnoredChannelIds.Any(x => x.Value == channel.Id))
                    continue;

                properties.IgnoredChannelIds.Add(new(channel.Id));
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(index, properties);
        }

        private async Task ExecuteAsync(int index, ModLogEntity propParams = null)
        {
            var output = new StringBuilder()
                .AppendLine("# Moderation Logging")
                .AppendLine("## Configure Ignore Channels");
            var properties = propParams ?? await GetPropertiesAsync();
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var logChannelTag = logChannel is null ? "**Not Set**" : $"<#{logChannel.Id}>";
            var menu = await MLMenu.BuildIgnoreLogChannelMenuAsync(Context.User as IGuildUser, index);

            output.AppendLine("Currently Ignored Channels: ");

            if (properties.IgnoredChannelIds.Count > 0)
                foreach (var channelId in properties.IgnoredChannelIds)
                {
                    var channel = Context.Guild.GetChannel(channelId.Value);

                    if (channel is null)
                        output.AppendLine($"* _UNKNOWN CHANNEL ({channelId.Value})_");
                    else
                        output.AppendLine($"* <#{channel.Id}>");
                }
            else
                output.AppendLine("* **None Set**");

            if (!menu.HasOptions)
                output.AppendLine("* No text channels available.");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
