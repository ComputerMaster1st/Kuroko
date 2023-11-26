using Discord;
using Discord.WebSocket;
using FluentScheduler;
using Kuroko.Core;
using Kuroko.Core.Attributes;

namespace Kuroko.Jobs
{
    [PreInitialize, ScheduledJob]
    public class StatusUpdate : IJob, IScheduleJob
    {
        private readonly DiscordShardedClient _client;

        private const string NAME = "Status Updater";

        public int PreviousServerCount { get; set; } = 0;

        public StatusUpdate(DiscordShardedClient client)
            => _client = client;

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

            var currentCount = _client.Guilds.Count;

            if (currentCount == PreviousServerCount)
            {
                await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                    $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> No change in guild count."));
                return;
            }

            PreviousServerCount = currentCount;

            await _client.SetGameAsync($"Prefix: \"/\" {Utilities.SeparatorCharacter} Servers: {currentCount}");
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                    $"{NAME}: Job finished at {DateTimeOffset.UtcNow} <> Guild count updated to {currentCount}!"));
        }
    }
}