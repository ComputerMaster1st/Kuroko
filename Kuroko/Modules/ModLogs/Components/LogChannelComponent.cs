using Discord;
using Discord.Interactions;
using Kuroko.Core.Attributes;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class LogChannelComponent : ModLogBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Moderation Logging")
                    .AppendLine("## Configure Log Channel");
            }
        }

        [ComponentInteraction($"{CommandIdMap.ModLogChannel}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(index, OutputMsg);
        }

        private async Task ExecuteAsync(int index, StringBuilder output)
        {
            var properties = await GetPropertiesAsync();
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var logChannelTag = logChannel is null ? "**Not Set**" : $"<#{logChannel.Id}>";
            var menu = await MLMenu.BuildLogChannelMenuAsync(Context.User as IGuildUser, index);

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
