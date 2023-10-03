using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Modules.Reports.Modals;

namespace Kuroko.Modules.Reports
{
    [RequireBotGuildPermission(GuildPermission.ManageChannels)]
    public class UserReport : KurokoModuleBase
    {
        [UserCommand("Report User")]
        public Task ReportUserAsync(IUser user)
        {
            return Context.Interaction.RespondWithModalAsync<ReportUserModal>($"{ReportsCommandMap.ReportUserModal}:{user.Id}", modifyModal: (x) =>
            {
                x.Title = $"Report User: {user.GlobalName ?? user.Username}";
            });
        }
    }
}
