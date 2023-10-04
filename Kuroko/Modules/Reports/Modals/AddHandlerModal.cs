using Discord;
using Discord.Interactions;

namespace Kuroko.Modules.Reports.Modals
{
    public class AddHandlerModal : IModal
    {
        public string Title => "Add Report Handler";

        [InputLabel("Name")]
        [ModalTextInput(ReportsCommandMap.ReportHandlersName, TextInputStyle.Short, "Give the handler type a name. Leave blank to use role name instead.", maxLength: 100)]
        public string Name { get; set; } = string.Empty;
    }
}
