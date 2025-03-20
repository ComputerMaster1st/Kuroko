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
                key.ExpiresAt.ReadableDateTime()})", key.Id));

        if ((properties.RootId == ctx.KurokoConfig.OwnerId || 
             ctx.KurokoConfig.AdminUserIds.Contains(properties.RootId)) && !properties.BotAdminEnabled)
            results.Add(new AutocompleteResult("WARNING: ENABLE BOT ADMIN BYPASS?", -2));
        
        if ((properties.RootId == ctx.KurokoConfig.OwnerId || 
             ctx.KurokoConfig.AdminUserIds.Contains(properties.RootId)) && properties.BotAdminEnabled)
            results.Add(new AutocompleteResult("WARNING: DISABLE BOT ADMIN BYPASS?", -3));

        var adminMode = properties.BotAdminEnabled && properties.PremiumKeys.Count < 10;
        if (properties.PremiumKeys.Count < properties.KeysAllowed || properties.KeysAllowed == -1 ||
            adminMode)
            results.Add(new AutocompleteResult($"Generate New Key! {
                (adminMode ? "(BOT ADMIN MODE ENABLED)" : "")}", -1));
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}