using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.AutoCompletes;

public class PatreonKeyRedeemAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        List<AutocompleteResult> results = [];
        var ctx = (KurokoInteractionContext)context;
        
        var properties = await ctx.Database.PatreonProperties
            .Include(patreonProperties => patreonProperties.PremiumKeys)
            .FirstOrDefaultAsync(x => x.RootId == ctx.User.Id);
        
        if (properties is null)
            return AutocompletionResult.FromSuccess();

        results.AddRange(from key in properties.PremiumKeys
            where key.GuildId == 0
            select new AutocompleteResult($"KEY: {key.Key} | (Expires: {
                key.ExpiresAt:dddd d, MMMM yy})", key.Id));
        
        if (properties.PremiumKeys.Count < properties.KeysAllowed || properties.KeysAllowed == -1)
            results.Add(new AutocompleteResult("Generate New Key!", -1));
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}