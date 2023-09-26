using Kuroko.Core;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;

namespace Kuroko.Modules.RoleRequest
{
    public abstract class RoleRequestBase : KurokoModuleBase
    {
        protected Task<RoleRequestEntity> GetProperties()
            => Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });
    }
}
