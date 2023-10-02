using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities
{
    public class UlongEntity : ITypeEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = 0;

        public ulong Value { get; private set; } = 0;

        public UlongEntity(ulong value)
            => Value = value;
    }
}
