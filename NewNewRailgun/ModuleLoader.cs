using Discord;
using Microsoft.Extensions.DependencyInjection;
using NNR.MDK;
using System.Reflection;

namespace NewNewRailgun
{
    internal class ModuleLoader
    {
        private readonly List<Assembly> _modules = new();

        public async Task<int> ScanForModulesAsync()
        {
            foreach (var dll in Directory.GetFiles(DataDirectories.MODULES))
            {
                var module = Assembly.LoadFile(dll);

                _modules.Add(module);

                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Found: " + module.FullName));
            }

            return _modules.Count;
        }


        public void LoadModuleEvents(IServiceCollection serviceCollection)
        {

        }
    }
}
