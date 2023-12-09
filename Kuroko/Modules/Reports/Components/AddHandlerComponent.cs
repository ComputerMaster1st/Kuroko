using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Modules.Reports.Modals;
using Kuroko.Services;
using System.Data;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class AddHandlerComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ReportsCommandMap.HANDLER_ADD}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ExecuteAsync(index);
        }

        [ComponentInteraction($"{ReportsCommandMap.HANDLER_ADD_CONFIRM}:*")]
        public async Task ConfirmAsync(ulong interactedUserId, string roleId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var role = Context.Guild.GetRole(ulong.Parse(roleId));

            await Context.Interaction.RespondWithModalAsync<AddHandlerModal>(
                $"{ReportsCommandMap.HANDLER_SAVE}:{Context.User.Id},{roleId}", modifyModal: x =>
                {
                    x.Title = $"Add \"{role.Name}\" as handler";
                });
        }

        [ModalInteraction($"{ReportsCommandMap.HANDLER_SAVE}:*,*")]
        public async Task SaveAsync(ulong interactedUserId, ulong roleId, AddHandlerModal modal)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var role = Context.Guild.GetRole(roleId);

            properties.ReportHandlers.Add(new()
            {
                Name = string.IsNullOrEmpty(modal.Name) ? role.Name : modal.Name,
                RoleId = role.Id,
                Level = role.Position
            });

            await ExecuteAsync(0, properties);
        }

        private async Task ExecuteAsync(int index, ReportsEntity propParam = null)
        {
            await DeferAsync();

            var user = Context.User as IGuildUser;
            var self = Context.Guild.GetUser(Context.Client.CurrentUser.Id) as IGuildUser;
            IRole selfHighestRole = null;
            var properties = propParam ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);

            foreach (var roleId in self.RoleIds)
            {
                var role = self.Guild.GetRole(roleId);

                if (selfHighestRole is null || role.Position > selfHighestRole.Position)
                    selfHighestRole = role;
            }

            var selectOptions = new List<SelectMenuOptionBuilder>();
            var count = 0;
            var output = new StringBuilder()
                .AppendLine("# Reports Configuration")
                .AppendLine("## Add Handlers");

            if (properties.ReportHandlers.Count > 0)
                foreach (var handler in properties.ReportHandlers.OrderByDescending(x => x.Level))
                {
                    var role = Context.Guild.GetRole(handler.RoleId);

                    if (role is null)
                        output.AppendLine($"* {handler.Name} - **_(Role Missing! Please fix!)_**");
                    else
                        output.AppendLine($"* {handler.Name} - **{role.Name}**");
                }
            else
                output.AppendLine("* **None Set**");

            foreach (var role in Context.Guild.Roles.OrderByDescending(x => x.Position))
            {
                if (role.Position < selfHighestRole.Position || properties.ReportHandlers.Any(x => x.RoleId == role.Id) || role.Name == "@everyone")
                    continue;

                selectOptions.Add(new()
                {
                    Label = role.Name,
                    Value = role.Id.ToString()
                });
                count++;

                if (count >= 25)
                    break;
            }

            var selectMenuBuilder = new SelectMenuBuilder()
            {
                CustomId = $"{ReportsCommandMap.HANDLER_ADD_CONFIRM}:{user.Id}",
                MinValues = 1,
                Placeholder = "Select a role to use set as handler",
                Options = selectOptions
            };
            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ReportsCommandMap.HANDLER_ADD, ReportsCommandMap.MENU, true);

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
