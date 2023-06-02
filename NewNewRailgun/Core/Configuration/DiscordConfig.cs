using Newtonsoft.Json;
using NNR.MDK;
using NNR.MDK.Configuration;

namespace NewNewRailgun.Core.Configuration
{
    internal class NnrDiscordConfig : BaseConfig, IConfig
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

        public static async Task<NnrDiscordConfig> LoadAsync()
        {
            return await BaseLoadAsync<NnrDiscordConfig>(FILEPATH) as NnrDiscordConfig;
        }

        public Task SaveAsync()
            => BaseSaveAsync(FILEPATH, this);
    }
}
