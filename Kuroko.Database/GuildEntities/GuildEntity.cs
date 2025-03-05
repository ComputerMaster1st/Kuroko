namespace Kuroko.Database.GuildEntities;

public class GuildEntity(ulong id) : DiscordEntity(id)
{
    public virtual BanSyncProperties BanSyncProperties { get; set; } = null;
}