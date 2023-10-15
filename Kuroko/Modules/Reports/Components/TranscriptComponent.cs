using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class TranscriptComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ReportsCommandMap.TRANSCRIPT}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ExecuteAsync(index);
        }

        [ComponentInteraction($"{ReportsCommandMap.TRANSCRIPT_SAVE}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string result)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var channelId = ulong.Parse(result);

            properties.TranscriptsChannelId = channelId;

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(index, properties);
        }

        private async Task ExecuteAsync(int index, ReportsEntity propParam = null)
        {
            await DeferAsync();

            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            string transcriptChannelName = "Not Set";

            var channel = Context.Guild.GetTextChannel(properties.TranscriptsChannelId);

            if (properties.TranscriptsChannelId != 0 && channel is null)
                transcriptChannelName = "**_Missing Channel! Please Fix!_**";
            else if (channel != null)
                transcriptChannelName = channel.Name;

            var selectOptions = new List<SelectMenuOptionBuilder>();

            if (properties.TranscriptsChannelId != 0)
            {
                selectOptions.Add(new()
                {
                    Label = "(Unset Channel)",
                    Value = "0"
                });
            }

            foreach (var chn in Context.Guild.TextChannels)
            {
                selectOptions.Add(new()
                {
                    Label = chn.Name,
                    Value = chn.Id.ToString()
                });
            }

            var output = new StringBuilder()
                .AppendLine("# Reports Configuration")
                .AppendLine("## Channel Selection")
                .AppendLine($"Selected Channel: **{transcriptChannelName}**");

            var selectMenuBuilder = new SelectMenuBuilder()
            {
                CustomId = $"{ReportsCommandMap.TRANSCRIPT_SAVE}:{user.Id},0",
                MinValues = 1,
                Placeholder = "Select a channel to use for transcripts",
                Options = selectOptions
            };
            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ReportsCommandMap.CATEGORY, ReportsCommandMap.MENU, true);

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
