﻿namespace Kuroko.Shared
{
    public static class DataDirectories
    {
        public const string LOG = "logs";
        public const string CONFIG = "config";
        public const string TEMPFILES = "temp_files";
        public const string TEMPZIPS = "temp_zips";

        public static void CreateDirectories()
        {
            Directory.CreateDirectory(LOG);
            Directory.CreateDirectory(CONFIG);
            Directory.CreateDirectory(TEMPFILES);
            Directory.CreateDirectory(TEMPZIPS);
        }
    }
}
