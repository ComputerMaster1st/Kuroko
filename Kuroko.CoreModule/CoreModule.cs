using Discord.Interactions;
using Kuroko.CoreModule.Events;
using Kuroko.MDK;
using Microsoft.Extensions.DependencyInjection;
using System.Net.NetworkInformation;

namespace Kuroko.CoreModule
{
    public class CoreModule : KurokoModule
    {
        public override string ModuleName => "Kuroko: Core Module";
        public override string ModuleCodeName => "KUROKO_CORE";
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

        public override async Task UnloadCommandsAsync(InteractionService interactionService)
        {
            await interactionService.RemoveModuleAsync<Ping>();
        }
    }
}
