using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities.Audio
{
    [Microsoft.EntityFrameworkCore.Index(nameof(IndexedHash))]
    public class HashEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; private set; } = 0;

        /// <summary>
        /// Gets the IndexedHash
        /// The lower 32bits stores a hash
        /// The upper 32bits contain the index (or table) of the hash in Sub Fingerprint
        /// Packed into a single field to allow the use of ANY to batch queries
        /// </summary>
        public long IndexedHash { get; set; } = 0;

        public long SubFingerprintId { get; private set; } = 0;
        public virtual SubFingerprintEntity SubFingerprint { get; private set; } = null;

        public HashEntity(long indexedHash)
        {
            IndexedHash = indexedHash;
        }
    }
}
