using Newtonsoft.Json;

namespace Kuroko.Shared.Configuration
{
    internal class KDatabaseConfig : BaseConfig, IConfig
    {
        [JsonIgnore]
        private const string FILEPATH = $"{DataDirectories.CONFIG}/kuroko_db_config.json";

        [JsonProperty]
        public string Hostname { get; private set; } = string.Empty;

        [JsonProperty]
        public string Database { get; private set; } = string.Empty;

        [JsonProperty]
        public string Username { get; private set; } = string.Empty;

        [JsonProperty]
        public string Password { get; private set; } = string.Empty;

        [JsonProperty]
        public int Port { get; private set; } = 5432;

        public string ConnectionUrl()
            => string.Format("Server={0};Port={1};Database{2};UserId={3};Password={4};",
                Hostname,
                Port,
                Database,
                Username,
                Password);

        public static async Task<KDatabaseConfig> LoadAsync()
        {
            return await BaseLoadAsync<KDatabaseConfig>(FILEPATH) as KDatabaseConfig;
        }

        public Task SaveAsync()
            => BaseSaveAsync(FILEPATH, this);
    }
}
