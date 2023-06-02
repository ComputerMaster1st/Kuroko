using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko;
using Kuroko.Core;
using Kuroko.Core.Configuration;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;
using Microsoft.Extensions.DependencyInjection;

#region Load Configurations

DataDirectories.CreateDirectories();

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Now Starting Kuroko. Please wait a few minutes to boot the operating system..."));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

NnrDiscordConfig discordConfig = await NnrDiscordConfig.LoadAsync();

if (discordConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "New \"kuroko_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
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

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
ModuleLoader moduleLoader = new();
int moduleCount = await moduleLoader.ScanForModulesAsync();
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"{moduleCount} modules found!"));

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
moduleLoader.RegisterModuleDependencies(serviceCollection);
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Loaded {serviceCollection.Count - 3} dependencies from modules!"));

#endregion

#region Finalize Loading

IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

moduleLoader.RegisterModuleCommands(interactionService, serviceProvider);

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"EVENTS             : {moduleLoader.CountEventsLoaded()}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"SLASH COMMANDS     : {interactionService.SlashCommands.Count}/100"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"MODAL COMMANDS     : {interactionService.ModalCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"COMPONENT COMMANDS : {interactionService.ComponentCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"TEXT COMMANDS      : {interactionService.ContextCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Initializing services..."));

foreach (ServiceDescriptor service in serviceCollection)
{
    if (service.ServiceType.GetCustomAttributes(typeof(PreInitializeAttribute), false) is null)
        continue;

    if (service.ImplementationType is null)
        continue;

    serviceProvider.GetService(service.ImplementationType);
}

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Service initialization completed!"));

#endregion

//#region Start Kuroko

//await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Setup Completed! Beginning connection to Discord..."));

//await discordClient.LoginAsync(TokenType.Bot, discordConfig.Token);
//await discordClient.StartAsync();
//await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
//await discordClient.SetGameAsync("Booting...");

//await Task.Delay(Timeout.Infinite);

//#endregion