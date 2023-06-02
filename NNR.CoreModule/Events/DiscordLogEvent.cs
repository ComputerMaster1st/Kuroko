using Discord;
using Discord.WebSocket;
using NNR.MDK;
using NNR.MDK.Attributes;

namespace NNR.CoreModule.Events
{
    [PreInitialize]
    public class DiscordLogEvent
    {
        public DiscordLogEvent(DiscordShardedClient discordClient)
            => discordClient.Log += LogEvent;

        private Task LogEvent(LogMessage logMessage)
            => Utilities.WriteLogAsync(logMessage);
    }
}
