using Discord;
using Discord.Interactions;
using Kuroko.Attributes;

namespace Kuroko.Commands.BanSync;

public partial class BanSync
{
    public class BanSyncConfig : KurokoCommandBase
    {
        [SlashCommand("config", "(Server Management Only) Configuration for BanSync")]
        [KurokoBotPermission(GuildPermission.BanMembers)]
        [KurokoUserPermission(GuildPermission.ManageGuild)]
        public Task ShowConfigAsync()
        {
            return RespondAsync("Nothing to see here at this time.", ephemeral: true);
        }
    }
}