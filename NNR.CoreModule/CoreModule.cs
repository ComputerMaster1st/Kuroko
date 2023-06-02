using Microsoft.Extensions.DependencyInjection;
using NNR.CoreModule.Events;
using NNR.MDK;

namespace NNR.CoreModule
{
    public class CoreModule : INnrModule
    {
        public void RegisterToDependencyInjection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<DiscordLogEvent>();
        }
    }
}
