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
        public async Task CreateAsync(ulong reportedUserId, ReportUserModal modal)
        {
            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var category = Context.Guild.GetCategoryChannel(properties.ReportCategoryId);
            var reportedUser = Context.Guild.GetUser(reportedUserId);
            var channel = await Context.Guild.CreateTextChannelAsync($"report-{reportedUser.GlobalName ?? reportedUser.Username}", x =>
            {
                x.CategoryId = category.Id;
            });

            var user = Context.User as IGuildUser;
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow));

            // TODO: Create the ticket

            await ExecuteAsync(properties, channel);
        }

        [RequireUserGuildPermission(GuildPermission.ManageMessages)]
        [ComponentInteraction(TicketsCommandMap.Escalate)]
        public async Task EscalateAsync(string result)
        {
            var reportProperties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var handler = reportProperties.ReportHandlers.FirstOrDefault(x => x.RoleId == ulong.Parse(result));

            // TODO: Modify Ticket w/ new handler
        }

        [RequireUserGuildPermission(GuildPermission.ManageMessages)]
        [ComponentInteraction(TicketsCommandMap.Severity)]
        public async Task SeverityAsync(string result)
        {
            // TODO: Modify Ticket w/ new severity
        }

        private async Task ExecuteAsync(ReportsEntity reportProperties, ITextChannel channel = null, ReportHandler handlerParam = null)
        {
            var handler = handlerParam;
            var handlers = reportProperties.ReportHandlers.OrderByDescending(x => x.Level);
            var escalateSelectMenu = new SelectMenuBuilder()
            {
                CustomId = TicketsCommandMap.Escalate,
                MaxValues = 1,
                MinValues = 1,
                Placeholder = "Select Handler Type"
            };

            foreach (var hnd in handlers)
            {
                var r = Context.Guild.GetRole(hnd.RoleId);

                if (r is null)
                    continue;

                escalateSelectMenu.AddOption(hnd.Name, r.Id.ToString());

                handler ??= hnd;
            }

            // Severity Level SelectMenu
            // var severitySelectMenu = new SelectMenuBuilder();

            var role = Context.Guild.GetRole(handler.RoleId);

            // TODO: Compile summary from ticket in database

            var output = new StringBuilder()
                .AppendLine("# Reported User")
                .AppendLine($"**Subject:** {modal.Subject}")
                .AppendLine($"**Accused:** {reportedUser.Mention} ({reportedUser.Id})")
                .AppendLine($"**Rules Violated:** {modal.Rules}")
                .AppendLine($"**Severity:** _TBI_")
                .AppendLine($"**Current Handler:** **{handler.Name} - {role.Mention}**")
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
