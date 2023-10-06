using Discord;
using Discord.Interactions;

namespace Kuroko.Modules.Reports.Modals
{
    public class ReportUserModal : IModal
    {
        public string Title => "Report User";

        [InputLabel("Subject")]
        [ModalTextInput(ReportsCommandMap.ReportUserModalSubject, TextInputStyle.Short, "Required", maxLength: 150)]
        [RequiredInput]
        public string Subject { get; set; } = string.Empty;

        [InputLabel("Violating Rules")]
        [ModalTextInput(ReportsCommandMap.ReportUserModalRules, TextInputStyle.Short, "Required", maxLength: 300)]
        [RequiredInput]
        public string Rules { get; set; } = string.Empty;

        [InputLabel("Description")]
        [ModalTextInput(ReportsCommandMap.ReportUserModalDescription, TextInputStyle.Paragraph, "Optional")]
        public string Description { get; set; } = string.Empty;
    }
}
