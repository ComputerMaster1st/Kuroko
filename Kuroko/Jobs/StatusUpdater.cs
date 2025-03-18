using Discord;
using Discord.WebSocket;
using FluentScheduler;
using Kuroko.Attributes;
using Kuroko.Services;

namespace Kuroko.Jobs;

[PreInitialize, KurokoJob]
public class StatusUpdater(DiscordShardedClient client, PatreonService patreonService) : IJob, IScheduleJob
{
    private const string NAME = "Status Updater";

    private int _previousServerCount = 0;
    private int _previousPatronCount = 0;

    public void Execute()
        => ExecuteAsync().GetAwaiter();

    public void ScheduleJob(Registry registry)
        => registry.Schedule(this)
            .WithName(NAME)
            .ToRunEvery(30)
            .Minutes();
        
    private async Task ExecuteAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, 
            $"{NAME}: Job started at {DateTimeOffset.UtcNow}"));

        var currentServerCount = client.Guilds.Count;
        var currentPatronCount = await patreonService.CountMembersAsync();

        if (currentServerCount == _previousServerCount || currentPatronCount == _previousPatronCount)
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> No status updates."));
            return;
        }

        _previousServerCount = currentServerCount;
        _previousPatronCount = currentPatronCount;

        await client.SetGameAsync($"Prefix \"/\" {Utilities.SepChar} Guilds: {currentServerCount} {
            Utilities.SepChar} Patrons: {currentPatronCount}");
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
            $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> Status Updated!"));
    }
}