using Discord;
using Discord.WebSocket;
using FluentScheduler;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Jobs
{
    [PreInitialize, ScheduledJob]
    public class MessagePruner : IJob, IScheduleJob
    {
        private readonly IServiceProvider _services;

        private const string NAME = "Blackbox Data Pruner";

        public MessagePruner(DiscordShardedClient client, IServiceProvider services)
        {
            _services = services;
        }

        public void Execute()
            => ExecuteAsync().GetAwaiter();

        public void ScheduleJob(Registry registry)
            => registry.Schedule(this)
                .WithName(NAME)
                .ToRunEvery(1)
                .Days()
                .At(6,0);

        public async Task ExecuteAsync()
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, $"{NAME}: Job started at {DateTimeOffset.UtcNow}"));

            var db = _services.GetRequiredService<DatabaseContext>();
            var entitiesToDelete = new List<MessageEntity>();
            
            await db.Messages.ForEachAsync(x => {
                if (x.CreatedAt < DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                    entitiesToDelete.Add(x);
            });

            if (entitiesToDelete.Any())
            {
                db.Messages.RemoveRange(entitiesToDelete);
                await db.SaveChangesAsync();
            }

            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                $"{NAME}: Job finished at {DateTimeOffset.UtcNow}. {entitiesToDelete.Count} entries removed"));
        }
    }
}