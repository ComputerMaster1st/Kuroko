using Discord;
using Discord.Interactions;
using Kuroko.Database;
using Kuroko.Database.GuildEntities;

namespace Kuroko;

public abstract class KurokoCommandBase : InteractionModuleBase<KurokoInteractionContext>
{
    protected bool IsInteractedUser(ulong userId)
        => userId == Context.User.Id;

    public override async Task AfterExecuteAsync(ICommandInfo command)
    {
        await Context.Database.SaveChangesAsync();
        await base.AfterExecuteAsync(command);
    }
    
    protected async Task<TPropertyEntity> GetPropertiesAsync<TPropertyEntity, TDiscordEntity>(ulong rootId)
        where TPropertyEntity : class, IPropertyEntity
        where TDiscordEntity : class, IDiscordEntity
    {
        var set = Context.Database.Set<TPropertyEntity>();
        var rootSet = Context.Database.Set<TDiscordEntity>();
        var result = await set.CreateOrGetPropertiesAsync(rootSet, rootId, (x, y) =>
        {
            var properties = x.GetType().GetProperties();
            var property = properties.FirstOrDefault(info => info.PropertyType == typeof(TPropertyEntity));
    
            property?.SetValue(x, y);
        });
    
        return result;
    }
    
    // protected async Task<TTicketEntity> GetTicketAsync<TTicketEntity>(int ticketId)
    //     where TTicketEntity : class, ITicketEntity
    // {
    //     var set = Context.Database.Set<TTicketEntity>();
    //     var result = await set.FirstOrDefaultAsync(x => x.Id == ticketId);
    //
    //     return result;
    // }
    
    protected async Task<TDiscordEntity> GetOrCreateDataAsync<TDiscordEntity>(ulong rootId)
        where TDiscordEntity : class, IDiscordEntity
    {
        var set = Context.Database.Set<TDiscordEntity>();
        var result = await set.GetOrCreateDataAsync(rootId);
    
        return result;
    }

    protected static ButtonStyle ButtonToggle(bool isEnabled)
        => isEnabled ? ButtonStyle.Success : ButtonStyle.Secondary;

    protected async Task<TPropertyEntity> QuickEditPropertiesAsync<TPropertyEntity>
        (ulong guildId, Action<TPropertyEntity> action)
        where TPropertyEntity : class, IPropertyEntity
    {
        var properties = await GetPropertiesAsync<TPropertyEntity, GuildEntity>(guildId);
        action(properties);
        await DeferAsync();
        return properties;
    }
}