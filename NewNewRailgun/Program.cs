using Discord;
using NewNewRailgun.Core.Configuration;
using NNR.MDK;

await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "Now Starting New New Railgun. Please wait a few minutes to boot the operating system..."));

var discordConfig = await NnrDiscordConfig.LoadAsync();

if (discordConfig is null)
{
    await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.SYSTEM, "New \"nnr_discord_config.json\" file has been generated! Please fill this in before restarting the bot!"));
    return;
}