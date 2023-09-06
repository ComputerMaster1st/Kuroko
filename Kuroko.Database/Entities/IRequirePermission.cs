using Kuroko.Database.Entities.Guild;

namespace Kuroko.Database.Entities
{
    public interface IRequirePermission
    {
        bool IsPermissionRequired { get; }

        GuildEntity Guild { get; }
    }
}
