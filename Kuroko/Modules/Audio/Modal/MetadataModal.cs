using Discord;
using Discord.Interactions;

namespace Kuroko.Modules.Audio.Modal
{
    public class MetadataModal : IModal
    {
        public string Title => "Set Metadata";

        [InputLabel("Title")]
        [RequiredInput]
        [ModalTextInput(TranscodeCommandMap.META_MODAL_TITLE, TextInputStyle.Short, "Sandstorm", maxLength: 1000)]
        public string SongTitle { get; set; } = string.Empty;

        [InputLabel("Artist")]
        [RequiredInput]
        [ModalTextInput(TranscodeCommandMap.META_MODAL_ARTIST, TextInputStyle.Short, "Darude", maxLength: 200)]
        public string SongArtist { get; set; } = string.Empty;

        [InputLabel("Album")]
        [ModalTextInput(TranscodeCommandMap.META_MODAL_ALBUM, TextInputStyle.Short, minLength: 0, maxLength: 200)]
        public string SongAlbum { get; set; } = string.Empty;
    }
}
