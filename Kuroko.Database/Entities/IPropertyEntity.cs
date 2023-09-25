using Kuroko.Database.Entities.Guild;

namespace Kuroko.Database.Entities
{
    public interface IPropertyEntity
    {
        ulong GuildId { get; }
        GuildEntity Guild { get; }
    }
}
