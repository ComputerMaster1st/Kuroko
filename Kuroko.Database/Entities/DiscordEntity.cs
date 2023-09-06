using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.Entities
{
    public abstract class DiscordEntity : IDiscordEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; private set; }

        internal DiscordEntity(ulong id)
         => Id = id;
    }
}
