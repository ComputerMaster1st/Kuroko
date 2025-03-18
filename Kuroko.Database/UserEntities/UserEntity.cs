namespace Kuroko.Database.UserEntities;

public class UserEntity(ulong id) : DiscordEntity(id)
{
    public virtual PatreonProperties Patreon { get; set; } = null;
}