using Kuroko.Database.UserEntities.Extras;

namespace Kuroko.Database.GuildEntities;

public class GuildEntity(ulong id) : DiscordEntity(id)
{
    public virtual BanSyncGuildProperties BanSyncGuildProperties { get; set; } = null;
    
    // Premium Key
    public virtual PremiumKey PremiumKey { get; set; } = null;
}