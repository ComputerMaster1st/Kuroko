using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Reports.Modals;
using Kuroko.Modules.Tickets;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class ProcessReportComponent : KurokoModuleBase
    {
        private readonly BlackboxService _blackbox;

        public ProcessReportComponent(BlackboxService blackbox)
            => _blackbox = blackbox;

        [ModalInteraction($"{ReportsCommandMap.REPORT_USER}:*")]
        public async Task CreateUserReportAsync(ulong reportedUserId, ReportModal modal)
        {
            var (TicketId, _) = await CreateAsync(TicketType.ReportUser, reportedUserId, modal);
            await _blackbox.GenerateUserMessageHistoryAsync(TicketId, Context.Client);
        }

        [ModalInteraction($"{ReportsCommandMap.REPORT_MESSAGE}:*")]
        public async Task CreateMessageReportAsync(ulong reportedMsgId, ReportModal modal)
        {
            var (_, TicketChannel) = await CreateAsync(TicketType.ReportMessage, reportedMsgId, modal);
            var reportedMsg = await Context.Channel.GetMessageAsync(reportedMsgId);
            var attachments = new List<FileAttachment>();

            var msgEntity = await _blackbox.GetMessageAsync(reportedMsgId);

            if (msgEntity.Message is null)
                msgEntity = await _blackbox.StoreMessageAsync(reportedMsg);

            var output = new StringBuilder()
                .AppendLine($"## Reported Message {reportedMsg.GetJumpUrl()} | ID: {reportedMsg.Id}")
                .AppendLine($"* **Attachments Found:** {attachments.Count}")
                .AppendLine("## Please provide any screenshots to provide context and support your report!");

            await TicketChannel.SendFilesAsync(attachments, reportedMsg.Content, embed: TrackedMessageEmbed.Build(reportedMsg.Content, reportedMsg.Timestamp));

            attachments.ForEach(x => x.Dispose());
        }

        private async Task<(int TicketId, ITextChannel TicketChannel)> CreateAsync(TicketType type, ulong reportedId, ReportModal modal)
        {
            var reportProperties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var guildRoot = await Context.Database.Guilds.GetOrCreateRootAsync(Context.Guild.Id);
            var user = Context.User as IGuildUser;

            IGuildUser reportedUser;
            IMessage reportedMessage = null;
            if (type == TicketType.ReportMessage)
            {
                reportedMessage = await Context.Channel.GetMessageAsync(reportedId);
                reportedUser = reportedMessage.Author as IGuildUser;
            }
            else
                reportedUser = Context.Guild.GetUser(reportedId);

            var category = Context.Guild.GetCategoryChannel(reportProperties.ReportCategoryId);
            var channel = await Context.Guild.CreateTextChannelAsync($"report-{reportedUser.GlobalName ?? reportedUser.Username}", x =>
            {
                x.CategoryId = category.Id;
            });
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow));

            var newTicket = new TicketEntity(TicketType.ReportUser, channel.Id, modal.Subject, modal.Rules, modal.Description, user.Id, reportedUser.Id, reportedMessage?.Id);
            guildRoot.Tickets.Add(newTicket);

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(newTicket, reportProperties);

            return (newTicket.Id, channel);
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

                if (lowestHandler is null || lowestHandler.Level > hnd.Level)
                    lowestHandler = hnd;
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

            var output = new StringBuilder();

            switch (ticket.Type)
            {
                case TicketType.ReportMessage:
                    output.AppendLine("# Reported Message");
                    break;
                default:
                    output.AppendLine("# Reported User");
                    break;
            }

            output.AppendLine($"**Date Created:** <t:{ticket.CreatedAtEpoch}:F>")
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
