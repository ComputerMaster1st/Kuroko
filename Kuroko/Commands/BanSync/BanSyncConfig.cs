using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Database.GuildEntities;
using Kuroko.Shared;

namespace Kuroko.Commands.BanSync;

public partial class BanSync
{
    [KurokoBotPermission(GuildPermission.BanMembers)]
    [KurokoUserPermission(GuildPermission.ManageGuild)]
    public class BanSyncConfig : KurokoCommandBase
    {
        [SlashCommand("bansync-config", 
            "(Server Management Only) Configuration for BanSync")]
        public Task ShowConfigAsync()
            => ExecuteAsync();

        [ComponentInteraction($"{CommandMap.BANSYNC_ENABLE}:*")]
        public async Task EnableAsync(ulong interactedUserId)
        {
            if (!IsInteractedUser(interactedUserId)) return;
            var properties = await QuickEditPropertiesAsync<BanSyncProperties>(Context.Guild.Id, x =>
            {
                x.IsEnabled = !x.IsEnabled;
            });
            await ExecuteAsync(properties, true);
        }

        [ComponentInteraction($"{CommandMap.BANSYNC_ALLOWREQUEST}:*")]
        public async Task AllowRequestAsync(ulong interactedUserId)
        {
            if (!IsInteractedUser(interactedUserId)) return;
            var properties = await QuickEditPropertiesAsync<BanSyncProperties>(Context.Guild.Id, x =>
            {
                x.AllowRequests = !x.AllowRequests;
            });
            await ExecuteAsync(properties, true);
        }
        
        [ComponentInteraction($"{CommandMap.BANSYNC_SYNCMODE_HOST}:*")]
        public async Task SetSyncModeAsync(ulong interactedUserId, string syncMode)
        {
            if (!IsInteractedUser(interactedUserId)) return;
            var mode = Enum.Parse<BanSyncMode>(syncMode);
            var properties = await QuickEditPropertiesAsync<BanSyncProperties>(Context.Guild.Id, x =>
            {
                x.HostMode = mode;
            });
            await ExecuteAsync(properties, true);
        }

        [ComponentInteraction($"{CommandMap.BANSYNC_CHANNEL}:*")]
        public async Task SetChannelAsync(ulong interactedUserId)
        {
            if (!IsInteractedUser(interactedUserId)) return;
            var properties = await QuickEditPropertiesAsync<BanSyncProperties>(Context.Guild.Id, x =>
            {
                x.BanSyncChannelId = Context.Channel.Id;
            });
            await ExecuteAsync(properties, true);
        }

        private async Task ExecuteAsync(BanSyncProperties propParam = null, bool isReturning = false)
        {
            var properties = propParam ?? await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
            var output = new StringBuilder()
                .AppendLine("# BanSync Configuration")
                .AppendLine($"* **UUID:** {properties.SyncId}")
                .AppendLine($"* **BanSync Channel:** {Context.Guild.GetTextChannel(
                    properties.BanSyncChannelId)?.Mention}")
                .AppendLine("## SyncMode Descriptions")
                .AppendLine("* **Disabled:** No Sync")
                .AppendLine("* **Simplex:** Read Host Banlist Only")
                .AppendLine("* **HalfDuplex:** Read Host Banlist & Send Warnings To Host")
                .AppendLine("* **FullDuplex:** Read Host Banlist & Execute Ban On Host")
                .AppendLine("  * **WARNING:** _ONLY TO BE USED FOR GROUPED COMMUNITY SERVERS_");
            var isBanSyncCurrentChannel = properties.BanSyncChannelId == Context.Channel.Id;
            var componentBuilder = new ComponentBuilder()
                .WithButton($"Enabled: {(properties.IsEnabled ? "Yes" : "No")}",
                    $"{CommandMap.BANSYNC_ENABLE}:{Context.User.Id}",
                    ButtonToggle(properties.IsEnabled),
                    row: 0)
                .WithButton($"Allow Requests: {(properties.AllowRequests ? "Yes" : "No")}",
                    $"{CommandMap.BANSYNC_ALLOWREQUEST}:{Context.User.Id}",
                    ButtonToggle(properties.AllowRequests),
                    row: 0)
                .WithButton(isBanSyncCurrentChannel ? "BanSync Channel Already Set" : "Set As BanSync Channel",
                    $"{CommandMap.BANSYNC_CHANNEL}:{Context.User.Id}",
                    ButtonToggle(isBanSyncCurrentChannel),
                    row: 0)
                .WithSelectMenu($"{CommandMap.BANSYNC_SYNCMODE_HOST}:{Context.User.Id}",
                    [
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.Disabled),
                            Value = nameof(BanSyncMode.Disabled)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.Simplex),
                            Value = nameof(BanSyncMode.Simplex)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.HalfDuplex),
                            Value = nameof(BanSyncMode.HalfDuplex)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.FullDuplex),
                            Value = nameof(BanSyncMode.FullDuplex)
                        }
                    ],
                    $"Current Host Mode: {properties.HostMode.ToString()}",
                    maxValues: 1,
                    row: 1)
                .WithSelectMenu($"{CommandMap.BANSYNC_SYNCMODE_CLIENT}:{Context.User.Id}",
                    [
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.Disabled),
                            Value = nameof(BanSyncMode.Disabled)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.HalfDuplex),
                            Value = nameof(BanSyncMode.HalfDuplex)
                        },
                        new SelectMenuOptionBuilder
                        {
                            Label = nameof(BanSyncMode.FullDuplex),
                            Value = nameof(BanSyncMode.FullDuplex)
                        }
                    ],
                    $"Current Client Mode: {properties.ClientMode.ToString()}",
                    maxValues: 1,
                    row: 1)
                .WithButton("Exit",
                    $"{CommandMap.EXIT_WITH_UID}:{Context.User.Id}",
                    ButtonStyle.Secondary,
                    row: 2);

            if (!isReturning)
                await RespondAsync(output.ToString(), components: componentBuilder.Build());
            else
                await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = componentBuilder.Build();
                });
        }
    }
}