using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Reports.Modals;
using Kuroko.Modules.Tickets;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class ProcessUserReportComponent : KurokoModuleBase
    {
        [ModalInteraction($"{ReportsCommandMap.USER_MODAL}:*")]
        public async Task CreateAsync(ulong reportedUserId, ReportModal modal)
        {
            var reportProperties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var guildRoot = await Context.Database.Guilds.GetOrCreateRootAsync(Context.Guild.Id);

            var reportedUser = Context.Guild.GetUser(reportedUserId);
            var user = Context.User as IGuildUser;

            var category = Context.Guild.GetCategoryChannel(reportProperties.ReportCategoryId);
            var channel = await Context.Guild.CreateTextChannelAsync($"report-{reportedUser.GlobalName ?? reportedUser.Username}", x =>
            {
                x.CategoryId = category.Id;
            });
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow));

            var newTicket = new TicketEntity(channel.Id, modal.Subject, modal.Rules, modal.Description, user.Id, reportedUser.Id);
            guildRoot.Tickets.Add(newTicket);

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(newTicket, reportProperties);
        }

        [RequireUserGuildPermission(GuildPermission.ManageMessages)]
        [ComponentInteraction($"{TicketsCommandMap.HANDLER_CHANGE}:*")]
        public async Task EscalateAsync(int ticketId, string result)
        {
            var ticket = await Context.Database.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            ticket.ReportHandlerId = int.Parse(result);

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var handler = properties.ReportHandlers.FirstOrDefault(x => x.Id == ticket.ReportHandlerId);
            var handlerName = handler is null ? "_Handler Missing! Please Fix!_" : handler.Name;
            var role = Context.Guild.GetRole(handler is null ? 0 : handler.RoleId);
            var roleMention = role is null ? "_Role Missing! Please Fix!_" : role.Mention;

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(ticket);
            await RespondAsync($"Ticket has been diverted to **{handlerName}** [{roleMention}] by {Context.User.Mention}.");
        }

        [RequireUserGuildPermission(GuildPermission.ManageMessages)]
        [ComponentInteraction($"{TicketsCommandMap.SEVERITY}:*")]
        public async Task SeverityAsync(int ticketId, string result)
        {
            var ticket = await Context.Database.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            var severity = Enum.Parse<Severity>(result);

            ticket.Severity = severity;

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(ticket);
            await RespondAsync($"Ticket severity has been updated to **{severity}** by {Context.User.Mention}.");
        }

        private async Task ExecuteAsync(TicketEntity ticket, ReportsEntity propParam = null)
        {
            var reportProperties = propParam ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var escalateSelectMenu = new SelectMenuBuilder()
            {
                CustomId = $"{TicketsCommandMap.HANDLER_CHANGE}:{ticket.Id}",
                MaxValues = 1,
                MinValues = 1,
                Placeholder = "Select Handler Type"
            };

            ReportHandler lowestHandler = null;
            foreach (var hnd in reportProperties.ReportHandlers.OrderByDescending(x => x.Level))
            {
                var r = Context.Guild.GetRole(hnd.RoleId);

                if (r is null)
                    continue;

                escalateSelectMenu.AddOption(hnd.Name, hnd.Id.ToString());

                lowestHandler ??= hnd;
            }

            if (ticket.ReportHandlerId == -1)
                ticket.ReportHandlerId = lowestHandler.Id;

            var severitySelectMenu = new SelectMenuBuilder()
            {
                CustomId = $"{TicketsCommandMap.SEVERITY}:{ticket.Id}",
                MaxValues = 1,
                MinValues = 1,
                Placeholder = "Select Severity",
                Options = new()
                {
                    new SelectMenuOptionBuilder()
                    {
                        Label = Severity.Critical.ToString(),
                        Value = Severity.Critical.ToString()
                    },
                    new SelectMenuOptionBuilder()
                    {
                        Label = Severity.Major.ToString(),
                        Value = Severity.Major.ToString()
                    },
                    new SelectMenuOptionBuilder()
                    {
                        Label = Severity.Minor.ToString(),
                        Value = Severity.Minor.ToString()
                    }
                }
            };

            var handler = reportProperties.ReportHandlers.FirstOrDefault(x => x.Id == ticket.ReportHandlerId);
            var handlerName = handler is null ? "_Handler Missing! Please Fix!_" : handler.Name;
            var role = Context.Guild.GetRole(handler is null ? 0 : handler.RoleId);
            var roleMention = role is null ? "_Role Missing! Please Fix!_" : role.Mention;
            var reportedUser = Context.Guild.GetUser(ticket.ReportedUserId);

            var output = new StringBuilder()
                .AppendLine("# Reported User")
                .AppendLine($"**Date Created:** <t:{ticket.CreatedAtEpoch}:F>")
                .AppendLine($"**Subject:** {ticket.Subject}")
                .AppendLine($"**Accused:** {reportedUser.Mention} ({reportedUser.Id})")
                .AppendLine($"**Rules Violated:** {ticket.RulesViolated}")
                .AppendLine($"**Severity:** {ticket.Severity}")
                .AppendLine($"**Current Handler:** {handlerName} [{roleMention}]")
                .AppendLine("## Description")
                .AppendLine(ticket.Description ?? "_No description provided!_")
                .AppendLine("## Please provide any evidence to help this report!");

            var componentBuilder = new ComponentBuilder()
                .WithSelectMenu(escalateSelectMenu, 0)
                .WithSelectMenu(severitySelectMenu, 0)
                .WithButton("Close Ticket", $"{TicketsCommandMap.CLOSE}:{ticket.Id}", ButtonStyle.Secondary, row: 1);

            var channel = Context.Guild.GetTextChannel(ticket.ChannelId);

            if (ticket.SummaryMessageId == 0)
            {
                var msg = await channel.SendMessageAsync(output.ToString(), components: componentBuilder.Build());
                ticket.SummaryMessageId = msg.Id;

                await RespondAsync($"Report Created! View it here at {channel.Mention}", ephemeral: true);
            }
            else
            {
                var msg = await Context.Channel.GetMessageAsync(ticket.SummaryMessageId) as IUserMessage;
                await msg.ModifyAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = componentBuilder.Build();
                });
            }
        }
    }
}
