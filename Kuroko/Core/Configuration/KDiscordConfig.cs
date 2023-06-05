using Kuroko.MDK;
using Kuroko.MDK.Configuration;
using Newtonsoft.Json;

namespace Kuroko.Core.Configuration
{
    internal class KDiscordConfig : BaseConfig, IConfig
    {
        [JsonIgnore]
        private const string FILEPATH = $"{DataDirectories.CONFIG}/kuroko_discord_config.json";

        [JsonProperty]
        public string Token { get; private set; } = "TOKEN GOES HERE";

        [JsonProperty]
        public ulong BotOwnerId { get; private set; } = 0;

        [JsonProperty]
        public ulong MasterGuildId { get; private set; } = 0;

        [JsonProperty]
        public List<ulong> BotAdminUserIds { get; private set; } = new();

        public static async Task<KDiscordConfig> LoadAsync()
        {
            return await BaseLoadAsync<KDiscordConfig>(FILEPATH) as KDiscordConfig;
        }

        public Task SaveAsync()
            => BaseSaveAsync(FILEPATH, this);
    }
}
