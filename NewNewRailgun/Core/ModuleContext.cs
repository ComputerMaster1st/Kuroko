using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using NNR.MDK;
using NNR.MDK.Attributes;
using System.Reflection;
using System.Runtime.Loader;

namespace NewNewRailgun.Core
{
    internal class ModuleContext
    {
        private readonly AssemblyLoadContext _assemblyContext;

        public string CodeName { get; }

        public NnrModule Module { get; }

        public Assembly Assembly
        {
            get
            {
                return _assemblyContext.Assemblies.FirstOrDefault();
            }
        }

        public int EventCount
        {
            get
            {
                return Assembly.GetTypes()
                    .SelectMany(x => x.GetMethods())
                    .Where(y => y.GetCustomAttributes(typeof(NnrEventAttribute), false).Length > 0)
                    .Count();
            }
        }

        public ModuleContext(AssemblyLoadContext assemblyContext, NnrModule module)
        {
            _assemblyContext = assemblyContext;

            Module = module;
            CodeName = module.ModuleCodeName;
        }

        public void LoadModuleDependencies(IServiceCollection serviceCollection)
            => Module.RegisterToDependencyInjection(serviceCollection);

        public void LoadModuleCommands(InteractionService interactionService, IServiceProvider serviceProvider)
        {
            interactionService.AddModulesAsync(Assembly, serviceProvider);
        }

        public void UnloadModule(IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            Module.UnloadCommands(interactionService);
            Module.UnloadEvents(serviceProvider);
            Module.UnregisterFromDependencyInjection(serviceCollection);

            _assemblyContext.Unload();
        }
    }
}
