using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuroko.Modules.Audio
{
    internal class TranscodeCommandMap
    {
        private const string PREFIX = "transcode_";

        public const string EDIT_META = PREFIX + "meta_edit";
        public const string ACCEPT_META = PREFIX + "meta_ok";
        
        public const string META_MODAL = PREFIX + "set_meta_modal";
        public const string META_MODAL_TITLE = PREFIX + "set_meta_title";
        public const string META_MODAL_ARTIST = PREFIX + "set_meta_artist";
        public const string META_MODAL_ALBUM = PREFIX + "set_meta_album";

        public const string JOB_TIMEOUT = PREFIX + "timeout";
    }
}
