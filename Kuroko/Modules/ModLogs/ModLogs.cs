using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;
using Kuroko.Services;
using System.Text;

namespace Kuroko.Modules.ModLogs
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    public class ModLogs : KurokoModuleBase
    {
        private static StringBuilder OutputMsg
        {
            get
            {
                return new StringBuilder()
                    .AppendLine("# Moderation Logging");
            }
        }


        [SlashCommand("modlogs", "Configure moderation logging")]
        public async Task EntryAsync()
            => await ExecuteAsync();

        [ComponentInteraction($"{ModLogCommandMap.MENU}:*")]
        public async Task ReturningAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync(true);
        }

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_DELETE}:*")]
        public async Task UnsetAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            properties.LogChannelId = 0;

            await SaveAndExecuteAsync(properties);
        }

        [ComponentInteraction($"{ModLogCommandMap.CHANNEL_IGNORE_RESET}:*")]
        public async Task ResetIgnoreAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            properties.IgnoredChannelIds.Clear(Context.Database);

            await SaveAndExecuteAsync(properties);
        }

        [ComponentInteraction($"{ModLogCommandMap.JOIN}:*")]
        public async Task JoinAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.Join = !x.Join);
        }

        [ComponentInteraction($"{ModLogCommandMap.LEAVE}:*")]
        public async Task LeaveAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.Leave = !x.Leave);
        }

        [ComponentInteraction($"{ModLogCommandMap.MESSAGE_DELETED}:*")]
        public async Task MessageDeleteAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.DeletedMessages = !x.DeletedMessages);
        }

        [ComponentInteraction($"{ModLogCommandMap.MESSAGE_EDITED}:*")]
        public async Task MessageEditAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.EditedMessages = !x.EditedMessages);
        }

        [ComponentInteraction($"{ModLogCommandMap.DOWNLOAD_ATTACHMENT}:*")]
        public async Task DownloadAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.SaveAttachments = !x.SaveAttachments);
        }

        [ComponentInteraction($"{ModLogCommandMap.BLACKBOX}:*")]
        public async Task BlackboxAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await ToggleAsync(x => x.EnableBlackbox = !x.EnableBlackbox);
        }

        private async Task ToggleAsync(Action<ModLogEntity> action)
        {
            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            action(properties);

            await SaveAndExecuteAsync(properties);
        }

        private async Task SaveAndExecuteAsync(ModLogEntity properties)
        {
            await DeferAsync();
            await Context.Database.SaveChangesAsync();
            await ExecuteAsync(true, properties);
        }

        private async Task ExecuteAsync(bool isReturning = false, ModLogEntity propParam = null)
        {
            var user = Context.User as IGuildUser;
            var properties = propParam ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var mainRow = 0;
            var toggleRow = 1;
            var specialRow = 4;
            var exitRow = 3;
            var componentBuilder = new ComponentBuilder()
                .WithButton("Configure Log Channel", $"{ModLogCommandMap.CHANNEL}:{user.Id},0", ButtonStyle.Primary, row: mainRow)
                .WithButton("Ignore Channels", $"{ModLogCommandMap.CHANNEL_IGNORE}:{user.Id},0", ButtonStyle.Primary, row: mainRow);

            if (properties.IgnoredChannelIds.Count > 0)
                componentBuilder
                    .WithButton("Resume Channels", $"{ModLogCommandMap.CHANNEL_RESUME}:{user.Id},0", ButtonStyle.Primary, row: mainRow)
                    .WithButton("Monitor All Channels", $"{ModLogCommandMap.CHANNEL_IGNORE_RESET}:{user.Id}", ButtonStyle.Success, row: mainRow);

            if (properties.LogChannelId != 0)
            {
                componentBuilder.WithButton("Unset Logging Channel", $"{ModLogCommandMap.CHANNEL_DELETE}:{user.Id}", ButtonStyle.Danger, row: mainRow)

                    .WithButton("User Joined", $"{ModLogCommandMap.JOIN}:{user.Id}", Pagination.IsButtonToggle(properties.Join), row: toggleRow)
                    .WithButton("User Left", $"{ModLogCommandMap.LEAVE}:{user.Id}", Pagination.IsButtonToggle(properties.Leave), row: toggleRow)
                    .WithButton("Message Edited", $"{ModLogCommandMap.MESSAGE_EDITED}:{user.Id}", Pagination.IsButtonToggle(properties.EditedMessages), row: toggleRow)
                    .WithButton("Message Deleted", $"{ModLogCommandMap.MESSAGE_DELETED}:{user.Id}", Pagination.IsButtonToggle(properties.DeletedMessages), row: toggleRow)

                    .WithButton("Download Attachments", $"{ModLogCommandMap.DOWNLOAD_ATTACHMENT}:{user.Id}", Pagination.IsButtonToggle(properties.SaveAttachments), row: specialRow)
                    .WithButton("Blackbox Recorder", $"{ModLogCommandMap.BLACKBOX}:{user.Id}", Pagination.IsButtonToggle(properties.EnableBlackbox), row: specialRow);
            }

            componentBuilder.WithButton("Exit", $"{GlobalCommandMap.EXIT}:{user.Id}", ButtonStyle.Secondary, row: exitRow);

            var msgComponents = componentBuilder.Build();
            var logChannel = Context.Guild.GetChannel(properties.LogChannelId);
            var channelTag = (logChannel is null) ? "**Not Set**" : $"<#{logChannel.Id}>";
            var output = OutputMsg
                .AppendLine($"Logging Channel: {channelTag}")
                .AppendLine("## Ignored Channels");
            var temp = new List<ulong>();

            if (properties.IgnoredChannelIds.Count > 0)
                foreach (var channelId in properties.IgnoredChannelIds)
                {
                    var channel = Context.Guild.GetChannel(channelId.Value);

                    if (channel is null)
                    {
                        temp.Add(channelId.Value);
                        continue;
                    }

                    output.AppendLine($"* <#{channel.Id}>");
                }
            else
                output.AppendLine($"* **None set**");

            if (!isReturning)
            {
                await RespondAsync(output.ToString(), components: msgComponents);
                (await Context.Interaction.GetOriginalResponseAsync()).SetTimeout(1);
            }
            else
                (await Context.Interaction.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = output.ToString();
                    x.Components = msgComponents;
                })).ResetTimeout();
        }
    }
}
