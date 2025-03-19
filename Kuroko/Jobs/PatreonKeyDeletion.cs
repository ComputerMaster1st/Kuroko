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
public class PatreonKeyDeletion(PatreonService patreonService, IServiceProvider services,
    KurokoConfig config) : IJob, IScheduleJob
{
    private const string NAME = "Patreon: Premium Key Expired";
    
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
        
        var keysDeleted = 0;
        await using var database = services.GetRequiredService<DatabaseContext>();
        var premiumKeys = await database.PremiumKeys
            .Include(premiumKey => premiumKey.PatreonProperties).ToListAsync();
        
        foreach (var key in premiumKeys.Where(key => key.ExpiresAt.AddDays(7) < DateTimeOffset.UtcNow))
        {
            database.PremiumKeys.Remove(key);
            keysDeleted++;
        }
        
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, 
            $"{NAME}: Job finished at {DateTimeOffset.UtcNow}. {keysDeleted} Keys Deleted."));
    }
}