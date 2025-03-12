using System.Text;
using Discord;
using Discord.Interactions;
using Kuroko.Attributes;
using Kuroko.Database.GuildEntities;
using Kuroko.Database.GuildEntities.Extras;
using Kuroko.Shared;
using Microsoft.EntityFrameworkCore;

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
            .AppendLine($"* **Default Sync Mode:** {properties.HostMode}");
        
        await RespondAsync(output.ToString(), ephemeral: true);
    }
    
    // Public/Client Request Command
    [SlashCommand("bansync-request", "(BanSync Id Required) BanSync public request")]
    public async Task ClientRequestAsync(string bansyncId = null)
    {
        var properties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);

        if (!properties.AllowRequests)
        {
            await RespondAsync("BanSync requests are not allowed on this server.", ephemeral: true);
            return;
        }
        var verifiedHostGuid = await VerifyGuidAsync(bansyncId, properties.SyncId);
        if (verifiedHostGuid == Guid.Empty) return;
        
        await Context.Interaction.RespondWithModalAsync<BanSyncClientModal>(
            $"{CommandMap.BANSYNC_CLIENTREQUEST}:{Context.User.Id}",
            modifyModal: x =>
            {
                x.UpdateTextInput(CommandMap.BANSYNC_CLIENTREQUEST_ID, 
                    y => y.Value = bansyncId);
            });
    }

    [ModalInteraction($"{CommandMap.BANSYNC_CLIENTREQUEST}:*")]
    public async Task ProcessClientRequestAsync(ulong interactedUserId, BanSyncClientModal modal)
    {
        if (!IsInteractedUser(interactedUserId)) return;
        var hostProperties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
        var verifiedHostGuid = await VerifyGuidAsync(modal.BanSyncId, hostProperties.SyncId);
        if (verifiedHostGuid == Guid.Empty) return;
        var clientProperties = await Context.Database.BanSyncProperties.FirstOrDefaultAsync(
            x => x.SyncId == Guid.Parse(modal.BanSyncId));
        var clientGuild = Context.Client.GetGuild(clientProperties.GuildId);
        
        var embedBuilder = new EmbedBuilder
        {
            Title = "BanSync Client Request",
            ThumbnailUrl = clientGuild?.IconUrl,
            Author = new EmbedAuthorBuilder
            {
                Name = Context.User.Username,
                IconUrl = Context.User.GetAvatarUrl()
            },
            Color = Color.Blue,
            Fields = [
                new EmbedFieldBuilder
                {
                    Name = "Requester BanSync Id",
                    Value = modal.BanSyncId
                },
                new EmbedFieldBuilder
                {
                    Name = "Requester Server/Guild",
                    Value = clientGuild?.Name
                },
                new EmbedFieldBuilder
                {
                    Name = "Reason",
                    Value = modal.Reason
                }
            ],
            Timestamp = DateTimeOffset.Now
        };
        var componentBuilder = new ComponentBuilder()
            .WithSelectMenu($"{CommandMap.BANSYNC_CLIENTREQUEST_ACCEPT}:{Context.User.Id},{modal.BanSyncId}",
                [
                    new SelectMenuOptionBuilder
                    {
                        Label = nameof(BanSyncMode.Default),
                        Value = nameof(BanSyncMode.Default)
                    },
                    new SelectMenuOptionBuilder
                    {
                        Label = nameof(BanSyncMode.Simplex),
                        Value = nameof(BanSyncMode.Simplex)
                    },
                    new SelectMenuOptionBuilder
                    {
                        Label = nameof(BanSyncMode.HalfDuplex),
                        Value = nameof(BanSyncMode.HalfDuplex)
                    },
                    new SelectMenuOptionBuilder
                    {
                        Label = nameof(BanSyncMode.FullDuplex),
                        Value = nameof(BanSyncMode.FullDuplex)
                    }
                ],
                $"Accept Request With BanSync Mode...",
                maxValues: 1)
            .WithButton("Reject Request",
                CommandMap.EXIT_WITH_PERM,
                ButtonStyle.Danger);
        var channel = Context.Guild.GetTextChannel(hostProperties.BanSyncChannelId);
        if (channel is null)
        {
            await RespondAsync("Unable to send request! Please ask an administrator to setup the ban sync channel.",
                ephemeral: true);
            return;
        }
        
        await channel.SendMessageAsync(embed: embedBuilder.Build(),
            components: componentBuilder.Build());
        await RespondAsync("Your request has been sent!", ephemeral: true);
    }

    [ComponentInteraction($"{CommandMap.BANSYNC_CLIENTREQUEST_ACCEPT}:*,*")]
    [KurokoUserPermission(GuildPermission.ManageGuild)]
    public async Task AcceptClientRequestAsync(ulong interactedUserId, string rawGuid, string rawMode)
    {
        if (!IsInteractedUser(interactedUserId)) return;
        var mode = Enum.Parse<BanSyncMode>(rawMode);
        await ProcessRequestAsync(rawGuid, mode);
        
        var msg = await Context.Interaction.GetOriginalResponseAsync();
        if (msg != null)
            await msg.DeleteAsync();
    }

    // Invite-Only Request
    [SlashCommand("bansync-invite", "(BanSync Id & Manage Perm Required) Invite Server To BanSync")]
    [KurokoUserPermission(GuildPermission.ManageGuild)]
    public async Task InviteAsync(string bansyncId, BanSyncMode mode)
    {
        if (await ProcessRequestAsync(bansyncId, mode))
            await RespondAsync("Server/Guild Successfully Synced!", ephemeral: true);
    }
    
    private async Task<bool> ProcessRequestAsync(string bansyncId, BanSyncMode mode)
    {
        await DeferAsync();
        
        var hostProperties = await GetPropertiesAsync<BanSyncProperties, GuildEntity>(Context.Guild.Id);
        var verifiedClientGuid = await VerifyGuidAsync(bansyncId, hostProperties.SyncId);
        var clientProperties = await Context.Database.BanSyncProperties.FirstOrDefaultAsync(
            x => x.SyncId == verifiedClientGuid);

        if (clientProperties is null)
        {
            await RespondAsync(
                "Can not identify client by BanSync Id. Please make sure the client has BanSync enabled!",
                ephemeral: true);
            return false;
        }

        var profile = new BanSyncProfile(hostProperties.SyncId, verifiedClientGuid, mode);
        var clientGuild = Context.Client.GetGuild(clientProperties.GuildId);
        var hostChannel = Context.Guild.GetTextChannel(hostProperties.BanSyncChannelId);
        var clientChannel = clientGuild.GetTextChannel(clientProperties.BanSyncChannelId);
        Context.Database.BanSyncProfiles.Add(profile);

        if (clientChannel != null)
            await clientChannel.SendMessageAsync(embed: MakeEmbed());
        if (hostChannel != null)
            await hostChannel.SendMessageAsync(embed: MakeEmbed(true));
        return true;

        Embed MakeEmbed(bool isClient = false)
        {
            return new EmbedBuilder
            {
                Title = "BanSync Confirmation!",
                Color = Color.Green,
                ThumbnailUrl = isClient ? clientGuild.IconUrl : Context.Guild.IconUrl,
                Timestamp = DateTimeOffset.Now,
                Fields = [
                    new EmbedFieldBuilder
                    {
                        Name = "BanSync Id",
                        Value = isClient ? clientProperties.SyncId.ToString() : hostProperties.SyncId.ToString()
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Server/Guild",
                        Value = isClient ? clientGuild.Name : Context.Guild.Name
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "BanSync Mode",
                        Value = mode.ToString()
                    }
                ]
            }.Build();
        }
    }
    
    private async Task<Guid> VerifyGuidAsync(string rawGuid, Guid guid)
    {
        if (!Guid.TryParse(rawGuid, out var verified))
        {
            await RespondAsync(
                "Invalid BanSync Id! Please double-check by running /bansync-config!",
                ephemeral: true);
            return Guid.Empty;
        }
        if (guid != verified) return verified;
        
        await RespondAsync(
            "The BanSync Id provided belongs to this server! Please make sure you are using this command on another server.",
            ephemeral: true);
        return Guid.Empty;
    }
}