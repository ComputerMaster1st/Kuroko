using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs.Components
{
    public class ResumeChannelComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_RESUME}:*,*")]
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

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_RESUME_SAVE}:*,*")]
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
                var temp = properties.IgnoredChannelIds.FirstOrDefault(x => x.Value == channelId);
                properties.IgnoredChannelIds.Remove(temp, Context.Database);
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(index, properties);
        }

        private async Task ExecuteAsync(int index, ModLogEntity propParams = null)
        {
            var output = new StringBuilder()
                .AppendLine("# Moderation Logging")
                .AppendLine("## Resume Moderating Channels");
            var properties = propParams ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var user = Context.User as IGuildUser;
            var count = 0;
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{ModLogCommandMap.CHANNEL_RESUME_SAVE}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select text channels to resume mod logging");

            foreach (var textChannelId in properties.IgnoredChannelIds.Skip(index).ToList())
            {
                var textChannel = await user.Guild.GetChannelAsync(textChannelId.Value);
                selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ModLogCommandMap.CHANNEL_RESUME, ModLogCommandMap.MENU);
            var hasUnknownChannels = false;

            output.AppendLine("Currently Ignored Channels: ");

            if (properties.IgnoredChannelIds.Count > 0)
                foreach (var channelId in properties.IgnoredChannelIds)
                {
                    var channel = Context.Guild.GetChannel(channelId.Value);

                    if (channel is null)
                    {
                        output.AppendLine($"* _UNKNOWN CHANNEL ({channelId.Value})_");
                        hasUnknownChannels = true;
                    }
                    else
                        output.AppendLine($"* <#{channel.Id}>");
                }
            else
                output.AppendLine("* **None Set**");

            if (hasUnknownChannels)
                output.AppendLine("**WARNING: Please remove unknown channels to optimize configuration by selecting them for removal.**");

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
