using Kuroko.Database.Entities.Guild;

namespace Kuroko.Database.Entities
{
    public interface IModuleEnabled
    {
        bool IsEnabled { get; }

        GuildEntity Guild { get; }
    }
}
