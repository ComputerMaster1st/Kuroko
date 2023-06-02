using Microsoft.Extensions.DependencyInjection;
using NNR.CoreModule.Events;
using NNR.MDK;

namespace NNR.CoreModule
{
    public class CoreModule : INnrModule
    {
        public string ModuleName => "New New Railgun: Core Module";
        public string ModuleCodeName => "NNR_CORE";
        public string ModuleDescription => "Contains primary events and basic commands.";

        public void RegisterToDependencyInjection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<DiscordLogEvent>();
        }
    }
}
