using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Reports.Modals;
using Kuroko.Modules.Tickets;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class ProcessUserReportComponent : KurokoModuleBase
    {
        [ModalInteraction($"{ReportsCommandMap.ReportUserModal}:*")]
        public async Task ExecuteAsync(ulong reportedUserId, ReportUserModal modal)
        {
            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var category = Context.Guild.GetCategoryChannel(properties.ReportCategoryId);
            var user = Context.User as IGuildUser;
            var reportedUser = Context.Guild.GetUser(reportedUserId);
            var channel = await Context.Guild.CreateTextChannelAsync($"report-{reportedUser.GlobalName ?? reportedUser.Username}", x =>
            {
                x.CategoryId = category.Id;
            });
            var handlers = properties.ReportHandlers.OrderByDescending(x => x.Level);
            var escalateSelectMenu = new SelectMenuBuilder()
            {
                CustomId = TicketsCommandMap.Escalate,
                MaxValues = 1,
                Placeholder = "Select Handler Type"
            };
            IRole lowestHandlerRole = null;

            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow));

            foreach (var handler in handlers)
            {
                var role = Context.Guild.GetRole(handler.RoleId);

                if (role is null)
                    continue;

                escalateSelectMenu.AddOption(handler.Name, role.Id.ToString());
                lowestHandlerRole = role;
            }

            // Severity Level SelectMenu
            // var severitySelectMenu = new SelectMenuBuilder();

            var output = new StringBuilder()
                .AppendLine("# Reported User")
                .AppendLine($"**Subject:** {modal.Subject}")
                .AppendLine($"**Accused:** {reportedUser.Mention} ({reportedUser.Id})")
                .AppendLine($"**Rules Violated:** {modal.Rules}")
                .AppendLine($"**Severity:** _TBI_")
                .AppendLine($"**Current Handler:** {lowestHandlerRole.Mention}")
                .AppendLine("## Description")
                .AppendLine(modal.Description ?? "_No description provided!_")
                .AppendLine("## Please provide any evidence to help this report!");

            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(escalateSelectMenu, 0)
                .WithButton("Close Ticket", $"{TicketsCommandMap.CloseTicket}", ButtonStyle.Secondary, row: 1);

            await channel.SendMessageAsync(output.ToString(), components: componentBuilder.Build());
            await RespondAsync($"Report Created! View it here at {channel.Mention}", ephemeral: true);
        }
    }
}
