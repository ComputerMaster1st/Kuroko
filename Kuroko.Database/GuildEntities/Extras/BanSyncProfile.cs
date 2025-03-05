using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Shared;

namespace Kuroko.Database.GuildEntities.Extras;

public class BanSyncProfile(Guid hostServerId, Guid clientServerId, BanSyncMode mode)
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; private set; } = 0;
    
    public Guid HostServerId { get; private set; } = hostServerId;
    public Guid ClientServerId { get; private set; } = clientServerId;
    public BanSyncMode Mode { get; set; } = mode;
}