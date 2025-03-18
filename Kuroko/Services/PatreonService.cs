using Kuroko.Attributes;
using Kuroko.Shared;
using Patreon.Net;
using Patreon.Net.Models;

namespace Kuroko.Services;

[PreInitialize]
public class PatreonService(KurokoConfig config) : IKurokoService
{
    private PatreonClient _client = null;

    public Task StartServiceAsync()
    {
        RefreshClient();
        return Task.CompletedTask;
    }

    public Task<PatreonResourceArray<Member, MemberRelationships>> GetMembershipsAsync()
        => _client.GetCampaignMembersAsync(config.PatreonCampaignId);

    public async Task<Member> GetMemberAsync(ulong discordUserId)
    {
        var memberships = await GetMembershipsAsync();

        return await memberships.FirstOrDefaultAsync(x => 
            x.Relationships.User.SocialConnections.Discord.UserId == discordUserId);
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
        
        _client.Dispose();
        RefreshClient();
    }
}