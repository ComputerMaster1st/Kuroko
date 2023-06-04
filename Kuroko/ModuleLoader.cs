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

            _modules.Add(moduleContext);
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
                module.UnloadModule(serviceCollection, serviceProvider, interactionService);

            _modules.Clear();
        }

        public bool UnloadModule(string codeName, IServiceCollection serviceCollection, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            var module = _modules.FirstOrDefault(x => x.CodeName == codeName);

            if (module is null)
                return false;

            module.UnloadModule(serviceCollection, serviceProvider, interactionService);
            _modules.Remove(module);

            return true;
        }
    }
}
