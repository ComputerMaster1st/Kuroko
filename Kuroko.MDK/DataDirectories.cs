namespace Kuroko.MDK
{
    public static class DataDirectories
    {
        public const string LOG = "logs";
        public const string CONFIG = "config";
        public const string MODULES = "modules";

        public static void CreateDirectories()
        {
            Directory.CreateDirectory(LOG);
            Directory.CreateDirectory(CONFIG);
            Directory.CreateDirectory(MODULES);
        }
    }
}
