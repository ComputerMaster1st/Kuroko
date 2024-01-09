using System.ComponentModel;
using System.Text;
using Castle.Components.DictionaryAdapter;
using Discord;
using Discord.Interactions;
using Kuroko.Core;
using Kuroko.Core.Attributes;
using Kuroko.Database.Entities.Guild;
using Kuroko.Services;

namespace Kuroko.Modules.ModLogs.Components
{
    [RequireUserGuildPermission(GuildPermission.ManageGuild)]
    [RequireBotGuildPermission(GuildPermission.ViewAuditLog)]
    public class LoggingOptionsComponent : KurokoModuleBase
    {
        [ComponentInteraction($"{ModLogCommandMap.ENTRIES_MENU}:*")]
        public async Task EntryAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            await DeferAsync();
            await ExecuteAsync();
        }

        [ComponentInteraction($"{ModLogCommandMap.ENTRIES_RESET}:*")]
        public async Task ResetOptionsAsync(ulong interactedUserId)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;
            
            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            properties.Join = false;
            properties.Leave = false;
            properties.EditedMessages = false;
            properties.DeletedMessages = false;
            properties.Kick = false;
            properties.Ban = false;
            properties.AuditLog = false;
            properties.ServerMute = false;
            properties.Timeout = false;
            
            await DeferAsync();
            await ExecuteAsync(properties);
        }

        [ComponentInteraction($"{ModLogCommandMap.ENTRIES}:*")]
        public async Task ConfigureOptionsAsync(ulong interactedUserId, string[] rawEntries)
        {
            if (!await IsInteractedUserAsync(interactedUserId))
                return;

            var properties = await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);

            foreach (var entry in rawEntries)
                switch (entry)
                {
                    case ModLogCommandMap.JOIN:
                        properties.Join = !properties.Join;
                        break;
                    case ModLogCommandMap.LEAVE:
                        properties.Leave = !properties.Leave;
                        break;
                    case ModLogCommandMap.MESSAGE_EDITED:
                        properties.EditedMessages = !properties.EditedMessages;
                        break;
                    case ModLogCommandMap.MESSAGE_DELETED:
                        properties.DeletedMessages = !properties.DeletedMessages;
                        break;
                    case ModLogCommandMap.KICK:
                        properties.Kick = !properties.Kick;
                        break;
                    case ModLogCommandMap.BAN:
                        properties.Ban = !properties.Ban;
                        break;
                    case ModLogCommandMap.AUDITLOG:
                        properties.AuditLog = !properties.AuditLog;
                        break;
                    // case ModLogCommandMap.SERVERMUTE:
                    //     properties.ServerMute = !properties.ServerMute;
                    //     break;
                    // case ModLogCommandMap.TIMEOUT:
                    //     properties.Timeout = !properties.Timeout;
                    //     break;
                    default:
                        await RespondAsync($"Unable to process flag: **{entry}**", ephemeral: true);
                        return;
                }
            
            await DeferAsync();
            await ExecuteAsync(properties);
        }

        private async Task ExecuteAsync(ModLogEntity propParams = null)
        {
            var properties = propParams ?? await GetPropertiesAsync<ModLogEntity, GuildEntity>(Context.Guild.Id);
            var optionsBuilder = new List<SelectMenuOptionBuilder>()
            {
                new()
                {
                    Description = "Monitors audit log and post all entries",
                    Label = properties.AuditLog ? "(Unset) Audit Log" : "Audit Log",
                    Value = ModLogCommandMap.AUDITLOG
                },
                new()
                {
                    Description = "Monitors when user joins the server",
                    Label = properties.Join ? "(Unset) User Join" : "User Join",
                    Value = ModLogCommandMap.JOIN
                },
                new()
                {
                    Description = "Monitors when user leaves the server",
                    Label = properties.Leave ? "(Unset) User Left" : "User Left",
                    Value = ModLogCommandMap.LEAVE
                },
                new()
                {
                    Description = "Monitors message editing",
                    Label = properties.EditedMessages ? "(Unset) Message Editing" : "Message Editing",
                    Value = ModLogCommandMap.MESSAGE_EDITED
                },
                new()
                {
                    Description = "Monitors message deletion",
                    Label = properties.DeletedMessages ? "(Unset) Message Deletion" : "Message Deletion",
                    Value = ModLogCommandMap.MESSAGE_DELETED
                },
                // new()
                // {
                //     Description = "Monitors server muting",
                //     Label = properties.ServerMute ? "(Unset) Server Mute" : "Server Mute (Audit Log)",
                //     Value = ModLogCommandMap.SERVERMUTE
                // },
                // new()
                // {
                //     Description = "Monitors user timeouts",
                //     Label = properties.Timeout ? "(unset) User Timeout" : "User Timeout (Audit Log)",
                //     Value = ModLogCommandMap.TIMEOUT
                // },
                new()
                {
                    Description = "Monitors server kicks",
                    Label = properties.Kick ? "(Unset) Server Kick" : "Server Kick (Audit Log)",
                    Value = ModLogCommandMap.KICK
                },
                new()
                {
                    Description = "Monitors server ban",
                    Label = properties.Ban ? "(Unset) Server Ban" : "Server Ban",
                    Value = ModLogCommandMap.BAN
                }
            };
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder()
            {
                CustomId = $"{ModLogCommandMap.ENTRIES}:{Context.User.Id}",
                MaxValues = optionsBuilder.Count,
                MinValues = 1,
                Placeholder = "Select options to (un)set for monitoring...",
                Options = optionsBuilder
            };
            var output = new StringBuilder()
                .AppendLine("# Moderation Logging")
                .AppendLine("## Configure Logging Options");

            componentBuilder.WithSelectMenu(selectMenuBuilder)
                .WithButton("Reset All", $"{ModLogCommandMap.ENTRIES_RESET}:{Context.User.Id}", ButtonStyle.Danger)
                .WithButton("Back To Menu", $"{ModLogCommandMap.MENU}:{Context.User.Id}", ButtonStyle.Secondary);

            (await Context.Interaction.ModifyOriginalResponseAsync(x =>
            {
                x.Content = output.ToString();
                x.Components = componentBuilder.Build();
            })).ResetTimeout();
        }
    }
}