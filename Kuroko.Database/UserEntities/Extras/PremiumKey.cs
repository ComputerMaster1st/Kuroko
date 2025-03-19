using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kuroko.Database.GuildEntities;

namespace Kuroko.Database.UserEntities.Extras;

public class PremiumKey
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private set; } = 0;
    
    // Patreon Properties
    public int PatreonPropertiesId { get; private set; } = 0;
    public virtual PatreonProperties PatreonProperties { get; private set; } = null;
    
    // Activated Server
    public ulong GuildId { get; set; } = 0;
    public virtual GuildEntity Guild { get; private set; } = null;


    [MaxLength(50)]
    public string Key { get; private set; } = Convert.ToBase64String(Guid.NewGuid()
        .ToByteArray()).TrimEnd('=');
    
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
}