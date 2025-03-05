using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Database.GuildEntities.Extras;
using Kuroko.Shared;

namespace Kuroko.Database.GuildEntities;

public class BanSyncProperties : IPropertyEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    // Relational Link
    public ulong GuildId { get; private set; } = 0;
    public virtual GuildEntity Guild { get; private set; } = null;
    
    // Properties
    public Guid SyncId { get; private set; } = Guid.NewGuid();
    public bool IsEnabled { get; set; } = false;
    
    // Permissions
    public BanSyncMode Mode { get; set; } = BanSyncMode.Disabled;
    public virtual List<BanSyncProfile> Profiles { get; private set; } = [];
}