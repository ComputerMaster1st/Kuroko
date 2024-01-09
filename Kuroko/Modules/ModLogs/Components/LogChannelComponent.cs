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
    public class LogChannelComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ModLogCommandMap.CHANNEL}:*,*")]
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

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_SAVE}:*")]
        public async Task ReturningAsync(ulong interactedUserId, string channelId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var selectedChannelId = ulong.Parse(channelId);
            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            if (properties.LogChannelId == selectedChannelId)
                properties.LogChannelId = 0;
            else
                properties.LogChannelId = selectedChannelId;

            await ExecuteAsync(0, properties);
        }

        private async Task ExecuteAsync(int index, ModLogEntity propParam = null)
        {
            var output = new StringBuilder()
                .AppendLine("# Moderation Logging")
                .AppendLine("## Configure Log Channel");
            var properties = propParam ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var logChannelTag = logChannel is null ? "**Not Set**" : $"<#{logChannel.Id}>";
            var count = 0;
            var user = Context.User as IGuildUser;
            var textChannels = await user.Guild.GetTextChannelsAsync();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{ModLogCommandMap.CHANNEL_SAVE}:{user.Id}")
                .WithMinValues(1)
                .WithPlaceholder("Select a text channel to send mod logs to");

            foreach (var textChannel in textChannels.Skip(index).ToList())
            {
                if (logChannel != null && logChannel.Id == textChannel.Id)
                    selectMenuBuilder.AddOption($"(Unset) {textChannel.Name}", textChannel.Id.ToString(), "Turns off ModLogs if unset!");
                else
                    selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ModLogCommandMap.CHANNEL, ModLogCommandMap.MENU, true);

            output.AppendLine($"Current Log Channel: {logChannelTag}");

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
