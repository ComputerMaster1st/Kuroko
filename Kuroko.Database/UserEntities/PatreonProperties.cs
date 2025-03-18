using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Database.UserEntities.Extras;

namespace Kuroko.Database.UserEntities;

public class PatreonProperties : IPropertyEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    public ulong RootId { get; private set; } = 0;
    public virtual UserEntity User { get; private set; } = null;
    
    public virtual List<PremiumKey> PremiumKeys { get; private set; } = [];
}