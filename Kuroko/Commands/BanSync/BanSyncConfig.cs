using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Database.GuildEntities;
using Kuroko.Shared;

namespace Kuroko.Commands.BanSync;

public partial class BanSync
{
    public class BanSyncConfig : KurokoCommandBase
    {
        [SlashCommand("config", "(Server Management Only) Configuration for BanSync")]
        [KurokoBotPermission(GuildPermission.BanMembers)]
        [KurokoUserPermission(GuildPermission.ManageGuild)]
        public Task ShowConfigAsync()
            => ExecuteAsync();

        private async Task ExecuteAsync()
        {
            var output = new StringBuilder()
                .AppendLine("# BanSync Configuration");
            var properties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
            var componentBuilder = new ComponentBuilder()
                .WithButton($"Enabled: {(properties.IsEnabled ? "Yes" : "No")}",
                    $"{CommandMap.BANSYNC_ENABLE}:{Context.User.Id}",
                    ButtonToggle(properties.IsEnabled),
                    row: 0)
                .WithButton($"Allow Requests: {(properties.AllowRequests ? "Yes" : "No")}",
                    $"{CommandMap.BANSYNC_ALLOWREQUEST}:{Context.User.Id}",
                    ButtonToggle(properties.AllowRequests),
                    row: 0)
                .WithSelectMenu($"{CommandMap.BANSYNC_SYNCMODE}:{Context.User.Id}",
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
                        },
                    ])
                .WithButton("Exit",
                    $"{CommandMap.EXIT}:{Context.User.Id}",
                    ButtonStyle.Secondary,
                    row: 2);
            
            await RespondAsync(output.ToString(), components: componentBuilder.Build());
        }
    }
}