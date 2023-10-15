using Discord;
using Discord.Interactions;

namespace Kuroko.Modules.Reports.Modals
{
    public class ReportUserModal : IModal
    {
        public string Title => "Report User";

        [InputLabel("Subject")]
        [ModalTextInput(ReportsCommandMap.USER_MODAL_SUBJECT, TextInputStyle.Short, "Required", maxLength: 150)]
        [RequiredInput]
        public string Subject { get; set; } = string.Empty;

        [InputLabel("Violating Rules")]
        [ModalTextInput(ReportsCommandMap.USER_MODAL_VIOLATED, TextInputStyle.Short, "Required", maxLength: 300)]
        [RequiredInput]
        public string Rules { get; set; } = string.Empty;

        [InputLabel("Description")]
        [ModalTextInput(ReportsCommandMap.USER_MODAL_DESCRIPTION, TextInputStyle.Paragraph, "Optional")]
        public string Description { get; set; } = string.Empty;
    }
}
