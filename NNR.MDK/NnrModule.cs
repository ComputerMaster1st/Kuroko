using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace NNR.MDK
{
    public abstract class NnrModule
    {
        public abstract string ModuleName { get; }
        public abstract string ModuleCodeName { get; }
        public abstract string ModuleDescription { get; }

        public virtual void RegisterToDependencyInjection(IServiceCollection serviceCollection) { }

        public virtual void UnregisterFromDependencyInjection(IServiceCollection serviceCollection) { }

        public virtual void UnloadEvents(IServiceProvider serviceProvider) { }

        public virtual void UnloadCommands(InteractionService interactionService) { }
    }
}
