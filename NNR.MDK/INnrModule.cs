using Microsoft.Extensions.DependencyInjection;

namespace NNR.MDK
{
    public interface INnrModule
    {
        void RegisterToDependencyInjection(ref IServiceCollection serviceCollection);
    }
}
