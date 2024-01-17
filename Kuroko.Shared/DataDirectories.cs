namespace Kuroko.Shared
{
    public static class DataDirectories
    {
        public const string LOG = "logs";
        public const string CONFIG = "config";
        public const string TEMPFILES = "temp_files";
        public const string TEMPZIPS = "temp_zips";

        public const string AUDIO = "audio";
        public static string TRANSCODE = Path.Combine(AUDIO, "transcode");

        public static void CreateDirectories()
        {
            Directory.CreateDirectory(LOG);
            Directory.CreateDirectory(CONFIG);
            Directory.CreateDirectory(TEMPFILES);
            Directory.CreateDirectory(TEMPZIPS);

            Directory.CreateDirectory(AUDIO);
            Directory.CreateDirectory(TRANSCODE);
        }
    }
}
