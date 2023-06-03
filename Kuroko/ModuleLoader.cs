using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.MDK;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Loader;

namespace Kuroko
{
    internal class ModuleLoader
    {
        private readonly List<ModuleContext> _modules = new();

        public async Task<int> ScanForModulesAsync()
        {
            foreach (var dll in Directory.GetFiles(DataDirectories.MODULES))
            {
                var moduleAssemblyContext = new AssemblyLoadContext(null, true);
                moduleAssemblyContext.LoadFromStream(File.OpenRead(dll));

                var moduleAssembly = moduleAssemblyContext.Assemblies.First();
                var moduleSetupType = moduleAssembly.GetTypes()
                    .Where(typeof(KurokoModule).IsAssignableFrom)
                    .FirstOrDefault();

                if (moduleSetupType is null)
                {
                    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Failed to load: {moduleAssembly.FullName}! Missing \"KurokoModule\". Contact Module Developer!"));

                    moduleAssemblyContext.Unload();
                    continue;
                }

                var module = Activator.CreateInstance(moduleSetupType) as KurokoModule;
                var moduleContext = new ModuleContext(moduleAssemblyContext, module);

                _modules.Add(moduleContext);

                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Found: {moduleContext.CodeName}"));
            }

            return _modules.Count;
        }

        public void RegisterModuleDependencies(IServiceCollection serviceCollection)
        {
            foreach (var module in _modules)
                module.LoadModuleDependencies(serviceCollection);
        }

        public void RegisterModuleCommands(InteractionService interactionService, IServiceProvider serviceProvider)
        {
            foreach (var module in _modules)
                module.LoadModuleCommands(interactionService, serviceProvider);
        }

        public int CountEventsLoaded()
        {
            var count = 0;

            foreach (var module in _modules)
                count += module.EventCount;

            return count;
        }

        public void UnloadModules(IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            foreach (var module in _modules)
            {
                module.UnloadModule(serviceCollection, serviceProvider, interactionService);
            }

            _modules.Clear();
        }
    }
}
