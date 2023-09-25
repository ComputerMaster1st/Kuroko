﻿using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class AddRoleComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{CommandIdMap.RoleRequestManageAdd}:*,*")]
        public async Task ExecuteAsync(ulong interactedUserId, int index)
        {
            await DeferAsync();

            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var roleRequest = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });
            var output = new StringBuilder()
                .AppendLine("# Role Request")
                .AppendLine("## Management")
                .AppendLine("### Add Roles");

            await EchoExecuteAsync(roleRequest, index, output);
        }

        [ComponentInteraction($"{CommandIdMap.RoleRequestManageSave}:*,*")]
        public async Task ExecuteAsync(ulong interactedUserId, int index, string[] roleIds)
        {
            await DeferAsync();

            if (interactedUserId != Context.User.Id)
            {
                await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
                return;
            }

            var properties = await Context.Database.GuildRoleRequests.CreateOrGetDataAsync(
                Context.Database.Guilds, Context.Guild.Id, (x, y) =>
                {
                    x.RoleRequest ??= y;
                });
            var selectedRoleIds = roleIds.Select(x => ulong.Parse(x));
            var output = new StringBuilder()
                .AppendLine("# Role Request")
                .AppendLine("## Management")
                .AppendLine("### Add Roles")
                .AppendLine("Selected roles for public use:");

            foreach (var roleId in selectedRoleIds)
            {
                var role = Context.Guild.GetRole(roleId);

                properties.RoleIds.Add(new(role.Id));
                output.AppendLine("* " + role.Name);
            }

            await Context.Database.SaveChangesAsync();
            await EchoExecuteAsync(properties, index, output);
        }

        private async Task EchoExecuteAsync(RoleRequestEntity roleRequest, int index, StringBuilder output)
        {
            var menu = RRMenu.BuildAddMenu(Context.User as IGuildUser, roleRequest, index);

            if (!menu.HasOptions)
                output.AppendLine("All roles already available! Nothing to list.");

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            });
        }
    }
}
