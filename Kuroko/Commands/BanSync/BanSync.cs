using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Database.GuildEntities;

namespace Kuroko.Commands.BanSync;

public partial class BanSync : KurokoCommandBase
{
    [SlashCommand("bansync-status", "Status of BanSync")]
    public async Task StatusAsync()
    {
        var properties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
        var output = new StringBuilder()
            .AppendLine("## BanSync Status")
            .AppendLine($"* **Enabled:** {(properties.IsEnabled ? "True" : "False")}")
            .AppendLine($"* **Allow Requests:** {(properties.AllowRequests ? "True" : "False")}")
            .AppendLine($"* **Servers Hosting For/As Client:** {
                properties.HostForProfiles.Count}/{properties.ClientOfProfiles.Count}")
            .AppendLine($"* **Default Sync Mode:** {properties.Mode}");
        
        await RespondAsync(output.ToString(), ephemeral: true);
    }
    
    // Public/Client Request Command
    [SlashCommand("bansync-request", "(UUID Required) BanSync public request")]
    public async Task ClientRequestAsync(string uuid = null)
    {
        var properties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);

        if (!properties.AllowRequests)
        {
            await RespondAsync("BanSync requests are not allowed on this server.", ephemeral: true);
            return;
        }
        if (!Guid.TryParse(uuid, out var guid))
        {
            await RespondAsync(
                "Invalid UUID! Please double-check by running /bansync-config on your server!",
                ephemeral: true);
            return;
        }
        if (properties.SyncId == guid)
        {
            await RespondAsync(
                "The UUID provided belongs to this server! Please make sure you are using this command on another server.",
                ephemeral: true);
            return;
        }
        
        await Context.Interaction.RespondWithModalAsync<BanSyncClientModal>(
            $"{CommandMap.BANSYNC_CLIENTREQUEST}:{Context.User.Id}",
            modifyModal: x =>
            {
                x.UpdateTextInput(CommandMap.BANSYNC_CLIENTREQUEST_UUID, 
                    y => y.Value = uuid);
            });
    }

    [ModalInteraction($"{CommandMap.BANSYNC_CLIENTREQUEST}:*")]
    public async Task ProcessClientRequestAsync(ulong interactedUserId, BanSyncClientModal modal)
    {
        if (!IsInteractedUser(interactedUserId)) return;
        var embedBuilder = new EmbedBuilder()
        {
            Title = "BanSync Client Request",
            Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username,
                IconUrl = Context.User.GetAvatarUrl()
            },
            Color = Color.Blue,
            Fields = [
                new EmbedFieldBuilder()
                {
                    Name = "Requester BanSync UUID",
                    Value = modal.UUID
                },
                new EmbedFieldBuilder()
                {
                    Name = "Reason",
                    Value = modal.Reason
                }
            ],
            Timestamp = DateTimeOffset.Now
        };
        
        await Context.Channel.SendMessageAsync(embed: embedBuilder.Build());
        await RespondAsync("Your request has been sent!", ephemeral: true);
    }

    // Invite-Only Request
}