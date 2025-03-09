using Discord;
using Discord.Interactions;

namespace Kuroko.Commands.BanSync;

public class BanSyncClientModal : IModal
{
    public string Title => "BanSync Client Request";

    [InputLabel("UUID")]
    [ModalTextInput($"{CommandMap.BANSYNC_CLIENTREQUEST_ID}",
        TextInputStyle.Short, "Put your BanSync UUID Here", 10, 50)]
    public string BanSyncId { get; set; } = string.Empty;
    
    [InputLabel("Reason")]
    [ModalTextInput($"{CommandMap.BANSYNC_CLIENTREQUEST_REASON}",
        TextInputStyle.Paragraph, "Your reason")]
    public string Reason { get; set; } = string.Empty;
}