using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class AddRoleComponent : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Role Request")
                    .AppendLine("## Management")
                    .AppendLine("### Add Roles");
            }
        }

        [ComponentInteraction($"{RoleRequestCommandMap.MANAGE_ADD}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id), index, OutputMsg);
        }

        [ComponentInteraction($"{RoleRequestCommandMap.MANAGE_SAVE}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();

            var selectedRoleIds = roleIds.Select(ulong.Parse);
            var output = OutputMsg.AppendLine("Selected roles for public use:");
            var properties = await GetPropertiesAsync<RoleRequestEntity, GuildEntity>(Context.Guild.Id);

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);

                if (properties.RoleIds.Any(x => x.Value == role.Id))
                    output.AppendLine("* **Already Available** - " + role.Name);
                else
                {
                    properties.RoleIds.Add(new(role.Id));
                    output.AppendLine("* " + role.Name);
                }
            }

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(properties, index, output);
        }

        private async Task ExecuteAsync(RoleRequestEntity properties, int index, StringBuilder output)
        {
            var self = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            var user = Context.User as IGuildUser;
            var count = 0;
            var roles = user.Guild.Roles.OrderByDescending(x => x.Position)
                .Skip(index)
                .ToList();
            IRole selfHighestRole = null;

            foreach (var roleId in self.RoleIds)
            {
                var role = self.Guild.GetRole(roleId);

                if (selfHighestRole is null || role.Position > selfHighestRole.Position)
                    selfHighestRole = role;
            }

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{RoleRequestCommandMap.MANAGE_SAVE}:{user.Id},{index}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to make public");

            foreach (var role in roles)
            {
                if (role.Position >= selfHighestRole.Position || properties.RoleIds.Any(x => x.Value == role.Id) || role.Name == "@everyone")
                    continue;

                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            var menu = Pagination.SelectMenu(selectMenu, index, user, RoleRequestCommandMap.MANAGE_ADD, RoleRequestCommandMap.RoleRequestMenu);

            if (!menu.HasOptions)
                output.AppendLine("All roles already available! Nothing to list.");

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
