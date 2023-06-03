using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko;
using Kuroko.Core;
using Kuroko.Core.Configuration;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

internal class Program
{
    private static KDiscordConfig _discordConfig = null;
    private static IServiceProvider _serviceProvider = null;

    private static readonly DiscordShardedClient _discordClient = new(new DiscordSocketConfig()
    {
        AlwaysDownloadUsers = true,
        DefaultRetryMode = RetryMode.AlwaysRetry,
        LogLevel = LogSeverity.Info,
        MaxWaitBetweenGuildAvailablesBeforeReady = 1000,
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
    });
    private static readonly InteractionService _interactionService = new(_discordClient, new()
    {
        DefaultRunMode = RunMode.Sync,
        LogLevel = LogSeverity.Info,
        UseCompiledLambda = true
    });
    private static readonly ModuleLoader _moduleLoader = new();

    private static readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private static async Task CheckDiscordConfig()
    {
        var config = await KDiscordConfig.LoadAsync();

        if (config is null)
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "New \"kuroko_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
            return;
        }

        _discordConfig = config;
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Mounted \"kuroko_discord_config.json\"!"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
    }

    private static async Task LoadModules()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, "Checking modules directory..."));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

        int moduleCount = await _moduleLoader.ScanForModulesAsync();

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"{moduleCount} modules found!"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

        _moduleLoader.RegisterModuleDependencies(_serviceCollection);

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"Loaded {_serviceCollection.Count - 3} dependencies from modules!"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

        _serviceProvider = _serviceCollection.BuildServiceProvider();
        _moduleLoader.RegisterModuleCommands(_interactionService, _serviceProvider);

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"EVENTS             : {_moduleLoader.CountEventsLoaded()}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"SLASH COMMANDS     : {_interactionService.SlashCommands.Count}/100"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"MODAL COMMANDS     : {_interactionService.ModalCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"COMPONENT COMMANDS : {_interactionService.ComponentCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"TEXT COMMANDS      : {_interactionService.ContextCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Initializing services..."));

        foreach (ServiceDescriptor service in _serviceCollection)
        {
            if (service.ServiceType.GetCustomAttributes(typeof(PreInitializeAttribute), false) is null)
                continue;

            if (service.ImplementationType is null)
                continue;

            _serviceProvider.GetService(service.ImplementationType);
        }
    }

    private static async Task StartAndWaitConsoleAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "For console commands, type \"help\" or press \"return\" key!"));

        bool shutdownNow = false;
        while (!shutdownNow)
        {
            var input = Console.ReadLine();
            switch (input ?? string.Empty)
            {
                case "discordStats":
                    Console.WriteLine(new StringBuilder()
                        .AppendFormat("Shard Count  : {0}", _discordClient.Shards.Count).AppendLine()
                        .AppendFormat("Guild Count  : {0}", _discordClient.Guilds.Count).AppendLine()
                        .AppendFormat("Latency (ms) : {0}", _discordClient.Latency).AppendLine()
                        .ToString());
                    break;
                case "reloadModules":
                    Console.WriteLine("Unloading Modules... ");

                    _moduleLoader.UnloadModules(_serviceCollection, _serviceProvider, _interactionService);

                    Console.WriteLine("Unloading completed! Restarting Module Loader...");

                    await LoadModules();
                    break;
                case string s when s.StartsWith("addModule"):
                    Console.WriteLine(new StringBuilder()
                        .AppendLine("Not Yet Implemented!")
                        .ToString());
                    break;
                case string s when s.StartsWith("removeModule"):
                    Console.WriteLine(new StringBuilder()
                        .AppendLine("Not Yet Implemented!")
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
                        .AppendLine("help          - Show all available commands")
                        .AppendLine("discordStats  - Show discord shard, guild & latency")
                        .AppendLine("reloadModules - Reload all modules")
                        .AppendLine("addModule     - Install a module")
                        .AppendLine("removeModule  - Remove a module")
                        .AppendLine("shutdown      - Stop & shutdown")
                        .ToString());
                    break;
                default:
                    Console.WriteLine("Unknown console command!");
                    break;
            };
        }

        //await _discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
        //await _discordClient.SetGameAsync("Shutting down...");
        //await _discordClient.StopAsync();
        //await _discordClient.LogoutAsync();

        _moduleLoader.UnloadModules(_serviceCollection, _serviceProvider, _interactionService);
        _interactionService.Dispose();
        _discordClient.Dispose();

        Console.WriteLine("Shutdown completed! Goodbye~!");
        Environment.Exit(0);
    }

    private static async Task Main()
    {
        DataDirectories.CreateDirectories();

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Now Starting Kuroko. Please wait a few minutes to boot the operating system..."));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

        await CheckDiscordConfig();

        _serviceCollection.AddSingleton(_discordConfig)
            .AddSingleton(_discordClient)
            .AddSingleton(_interactionService);

        await LoadModules();

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Startup completed!"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));

        //await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Beginning connection to Discord..."));

        //await discordClient.LoginAsync(TokenType.Bot, discordConfig.Token);
        //await discordClient.StartAsync();
        //await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
        //await discordClient.SetGameAsync("Booting...");

        await StartAndWaitConsoleAsync();
    }
}