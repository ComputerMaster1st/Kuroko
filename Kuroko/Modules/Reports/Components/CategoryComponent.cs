using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class CategoryComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ReportsCommandMap.CATEGORY}:*,*")]
        public async Task InitialAsync(ulong interactedUserId, int index)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ExecuteAsync(index);
        }

        [ComponentInteraction($"{ReportsCommandMap.CATEGORY_SAVE}:*,*")]
        public async Task ReturningAsync(ulong interactedUserId, int index, string result)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var categoryId = ulong.Parse(result);

            properties.ReportCategoryId = categoryId;

            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(index, properties);
        }

        private async Task ExecuteAsync(int index, ReportsEntity propParam = null)
        {
            await DeferAsync();

            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            string categoryName = "Not Set";

            var category = Context.Guild.GetCategoryChannel(properties.ReportCategoryId);

            if (properties.ReportCategoryId != 0 && category is null)
                categoryName = "**_Missing Category! Please Fix!_**";
            else if (category != null)
                categoryName = category.Name;

            var selectOptions = new List<SelectMenuOptionBuilder>();
            var count = 0;
            var isSet = false;

            if (properties.ReportCategoryId != 0)
            {
                selectOptions.Add(new()
                {
                    Label = "(Unset Category)",
                    Value = "0"
                });

                isSet = true;
                count++;
            }

            if (isSet && index > 0)
                index -= 1;

            foreach (var cat in Context.Guild.CategoryChannels.Skip(index).ToList())
            {
                selectOptions.Add(new()
                {
                    Label = cat.Name,
                    Value = cat.Id.ToString()
                });

                count++;

                if (count >= 25)
                    break;
            }

            var output = new StringBuilder()
                .AppendLine("# Reports Configuration")
                .AppendLine("## Category Selection")
                .AppendLine($"Selected Category: **{categoryName}**");

            var selectMenuBuilder = new SelectMenuBuilder()
            {
                CustomId = $"{ReportsCommandMap.CATEGORY_SAVE}:{user.Id},0",
                MinValues = 1,
                Placeholder = "Select a category to use for reports",
                Options = selectOptions
            };
            var menu = Pagination.SelectMenu(selectMenuBuilder, index, user, ReportsCommandMap.CATEGORY, ReportsCommandMap.MENU, true);

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = menu.Components;
            })).ResetTimeout();
        }
    }
}
