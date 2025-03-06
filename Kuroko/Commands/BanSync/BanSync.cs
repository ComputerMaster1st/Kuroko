using System.Text;
using Discord.Interactions;
using Kuroko.Database.GuildEntities;

namespace Kuroko.Commands.BanSync;

[Group("bansync", "Ban synchronization between servers")]
public class BanSync : KurokoCommandBase
{
    [SlashCommand("status", "Status of BanSync")]
    public async Task StatusAsync()
    {
        var properties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
        var output = new StringBuilder()
            .AppendLine("## BanSync Status")
            .AppendLine($"* **Enabled:** {(properties.IsEnabled ? "True" : "False")}")
            .AppendLine($"* **Allow Requests:** {(properties.AllowRequests ? "True" : "False")}")
            .AppendLine($"* **Servers Hosting For/As Client:** {properties.HostForProfiles.Count}/{properties.ClientOfProfiles.Count}")
            .AppendLine($"* **Default Sync Mode:** {properties.Mode}");
        
        await RespondAsync(output.ToString(), ephemeral: true);
    }

    [SlashCommand("config", "(Server Management Only) Configuration for BanSync")]
    public Task ConfigAsync()
    {
        return RespondAsync("Nothing to see here at this time.", ephemeral: true);
    }
}
