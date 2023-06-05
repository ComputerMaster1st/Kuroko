using Discord.Interactions;
using Discord.WebSocket;

namespace Kuroko.CoreModule.Commands
{
    [Group("testgroup", "Just some random test group module")]
    public class Ping : InteractionModuleBase
    {
        [SlashCommand("ping", "Just some ping command")]
        public Task ExecuteAsync()
        {
            return RespondAsync("Pong!");
        }

        [Group("subtestgroup", "More random testing")]
        public class PingMore : InteractionModuleBase
        {
            private readonly DiscordShardedClient _client;

            public PingMore(DiscordShardedClient client)
                => _client = client;

            [SlashCommand("ping", "Ping discord latency")]
            public Task ExecuteAsync()
            {
                return RespondAsync($"Latency: {_client.Latency}ms");
            }
        }
    }
}
