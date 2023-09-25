using Discord;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public static class RRMenu
    {
        public static MessageComponent BuildUserMenu(bool hasRolesAvailable, bool hasManageRolePermission, ulong contextUserId, StringBuilder output)
        {
            var builder = new ComponentBuilder();
            var manageRowId = 0;

            if (hasRolesAvailable)
            {
                manageRowId = 1;
                builder
                    .WithButton("Assign", $"{CommandIdMap.RoleRequestAssign}:{contextUserId},0", ButtonStyle.Success, row: 0)
                    .WithButton("Remove", $"{CommandIdMap.RoleRequestRemove}:{contextUserId},0", ButtonStyle.Danger, row: 0);
            }

            if (hasManageRolePermission)
            {
                output.AppendLine("## Role Management");

                builder
                    .WithButton("Add Roles", $"{CommandIdMap.RoleRequestManageAdd}:{contextUserId},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove Roles", $"{CommandIdMap.RoleRequestManageRemove}:{contextUserId},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove All", CommandIdMap.RoleRequestManageRemoveAll, ButtonStyle.Danger, row: manageRowId);
            }

            builder.WithButton("Exit", CommandIdMap.Exit, ButtonStyle.Secondary, row: manageRowId + 1);

            return builder.Build();
        }
    }
}
