using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.CoreModule.Events;
using Kuroko.Database;
using Kuroko.Events;
using Kuroko.Events.ModLogEvents;
using Kuroko.Events.TicketEvents;
using Kuroko.Shared;
using Kuroko.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

DiscordShardedClient _discordClient = new(new DiscordSocketConfig()
{
    AlwaysDownloadUsers = true,
    DefaultRetryMode = RetryMode.AlwaysRetry,
    LogLevel = LogSeverity.Info,
    MaxWaitBetweenGuildAvailablesBeforeReady = 1000,
    MessageCacheSize = 1000,
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
});
InteractionService _interactionService = new(_discordClient, new()
{
    DefaultRunMode = RunMode.Sync,
    LogLevel = LogSeverity.Info,
    UseCompiledLambda = true
});
IServiceCollection _serviceCollection = new ServiceCollection();

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Now Starting Kuroko. Please wait a few minutes to boot the operating system..."));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "--------------------------------"));

DataDirectories.CreateDirectories();

KDiscordConfig _discordConfig = await KDiscordConfig.LoadAsync();
KDatabaseConfig _databaseConfig = await KDatabaseConfig.LoadAsync();
bool newConfig = false;

if (_discordConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "New \"kuroko_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
    newConfig = true;
}

if (_databaseConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "New \"kuroko_db_config.json\" file has been generated! Please fill this in before restarting the bot!"));
    newConfig = true;
}

if (newConfig)
    return;

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Mounted \"kuroko_discord_config.json\"!"));

#region DI: Core Modules

// TODO: !!! ONLY CORE SERVICES SHOULD BE ADDED HERE !!!

_serviceCollection.AddSingleton(_discordConfig)
    .AddSingleton(_discordClient)
    .AddSingleton(_interactionService);

#endregion

#region DI: Database

// TODO: !!! DO NOT TOUCH THIS AS THIS HANDLES DATABASE CONNECTION !!!

_serviceCollection.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(_databaseConfig.ConnectionUrl())
        .EnableSensitiveDataLogging()
        .UseLazyLoadingProxies();
}, ServiceLifetime.Transient);

#endregion

#region DI: Events

// TODO: Add any events that use '[PreInitialise]' attribute here.

#region Base Events

_serviceCollection.AddSingleton<DiscordLogEvent>()
    .AddSingleton<DiscordShardReadyEvent>()
    .AddSingleton<DiscordSlashCommandEvent>()
    .AddSingleton<UnobservedErrorEvent>();

#endregion

#region ModLog Events

_serviceCollection.AddSingleton<ModLogUserJoinLeaveEvent>()
    .AddSingleton<ModLogMessageEditedEvent>()
    .AddSingleton<ModLogMessageDeletedEvent>();

#endregion

#region Ticket Events

_serviceCollection.AddSingleton<TicketMessageNewEvent>()
    .AddSingleton<TicketMessageEditEvent>()
    .AddSingleton<TicketMessageDeleteEvent>();

#endregion

#endregion

IServiceProvider _serviceProvider = _serviceCollection.BuildServiceProvider();

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"Loaded {_serviceCollection.Count - 7} dependencies from modules!"));

await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"SLASH COMMANDS     : {_interactionService.SlashCommands.Count}/100"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"MODAL COMMANDS     : {_interactionService.ModalCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"COMPONENT COMMANDS : {_interactionService.ComponentCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"TEXT COMMANDS      : {_interactionService.ContextCommands.Count}"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Initializing services..."));

// TODO: Any custom attributes that require to be preloaded into dependency injection can be done here.

foreach (ServiceDescriptor service in _serviceCollection)
{
    if (service.ServiceType.GetCustomAttributes(typeof(PreInitializeAttribute), false) is null)
        continue;

    if (service.ImplementationType is null)
        continue;

    _serviceProvider.GetService(service.ImplementationType);
}

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Startup completed!"));
await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Beginning connection to Discord..."));

await _discordClient.LoginAsync(TokenType.Bot, _discordConfig.Token);
await _discordClient.StartAsync();
await _discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
await _discordClient.SetGameAsync("Booting...");

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "For console commands, type \"help\" or press \"return\" key!"));

bool shutdownNow = false;
while (!shutdownNow)
{
    var input = Console.ReadLine();
    switch (input ?? string.Empty)
    {
        case "discord-stats":
            Console.WriteLine(new StringBuilder()
                .AppendFormat("Shard Count  : {0}", _discordClient.Shards.Count).AppendLine()
                .AppendFormat("Guild Count  : {0}", _discordClient.Guilds.Count).AppendLine()
                .AppendFormat("Latency (ms) : {0}", _discordClient.Latency).AppendLine()
                .ToString());
            break;
        case "shutdown":
            Console.WriteLine(new StringBuilder()
                .AppendLine("Shutting down now...")
                .ToString());

            shutdownNow = true;
            break;
        case "":
        case "help":
            Console.WriteLine(new StringBuilder()
                .AppendLine("help           - Show all available commands")
                .AppendLine("discord-stats  - Show discord shard, guild & latency")
                .AppendLine("shutdown       - Stop & shutdown")
                .ToString());
            break;
        default:
            Console.WriteLine("Unknown console command!");
            break;
    };
}

_interactionService.Dispose();

await _discordClient.StopAsync();
await _discordClient.LogoutAsync();

_discordClient.Dispose();

Console.WriteLine("Shutdown completed! Goodbye~!");
Environment.Exit(0);
