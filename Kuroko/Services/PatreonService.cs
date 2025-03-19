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
        var members = new Dictionary<Member, MemberRelationships>();
        var incomingMembers = await _client.GetCampaignMembersAsync(config.PatreonCampaignId);
        if (incomingMembers == null) return members;

        await foreach (var member in incomingMembers)
            members.Add(member, member.Relationships);

        return members;
    }

    public async Task<int> CountMembersAsync()
        => (await GetMembershipsAsync())
            .Count(x => x.Key.PatronStatus == Member.PatronStatusValue.ActivePatron);

    public async Task<Member> GetMemberAsync(ulong discordUserId)
    {
        var memberships = await GetMembershipsAsync();

        return memberships.FirstOrDefault(x => 
            x.Value.User.SocialConnections.Discord.UserId == discordUserId).Key;
    }

    private void RefreshClient()
    {
        _client = new PatreonClient(config.PatreonOAuthToken.AccessToken,
            config.PatreonOAuthToken.RefreshToken,
            config.PatreonClientId);
        _client.TokensRefreshedAsync += OnTokensRefreshedAsync;
    }

    private async Task OnTokensRefreshedAsync(OAuthToken token)
    {
        config.PatreonOAuthToken = token;
        await config.SaveAsync();
        
        await Utilities.WriteLogAsync(new LogMessage(LogSeverity.Info, LogHeader.PATREON, 
            "OAuth Token Refreshed"));
        
        _client.Dispose();
        RefreshClient();
    }
}