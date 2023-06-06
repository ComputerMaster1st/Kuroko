using Discord.Interactions;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.Loader;

namespace Kuroko.Core
{
    internal class ModuleContext
    {
        private readonly AssemblyLoadContext _assemblyContext;

        public string CodeName { get; }

        public KurokoModule Module { get; }

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
                    .Where(y => y.GetCustomAttributes(typeof(KurokoEventAttribute), false).Length > 0)
                    .Count();
            }
        }

        public ModuleContext(AssemblyLoadContext assemblyContext, KurokoModule module)
        {
            _assemblyContext = assemblyContext;

            Module = module;
            CodeName = module.ModuleCodeName;
        }

        public void LoadModuleDependencies(IServiceCollection serviceCollection)
            => Module.RegisterToDependencyInjection(serviceCollection);

        public Task LoadModuleCommandsAsync(InteractionService interactionService, IServiceProvider serviceProvider)
            => interactionService.AddModulesAsync(Assembly, serviceProvider);

        public async Task UnloadModuleAsync(IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            await UnloadCommandAsync(Assembly.GetTypes(), interactionService);

            Module.UnloadEvents(serviceProvider);
            Module.UnregisterFromDependencyInjection(serviceCollection);

            _assemblyContext.Unload();
        }

        private async Task UnloadCommandAsync(Type[] types, InteractionService interactionService)
        {
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(KurokoInteractionModuleBase)))
                {
                    var success = await interactionService.RemoveModuleAsync(type);
                    Console.WriteLine($"UNLOADED {success} {type.FullName}");
                }
            }
        }
    }
}
