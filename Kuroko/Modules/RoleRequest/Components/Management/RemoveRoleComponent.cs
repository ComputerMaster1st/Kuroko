using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class RemoveRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Management")
                    .AppendLine("### Remove Roles");
            }
        }

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestManageRemove}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id), index, OutputMsg);
        }

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestManageDelete}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();

            var properties = await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id);
            var selectedRoleIds = roleIds.Select(ulong.Parse);

            OutputMsg.AppendLine("Selected roles removed from public use:");

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);
                var temp = properties.RoleIds.FirstOrDefault(x => x.Value == roleId);
                properties.RoleIds.Remove(temp, Context.Database);

                OutputMsg.AppendLine("* " + role.Name);
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(properties, index, OutputMsg);
        }

        private async Task ExecuteAsync(RoleRequestEntity properties, int index, StringBuilder output)
        {
            var count = 0;
            var user = Context.User as IGuildUser;
            var roleIds = properties.RoleIds.Skip(index).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{RoleRequestCommandMap.RoleRequestManageDelete}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to remove from public");
            var guildRoles = new List<IRole>();

            foreach (var roleId in roleIds)
            {
                var role = user.Guild.GetRole(roleId.Value);
                guildRoles.Add(role);
            }

            foreach (var role in guildRoles.OrderByDescending(x => x.Position))
            {
                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenu, index, user, RoleRequestCommandMap.RoleRequestManageRemove, RoleRequestCommandMap.RoleRequestMenu);

            if (!menu.HasOptions)
                output.AppendLine("Nothing to list.");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
