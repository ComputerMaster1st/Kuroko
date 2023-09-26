using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Modules.Utilities
{
    // TODO: Example slash command. Use KurokoModuleBase as it already includes the custom interaction context.

    public class Ping : KurokoModuleBase
    {
        [SlashCommand("ping", "Ping discord latency")]
        public Task ExecuteAsync()
        {
            return RespondAsync($"Latency: {Context.ServiceProvider.GetService<DiscordShardedClient>().Latency}ms");
        }
    }
}
