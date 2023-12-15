using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.Reports
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class ConfigReport : KurokoModuleBase
    {
        [SlashCommand("reports-config", "Configure the report/ticket system (Requires ManageGuild Permission)")]
        public Task ConfigAsync()
        {
            return ExecuteAsync();
        }

        [ComponentInteraction($"{ReportsCommandMap.MENU}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{ReportsCommandMap.AUTOMATE_MESSAGE}:*")]
        public async Task AutomateMessagesAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);

            properties.RecordMessages = !properties.RecordMessages;

            await DeferAsync();
            await ExecuteAsync(true, properties);
        }

        private async Task ExecuteAsync(bool isReturning = false, ReportsEntity propParam = null)
        {
            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            string categoryName = "Not Set";
            string transcriptChannelName = "**Not Set**";

            var category = Context.Guild.GetCategoryChannel(properties.ReportCategoryId);
            var transcript = Context.Guild.GetTextChannel(properties.TranscriptsChannelId);

            if (properties.ReportCategoryId != 0 && category is null)
                categoryName = "_Missing Category! Please Fix!_";
            else if (category != null)
                categoryName = category.Name;

            if (properties.TranscriptsChannelId != 0 && transcript is null)
                transcriptChannelName = "**_Missing Channel! Please Fix!_**";
            else if (transcript != null)
                transcriptChannelName = transcript.Mention;

            var output = new StringBuilder()
                .AppendLine("# Reports Configuration")
                .AppendLine($"Report Category : **{categoryName}**")
                .AppendLine($"Transcript Channel : {transcriptChannelName}")
                .AppendLine("## Report Handlers");

            if (properties.ReportHandlers.Count > 0)
                foreach (var handler in properties.ReportHandlers.OrderByDescending(x => x.Level))
                {
                    var role = Context.Guild.GetRole(handler.RoleId);

                    if (role is null)
                    {
                        output.AppendLine($"* {handler.Name} : **_(Role missing! Please Fix!)_**");
                        continue;
                    }

                    output.AppendLine($"* {handler.Name} : **{role.Name}**");
                }
            else
                output.AppendLine("* **No handlers configured. Please set them up.**");

            var menusRow = 0;
            var togglesRow = 1;
            var exitRow = 2;
            var componentBuilder = new ComponentBuilder()
                .WithButton("Report Category",
                    $"{ReportsCommandMap.CATEGORY}:{user.Id},0",
                    ButtonStyle.Primary,
                    row: menusRow)
                .WithButton("Report Transcript Channel",
                    $"{ReportsCommandMap.TRANSCRIPT}:{user.Id},0",
                    ButtonStyle.Primary,
                    row: menusRow)
                .WithButton("Add Handlers",
                    $"{ReportsCommandMap.HANDLER_ADD}:{user.Id},0",
                    ButtonStyle.Primary,
                    row: menusRow);

            if (properties.ReportHandlers.Count > 0)
                componentBuilder.WithButton("Remove Handlers",
                    $"{ReportsCommandMap.HANDLER_REMOVE}:{user.Id},0",
                    ButtonStyle.Danger,
                    row: menusRow);

            componentBuilder.WithButton("Automated Message Log Generation",
                    $"{ReportsCommandMap.AUTOMATE_MESSAGE}:{user.Id}",
                    Pagination.IsButtonToggle(properties.RecordMessages),
                    row: togglesRow)
                .WithButton("Exit",
                    $"{GlobalCommandMap.EXIT}:{user.Id}",
                    ButtonStyle.Secondary,
                    row: exitRow);

            if (!isReturning)
            {
                await RespondAsync(output.ToString(), components: componentBuilder.Build());
                (await Context.Interaction.GetOriginalResponseAsync()).SetTimeout(1);
            }
            else
                (await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = componentBuilder.Build();
                })).ResetTimeout();
        }
    }
}
