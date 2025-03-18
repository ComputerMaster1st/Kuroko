using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Shared;

namespace Kuroko.Database.GuildEntities.Extras;

public class BanSyncProfile(Guid hostSyncId, Guid clientSyncId, BanSyncMode mode)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    // Host Guild
    public Guid HostSyncId { get; private set; } = hostSyncId;
    public virtual BanSyncGuildProperties HostGuildProperties { get; private set; } = null;
    
    // Client Guild
    public Guid ClientSyncId { get; private set; } = clientSyncId;
    public virtual BanSyncGuildProperties ClientGuildProperties { get; private set; } = null;
    
    // Permission Level
    public BanSyncMode Mode { get; set; } = mode;
}