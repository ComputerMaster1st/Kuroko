using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public class RoleRequest : KurokoModuleBase
    {
        [SlashCommand("roles", "Public roles !")]
        public async Task ExecuteAsync()
        {
            var user = Context.User as IGuildUser;
            var roleRequestData = await Context.Database.GuildRoleRequests.FirstOrDefaultAsync(x => x.Guild.Id == Context.Guild.Id);
            var output = new StringBuilder()
                .AppendLine("# Role Request");
            var hasRoles = false;

            if (roleRequestData is null || roleRequestData.RoleIds.Count < 1)
                output.AppendLine("Role Request is currently not setup on this server.");
            else
            {
                hasRoles = true;
                output.AppendFormat("**{0}** roles available for you to choose!", roleRequestData.RoleIds.Count).AppendLine();
            }

            var msgComponents = RRMenu.BuildMainMenu(hasRoles, user.GuildPermissions.ManageRoles, user.Id, output);

            await RespondAsync(output.ToString(), components: msgComponents);
        }
    }
}
