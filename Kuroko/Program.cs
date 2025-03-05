using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko.Shared;
using Microsoft.Extensions.DependencyInjection;

DataDirectories.CheckDirectories();

DiscordShardedClient client = new(new DiscordSocketConfig()
{
    AlwaysDownloadUsers = true,
    DefaultRetryMode = RetryMode.AlwaysRetry,
    LogLevel = LogSeverity.Info,
    MaxWaitBetweenGuildAvailablesBeforeReady = 1000,
    GatewayIntents = GatewayIntents.Guilds |
                     GatewayIntents.GuildMembers |
                     GatewayIntents.GuildMessages |
                     GatewayIntents.MessageContent |
                     GatewayIntents.GuildBans,
    MessageCacheSize = 1000
});
InteractionService interactionService = new(client, new InteractionServiceConfig()
{
    DefaultRunMode = RunMode.Sync,
    LogLevel = LogSeverity.Info,
    UseCompiledLambda = true
});
IServiceCollection serviceCollection = new ServiceCollection();

