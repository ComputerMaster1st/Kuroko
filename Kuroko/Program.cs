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

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "Initializing services..."));
        await PrintModuleStatusAsync();

        foreach (ServiceDescriptor service in _serviceCollection)
        {
            if (service.ServiceType.GetCustomAttributes(typeof(PreInitializeAttribute), false) is null)
                continue;

            if (service.ImplementationType is null)
                continue;

            _serviceProvider.GetService(service.ImplementationType);
        }
    }

    private static async Task PrintModuleStatusAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"EVENTS             : {_moduleLoader.CountEventsLoaded()}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"SLASH COMMANDS     : {_interactionService.SlashCommands.Count}/100"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"MODAL COMMANDS     : {_interactionService.ModalCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"COMPONENT COMMANDS : {_interactionService.ComponentCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.MODLOADER, $"TEXT COMMANDS      : {_interactionService.ContextCommands.Count}"));
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "--------------------------------"));
    }

    private static async Task StartAndWaitConsoleAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, CoreLogHeader.SYSTEM, "For console commands, type \"help\" or press \"return\" key!"));

        //await _discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
        //await _discordClient.SetGameAsync("Shutting down...");
        //await _discordClient.StopAsync();
        //await _discordClient.LogoutAsync();

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
                case "module-reload":
                    Console.WriteLine("Unloading Modules... ");

                    _moduleLoader.UnloadModules(_serviceCollection, _serviceProvider, _interactionService);

                    Console.WriteLine("Unloading completed! Restarting Module Loader...");

                    await LoadModules();
                    break;
                case string s when s.StartsWith("module-add"):
                    string[] addArgs = s.Split(new char[] { ' ' });

                    if (addArgs.Length < 2)
                    {
                        Console.WriteLine("Please specify the module to add by it's filename. For example, \"kuroko_core.dll\".");
                        break;
                    }

                    if (!_moduleLoader.LoadModule(addArgs[1], out string moduleName))
                    {
                        if (string.IsNullOrEmpty(moduleName))
                        {
                            Console.WriteLine("Unable to locate the module file. Please make sure its spelt correctly and is in the modules directory.");
                            break;
                        }

                        Console.WriteLine($"Failed to load: {moduleName}! Missing \"KurokoModule\". Contact Module Developer!");
                        break;
                    }

                    Console.WriteLine($"Module {moduleName} successfully loaded!");
                    await PrintModuleStatusAsync();
                    break;
                case string s when s.StartsWith("module-remove"):
                    string[] remArgs = s.Split(new char[] { ' ' });

                    if (remArgs.Length < 2)
                    {
                        Console.WriteLine("Please specify the module to remove by it's code name. For example, \"KUROKO_CORE\".");
                        break;
                    }

                    if (!_moduleLoader.UnloadModule(remArgs[1], _serviceCollection, _serviceProvider, _interactionService))
                    {
                        Console.WriteLine("Module not found. Make sure you've typed the codename correctly.");
                        break;
                    }

                    Console.WriteLine("Module unloaded! Reprinting module stats...");

                    await PrintModuleStatusAsync();
                    break;
                case "module-stats":
                    await PrintModuleStatusAsync();
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
                        .AppendLine("module-reload  - Reload all modules")
                        .AppendLine("module-add     - Install a module")
                        .AppendLine("module-remove  - Remove a module")
                        .AppendLine("module-stats   - Status on modules")
                        .AppendLine("shutdown       - Stop & shutdown")
                        .ToString());
                    break;
                default:
                    Console.WriteLine("Unknown console command!");
                    break;
            };
        }

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