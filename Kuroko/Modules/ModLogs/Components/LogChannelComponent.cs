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
        [ComponentInteraction($"{ModLogCommandMap.ModLogChannel}:*,*")]
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

        [ComponentInteraction($"{ModLogCommandMap.ModLogChannelSave}:*")]
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

            properties.LogChannelId = selectedChannelId;

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(0);
        }

        private async Task ExecuteAsync(int index)
        {
            var output = new StringBuilder()
                .AppendLine("# Moderation Logging")
                .AppendLine("## Configure Log Channel");
            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var logChannelTag = logChannel is null ? "**Not Set**" : $"<#{logChannel.Id}>";
            var count = 0;
            var user = Context.User as IGuildUser;
            var textChannels = await user.Guild.GetTextChannelsAsync();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{ModLogCommandMap.ModLogChannelSave}:{user.Id}")
                .WithMinValues(1)
                .WithPlaceholder("Select a text channel to send mod logs to");

            foreach (var textChannel in textChannels)
            {
                selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ModLogCommandMap.ModLogChannelIgnore, ModLogCommandMap.ModLogMenu, true);

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
