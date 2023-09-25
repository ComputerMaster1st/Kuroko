using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public class RoleRequest : KurokoModuleBase
    {
        [SlashCommand("roles", "Role assignment & configuration")]
        public Task InitialAsync()
            => ExecuteAsync();

        [ComponentInteraction($"{CommandIdMap.RoleRequestMenu}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            await DeferAsync();

            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await ExecuteAsync(true);
        }

        private async Task ExecuteAsync(bool isReturning = false)
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

            var msgComponents = RRMenu.BuildMainMenu(hasRoles, user, output);

            if (!isReturning)
                await RespondAsync(output.ToString(), components: msgComponents);
            else
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = msgComponents;
                });
        }
    }
}
