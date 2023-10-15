using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
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

            var zipLocation = Kuroko.Utilities.CreateZip($"ticket_{ticket.Id}", directory, out int segments);
            var discordAttachments = new List<FileAttachment>();

            void clearAttachments()
            {
                foreach (var file in discordAttachments)
                    file.Dispose();
            }

            if (segments < 10)
            {
                foreach (var file in zipLocation.GetFiles())
                    discordAttachments.Add(new FileAttachment(file.FullName));

                await chn.SendFilesAsync(discordAttachments);
                clearAttachments();
            }
            else
            {
                foreach (var file in zipLocation.GetFiles())
                {
                    discordAttachments.Add(new FileAttachment(file.FullName));

                    if (!(discordAttachments.Count < 10))
                    {
                        await chn.SendFilesAsync(discordAttachments);

                        clearAttachments();
                        discordAttachments.Clear();
                    }
                }

                await chn.SendFilesAsync(discordAttachments);
            }

            directory.Delete(true);
            zipLocation.Delete(true);
            root.Tickets.Remove(ticket, Context.Database);

            await (Context.Channel as ITextChannel).DeleteAsync();
            await msg.ModifyAsync(x =>
            {
                x.Content = $"Ticket Transcript: {ticket.Id}. Uploaded {segments} zip file(s).";
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
                .AppendLine()
                .AppendLine("/// MESSAGES")
                .AppendLine();

            foreach (var msg in ticket.Messages)
            {
                var user = Context.Guild.GetUser(msg.UserId);
                var userName = user is null ? msg.UserId.ToString() : user.GlobalName ?? user.Username;

                output
                    .AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                    .AppendLine($"{userName} : {msg.CreatedAt.UtcDateTime}")
                    .AppendLine()
                    .AppendLine(msg.Content);

                if (msg.Attachments.Count > 0)
                {
                    output.AppendLine("─────────────────── [ Attachment Chain ] ───────────────────")
                        .AppendLine();
                    var attachmentDir = directory.CreateSubdirectory("attachments");

                    foreach (var attachment in msg.Attachments)
                    {
                        var filePath = Path.Combine(attachmentDir.ToString(), $"{attachment.Id}_{attachment.FileName}");
                        var bytes = attachment.GetBytes();

                        using (FileStream file = File.OpenWrite(filePath))
                        {
                            await file.WriteAsync(bytes);
                        }

                        var mimes = FileMimeType.GetFromBytes(bytes);
                        var mime = mimes.OrderByDescending(x => x.Points).FirstOrDefault();

                        output
                            .AppendLine("Attachment ID : " + attachment.Id)
                            .AppendLine("Name          : " + attachment.FileName)
                            .AppendLine("Size (Bytes)  : " + attachment.FileSize)
                            .AppendLine("MIME Type     : " + mime is null ? "No Mime Type Found" : mime.MimeType)
                            .AppendLine();
                    }

                    output.AppendLine("────────────────────────────────────────────────────────────");
                }

                if (msg.EditedMessages.Count > 0)
                {
                    output.AppendLine("───────────────── [ Edited Message Chain ] ─────────────────");

                    foreach (var edited in msg.EditedMessages)
                    {
                        output
                            .AppendLine("────────────────────────────────────────────────────────────")
                            .AppendLine(edited.EditedAt.UtcDateTime.ToString())
                            .AppendLine()
                            .AppendLine(edited.Content)
                            .AppendLine("────────────────────────────────────────────────────────────");
                    }

                    output.AppendLine("────────────────────────────────────────────────────────────");
                }

                if (msg.DeletedAt.HasValue)
                {
                    output.AppendLine()
                        .AppendLine($">>> [ Message Deleted At {msg.DeletedAt.Value.UtcDateTime} ] <<<");
                }

                output.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }

            return output.ToString();
        }
    }
}
