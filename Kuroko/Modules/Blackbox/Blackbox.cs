using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;

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

        [ComponentInteraction($"{BlackboxCommandMap.ENABLE}:*")]
        public async Task BlackboxAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.IsEnabled = !x.IsEnabled);
        }

        [ComponentInteraction($"{BlackboxCommandMap.ATTACHMENTS}:*")]
        public async Task SaveAttachmentsAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.SaveAttachments = !x.SaveAttachments);
        }

        [ComponentInteraction($"{BlackboxCommandMap.SYNCMODLOG}:*")]
        public async Task SyncModLogAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.SyncModLog = !x.SyncModLog);
        }

        private async Task ToggleAsync(Action<BlackboxEntity> action)
        {
            var properties = await GetPropertiesAsync<BlackboxEntity, GuildEntity>(Context.Guild.Id);

            action(properties);

            await DeferAsync();
            await ExecuteAsync(properties, true);
        }

        private async Task ExecuteAsync(BlackboxEntity propParams = null, bool isReturning = false)
        {
            var output = new StringBuilder()
                .AppendLine("# Blackbox Recorder")
                .AppendLine("## NOTICE!")
                .AppendLine("Enabling \"Blackbox Recorder\" will record all messages from the point of activation. We recommend you to make your server aware that all messages are being recorded for moderation purposes.")
                .AppendLine("_NOTE: Blackbox can only record channels the bot can see. To check, look at members list to verify you can see the bot there in the channel._")
                .AppendLine();

            var properties = propParams ?? await GetPropertiesAsync<BlackboxEntity, GuildEntity>(Context.Guild.Id);
            var componentBuilder = new ComponentBuilder()
                .WithButton("Blackbox Recorder", $"{BlackboxCommandMap.ENABLE}:{Context.User.Id}",
                    Pagination.IsButtonToggle(properties.IsEnabled), row: 0)
                .WithButton("Exit",
                    $"{GlobalCommandMap.EXIT}:{Context.User.Id}",
                    ButtonStyle.Secondary,
                    row: 0);

            if (properties.IsEnabled)
            {
                output.AppendLine("\"Synchronize ModLogs\" will configure the blackbox to use the same ignored channel configuration within \"ModLogs\". By default, this will be active. Disabling will force the blackbox to record all visible channels.");

                componentBuilder
                    .WithButton("Download Attachments", $"{BlackboxCommandMap.ATTACHMENTS}:{Context.User.Id}",
                        Pagination.IsButtonToggle(properties.SaveAttachments), row: 1)
                    .WithButton("Synchronize ModLogs", $"{BlackboxCommandMap.SYNCMODLOG}:{Context.User.Id}",
                        Pagination.IsButtonToggle(properties.SyncModLog), row: 1);
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