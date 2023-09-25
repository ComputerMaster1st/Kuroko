using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using System.Text;

namespace Kuroko.Modules.RoleRequest.Components.Management
{
    [RequireUserGuildPermission(GuildPermission.ManageRoles)]
    [RequireBotGuildPermission(GuildPermission.ManageRoles)]
    public class AddRoleComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{CommandIdMap.RoleRequestManageAdd}:*,*")]
        public async Task ExecuteAsync(ulong interactedUserId, int startIndex)
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
            var menu = RRMenu.BuildAddMenu(Context.Guild, roleRequest, Context.User as IGuildUser, startIndex);

            if (!menu.HasOptions)
                output.AppendLine("Oops, looks like you reached the end of the list already!");

            await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            });
        }
    }
}
