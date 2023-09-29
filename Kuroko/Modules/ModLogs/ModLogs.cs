using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using System.Text;

namespace Kuroko.Modules.ModLogs
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class ModLogs : KurokoModuleBase
    {
        private StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Moderation Logging");
            }
        }


        [SlashCommand("modlogs", "Configure moderation logging")]
        public async Task EntryAsync()
        {

        }
    }
}
