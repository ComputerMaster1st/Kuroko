using Newtonsoft.Json;
using NNR.MDK;
using NNR.MDK.Configuration;

namespace NewNewRailgun.Core.Configuration
{
    internal class DiscordConfig : BaseConfig, IConfig
    {
        [JsonIgnore]
        private const string FILEPATH = $"{DataDirectories.CONFIG}/nnr_discord_config.json";

        [JsonProperty]
        public string Token { get; private set; } = "TOKEN GOES HERE";

        [JsonProperty]
        public ulong BotOwnerId { get; private set; } = 0;

        [JsonProperty]
        public ulong MasterGuildId { get; private set; } = 0;

        [JsonProperty]
        public List<ulong> BotAdminUserIds { get; private set; } = new();

        public static async Task<DiscordConfig> LoadAsync()
        {
            return await BaseLoadAsync<DiscordConfig>(FILEPATH) as DiscordConfig;
        }

        public Task SaveAsync()
            => BaseSaveAsync(FILEPATH, this);
    }
}
