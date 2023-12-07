using System.Text;
using Discord;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Message;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Services
{
    [PreInitialize]
    public class BlackboxService
    {
        private readonly IServiceProvider _services;
        private readonly HttpClient _http = new();

        public BlackboxService(IServiceProvider services)
            => _services = services;

        public async Task<(MessageEntity Message, IEnumerable<FileAttachment> Attachments)> CreateMessageEntityAsync(IMessage message,
            bool downloadAttachments, bool returnAttachments)
        {
            var attachments = new List<FileAttachment>();
            var entity = new MessageEntity(message.Id, message.Channel.Id,
                message.Author.Id, message.Content);

            if (downloadAttachments)
            {
                if (message.Attachments.Count > 0)
                {
                    foreach (var att in message.Attachments)
                    {
                        var bytes = await _http.GetByteArrayAsync(att.Url ?? att.ProxyUrl);

                        attachments.Add(new(new MemoryStream(bytes), att.Filename));
                        entity.Attachments.Add(new(att.Id, att.Filename, bytes));
                    }
                }
            }

            if (returnAttachments)
                return (entity, attachments);
            
            if (attachments.Any())
                attachments.ForEach(x => x.Dispose());
            
            return (entity, null);
        }

        public async Task<(MessageEntity Message, IEnumerable<FileAttachment> Attachments)> StoreMessageAsync(IMessage message,
            bool downloadAttachments, bool returnAttachments)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var (Message, Attachments) = await CreateMessageEntityAsync(message, downloadAttachments, returnAttachments);
            var channel = message.Channel as IGuildChannel;
            var guildRoot = await db.Guilds.GetOrCreateRootAsync(channel.GuildId);

            guildRoot.Messages.Add(Message);

            return (Message, Attachments);
        }

        public async Task<(MessageEntity Message, IEnumerable<FileAttachment> Attachments)> GetMessageAsync(ulong messageId)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var attachments = new List<FileAttachment>();
            var entity = await db.Messages.FirstOrDefaultAsync(x => x.Id == messageId);

            if (entity is null)
                return (null, null);

            if (entity.Attachments.Count > 0)
                foreach (var attachment in entity.Attachments)
                    attachments.Add(new(attachment.GetStream(), attachment.FileName));
            
            return (entity, attachments);
        }

        public async Task EditMessageAsync(ulong editedMessageId, string newContent)
        {
            var (Message, _) = await GetMessageAsync(editedMessageId);

            if (Message is null)
                return;

            Message.EditedMessages.Add(new(newContent));
        }

        public async Task DeleteMessageAsync(ulong deletedMessageId)
        {
            var (Message, _) = await GetMessageAsync(deletedMessageId);

            if (Message is null)
                return;

            Message.DeletedAt = DateTime.UtcNow;
        }

        public async Task GenerateUserMessageHistoryAsync(int ticketId, IDiscordClient client)
        {
            var db = _services.GetRequiredService<DatabaseContext>();
            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.Id == ticketId);
            var root = await db.Guilds.FirstOrDefaultAsync(x => x.Id == ticket.GuildId);
            var messages = root.Messages.Where(x => x.UserId == ticket.ReportedUserId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (!messages.Any())
                return;

            var guild = await client.GetGuildAsync(ticket.GuildId);
            var user = await guild.GetUserAsync(ticket.ReportedUserId);
            var ticketDir = Directory.CreateDirectory($"{DataDirectories.TEMPFILES}/ticket_{ticket.Id}");

            await CreateHistoryLogAsync(messages, user, ticketDir);

            var (ZipDir, Segments, _) = await Utilities.ZipAndUploadAsync(ticket, ticketDir, await guild.GetTextChannelAsync(ticket.ChannelId));

            ticketDir.Delete(true);
            ZipDir.Delete(true);
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

        private static async Task<string> CreateAttachmentChainAsync(IEnumerable<AttachmentEntity> attachments, DirectoryInfo attachmentDir)
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
    }
}