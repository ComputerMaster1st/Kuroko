using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Kuroko.AutoCompletes;

public class BanSyncProfileAutocomplete : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        List<AutocompleteResult> results = [];
        var ctx = (KurokoInteractionContext)context;
        
        var properties = await ctx.Database.BanSyncProperties
            .Include(banSyncProperties => banSyncProperties.HostForProfiles)
            .Include(banSyncProperties => banSyncProperties.ClientOfProfiles)
            .FirstOrDefaultAsync(x => x.RootId == ctx.Guild.Id);
        
        if (properties is null)
            return AutocompletionResult.FromSuccess();

        foreach (var x in properties.HostForProfiles)
        {
            var guild = ctx.Client.GetGuild(x.ClientGuildProperties.RootId);
            if (guild is null)
            {
                ctx.Database.BanSyncProfiles.Remove(x);
                continue;
            }
            
            results.Add(new AutocompleteResult(guild.Name, x.Id));
        }
        
        foreach (var x in properties.ClientOfProfiles)
        {
            var guild = ctx.Client.GetGuild(x.HostGuildProperties.RootId);
            if (guild is null)
            {
                ctx.Database.BanSyncProfiles.Remove(x);
                continue;
            }
            
            results.Add(new AutocompleteResult(guild.Name, x.Id));
        }
        
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}