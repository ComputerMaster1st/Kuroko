using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NewNewRailgun.Core.Configuration;
using NNR.MDK;

#region Load Configurations

DataDirectories.CreateDirectories();

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Now Starting New New Railgun. Please wait a few minutes to boot the operating system..."));

NnrDiscordConfig discordConfig = await NnrDiscordConfig.LoadAsync();

if (discordConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "New \"nnr_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
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

ServiceCollection serviceCollection = new();

serviceCollection.AddSingleton(discordConfig)
.AddSingleton(discordClient)
.AddSingleton(interactionService);

#endregion

#region Find & Load Modules

int moduleCount = 0;

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Checking modules directory..."));


await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, $"{moduleCount} modules found!"));

#endregion

#region Start "New New Railgun"

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Setup Completed! Beginning connection to Discord..."));

await discordClient.LoginAsync(TokenType.Bot, discordConfig.Token);
await discordClient.StartAsync();
await discordClient.SetStatusAsync(UserStatus.DoNotDisturb);
await discordClient.SetGameAsync("Booting...");

await Task.Delay(Timeout.Infinite);

#endregion