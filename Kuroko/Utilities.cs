using Discord;
using Kuroko.Shared;

namespace Kuroko;

internal static class Utilities
{
    private static readonly SemaphoreSlim LogLock = new(1);
    
    public static async Task WriteLogAsync(LogMessage message)
    {
        await LogLock.WaitAsync();

        try
        {
            await File.AppendAllTextAsync($"{DataDirectories.LOG}/{DateTime.Today:yyyy_MM_dd}.log",
                message + Environment.NewLine);
        }
        finally { LogLock.Release(); }

        Console.WriteLine(message);
    }
}