using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Modules.Reports.Modals;

namespace Kuroko.Modules.Reports
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels), TicketPrecondition]
    public class Report : KurokoModuleBase
    {
        [UserCommand("Report User")]
        public Task ReportUserAsync(IUser user)
        {
            return Context.Interaction.RespondWithModalAsync<ReportModal>($"{ReportsCommandMap.REPORT_USER}:{user.Id}", modifyModal: (x) =>
            {
                x.Title = $"Report User: {user.GlobalName ?? user.Username}";
            });
        }

        [MessageCommand("Report Message")]
        public Task ReportMessage(IUserMessage msg)
        {
            return Context.Interaction.RespondWithModalAsync<ReportModal>($"{ReportsCommandMap.REPORT_MESSAGE}:{msg.Id}", modifyModal: (x) =>
            {
                x.Title = $"Report Message: ID ({msg.Id})";
            });
        }
    }
}
