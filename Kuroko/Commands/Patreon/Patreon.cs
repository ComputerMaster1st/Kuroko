using System.Text;
using Discord.Interactions;
using Kuroko.AutoCompletes;
using Kuroko.Database.UserEntities;
using Kuroko.Database.UserEntities.Extras;
using Kuroko.Services;

namespace Kuroko.Commands.Patreon;

public class Patreon(PatreonService patreonService) : KurokoCommandBase
{
    [SlashCommand("patreon-status", "Check your patron status")]
    public async Task StatusAsync()
    {
        var properties = await GetPropertiesAsync<PatreonProperties, UserEntity>(Context.User.Id);
        var membership = await patreonService.GetMemberAsync(properties.RootId);

        var output = new StringBuilder()
            .AppendLine("## Patreon Status")
            .AppendLine("_NOTE: Patreon memberships are synced every 15 minutes. If you are new and do not see any " +
                        "changes, please try this command again later. Also, please double-check to make sure your " +
                        "Discord account is linked to your Patreon account._")
            .AppendLine()
            .AppendLine($"* **Patron Status:** {(membership is null ? "No Membership Found" : 
                membership.PatronStatus)} {(properties.BotAdminEnabled ? "_(Bot Admin Mode Enabled!)_" : "")}")
            .AppendLine($"* **Premium keys:** {properties.PremiumKeys.Count}/{
                (properties.KeysAllowed == -1 ? "Unlimited" : properties.KeysAllowed)} {
                    (properties.BotAdminEnabled ? "_(Bot Admin Limit: 10)_" : "")}")
            .AppendLine($"* **Pledged Since:** {membership?.PledgeRelationshipStart?.ReadableDateTime()}")
            .AppendLine("### Patreon Payments")
            .AppendLine($"* **Next Charge Date:** {membership?.NextChargeDate?.ReadableDateTime()}")
            .AppendLine($"* **Next Payment Amount:** {(membership is null ? "$0.00" : 
                $"${membership.WillPayAmountCents / 100.00:0.00}")}");
        
        await RespondAsync(output.ToString(), ephemeral: true);
    }
    
    [SlashCommand("patreon-redeem", "Redeem a key on this server")]
    public async Task RedeemAsync([Autocomplete(typeof(PatreonKeyRedeemAutocomplete))] int keyId)
    {
        var properties = await GetPropertiesAsync<PatreonProperties, UserEntity>(Context.User.Id);
        
        PremiumKey key;
        
        switch (keyId)
        {
            case -2:
                properties.BotAdminEnabled = true;
                await RespondAsync("**CAUTION:** Bot Admin Bypass is now enabled!", ephemeral: true);
                return;
            case -3:
                properties.BotAdminEnabled = false;
                await RespondAsync("**CAUTION:** Bot Admin Bypass is now disabled!", ephemeral: true);
                return;
            case -1 when properties.KeysAllowed == -1 || 
                         (properties.KeysAllowed > 0 && properties.PremiumKeys.Count < properties.KeysAllowed)
                         || (properties.BotAdminEnabled && properties.PremiumKeys.Count < 10):
                key = new PremiumKey();
                properties.PremiumKeys.Add(key);
                break;
            default:
                key = properties.PremiumKeys.FirstOrDefault(p => p.Id == keyId);
                break;
        }

        if (key is null)
        {
            await RespondAsync("**ERROR:** Invalid Premium Key!", ephemeral: true);
            return;
        }
        if (key.GuildId == Context.Guild.Id)
        {
            await RespondAsync("**ERROR:** This server already has a premium key redeemed.", ephemeral: true);
            return;
        }
        
        key.GuildId = Context.Guild.Id;
        await RespondAsync("**SUCCESS:** Premium Key has been installed for this server! " +
                           "The server hamsters are dancing with joy!", ephemeral: true);
    }
    
    [SlashCommand("patreon-revoke", "Remove a key from a server to re-use somewhere else")]
    public async Task RevokeAsync([Autocomplete(typeof(PatreonKeyRevokeAutocomplete))] int keyId)
    {
        var properties = await GetPropertiesAsync<PatreonProperties, UserEntity>(Context.User.Id);
        var key = properties.PremiumKeys.FirstOrDefault(p => p.Id == keyId);
        
        if (key is null)
        {
            await RespondAsync("**ERROR:** Invalid Premium Key!", ephemeral: true);
            return;
        }
        
        key.GuildId = 0;
        await RespondAsync("**SAD NOISES:** The server hamsters are sad now. Maybe next time...", ephemeral: true);
    }
}