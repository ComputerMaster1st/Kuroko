namespace Kuroko.Database.Entities.Guild
{
    public class GuildEntity : DiscordEntity
    {
        public virtual RoleRequestEntity RoleRequest { get; set; } = null;
        public virtual ModLogEntity ModLog { get; set; } = null;

        public GuildEntity(ulong id) : base(id) { }
    }
}
