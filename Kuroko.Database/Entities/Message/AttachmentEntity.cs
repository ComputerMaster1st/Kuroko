using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Message
{
    public class AttachmentEntity : IDiscordEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; private set; } = 0;

        public string FileName { get; private set; } = string.Empty;
        public string Base64Bytes { get; private set; } = string.Empty;

        [NotMapped]
        public long FileSize
        {
            get
            {
                return GetBytes().LongLength;
            }
        }

        public AttachmentEntity(ulong attachmentId, string fileName, string base64Bytes)
        {
            Id = attachmentId;
            FileName = fileName;
            Base64Bytes = base64Bytes;
        }

        public byte[] GetBytes()
            => Convert.FromBase64String(Base64Bytes);

        public Stream GetStream()
            => new MemoryStream(GetBytes());
    }
}
