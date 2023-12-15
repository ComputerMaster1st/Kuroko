using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.Modules.Blackbox
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class Blackbox : KurokoModuleBase
    {
        [SlashCommand("blackbox", "Blackbox Recorder configuration")]
        public Task EntryAsync()
            => ExecuteAsync();
        
        [ComponentInteraction($"{BlackboxCommandMap.MENU}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(isReturning: true);
        }

        private async Task ExecuteAsync(BlackboxEntity propParams = null, bool isReturning = false)
        {
            var output = new StringBuilder()
                .AppendLine("# Blackbox Recorder")
                .AppendLine("## NOTICE!")
                .AppendLine("Enabling \"Blackbox Recorder\" will record all messages from all channels that are not ignored by ModLogs. We recommend you to make your server aware that all messages are being recorded for moderation purposes.");

            var properties = propParams ?? await GetPropertiesAsync<BlackboxEntity, GuildEntity>(Context.Guild.Id);
            var componentBuilder = new ComponentBuilder()
                .WithButton("Blackbox Recorder", $"{BlackboxCommandMap.ENABLE}:{Context.User.Id}",
                    Pagination.IsButtonToggle(properties.IsEnabled), row: 0);

            if (properties.IsEnabled)
            {
                componentBuilder.WithButton("Download Attachments", $"{BlackboxCommandMap.ATTACHMENTS}:{Context.User.Id}",
                    Pagination.IsButtonToggle(properties.SaveAttachments), row: 0);
            }

            if (!isReturning)
            {
                await RespondAsync(output.ToString(), components: componentBuilder.Build());
                (await Context.Interaction.GetOriginalResponseAsync()).SetTimeout(1);
            }
            else
                (await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = componentBuilder.Build();
                })).ResetTimeout();
        }
    }
}