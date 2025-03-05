using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kuroko.Database.GuildEntities;

public class BanSyncProperties : IPropertyEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    public ulong GuildId { get; private set; } = 0;
    public virtual GuildEntity Guild { get; private set; } = null;
    
    public bool IsEnabled { get; set; } = false;
}