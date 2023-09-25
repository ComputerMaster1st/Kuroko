using Discord;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public static class RRMenu
    {
        public static MessageComponent BuildUserMenu(bool hasRolesAvailable, bool hasManageRolePermission, StringBuilder output)
        {
            var builder = new ComponentBuilder();
            var manageRowId = 0;

            if (hasRolesAvailable)
            {
                manageRowId = 1;
                builder
                    .WithButton("Assign", CommandIdMap.RoleRequestAssign + ":0", ButtonStyle.Success, row: 0)
                    .WithButton("Remove", CommandIdMap.RoleRequestRemove + ":0", ButtonStyle.Danger, row: 0);
            }

            if (hasManageRolePermission)
            {
                output.AppendLine("## Role Management");

                builder
                    .WithButton("Add Roles", CommandIdMap.RoleRequestManageAdd + ":0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove Roles", CommandIdMap.RoleRequestManageRemove + ":0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove All", CommandIdMap.RoleRequestManageRemoveAll, ButtonStyle.Danger, row: manageRowId);
            }

            builder.WithButton("Exit", CommandIdMap.Exit, ButtonStyle.Secondary, row: manageRowId + 1);

            return builder.Build();
        }
    }
}
