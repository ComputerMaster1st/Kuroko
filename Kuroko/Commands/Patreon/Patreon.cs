using System.Text;
using Discord.Interactions;
using Kuroko.Database.UserEntities;
using Kuroko.Services;

namespace Kuroko.Commands.Patreon;

public class Patreon(PatreonService patreonService) : KurokoCommandBase
{
    // redeem command
    // revoke command
    // Check Key command

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
                membership.PatronStatus)}")
            .AppendLine($"* **Premium keys:** {properties.PremiumKeys.Count}/{
                (properties.KeysAllowed == -1 ? "Unlimited" : properties.KeysAllowed)}")
            .AppendLine($"* **Pledged Since:** {membership?.PledgeRelationshipStart?
                .ToString("dddd d, MMMM yy")}")
            .AppendLine("### Patreon Payments")
            .AppendLine($"* **Next Charge Date:** {membership?.NextChargeDate?.ToString("dddd d, MMMM yy")}")
            .AppendLine($"* **Next Payment Amount:** {(membership is null ? "$0.00" : 
                membership.WillPayAmountCents / 100.00)}");
        
        await RespondAsync(output.ToString(), ephemeral: true);
    }
}