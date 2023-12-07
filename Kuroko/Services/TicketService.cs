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
            => _services = services;

        public async Task BuildAndSendTranscriptAsync(ReportsEntity properties, IGuild guild, ITextChannel reportChannel,
            ITextChannel transcriptChannel, int ticketId)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var root = await db.Guilds.FirstOrDefaultAsync(x => x.Id == guild.Id);
            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            var handler = properties.ReportHandlers.FirstOrDefault(x => x.Id == ticket.ReportHandlerId);
            var directory = Directory.CreateDirectory($"{DataDirectories.TEMPFILES}/ticket_{ticket.Id}");
            var reportedUser = await guild.GetUserAsync(ticket.ReportedUserId);

            await File.WriteAllTextAsync(Path.Combine(directory.ToString(), "ticket.txt"),
                await GenerateReportTranscriptAsync(ticket, handler, guild));
            var (ZipDir, Segments, Message) = await Utilities.ZipAndUploadAsync(ticket, directory, transcriptChannel);

            directory.Delete(true);
            ZipDir.Delete(true);
            root.Tickets.Remove(ticket, db);

            var embedFieldList = new List<EmbedFieldBuilder>()
            {
                new()
                {
                    IsInline = true,
                    Name = "Accused",
                    Value = reportedUser is null ? ticket.ReportedUserId : reportedUser.Mention
                },
                new()
                {
                    IsInline = true,
                    Name = "Severity",
                    Value = ticket.Severity
                },
                new()
                {
                    IsInline = false,
                    Name = "Rules Violated",
                    Value = ticket.RulesViolated
                },
                new()
                {
                    IsInline = false,
                    Name = "Subject",
                    Value = ticket.Subject
                }
            };

            await reportChannel.DeleteAsync();
            await Message.ModifyAsync(async x =>
            {
                x.Embed = await GenerateEmbedAsync(ticket, guild, handler);
            });
        }

        private static async Task<Embed> GenerateEmbedAsync(TicketEntity ticket, IGuild guild, ReportHandler handler,
            List<EmbedFieldBuilder> additionalFields = null)
        {
            var submitter = await guild.GetUserAsync(ticket.SubmitterId);
            var usersInvolved = new Dictionary<IGuildUser, int>();

            foreach (var msg in ticket.Messages)
            {
                var user = await guild.GetUserAsync(msg.UserId);

                if (!usersInvolved.TryAdd(user, 1))
                    usersInvolved[user]++;
            }

            var outputUsersInvolved = new StringBuilder();
            foreach (var user in usersInvolved)
                outputUsersInvolved.AppendLine($"{user.Value} - {user.Key.Mention}");

            var fieldBuilders = new List<EmbedFieldBuilder>()
            {
                new()
                {
                    IsInline = true,
                    Name = "Submitter",
                    Value = submitter.Mention
                },
                new()
                {
                    IsInline = true,
                    Name = "Created",
                    Value = ticket.CreatedAt
                },
                new()
                {
                    IsInline = true,
                    Name = "Resolved",
                    Value = DateTimeOffset.UtcNow
                },
                new()
                {
                    IsInline = true,
                    Name = "Users In Ticket",
                    Value = outputUsersInvolved.ToString()
                },
                new()
                {
                    IsInline = true,
                    Name = "Handler",
                    Value = handler.Name
                }
            };

            if (additionalFields != null)
                fieldBuilders.AddRange(additionalFields);

            var embedBuilder = new EmbedBuilder()
            {
                Title = $"Ticket ID: {ticket.Id}",
                ThumbnailUrl = submitter.GetDisplayAvatarUrl(),
                Timestamp = DateTimeOffset.UtcNow,
                Description = ticket.Description ?? "_None Provided_",
                Fields = fieldBuilders
            };

            return embedBuilder.Build();
        }

        private async Task<string> GenerateReportTranscriptAsync(TicketEntity ticket, ReportHandler handler, IGuild guild)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var reportedUser = await guild.GetUserAsync(ticket.ReportedUserId);
            var output = new StringBuilder()
                .AppendLine("############################################")
                .AppendLine("## REPORT TRANSCRIPT! PLEASE DO NOT EDIT! ##")
                .AppendLine("############################################")
                .AppendLine()
                .AppendLine("/// SUMMARY")
                .AppendLine()
                .AppendLine("- CREATED AT (UTC) : " + ticket.CreatedAt.UtcDateTime)
                .AppendLine("- CLOSED AT (UTC)  : " + DateTimeOffset.UtcNow)
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