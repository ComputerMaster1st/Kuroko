using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using NewNewRailgun.Core;
using NNR.MDK;
using System.Reflection;
using System.Runtime.Loader;

namespace NewNewRailgun
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
                    .Where(typeof(NnrModule).IsAssignableFrom)
                    .FirstOrDefault();

                if (moduleSetupType is null)
                {
                    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Failed to load: {moduleAssembly.FullName}! Missing \"INnrModule\". Contact Module Developer!"));

                    moduleAssemblyContext.Unload();
                    continue;
                }

                var module = Activator.CreateInstance(moduleSetupType) as NnrModule;
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
    }
}
