using Discord;
using Discord.WebSocket;
using FluentScheduler;
using Kuroko.Attributes;

namespace Kuroko.Jobs
{
    [PreInitialize, KurokoJob]
    public class StatusUpdater(DiscordShardedClient client) : IJob, IScheduleJob
    {
        private const string NAME = "Status Updater";

        public int PreviousServerCount { get; set; } = 0;

        public void Execute()
            => ExecuteAsync().GetAwaiter();

        public void ScheduleJob(Registry registry)
            => registry.Schedule(this)
                .WithName(NAME)
                .ToRunEvery(30)
                .Minutes();
        
        public async Task ExecuteAsync()
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, $"{NAME}: Job started at {DateTimeOffset.UtcNow}"));

            var currentCount = client.Guilds.Count;

            if (currentCount == PreviousServerCount)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                    $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> No change in guild count."));
                return;
            }

            PreviousServerCount = currentCount;

            await client.SetGameAsync($"Prefix \"/\" {Utilities.SepChar} Guilds: {client.Guilds.Count}");
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> Guild count updated to {currentCount}!"));
        }
    }
}