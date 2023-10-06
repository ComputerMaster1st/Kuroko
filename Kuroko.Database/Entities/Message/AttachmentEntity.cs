using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Message
{
    public class AttachmentEntity : IDiscordEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; private set; } = 0;

        public string FileName { get; private set; } = string.Empty;
        public long FileSize { get; private set; } = 0;

        public string Base64Bytes { get; private set; } = string.Empty;

        public AttachmentEntity(ulong id, string fileName, string base64Bytes)
        {
            Id = id;
            FileName = fileName;
            Base64Bytes = base64Bytes;
            FileSize = GetBytes().LongLength;
        }

        public AttachmentEntity(ulong id, string fileName, byte[] bytes)
        {
            Id = id;
            FileName = fileName;
            Base64Bytes = Convert.ToBase64String(bytes);
            FileSize = bytes.LongLength;
        }

        public byte[] GetBytes()
            => Convert.FromBase64String(Base64Bytes);

        public Stream GetStream()
            => new MemoryStream(GetBytes());
    }
}
