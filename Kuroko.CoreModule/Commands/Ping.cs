using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.MDK;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.CoreModule.Commands
{
    [Group("testgroup", "Just some random test group module")]
    public class Ping : KurokoInteractionModuleBase
    {
        [SlashCommand("ping", "Just some ping command")]
        public Task ExecuteAsync()
        {
            return RespondAsync("Pong!");
        }

        [Group("subtestgroup", "More random testing")]
        public class PingMore : KurokoInteractionModuleBase
        {
            [SlashCommand("ping", "Ping discord latency")]
            public Task ExecuteAsync()
            {
                return RespondAsync($"Latency: {Context.ServiceProvider.GetService<DiscordShardedClient>().Latency}ms");
            }
        }
    }
}
