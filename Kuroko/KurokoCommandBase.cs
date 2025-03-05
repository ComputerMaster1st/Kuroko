using Discord.Interactions;
using Kuroko.Database;

namespace Kuroko;

public abstract class KurokoCommandBase : InteractionModuleBase<KurokoInteractionContext>
{
    protected async Task<bool> IsInteractedUserAsync(ulong userId)
    {
        if (userId == Context.User.Id) return true;
        await RespondAsync("You can not perform this action due to not being the original user.", ephemeral: true);
        return false;
    }

    public override async Task AfterExecuteAsync(ICommandInfo command)
    {
        await Context.Database.SaveChangesAsync();
        await base.AfterExecuteAsync(command);
    }
    
    // protected async Task<TPropertyEntity> GetPropertiesAsync<TPropertyEntity, TDiscordEntity>(ulong rootId)
    //     where TPropertyEntity : class, IPropertyEntity
    //     where TDiscordEntity : class, IDiscordEntity
    // {
    //     var set = Context.Database.Set<TPropertyEntity>();
    //     var rootSet = Context.Database.Set<TDiscordEntity>();
    //     var result = await set.CreateOrGetPropertiesAsync(rootSet, rootId, (x, y) =>
    //     {
    //         var properties = x.GetType().GetProperties();
    //         var property = properties.FirstOrDefault(x => x.PropertyType == typeof(TPropertyEntity));
    //
    //         property.SetValue(x, y);
    //     });
    //
    //     return result;
    // }
    
    // protected async Task<TTicketEntity> GetTicketAsync<TTicketEntity>(int ticketId)
    //     where TTicketEntity : class, ITicketEntity
    // {
    //     var set = Context.Database.Set<TTicketEntity>();
    //     var result = await set.FirstOrDefaultAsync(x => x.Id == ticketId);
    //
    //     return result;
    // }
    
    // protected async Task<TDiscordEntity> GetRootAsync<TDiscordEntity>(ulong rootId)
    //     where TDiscordEntity : class, IBaseEntity
    // {
    //     var set = Context.Database.Set<TDiscordEntity>();
    //     var result = await set.GetDataAsync(rootId);
    //
    //     return result;
    // }
}