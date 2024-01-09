using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuroko.Modules.Audio.FFmpeg
{
    internal class Metadata
    {
        public Format format { get; set; }
    }

    internal class Format
    {
        public string format_name { get; set; }
        public string format_long_name { get; set; }
        public string duration { get; set; }
        public Tags? tags { get; set; }
    }

    internal class Tags
    {
        public string artist { get; set; }
        public string album_artist { get; set; }
        public string TBPM { get; set; }
        public string album { get; set; }
        public string title { get; set; }
        public string date { get; set; }
    }
}
