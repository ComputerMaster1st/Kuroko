using Kuroko.Database.GuildEntities;

namespace Kuroko.Database;

public interface IPropertyEntity
{
    ulong GuildId { get; }
    GuildEntity Guild { get; }
}