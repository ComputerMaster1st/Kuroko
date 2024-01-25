using System;

namespace Kuroko.Audio
{
    public class SongMetadata
    {
        /// <summary>
        /// Title of this song
        /// </summary>
        public string Title { get; set; } = "Unknown";

        /// <summary>
        /// Artist of this song
        /// </summary>
        public string Artist { get; set; } = "Unknown";

        /// <summary>
        /// Album
        /// </summary>
        public string Album { get; set; } = "";

        /// <summary>
        /// Total duration of this song
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        internal bool TranscodeNeedsFile { get; set; } = false;
    }
}
