using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Commands.Utilities;

public class Ping : KurokoCommandBase
{
    [SlashCommand("ping", "Bot latency")]
    public Task ExecuteAsync()
        => RespondAsync($"Gateway Latency: {Context.ServiceProvider.GetRequiredService<DiscordShardedClient>().Latency} ms");
}