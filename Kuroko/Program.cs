using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Kuroko;
using Kuroko.Core;
using Kuroko.Core.Configuration;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static NnrDiscordConfig _discordConfig = null;
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
        var config = await NnrDiscordConfig.LoadAsync();

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

    private static async Task UnloadModules()
    {

    }

    private static async Task StartAndWaitConsoleAsync()
    {
        while (true)
        {

        }
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