using Discord;
using FluentScheduler;
using Kuroko.Attributes;
using Kuroko.Database;
using Kuroko.Services;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Jobs;

[PreInitialize, KurokoJob]
public class PatreonKeyRefresh(PatreonService patreonService, IServiceProvider services,
    KurokoConfig config) : IJob, IScheduleJob
{
    private const string NAME = "Patreon: Premium Key Refresh";
    
    public void Execute()
        => Task.Factory.StartNew(ExecuteAsync).GetAwaiter();

    public void ScheduleJob(Registry registry)
        => registry.Schedule(this)
            .WithName(NAME)
            .ToRunEvery(12)
            .Hours();

    private async Task ExecuteAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
            $"{NAME}: Job started at {DateTimeOffset.UtcNow}"));

        var keysRefreshed = 0;
        await using var database = services.GetRequiredService<DatabaseContext>();
        
        foreach (var properties in database.PatreonProperties
                     .Include(patreonProperties => patreonProperties.PremiumKeys))
        {
            if (properties.PremiumKeys.Count < 1)
                continue;

            if (properties.KeysAllowed == 0)
                continue;
            
            var cycle = 0;
            foreach (var key in properties.PremiumKeys.TakeWhile(key => 
                         cycle < properties.KeysAllowed || properties.KeysAllowed == -1))
            {
                cycle++;

                if (key.ExpiresAt >= DateTimeOffset.UtcNow)
                    continue;
            
                key.ExpiresAt = DateTimeOffset.UtcNow.AddMonths(1);
                keysRefreshed++;
            }
        }
        
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, 
            $"{NAME}: Job finished at {DateTimeOffset.UtcNow}. {keysRefreshed} Keys Refreshed."));
    }
}