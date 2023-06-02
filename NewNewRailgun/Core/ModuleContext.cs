using NNR.MDK;
using System.Reflection;
using System.Runtime.Loader;

namespace NewNewRailgun.Core
{
    internal class ModuleContext
    {
        private readonly AssemblyLoadContext _assemblyContext;

        public string CodeName { get; }

        public INnrModule Module { get; }

        public Assembly Assembly
        {
            get
            {
                return _assemblyContext.Assemblies.First();
            }
        }

        public ModuleContext(AssemblyLoadContext assemblyContext, INnrModule module)
        {
            _assemblyContext = assemblyContext;

            Module = module;
            CodeName = module.ModuleCodeName;
        }
    }
}
