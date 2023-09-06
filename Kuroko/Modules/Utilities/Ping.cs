using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Modules.Utilities
{
    public class Ping : KurokoModuleBase
    {
        [SlashCommand("ping", "Ping discord latency")]
        public Task ExecuteAsync()
        {
            return RespondAsync($"Latency: {Context.ServiceProvider.GetService<DiscordShardedClient>().Latency}ms");
        }
    }
}
