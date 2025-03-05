namespace Kuroko.Database.GuildEntities;

public sealed class GuildEntity(ulong id) : DiscordEntity(id)
{
    public BanSyncProperties BanSyncProperties { get; init; }
}