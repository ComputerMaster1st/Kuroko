using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Modules.Reports.Modals;

namespace Kuroko.Modules.Reports
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class MessageReport : KurokoModuleBase
    {
        [UserCommand("Report Message")]
        public Task ReportMessage(IUserMessage msg)
        {
            return Context.Interaction.RespondWithModalAsync<ReportModal>($"{ReportsCommandMap.USER_MODAL}:{user.Id}", modifyModal: (x) =>
            {
                x.Title = $"Report User: {user.GlobalName ?? user.Username}";
            });
        }
    }
}
