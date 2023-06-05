using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Core.Configuration;
using Kuroko.MDK.Attributes;

namespace Kuroko.Events
{
    [PreInitialize]
    internal class DiscordShardReadyEvent
    {
        private readonly KDiscordConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;

        public readonly List<int> _shardIds = new();

        public DiscordShardReadyEvent(KDiscordConfig config, DiscordShardedClient client, InteractionService interactionService)
        {
            _config = config;
            _client = client;
            _interactionService = interactionService;

            _client.ShardReady += ShardReadyEvent;
        }

        private async Task ShardReadyEvent(DiscordSocketClient shard)
        {
            if (!_shardIds.Contains(shard.ShardId))
                _shardIds.Add(shard.ShardId);

            if (_shardIds.Count < _client.Shards.Count)
                return;

#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(_config.MasterGuildId);
#else
            await _interactionService.RegisterCommandsGloballyAsync();
#endif

            await _client.SetGameAsync($"Prefix: \"/\" # Servers: {_client.Guilds.Count}");
            await _client.SetStatusAsync(UserStatus.Online);
        }
    }
}
