using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.User
{
    public class AssignRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg => new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Assign Role");

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestAssign}:*,*")]
        public async Task EntryAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(index, OutputMsg);
        }

        [ComponentInteraction($"{RoleRequestCommandMap.RoleRequestSave}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();

            var selectedRoleIds = roleIds.Select(ulong.Parse);
            var output = OutputMsg.AppendLine("Assiged Roles:");
            var user = Context.User as IGuildUser;

            await user.AddRolesAsync(selectedRoleIds);

            foreach (var roleId in selectedRoleIds)
            {
                var role = user.Guild.GetRole(roleId);
                output.AppendLine("* " + role.Name);
            }

            await ExecuteAsync(index, output);
        }

        private async Task ExecuteAsync(int index, StringBuilder output)
        {
            var properties = await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id);
            var user = Context.User as IGuildUser;
            var count = 0;
            var roleIds = properties.RoleIds.Skip(index).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{RoleRequestCommandMap.RoleRequestSave}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to assign yourself");
            var guildRoles = new List<IRole>();

            foreach (var roleId in roleIds)
                if (!user.RoleIds.Any(x => x == roleId.Value))
                    guildRoles.Add(user.Guild.GetRole(roleId.Value));

            foreach (var role in guildRoles.OrderByDescending(x => x.Position))
            {
                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenu, index, user, RoleRequestCommandMap.RoleRequestAssign, RoleRequestCommandMap.RoleRequestMenu);

            if (!menu.HasOptions)
                output.AppendLine()
                    .AppendLine("Oops, looks like you reached the end of the list already!");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
