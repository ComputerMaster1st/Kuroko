using Discord;
using Kuroko.Attributes;
using Kuroko.Shared;
using Patreon.Net;
using Patreon.Net.Models;

namespace Kuroko.Services;

[PreInitialize, KurokoDependency]
public class PatreonService(KurokoConfig config) : IKurokoService
{
    private PatreonClient _client = null;
    private DateTimeOffset _lastChecked = DateTimeOffset.UtcNow;
    private Dictionary<Member, MemberRelationships> _downloadedMembers = new();
    
    public int StartingPatronCount { get; private set; } = 0;

    public async Task StartServiceAsync()
    {
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.PATREON, "Starting service..."));
        RefreshClient();
        StartingPatronCount = await CountMembersAsync();
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.PATREON, 
            $"Found {StartingPatronCount} Patrons"));
    }

    public async Task<IDictionary<Member, MemberRelationships>> GetMembershipsAsync()
    {
        if (_downloadedMembers.Count != 0 && _lastChecked.AddMinutes(15) < DateTimeOffset.UtcNow)
            return _downloadedMembers;
        
        var members = new Dictionary<Member, MemberRelationships>();
        var incomingMembers = 
            await _client.GetCampaignMembersAsync(config.PatreonCampaignId, 
                Includes.Tiers | Includes.User | Includes.CurrentlyEntitledTiers | 
                Includes.Memberships);
        if (incomingMembers == null) return members;

        await foreach (var member in incomingMembers)
            members.Add(member, member.Relationships);
        
        _downloadedMembers = members;
        _lastChecked = DateTimeOffset.UtcNow;
        
        return _downloadedMembers;
    }

    public async Task<int> CountMembersAsync()
        => (await GetMembershipsAsync())
            .Count(x => x.Key.PatronStatus == Member.PatronStatusValue.ActivePatron);

    public async Task<Member> GetMemberAsync(ulong discordUserId)
    {
        var memberships = await GetMembershipsAsync();
        
        if (memberships.All(x => 
                x.Value.User.SocialConnections.Discord?.UserId != discordUserId))
            return null;
        return memberships.First(x => 
            x.Value.User.SocialConnections.Discord.UserId == discordUserId).Key;
    }

    private void RefreshClient()
    {
        _client = new PatreonClient(config.PatreonOAuthToken.AccessToken,
            config.PatreonOAuthToken.RefreshToken,
            config.PatreonClientId, config.PatreonOAuthLastUpdated
                .AddSeconds(config.PatreonOAuthToken.ExpiresIn));
        _client.TokensRefreshedAsync += OnTokensRefreshedAsync;
    }

    private async Task OnTokensRefreshedAsync(OAuthToken token)
    {
        config.PatreonOAuthToken = token;
        config.PatreonOAuthLastUpdated = DateTimeOffset.UtcNow;
        await config.SaveAsync();
        
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.PATREON, 
            "OAuth Token Refreshed"));
    }
}