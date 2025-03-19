using Newtonsoft.Json;
using Patreon.Net.Models;

namespace Kuroko.Shared;

public class KurokoConfig
{
    [JsonIgnore]
    public const string FILEPATH = $"{DataDirectories.CONFIG}/KurokoConfig.json";
    
    // Discord Properties
    [JsonProperty]
    public string Token { get; private set; } = "Token Goes Here";
    [JsonProperty]
    public ulong OwnerId { get; private set; }
    [JsonProperty]
    public ulong MasterGuildId { get; private set; }
    [JsonProperty]
    public List<ulong> AdminUserIds { get; private set; } = [];
    
    // Database Properties
    [JsonProperty]
    public string Hostname { get; private set; } = "localhost";
    [JsonProperty]
    public string Database { get; private set; } = "Kuroko";
    [JsonProperty]
    public string Username { get; private set; } = "username";
    [JsonProperty]
    public string Password { get; private set; } = "password";
    [JsonProperty]
    public int Port { get; private set; } = 5432;
    
    // Patreon Properties
    [JsonProperty]
    public OAuthToken PatreonOAuthToken { get; set; } = new();
    [JsonProperty]
    public string PatreonClientId { get; private set; } = "PATREON_CLIENT_ID";
    [JsonProperty]
    public string PatreonCampaignId { get; private set; } = "PATREON_CAMPAIGN_ID";
    [JsonProperty]
    public Dictionary<string, int> PatreonTiers { get; private set; } = new()
        {{ "<UNK>", 1 }}; // Key is tier name, Value is keys given

    // Connection String
    public string ConnectionString()
        => $"Server={Hostname};Port={Port};Database={Database};UserId={Username};Password={Password};";

    public static async Task<KurokoConfig> LoadAsync()
    {
        if (File.Exists(FILEPATH))
            return JsonConvert.DeserializeObject<KurokoConfig>(await File.ReadAllTextAsync(FILEPATH));
        
        var config = new KurokoConfig();
        await config.SaveAsync();

        return null;
    }
    
    public Task SaveAsync()
        => File.WriteAllTextAsync(FILEPATH, JsonConvert.SerializeObject(this, Formatting.Indented));
}