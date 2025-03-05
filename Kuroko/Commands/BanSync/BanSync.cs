using System.Text;
using Discord.Interactions;

namespace Kuroko.Commands.BanSync;

[Group("bansync", "Ban synchronization between servers")]
public class BanSync : KurokoCommandBase
{
    [SlashCommand("status", "Status of BanSync")]
    public Task StatusAsync()
    {
        var output = new StringBuilder()
            .AppendLine("## BanSync Status")
            .AppendLine("* **Enabled:** True/False")
            .AppendLine("* **Servers Remote/Serving:** 10/5")
            .AppendLine("* **Default Sync Mode:** Simplex/Half-Duplex/Full-Duplex");
        return RespondAsync(output.ToString(), ephemeral: true);
    }

    [SlashCommand("config", "(Server Management Only) Configuration for BanSync")]
    public Task ConfigAsync()
    {
        return RespondAsync("Nothing to see here at this time.", ephemeral: true);
    }
}
