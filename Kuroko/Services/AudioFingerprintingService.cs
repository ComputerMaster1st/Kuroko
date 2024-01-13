using Kuroko.Audio.Fingerprinting;
using Kuroko.Core.Attributes;

namespace Kuroko.Services
{
    [PreInitialize, KurokoEvent]
    internal class AudioFingerprintingService
    {
        public AudioFingerprintingService(IServiceProvider services)
            => Fingerprinting.InitService(services);
    }
}
