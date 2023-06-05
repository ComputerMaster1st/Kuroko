using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.MDK
{
    public abstract class KurokoModule
    {
        public abstract string ModuleName { get; }
        public abstract string ModuleCodeName { get; }
        public abstract string ModuleDescription { get; }

        public virtual void RegisterToDependencyInjection(IServiceCollection serviceCollection) { }

        public virtual void UnregisterFromDependencyInjection(IServiceCollection serviceCollection) { }

        public virtual void UnloadEvents(IServiceProvider serviceProvider) { }

        public virtual Task UnloadCommandsAsync(InteractionService interactionService) { return Task.CompletedTask; }
    }
}
