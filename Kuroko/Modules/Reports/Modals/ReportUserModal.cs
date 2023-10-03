using Discord;
using Discord.Interactions;

namespace Kuroko.Modules.Reports.Modals
{
    public class ReportUserModal : IModal
    {
        public string Title => "Report User";

        [InputLabel("Violating Rules")]
        [ModalTextInput(ReportsCommandMap.ReportUserModalRules, TextInputStyle.Short, "Specify rules violated", maxLength: 300)]
        [RequiredInput]
        public string Rules { get; set; } = string.Empty;

        [InputLabel("Description")]
        [ModalTextInput(ReportsCommandMap.ReportUserModalDescription, TextInputStyle.Paragraph, "(Optional) Provide a description for the report")]
        public string Description { get; set; } = string.Empty;
    }
}
