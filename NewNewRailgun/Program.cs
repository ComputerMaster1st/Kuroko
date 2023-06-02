using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NewNewRailgun;
using NewNewRailgun.Core;
using NewNewRailgun.Core.Configuration;
using NNR.MDK;

#region Load Configurations

DataDirectories.CreateDirectories();

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Now Starting New New Railgun. Please wait a few minutes to boot the operating system..."));

NnrDiscordConfig discordConfig = await NnrDiscordConfig.LoadAsync();

if (discordConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "New \"nnr_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
    return;
}

#endregion

#region Setup Core Services

DiscordShardedClient discordClient = new(new DiscordSocketConfig()
{
    AlwaysDownloadUsers = true,
    DefaultRetryMode = RetryMode.AlwaysRetry,
    LogLevel = LogSeverity.Info,
    MaxWaitBetweenGuildAvailablesBeforeReady = 1000,
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
});

InteractionService interactionService = new(discordClient, new()
{
    DefaultRunMode = RunMode.Sync,
    LogLevel = LogSeverity.Info,
    UseCompiledLambda = true
});

IServiceCollection serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton(discordConfig)
.AddSingleton(discordClient)
.AddSingleton(interactionService);

#endregion

#region Find & Load Modules

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, "Checking modules directory..."));

ModuleLoader moduleLoader = new();
int moduleCount = await moduleLoader.ScanForModulesAsync();
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"{moduleCount} modules found!"));

moduleLoader.RegisterModuleDependencies(serviceCollection);
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, "Loaded module dependencies!"));

#endregion

#region Finalize Loading

IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

moduleLoader.RegisterModuleCommands(interactionService, serviceProvider);
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, "Loaded module commands!"));

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Initializing services..."));
Utilities.PreloadServices(serviceCollection, serviceProvider);

#endregion

#region Start "New New Railgun"

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Setup Completed! Beginning connection to Discord..."));

await discordClient.LoginAsync(TokenType.Bot, discordConfig.Token);
await discordClient.StartAsync();
await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
await discordClient.SetGameAsync("Booting...");

await Task.Delay(Timeout.Infinite);

#endregion