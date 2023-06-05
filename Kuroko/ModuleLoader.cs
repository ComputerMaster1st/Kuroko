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
        public List<ModuleContext> Modules { get; } = new();

        private bool TryLoadModule(string filePath, out string moduleName)
        {
            var moduleAssemblyContext = new AssemblyLoadContext(null, true);
            moduleAssemblyContext.LoadFromStream(File.OpenRead(filePath));

            var moduleAssembly = moduleAssemblyContext.Assemblies.First();
            var moduleSetupType = moduleAssembly.GetTypes()
                .Where(typeof(KurokoModule).IsAssignableFrom)
                .FirstOrDefault();

            if (moduleSetupType is null)
            {
                moduleName = moduleAssembly.FullName;
                moduleAssemblyContext.Unload();
                return false;
            }

            var moduleContext = new ModuleContext(moduleAssemblyContext, Activator.CreateInstance(moduleSetupType) as KurokoModule);

            Modules.Add(moduleContext);
            moduleName = moduleContext.CodeName;

            return true;
        }

        public bool LoadModule(string fileName, out string moduleName)
        {
            if (!File.Exists(DataDirectories.MODULES + "/" + fileName))
            {
                moduleName = string.Empty;
                return false;
            }

            return TryLoadModule(DataDirectories.MODULES + "/" + fileName, out moduleName);
        }

        public async Task<int> ScanForModulesAsync()
        {
            foreach (var dll in Directory.GetFiles(DataDirectories.MODULES))
            {
                var moduleSuccess = TryLoadModule(dll, out string moduleName);

                if (!moduleSuccess)
                {
                    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Failed to load: {moduleName}! Missing \"KurokoModule\". Contact Module Developer!"));
                    continue;
                }

                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Found: {moduleName}"));
            }

            return Modules.Count;
        }

        public void RegisterModuleDependencies(IServiceCollection serviceCollection)
        {
            foreach (var module in Modules)
                module.LoadModuleDependencies(serviceCollection);
        }

        public async Task RegisterModuleCommandsAsync(InteractionService interactionService, IServiceProvider serviceProvider)
        {
            foreach (var module in Modules)
                await module.LoadModuleCommandsAsync(interactionService, serviceProvider);
        }

        public int CountEventsLoaded()
        {
            var count = 0;

            foreach (var module in Modules)
                count += module.EventCount;

            return count;
        }

        public async Task UnloadModulesAsync(IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            foreach (var module in Modules)
                await module.UnloadModuleAsync(serviceCollection, serviceProvider, interactionService);

            Modules.Clear();
        }

        public async Task<bool> UnloadModuleAsync(string codeName, IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            var module = Modules.FirstOrDefault(x => x.CodeName == codeName);

            if (module is null)
                return false;

            await module.UnloadModuleAsync(serviceCollection, serviceProvider, interactionService);

            Modules.Remove(module);

            return true;
        }
    }
}
