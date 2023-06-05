using Discord;
using Kuroko.Core;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;

namespace Kuroko.Events
{
    [PreInitialize]
    internal class UnobservedErrorEvent
    {
        public UnobservedErrorEvent()
            => TaskScheduler.UnobservedTaskException += (sender, args)
                => Task.Factory.StartNew(() => UnobservedTaskEvent(a));

        private static async Task UnobservedTaskEvent(UnobservedTaskExceptionEventArgs args)
        {
            args.SetObserved();
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Error, CoreLogHeader.SYSTEM, "Unobserved Exception Detected!", args.Exception));
        }
    }
}
