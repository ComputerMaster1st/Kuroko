﻿using Discord;
using NewNewRailgun.Core;
using NNR.MDK;
using System.Reflection;

namespace NewNewRailgun
{
    internal class ModuleLoader
    {
        private readonly List<Assembly> _moduleAssemblies = new();
        private readonly List<INnrModule> _modules = new();

        public async Task<int> ScanForModulesAsync()
        {

            foreach (var dll in Directory.GetFiles(DataDirectories.MODULES))
            {
                var moduleDll = Assembly.LoadFile(dll);

                var moduleSetupType = moduleDll.GetTypes()
                    .Where(typeof(INnrModule).IsAssignableFrom)
                    .FirstOrDefault();

                if (moduleSetupType is null)
                {
                    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Failed to load: {moduleDll.FullName}! Missing \"INnrModule\". Contact Module Developer!"));
                    continue;
                }

                var module = Activator.CreateInstance(moduleSetupType) as INnrModule;

                _modules.Add(module);
                _moduleAssemblies.Add(moduleDll);

                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Found: {module.ModuleName}"));
            }

            return _moduleAssemblies.Count;
        }
    }
}
