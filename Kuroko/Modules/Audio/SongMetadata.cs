﻿using System;

namespace Kuroko.Modules.Audio
{
    internal class SongMetadata
    {
        /// <summary>
        /// Title of this song
        /// </summary>
        public string Title { get; set; } = "Unknown title";

        /// <summary>
        /// Artist of this song
        /// </summary>
        public string Artist { get; set; } = "Unknown artist";

        /// <summary>
        /// Album
        /// </summary>
        public string Album { get; set; } = "";

        /// <summary>
        /// Total duration of this song
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    }
}
