using Discord;
using Kuroko.Attributes;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class UnobservedErrorEvent
{
    public UnobservedErrorEvent()
        => TaskScheduler.UnobservedTaskException += (_, args)
            => Task.Factory.StartNew(() => UnobservedTaskEvent(args));

    private static async Task UnobservedTaskEvent(UnobservedTaskExceptionEventArgs args)
    {
        args.SetObserved();
        await Utilities.WriteLogAsync(
            new LogMessage(
                LogSeverity.Error, 
                LogHeader.SYSTEM, 
                "Unobserved Exception Detected!", 
                args.Exception
            ));
    }
}