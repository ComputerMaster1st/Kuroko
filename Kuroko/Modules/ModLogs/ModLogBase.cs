using Kuroko.Core;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;

namespace Kuroko.Modules.ModLogs
{
    public abstract class ModLogBase : KurokoModuleBase
    {
        protected Task<ModLogEntity> GetPropertiesAsync()
            => Context.Database.GuildModLogs.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.ModLog ??= y;
                });
    }
}
