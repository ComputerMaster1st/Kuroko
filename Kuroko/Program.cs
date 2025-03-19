using System.Reflection;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FluentScheduler;
using Kuroko;
using Kuroko.Attributes;
using Kuroko.Database;
using Kuroko.Jobs;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

DataDirectories.CheckDirectories();

DiscordShardedClient client = new(new DiscordSocketConfig
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
InteractionService interactionService = new(client, new InteractionServiceConfig
{
    DefaultRunMode = RunMode.Sync,
    LogLevel = LogSeverity.Info,
    UseCompiledLambda = true
});
var serviceCollection = new ServiceCollection();
Registry registry = new();
JobManager.UseUtcTime();

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
    .AddSingleton(interactionService)
    .AddSingleton(registry)
    .AddDbContext<DatabaseContext>(options =>
    {
        options.UseNpgsql(config.ConnectionString())
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies();
    }, ServiceLifetime.Transient);
    
var currentAssembly = Assembly.GetExecutingAssembly();

foreach (var type in currentAssembly.GetTypes())
{
    if (type.GetCustomAttributes().Any(x => 
            x is KurokoJobAttribute or 
                KurokoEventAttribute or
                KurokoDependencyAttribute))
        serviceCollection.AddSingleton(type);
}

IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SYSTEM,
        $"Loaded {serviceCollection.Count} dependencies!"));
        
await interactionService.AddModulesAsync(currentAssembly, serviceProvider);

await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SLASHCMD,
        "Loaded following slash commands:"
    ));
Console.WriteLine(new StringBuilder()
    .Append($"SLASH COMMANDS     : {interactionService.SlashCommands.Count}/100").AppendLine()
    .Append($"MODAL COMMANDS     : {interactionService.ModalCommands.Count}").AppendLine()
    .Append($"COMPONENT COMMANDS : {interactionService.ComponentCommands.Count}").AppendLine()
    .Append($"TEXT COMMANDS      : {interactionService.ContextCommands.Count}").AppendLine()
    .AppendLine("Initializing services...").ToString());

foreach (var service in serviceCollection)
{
    if (service.ServiceType.GetCustomAttribute<PreInitializeAttribute>(false) is null)
        continue;

    if (service.ImplementationType is null)
        continue;
    
    var initialized = serviceProvider.GetService(service.ImplementationType);
    switch (initialized)
    {
        case IKurokoService kurokoService:
            await kurokoService.StartServiceAsync();
            break;
        case IJob job:
            var scheduleJob = job as IScheduleJob;
            scheduleJob?.ScheduleJob(registry);
            break;
    }
}
    
await Utilities.WriteLogAsync(
    new LogMessage(
        LogSeverity.Info, 
        LogHeader.SYSTEM, 
        "Startup completed! Beginning connection to Discord..."
    ));

await client.LoginAsync(TokenType.Bot, config.Token);
await client.StartAsync();
await client.SetStatusAsync(UserStatus.DoNotDisturb);
await client.SetGameAsync("Starting...");

await Task.Delay(-1);