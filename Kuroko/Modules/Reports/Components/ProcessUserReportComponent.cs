using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
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
            var properties = await GetPropertiesAsync<ReportsEntity, GuildEntity>(Context.Guild.Id);
            var category = Context.Guild.GetCategoryChannel(properties.ReportCategoryId);
            var user = Context.User as IGuildUser;
            var reportedUser = Context.Guild.GetUser(reportedUserId);
            var channel = await Context.Guild.CreateTextChannelAsync($"report-{reportedUser.GlobalName ?? reportedUser.Username}", x =>
            {
                x.CategoryId = category.Id;
            });
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Allow));

            var output = new StringBuilder()
                .AppendLine("# Reported User")
                .AppendLine($"**Subject:** {modal.Subject}")
                .AppendLine($"**Accused:** {reportedUser.Mention} ({reportedUser.Id})")
                .AppendLine($"**Rules Violated:** {modal.Rules}")
                .AppendLine("## Description")
                .AppendLine(modal.Description ?? "_No description provided!_")
                .AppendLine("## Please provide any evidence to help this report!");
            await channel.SendMessageAsync(output.ToString());

            await RespondAsync($"Report Created! View it here at {channel.Mention}", ephemeral: true);
        }
    }
}
