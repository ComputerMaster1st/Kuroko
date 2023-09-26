using Discord;
using Kuroko.Core;
using Kuroko.Core.Attributes;

namespace Kuroko.Events
{
    [PreInitialize]
    internal class UnobservedErrorEvent
    {
        public UnobservedErrorEvent()
            => TaskScheduler.UnobservedTaskException += (sender, args)
                => Task.Factory.StartNew(() => UnobservedTaskEvent(args));

        private static async Task UnobservedTaskEvent(UnobservedTaskExceptionEventArgs args)
        {
            args.SetObserved();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Error, LogHeader.SYSTEM, "Unobserved Exception Detected!", args.Exception));
        }
    }
}
