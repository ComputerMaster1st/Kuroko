using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Core.Attributes;
using Kuroko.Jobs;
using Kuroko.Shared.Configuration;

namespace Kuroko.Events
{
    [PreInitialize, KurokoEvent]
    internal class DiscordShardReadyEvent
    {
        private readonly KDiscordConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interactionService;
        private readonly StatusUpdate _statusUpdate;

        public readonly List<int> _shardIds = new();

        public DiscordShardReadyEvent(KDiscordConfig config, DiscordShardedClient client, InteractionService interactionService, StatusUpdate statusUpdate)
        {
            _config = config;
            _client = client;
            _interactionService = interactionService;
            _statusUpdate = statusUpdate;

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

            var guildCount = _client.Guilds.Count;
            _statusUpdate.PreviousServerCount = guildCount;

            await _client.SetGameAsync($"Prefix: \"/\" {Utilities.SeparatorCharacter} Servers: {guildCount}");
            await _client.SetStatusAsync(UserStatus.Online);
        }
    }
}
