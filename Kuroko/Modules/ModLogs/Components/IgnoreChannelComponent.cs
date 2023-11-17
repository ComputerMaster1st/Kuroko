using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class IgnoreChannelComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_IGNORE}:*,*")]
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

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_IGNORE_SAVE}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] channelIds)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var selectedChannelIds = channelIds.Select(ulong.Parse);
            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

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
            var properties = propParams ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var count = 0;
            var user = Context.User as IGuildUser;
            var textChannels = await user.Guild.GetTextChannelsAsync();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{ModLogCommandMap.CHANNEL_IGNORE_SAVE}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select text channels to ignore mod logging");

            foreach (var textChannel in textChannels.Skip(index).ToList())
            {
                if (properties.IgnoredChannelIds.Any(x => x.Value == textChannel.Id))
                    continue;

                selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ModLogCommandMap.CHANNEL_IGNORE, ModLogCommandMap.MENU);

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
