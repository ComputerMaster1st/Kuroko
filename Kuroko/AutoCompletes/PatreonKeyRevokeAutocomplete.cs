using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;

namespace Kuroko.AutoCompletes;

public class PatreonKeyRevokeAutocomplete : AutocompleteHandler
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

        foreach (var key in properties.PremiumKeys.Where(key => key.GuildId != 0))
        {
            var guild = await context.Client.GetGuildAsync(key.GuildId);
            if (guild is null)
            {
                key.GuildId = 0;
                continue;
            }
            
            results.Add(new AutocompleteResult($"KEY: {key.Key} | Redeemed At: {guild.Name} | (Expires: {
                key.ExpiresAt:dddd d, MMMM yy})", key.Id));
        }
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}