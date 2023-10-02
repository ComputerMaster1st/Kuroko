using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public class RoleRequest : KurokoModuleBase
    {
        [SlashCommand("roles", "Role assignment & configuration")]
        public Task InitialAsync()
            => ExecuteAsync();

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestMenu}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestManageReset}:*")]
        [RequireUserGuildPermission(GuildPermission.ManageRoles)]
        public async Task ResetAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();

            var properties = await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id);
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

            var builder = new ComponentBuilder();
            var manageRowId = 0;

            if (hasRoles)
            {
                manageRowId = 1;

                if (properties.RoleIds.Count > userRoleCount)
                    builder.WithButton("Assign", $"{RoleRequestCommandMap.RoleRequestAssign}:{user.Id},0", ButtonStyle.Success, row: 0);

                if (userRoleCount > 0)
                    builder.WithButton("Remove", $"{RoleRequestCommandMap.RoleRequestRemove}:{user.Id},0", ButtonStyle.Danger, row: 0);
            }

            if (user.GuildPermissions.ManageRoles)
            {
                output.AppendLine("## Manager Edition");

                builder
                    .WithButton("Add Roles", $"{RoleRequestCommandMap.RoleRequestManageAdd}:{user.Id},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove Roles", $"{RoleRequestCommandMap.RoleRequestManageRemove}:{user.Id},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove All", $"{RoleRequestCommandMap.RoleRequestManageReset}:{user.Id}", ButtonStyle.Danger, row: manageRowId);
            }

            builder.WithButton("Exit", $"{GlobalCommandMap.Exit}:{user.Id}", ButtonStyle.Secondary, row: manageRowId + 1);

            var msgComponents = builder.Build();

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
