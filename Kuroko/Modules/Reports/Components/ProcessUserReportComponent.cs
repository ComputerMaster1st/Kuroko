using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Modules.Reports.Modals;
using System.Text;

namespace Kuroko.Modules.Reports.Components
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class ProcessUserReportComponent : KurokoModuleBase
    {
        [ModalInteraction($"{ReportsCommandMap.ReportUserModal}:*")]
        public async Task ExecuteAsync(ulong reportedUserId, ReportUserModal modal)
        {
            var output = new StringBuilder()
                .AppendLine("Reported User: " + reportedUserId)
                .AppendLine("Rules Violated: " + modal.Rules)
                .AppendLine("Descroption: " + modal.Description);

            await RespondAsync(output.ToString(), ephemeral: true);
        }
    }
}
