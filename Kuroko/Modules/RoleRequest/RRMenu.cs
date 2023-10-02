using Discord;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using System.Text;

namespace Kuroko.Modules.RoleRequest
{
    public static class RRMenu
    {
        public static MessageComponent BuildMainMenu(IGuildUser user, StringBuilder output, bool hasRolesAvailable, bool enableAssign, bool enableRevoke)
        {
            var builder = new ComponentBuilder();
            var manageRowId = 0;

            if (hasRolesAvailable)
            {
                manageRowId = 1;

                if (enableAssign)
                    builder.WithButton("Assign", $"{CommandIdMap.RoleRequestAssign}:{user.Id},0", ButtonStyle.Success, row: 0);

                if (enableRevoke)
                    builder.WithButton("Remove", $"{CommandIdMap.RoleRequestRemove}:{user.Id},0", ButtonStyle.Danger, row: 0);
            }

            if (user.GuildPermissions.ManageRoles)
            {
                output.AppendLine("## Manager Edition");

                builder
                    .WithButton("Add Roles", $"{CommandIdMap.RoleRequestManageAdd}:{user.Id},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove Roles", $"{CommandIdMap.RoleRequestManageRemove}:{user.Id},0", ButtonStyle.Primary, row: manageRowId)
                    .WithButton("Remove All", $"{CommandIdMap.RoleRequestManageReset}:{user.Id}", ButtonStyle.Danger, row: manageRowId);
            }

            builder.WithButton("Exit", $"{CommandIdMap.Exit}:{user.Id}", ButtonStyle.Secondary, row: manageRowId + 1);

            return builder.Build();
        }

        public static (bool HasOptions, MessageComponent Components) BuildAddMenu(IGuildUser self, IGuildUser user, RoleRequestEntity properties, int indexStart)
        {
            var count = 0;
            var roles = user.Guild.Roles.OrderByDescending(x => x.Position)
                .Skip(indexStart)
                .ToList();
            IRole selfHighestRole = null;

            foreach (var roleId in self.RoleIds)
            {
                var role = self.Guild.GetRole(roleId);

                if (selfHighestRole is null || role.Position > selfHighestRole.Position)
                    selfHighestRole = role;
            }

            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.RoleRequestManageSave}:{user.Id},{indexStart}")
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

            return Pagination.SelectMenu(selectMenu, indexStart, user, CommandIdMap.RoleRequestManageAdd, CommandIdMap.RoleRequestMenu);
        }

        public static (bool HasOptions, MessageComponent Components) BuildRemoveMenu(IGuildUser user, RoleRequestEntity properties, int indexStart)
        {
            var count = 0;
            var roleIds = properties.RoleIds.Skip(indexStart).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.RoleRequestManageDelete}:{user.Id},{indexStart}")
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

            return Pagination.SelectMenu(selectMenu, indexStart, user, CommandIdMap.RoleRequestManageRemove, CommandIdMap.RoleRequestMenu);
        }

        public static (bool HasOptions, MessageComponent Components) BuildAssignMenu(IGuildUser user, RoleRequestEntity properties, int indexStart)
        {
            var count = 0;
            var roleIds = properties.RoleIds.Skip(indexStart).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.RoleRequestSave}:{user.Id},{indexStart}")
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

            return Pagination.SelectMenu(selectMenu, indexStart, user, CommandIdMap.RoleRequestAssign, CommandIdMap.RoleRequestMenu);
        }

        public static (bool HasOptions, MessageComponent Components) BuildRevokeMenu(IGuildUser self, IGuildUser user, RoleRequestEntity properties, int indexStart)
        {
            var count = 0;
            var roleIds = user.RoleIds.Skip(indexStart).ToList();
            var selectMenu = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.RoleRequestDelete}:{user.Id},{indexStart}")
                .WithMinValues(1)
                .WithPlaceholder("Select role(s) to remove from yourself");
            var guildRoles = new List<IRole>();
            IRole selfHighestRole = null;

            foreach (var roleId in self.RoleIds)
            {
                var role = self.Guild.GetRole(roleId);

                if (selfHighestRole is null || role.Position > selfHighestRole.Position)
                    selfHighestRole = role;
            }

            foreach (var roleId in roleIds)
            {
                var role = user.Guild.GetRole(roleId);
                guildRoles.Add(role);
            }

            foreach (var role in guildRoles.OrderByDescending(x => x.Position))
            {
                if (role.Position >= selfHighestRole.Position || role.Name == "@everyone")
                    continue;

                selectMenu.AddOption(role.Name, role.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            return Pagination.SelectMenu(selectMenu, indexStart, user, CommandIdMap.RoleRequestRemove, CommandIdMap.RoleRequestMenu);
        }
    }
}