using Discord;
using FluentScheduler;
using Kuroko.Attributes;
using Kuroko.Database;
using Kuroko.Services;
using Kuroko.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Kuroko.Jobs;

[PreInitialize, KurokoJob]
public class PatreonMemberSync(PatreonService patreonService, IServiceProvider services,
    KurokoConfig config) : IJob, IScheduleJob
{
    private const string NAME = "Patreon: Membership Sync";
    
    public void Execute()
        => Task.Factory.StartNew(ExecuteAsync).GetAwaiter();

    public void ScheduleJob(Registry registry)
        => registry.Schedule(this)
            .WithName(NAME)
            .ToRunEvery(15)
            .Minutes();

    private async Task ExecuteAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
            $"{NAME}: Job started at {DateTimeOffset.UtcNow}"));

        var members = await patreonService.GetMembershipsAsync();
        if (members.Count < 1 || config.PatreonTiers.Count == 0)
        {
            await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS,
                $"{NAME}: Job finished at {DateTimeOffset.UtcNow}. Nothing to do."));
            return;
        }

        var memberCount = 0;
        await using var database = services.GetRequiredService<DatabaseContext>();
        
        foreach (var member in members)
        {
            var tiers = member.Value.Tiers;
            if (tiers.Length < 1)
                continue;
            
            var memberDiscord = member.Value.User.SocialConnections.Discord;
            if (memberDiscord.UserId == 0)
                continue;

            if (!config.PatreonTiers.TryGetValue(tiers[0].Title, out var keys)) 
                continue;

            var user = await database.Users.GetOrCreateDataAsync(memberDiscord.UserId);
            user.Patreon.KeysAllowed = keys;
            memberCount++;
        }

        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.JOBS, 
            $"{NAME}: Job finished at {DateTimeOffset.UtcNow}. Synced {memberCount} patrons."));
    }
}