using Discord;
using Discord.Interactions;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public class RoleRequest : RoleRequestBase
    {
        [SlashCommand("roles", "Role assignment & configuration")]
        public Task InitialAsync()
            => ExecuteAsync();

        [ComponentInteraction($"{CommandIdMap.RoleRequestMenu}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageReset}:*")]
        [RequireUserGuildPermission(GuildPermission.ManageRoles)]
        public async Task ResetAsync(ulong interactedUserId)
        {
            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            await DeferAsync();

            var properties = await GetPropertiesAsync();
            properties.RoleIds.Clear(Context.Database);

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(true);
        }

        private async Task ExecuteAsync(bool isReturning = false)
        {
            var user = Context.User as IGuildUser;
            var properties = await Context.Database.GuildRoleRequests.FirstOrDefaultAsync(x => x.Guild.Id == Context.Guild.Id);
            var output = new StringBuilder()
                .AppendLine("# Role Request");
            var hasRoles = false;
            var userRoleCount = 0;

            if (properties is null || properties.RoleIds.Count < 1)
                output.AppendLine("Role Request is currently not setup on this server.");
            else
            {
                foreach (var roleId in properties.RoleIds)
                    if (user.RoleIds.Any(x => x == roleId.Value))
                        userRoleCount++;

                hasRoles = true;
                output.AppendFormat("**{0}** roles available for public use!", properties.RoleIds.Count).AppendLine();
                output.AppendFormat("**{0}** roles available for you to choose!", properties.RoleIds.Count - userRoleCount).AppendLine();
            }

            var msgComponents = RRMenu.BuildMainMenu(user, output, hasRoles, properties.RoleIds.Count > userRoleCount, userRoleCount > 0);

            if (!isReturning)
            {
                await RespondAsync(output.ToString(), components: msgComponents);
                (await Context.Interaction.GetOriginalResponseAsync()).SetTimeout(1);
            }
            else
                (await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = msgComponents;
                })).ResetTimeout();
        }
    }
}
