using Discord;
using Microsoft.Extensions.DependencyInjection;
using NewNewRailgun.Core.Attributes;
using System.Collections;

namespace NewNewRailgun
{
    public class Utilities
    {
        private static readonly string LOG_DIRECTORY = $"{VirtualVolume.VIRT_DATA_DIRECTORY}/logs";
        private static readonly SemaphoreSlim _logLock = new(1);

        public const string SeparatorCharacter = "⬤";

        public static async Task WriteLogAsync(LogMessage message)
        {
            await _logLock.WaitAsync();

            if (!Directory.Exists(LOG_DIRECTORY))
                Directory.CreateDirectory(LOG_DIRECTORY);

            try
            {
                await File.AppendAllTextAsync(string.Format("{0}/{1}.log",
                    LOG_DIRECTORY,
                    DateTime.Today.ToString("yyyy_MM_dd")),
                    message + Environment.NewLine);
            }
            finally
            {
                _logLock.Release();
            }

            Console.WriteLine(message);
        }

        public static void PreloadServices(IEnumerable collection, IServiceProvider services)
        {
            foreach (ServiceDescriptor service in collection)
            {
                if (service.ServiceType.GetCustomAttributes(typeof(PreInitialize), false) is null)
                    continue;

                if (service.ImplementationType is null)
                    continue;

                services.GetService(service.ImplementationType);
            }
        }
    }
}
