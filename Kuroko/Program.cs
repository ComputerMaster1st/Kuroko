using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko;
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

await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SYSTEM, 
        "Now Starting Kuroko! Please wait a few minutes..."
    ));
await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SYSTEM, 
        "--------------------------------"
    ));

var config = await KurokoConfig.LoadAsync();
if (config is null)
{
    await Utilities.WriteLogAsync(
        new LogMessage(
            LogSeverity.Info, 
            LogHeader.SYSTEM,
            $"New \"{KurokoConfig.FILEPATH}\" file has been generated! Please edit before restarting the bot!"));
    return;
}
    
await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SYSTEM,
        $"Mounted \"{KurokoConfig.FILEPATH}\"!"));
        
serviceCollection.AddSingleton(config)
    .AddSingleton(client)
    .AddSingleton(interactionService);
    
var currentAssembly = Assembly.GetExecutingAssembly();
IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();