using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Reports;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.Tickets.Components
{
    public class CloseComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{TicketsCommandMap.CLOSE}:*")]
        public async Task ExecuteAsync(int ticketId)
        {
            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            ITextChannel chn = null;

            if (properties.TranscriptsChannelId != 0)
            {
                chn = Context.Guild.GetTextChannel(properties.TranscriptsChannelId);

                if (chn is null)
                {
                    await (Context.Channel as ITextChannel).DeleteAsync();
                    return;
                }
            }

            var msg = await chn.SendMessageAsync($"Preparing Ticket Transcript: {ticketId}... This may take a while...");

            await RespondAsync("Ticket Closed! Performing cleanup now...");
            await Task.Delay(2000);

            var root = await Context.Database.Guilds.FirstOrDefaultAsync(x => x.Id == Context.Guild.Id);
            var ticket = await Context.Database.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            var handler = properties.ReportHandlers.FirstOrDefault(x => x.Id == ticket.ReportHandlerId);
            var directory = Directory.CreateDirectory($"{DataDirectories.TEMPFILES}/ticket_{ticket.Id}");

            await File.WriteAllTextAsync(Path.Combine(directory.ToString(), "ticket.txt"), await PrepareTranscriptAsync(ticket, handler, directory));
            var (ZipDir, Segments) = await UserMessageHistory.ZipAndUploadAsync(ticket, directory, chn);

            directory.Delete(true);
            ZipDir.Delete(true);
            root.Tickets.Remove(ticket, Context.Database);

            await (Context.Channel as ITextChannel).DeleteAsync();
            await msg.ModifyAsync(x =>
            {
                x.Content = $"Ticket Transcript: {ticket.Id}. Uploaded {Segments} zip file(s).";
            });
        }

        private async Task<string> PrepareTranscriptAsync(TicketEntity ticket, ReportHandler handler, DirectoryInfo directory)
        {
            var reportedUser = Context.Guild.GetUser(ticket.ReportedUserId);
            var output = new StringBuilder()
                .AppendLine("############################################")
                .AppendLine("## TICKET TRANSCRIPT! PLEASE DO NOT EDIT! ##")
                .AppendLine("############################################")
                .AppendLine()
                .AppendLine("/// SUMMARY")
                .AppendLine()
                .AppendLine("- CREATED AT (UTC) : " + ticket.CreatedAt.UtcDateTime)
                .AppendLine("- CLOSED AT (UTC)  : " + DateTime.UtcNow)
                .AppendLine("- SUBJECT          : " + ticket.Subject)
                .AppendLine("- ACCUSED          : " + (reportedUser is null ? ticket.ReportedUserId : (reportedUser.GlobalName ?? reportedUser.Username)))
                .AppendLine("- RULES VIOLATED   : " + ticket.RulesViolated)
                .AppendLine("- SEVERITY         : " + ticket.Severity)
                .AppendLine("- HANDLER          : " + handler.Name)
                .AppendLine()
                .AppendLine("/// DESCRIPTION")
                .AppendLine()
                .AppendLine(ticket.Description ?? "None Provided!")
                .AppendLine();


            if (ticket.ReportedMessageId.HasValue)
            {
                output.AppendLine("/// REPORTED MESSAGE")
                    .AppendLine();

                var reportedMsg = await Context.Database.Messages.FirstOrDefaultAsync(x => x.Id == ticket.ReportedMessageId);
                output.AppendLine(UserMessageHistory.CreateMessageChain(reportedMsg, Context.Guild.GetUser(reportedMsg.UserId)));

                Context.Database.Messages.Remove(reportedMsg);

                output.AppendLine();
            }

            output.AppendLine("/// MESSAGES")
                .AppendLine();

            foreach (var msg in ticket.Messages)
                output.AppendLine(UserMessageHistory.CreateMessageChain(msg, Context.Guild.GetUser(msg.UserId)));

            return output.ToString();
        }
    }
}
