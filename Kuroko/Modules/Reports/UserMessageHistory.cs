using Discord;
using Discord.WebSocket;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Database.Entities.Message;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Kuroko.Modules.Reports
{
    public static class UserMessageHistory
    {
        public static async Task GenerateUserMessageHistoryAsync(TicketEntity ticket, IServiceProvider services)
        {
            using var db = services.GetRequiredService<DatabaseContext>();
            var messages = db.Messages.Where(x => x.UserId == ticket.ReportedUserId && x.GuildId == ticket.GuildId).OrderByDescending(x => x.CreatedAt);

            if (!await messages.AnyAsync())
                return;

            var client = services.GetRequiredService<DiscordShardedClient>();
            var guild = client.GetGuild(ticket.GuildId);
            var user = guild.GetUser(ticket.ReportedUserId);
            var ticketDir = Directory.CreateDirectory($"{DataDirectories.TEMPFILES}/ticket_{ticket.Id}");

            await CreateHistoryLogAsync(messages, user, ticketDir);
            await ZipAndUploadAsync(ticket, ticketDir, guild.GetTextChannel(ticket.ChannelId));
        }

        private static async Task CreateHistoryLogAsync(IEnumerable<MessageEntity> messageHistory, IUser user, DirectoryInfo ticketDir)
        {
            var output = new StringBuilder()
                .AppendLine("###############################################")
                .AppendLine("## USER MESSAGE HISTORY! PLEASE DO NOT EDIT! ##")
                .AppendLine("###############################################");

            foreach (var message in messageHistory)
            {
                output.AppendLine(CreateMessageChain(message, user));

                if (message.Attachments.Any())
                {
                    var attachmentDir = ticketDir.GetDirectories()
                        .FirstOrDefault(x => x.Name == "attachments") ?? ticketDir.CreateSubdirectory("attachments");
                    output.AppendLine(await CreateAttachmentChainAsync(message.Attachments, attachmentDir));
                }
            }

            await File.WriteAllTextAsync(Path.Combine(ticketDir.FullName, $"{user.GlobalName ?? user.Username}_history.log"), output.ToString(), Encoding.UTF8);
        }

        public static string CreateMessageChain(MessageEntity message, IUser user)
        {
            var userName = user is null ? message.UserId.ToString() : user.GlobalName ?? user.Username;

            var output = new StringBuilder()
                .AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")
                .AppendLine($"{userName} : {message.CreatedAt.UtcDateTime}")
                .AppendLine()
                .AppendLine(message.Content);

            if (message.EditedMessages.Count > 0)
                output.AppendLine(CreateEditedMessageChain(message.EditedMessages));

            if (message.DeletedAt.HasValue)
            {
                output.AppendLine()
                    .AppendLine($">>> [ Message Deleted At {message.DeletedAt.Value.UtcDateTime} ] <<<");
            }

            output.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            return output.ToString();
        }

        private static string CreateEditedMessageChain(IEnumerable<EditedMessageEntity> editedMessages)
        {
            var output = new StringBuilder()
                .AppendLine("───────────────── [ Edited Message Chain ] ─────────────────");

            foreach (var edited in editedMessages)
            {
                output
                    .AppendLine("────────────────────────────────────────────────────────────")
                    .AppendLine(edited.EditedAt.UtcDateTime.ToString())
                    .AppendLine()
                    .AppendLine(edited.Content)
                    .AppendLine("────────────────────────────────────────────────────────────");
            }

            output.AppendLine("────────────────────────────────────────────────────────────"); ;

            return output.ToString();
        }

        public static async Task<string> CreateAttachmentChainAsync(IEnumerable<AttachmentEntity> attachments, DirectoryInfo attachmentDir)
        {
            var output = new StringBuilder()
                .AppendLine("─────────────────── [ Attachment Chain ] ───────────────────")
                .AppendLine();

            foreach (var attachment in attachments)
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

            return output.ToString();
        }

        public static async Task ZipAndUploadAsync(TicketEntity ticket, DirectoryInfo ticketDir, ITextChannel ticketChannel)
        {
            var zipLocation = Kuroko.Utilities.CreateZip($"ticket_{ticket.Id}_history", ticketDir, out int segments);
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

                await ticketChannel.SendFilesAsync(discordAttachments);
                clearAttachments();
            }
            else
            {
                foreach (var file in zipLocation.GetFiles())
                {
                    discordAttachments.Add(new FileAttachment(file.FullName));

                    if (!(discordAttachments.Count < 10))
                    {
                        await ticketChannel.SendFilesAsync(discordAttachments);

                        clearAttachments();
                        discordAttachments.Clear();
                    }
                }

                await ticketChannel.SendFilesAsync(discordAttachments);
            }
        }
    }
}
