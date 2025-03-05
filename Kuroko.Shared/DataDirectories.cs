namespace Kuroko.Shared;

public class DataDirectories
{
    public const string LOG = "logs";
    public const string CONFIG = "config";

    public static void CheckDirectories()
    {
        Directory.CreateDirectory(LOG);
        Directory.CreateDirectory(CONFIG);
    }
}