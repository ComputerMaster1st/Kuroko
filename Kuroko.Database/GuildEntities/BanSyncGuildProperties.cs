using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Database.GuildEntities.Extras;
using Kuroko.Shared;

namespace Kuroko.Database.GuildEntities;

public class BanSyncGuildProperties : IPropertyEntity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    // Relational Link
    public ulong RootId { get; private set; } = 0;
    public virtual GuildEntity Guild { get; private set; } = null;
    
    // Properties
    public Guid SyncId { get; private set; } = Guid.NewGuid();
    public bool IsEnabled { get; set; } = false;
    public bool AllowRequests { get; set; } = false;
    public ulong BanSyncChannelId { get; set; } = 0;

    // Permission Level
    public BanSyncMode HostMode { get; set; } = BanSyncMode.Disabled;
    public BanSyncMode ClientMode { get; set; } = BanSyncMode.Disabled;

    // Sync Profiles
    public virtual List<BanSyncProfile> HostForProfiles { get; private set; } = [];
    public virtual List<BanSyncProfile> ClientOfProfiles { get; private set; } = [];
}