using Microsoft.Extensions.DependencyInjection;
using NNR.CoreModule.Events;
using NNR.MDK;

namespace NNR.CoreModule
{
    public class CoreModule : NnrModule
    {
        public override string ModuleName => "New New Railgun: Core Module";
        public override string ModuleCodeName => "NNR_CORE";
        public override string ModuleDescription => "Contains primary events and basic commands.";

        public override void RegisterToDependencyInjection(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<DiscordLogEvent>();
        }

        public override void UnregisterFromDependencyInjection(IServiceCollection serviceCollection)
        {
            var descriptor = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(DiscordLogEvent));

            if (descriptor == null)
                return;

            _ = serviceCollection.Remove(descriptor);
        }

        public override void UnloadEvents(IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<DiscordLogEvent>()?.Unload();
        }
    }
}
