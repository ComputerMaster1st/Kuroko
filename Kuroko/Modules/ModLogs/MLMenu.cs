using Discord;
using Kuroko.Database.Entities.Guild;
using Kuroko.Modules.Globals;

namespace Kuroko.Modules.ModLogs
{
    public static class MLMenu
    {
        private static ButtonStyle ButtonEnabled(bool isEnabled)
        {
            if (isEnabled)
                return ButtonStyle.Success;
            return ButtonStyle.Secondary;
        }

        public static MessageComponent BuildMenu(IGuildUser user, ModLogEntity properties)
        {
            var mainRow = 0;
            var toggleRow = 1;
            var exitRow = 2;
            var componentBuilder = new ComponentBuilder()
                .WithButton("Configure Log Channel", $"{CommandIdMap.ModLogChannel}:{user.Id},0", ButtonStyle.Primary, row: mainRow)
                .WithButton("Ignore Channels", $"{CommandIdMap.ModLogChannelIgnore}:{user.Id},0", ButtonStyle.Primary, row: mainRow);

            if (properties.IgnoredChannelIds.Count > 0)
                componentBuilder.WithButton("Resume Channels", $"{CommandIdMap.ModLogChannelResume}:{user.Id},0", ButtonStyle.Primary, row: mainRow)
                    .WithButton("Monitor All Channels", $"{CommandIdMap.ModLogChannelIgnoreReset}:{user.Id}", ButtonStyle.Success, row: mainRow);

            if (properties.LogChannelId != 0)
                componentBuilder.WithButton("Unset Logging Channel", $"{CommandIdMap.ModLogChannelDelete}:{user.Id}", ButtonStyle.Danger, row: mainRow);

            if (properties.LogChannelId != 0)
            {

                componentBuilder
                    .WithButton("User Joined", $"{CommandIdMap.ModLogJoin}:{user.Id}", ButtonEnabled(properties.Join), row: toggleRow)
                    .WithButton("User Left", $"{CommandIdMap.ModLogLeave}:{user.Id}", ButtonEnabled(properties.Leave), row: toggleRow)
                    .WithButton("Message Edited", $"{CommandIdMap.ModLogMessageEdited}:{user.Id}", ButtonEnabled(properties.EditedMessages), row: toggleRow)
                    .WithButton("Message Deleted", $"{CommandIdMap.ModLogMessageDeleted}:{user.Id}", ButtonEnabled(properties.DeletedMessages), row: toggleRow);
            }

            componentBuilder.WithButton("Exit", $"{CommandIdMap.Exit}:{user.Id}", ButtonStyle.Secondary, row: exitRow);

            return componentBuilder.Build();
        }

        public static async Task<(bool HasOptions, MessageComponent Components)> BuildLogChannelMenuAsync(IGuildUser user, int indexStart)
        {
            var count = 0;
            var textChannels = await user.Guild.GetTextChannelsAsync();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.ModLogChannelSave}:{user.Id}")
                .WithMinValues(1)
                .WithPlaceholder("Select a text channel to send mod logs to");

            foreach (var textChannel in textChannels)
            {
                selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            return Pagination.SelectMenu(selectMenuBuilder, indexStart, user, CommandIdMap.ModLogChannelIgnore, CommandIdMap.ModLogMenu, true);
        }

        public static async Task<(bool HasOptions, MessageComponent Components)> BuildIgnoreLogChannelMenuAsync(IGuildUser user, int indexStart)
        {
            var count = 0;
            var textChannels = await user.Guild.GetTextChannelsAsync();
            var selectMenuBuilder = new SelectMenuBuilder()
                .WithCustomId($"{CommandIdMap.ModLogChannelIgnoreSave}:{user.Id},{indexStart}")
                .WithMinValues(1)
                .WithPlaceholder("Select text channels to ignore mod logging");

            foreach (var textChannel in textChannels)
            {
                selectMenuBuilder.AddOption(textChannel.Name, textChannel.Id.ToString());
                count++;

                if (count >= 25)
                    break;
            }

            return Pagination.SelectMenu(selectMenuBuilder, indexStart, user, CommandIdMap.ModLogChannelIgnore, CommandIdMap.ModLogMenu);
        }
    }
}
