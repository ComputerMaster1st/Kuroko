namespace Kuroko
{
    public static class DataDirectories
    {
        public const string LOG = "logs";
        public const string CONFIG = "config";

        public static void CreateDirectories()
        {
            Directory.CreateDirectory(LOG);
            Directory.CreateDirectory(CONFIG);
        }
    }
}
