using Discord.Interactions;

namespace Kuroko.Commands.BanSync;

[Group("bansync", "Ban synchronization between servers")]
public class BanSync : KurokoCommandBase
{
    [SlashCommand("status", "Status of BanSync")]
    public Task StatusAsync()
    {
        return RespondAsync("Nothing to see here at this time.", ephemeral: true);
    }

    [SlashCommand("config", "(Server Management Only) Configuration for BanSync")]
    public Task ConfigAsync()
    {
        return RespondAsync("Nothing to see here at this time.", ephemeral: true);
    }
}
