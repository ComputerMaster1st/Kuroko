using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Attributes;
using Kuroko.Shared;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class DiscordShardReadyEvent
{
    private readonly DiscordShardedClient _client;
    private readonly InteractionService _interactions;
    private readonly KurokoConfig _config;

    private readonly List<int> _shardIds = [];

    public DiscordShardReadyEvent(DiscordShardedClient client, InteractionService interactions, KurokoConfig config)
    {
        _client = client;
        _interactions = interactions;
        _config = config;

        _client.ShardReady += ShardReadyEvent;
    }

    private async Task ShardReadyEvent(DiscordSocketClient shard)
    {
        if (!_shardIds.Contains(shard.ShardId))
            _shardIds.Add(shard.ShardId);
        
        if (_shardIds.Count < _client.Shards.Count)
            return;

#if DEBUG
        await _interactions.RegisterCommandsToGuildAsync(_config.MasterGuildId);
#else
        await _interactions.RegisterCommandsGloballyAsync();
#endif

        await _client.SetGameAsync(
            $"Prefix \"/\" {Utilities.SepChar} Guilds: {_client.Guilds.Count}");
        await _client.SetStatusAsync(UserStatus.Online);
    }
}