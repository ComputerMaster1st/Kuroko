using Discord;
using Kuroko.Database.Entities.Guild;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public static class RRMenu
    {
        public static MessageComponent BuildMainMenu(bool hasRolesAvailable, bool hasManageRolePermission, ulong contextUserId, StringBuilder output)
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
                output.AppendLine("## Management");

                builder
                    .WithButton("Add Roles", $"{CommandIdMap.RoleRequestManageAdd}:{contextUserId},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove Roles", $"{CommandIdMap.RoleRequestManageRemove}:{contextUserId},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove All", CommandIdMap.RoleRequestManageRemoveAll, ButtonStyle.Danger, row: manageRowId);
            }

            builder.WithButton("Exit", CommandIdMap.Exit, ButtonStyle.Secondary, row: manageRowId + 1);

            return builder.Build();
        }

        public static (bool HasOptions, MessageComponent Components) BuildAddMenu(IGuild guild, RoleRequestEntity properties, IGuildUser user, int indexStart)
        {
            var count = 0;
            var roles = guild.Roles.OrderByDescending(x => x.Position)
                .Skip(indexStart)
                .ToList();

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.RoleRequestManageSave}:{user.Id},{indexStart}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to make public");

            foreach (var role in roles)
            {
                if (properties.RoleIds.Any(x => x.Value == role.Id) || role.Name == "@everyone")
                    continue;

                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            return PagedSelectMenu(selectMenu, indexStart, user, CommandIdMap.RoleRequestManageAdd);
        }

        private static (bool HasOptions, MessageComponent Components) PagedSelectMenu(
            SelectMenuBuilder builder,
            int startIndex,
            IGuildUser user,
            string commandId)
        {
            var componentBuilder = new ComponentBuilder();
            (bool HasOptions, MessageComponent Components) output = new();

            if (builder.Options.Count > 0)
            {
                builder.WithMaxValues(builder.Options.Count);
                componentBuilder.WithSelectMenu(builder);
                output.HasOptions = true;
            }
            else
                output.HasOptions = false;

            if (startIndex > 0)
                componentBuilder.WithButton("<<--", $"{commandId}:{user.Id},{startIndex - 25}", ButtonStyle.Primary);

            if (builder.Options.Count >= 25)
                componentBuilder.WithButton("-->>", $"{commandId}:{user.Id},{startIndex + 25}", ButtonStyle.Primary);

            componentBuilder.WithButton("Back To Menu", $"{CommandIdMap.RoleRequestMenu}:{user.Id}", ButtonStyle.Secondary);

            output.Components = componentBuilder.Build();

            return output;
        }
    }
}