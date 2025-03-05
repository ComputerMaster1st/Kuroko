using Newtonsoft.Json;

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
    public static string Hostname { get; private set; } = "localhost";
    [JsonProperty]
    public static string Database { get; private set; } = "Kuroko";
    [JsonProperty]
    public static string Username { get; private set; } = "username";
    [JsonProperty]
    public static string Password { get; private set; } = "password";
    [JsonProperty]
    public static int Port { get; private set; } = 5432;
    
    // Connection String
    [JsonIgnore]
    public string ConnectionString = $"Server={Hostname};Port={Port};Database={Database};UserId={Username}Password={Password}";

    public static async Task<KurokoConfig> LoadAsync()
    {
        if (File.Exists(FILEPATH))
            return JsonConvert.DeserializeObject<KurokoConfig>(await File.ReadAllTextAsync(FILEPATH));
        
        var config = new KurokoConfig();
        await config.SaveAsync();

        return config;
    }
    
    public Task SaveAsync()
        => File.WriteAllTextAsync(FILEPATH, JsonConvert.SerializeObject(this, Formatting.Indented));
}