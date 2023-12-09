using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class RemoveHandlerComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ReportsCommandMap.HANDLER_REMOVE}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ExecuteAsync(index);
        }

        [ComponentInteraction($"{ReportsCommandMap.HANDLER_DELETE}:*")]
        public async Task ReturningAsync(ulong interactedUserId, string handlerId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var handler = properties.ReportHandlers.FirstOrDefault(x => x.Id == int.Parse(handlerId));

            if (handler != null)
                properties.ReportHandlers.Remove(handler, Context.Database);

            await ExecuteAsync(0, properties);
        }

        private async Task ExecuteAsync(int index, ReportsEntity propParams = null)
        {
            await DeferAsync();

            var properties = propParams ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var user = Context.User as IGuildUser;
            var selectOptions = new List<SelectMenuOptionBuilder>();
            var count = 0;
            var output = new StringBuilder()
                .AppendLine("# Reports Configuration")
                .AppendLine("## Remove Handlers");

            if (properties.ReportHandlers.Count > 0)
                foreach (var handler in properties.ReportHandlers.OrderByDescending(x => x.Level))
                {
                    var role = Context.Guild.GetRole(handler.RoleId);
                    var roleName = role is null ? "Role Missing! Please Fix!" : role.Name;

                    selectOptions.Add(new()
                    {
                        Label = $"{handler.Name} : {roleName}",
                        Value = handler.Id.ToString()
                    });

                    count++;

                    if (count >= 25)
                        break;
                }
            else
                output.AppendLine("* **None Set**");

            var selectMenuBuilder = new SelectMenuBuilder()
            {
                CustomId = $"{ReportsCommandMap.HANDLER_DELETE}:{user.Id}",
                MinValues = 1,
                Placeholder = "Select a handler to remove",
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
