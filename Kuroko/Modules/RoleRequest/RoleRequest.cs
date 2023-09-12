using Discord.Interactions;
using Kuroko.Core;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public class RoleRequest : KurokoModuleBase
    {
        [SlashCommand("roles", "Manage my roles!")]
        public async Task ExecuteAsync()
        {
            var roleRequestData = await Context.Database.GuildRoleRequests.FirstOrDefaultAsync(x => x.Guild.Id == Context.Guild.Id);
            var output = new StringBuilder()
                .AppendLine("# Role Request");

            if (roleRequestData is null || roleRequestData.RoleIds.Count < 1)
            {
                output.AppendLine("Role Request is currently not setup on this server.");

                await RespondAsync(output.ToString(), ephemeral: true);

                return;
            }

            output.AppendFormat("**{0}** roles available for you to choose!", roleRequestData.RoleIds.Count).AppendLine();

            await RespondAsync(output.ToString(), components: RRMenu.BuildUserMenu());
        }
    }
}
