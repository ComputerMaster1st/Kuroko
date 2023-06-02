using Microsoft.Extensions.DependencyInjection;

namespace NNR.MDK
{
    public interface INnrModule
    {
        string ModuleName { get; }
        string ModuleCodeName { get; }
        string ModuleDescription { get; }

        void RegisterToDependencyInjection(ref IServiceCollection serviceCollection);
    }
}
