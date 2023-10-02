using Discord;
using Discord.Interactions;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs.Components
{
    public class ResumeChannelComponent : ModLogBase
    {
        [ComponentInteraction($"{CommandIdMap.ModLogChannelResume}:*,*")]
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

        [ComponentInteraction($"{CommandIdMap.ModLogChannelResumeSave}:*,*")]
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
            var properties = propParams ?? await GetPropertiesAsync();
            var menu = await MLMenu.BuildResumeLogChannelMenuAsync(Context.User as IGuildUser, properties, index);
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
