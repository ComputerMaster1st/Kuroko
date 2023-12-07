using System.Text;
using Discord;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Services
{
    [PreInitialize]
    public class TicketService
    {
        private readonly IServiceProvider _services;

        public TicketService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task BuildAndSendTranscriptAsync(ReportsEntity properties, IGuild guild, ITextChannel reportChannel,
            ITextChannel transcriptChannel, int ticketId)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var root = await db.Guilds.FirstOrDefaultAsync(x => x.Id == guild.Id);
            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            var handler = properties.ReportHandlers.FirstOrDefault(x => x.Id == ticket.ReportHandlerId);
            var directory = Directory.CreateDirectory($"{DataDirectories.TEMPFILES}/ticket_{ticket.Id}");

            await File.WriteAllTextAsync(Path.Combine(directory.ToString(), "ticket.txt"),
                await GenerateTranscriptAsync(ticket, handler, guild));
            var (ZipDir, Segments, Message) = await Utilities.ZipAndUploadAsync(ticket, directory, transcriptChannel);

            directory.Delete(true);
            ZipDir.Delete(true);
            root.Tickets.Remove(ticket, db);

            await reportChannel.DeleteAsync();
            await Message.ModifyAsync(x =>
            {
                x.Content = $"Ticket Transcript: {ticket.Id}. Uploaded {Segments} zip file(s).";
            });
        }

        private async Task<string> GenerateTranscriptAsync(TicketEntity ticket, ReportHandler handler, IGuild guild)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var reportedUser = await guild.GetUserAsync(ticket.ReportedUserId);
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

                var reportedMsg = await db.Messages.FirstOrDefaultAsync(x => x.Id == ticket.ReportedMessageId);
                output.AppendLine(BlackboxService.CreateMessageChain(reportedMsg, await guild.GetUserAsync(reportedMsg.UserId)));

                db.Messages.Remove(reportedMsg);

                output.AppendLine();
            }

            output.AppendLine("/// MESSAGES")
                .AppendLine();

            foreach (var msg in ticket.Messages)
                output.AppendLine(BlackboxService.CreateMessageChain(msg, await guild.GetUserAsync(msg.UserId)));

            return output.ToString();
        }
    }
}