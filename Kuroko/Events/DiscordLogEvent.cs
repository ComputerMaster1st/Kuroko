using Discord;
using Discord.WebSocket;
using Kuroko.MDK;
using Kuroko.MDK.Attributes;

namespace Kuroko.CoreModule.Events
{
    [PreInitialize]
    public class DiscordLogEvent
    {
        private DiscordShardedClient _client;

        public DiscordLogEvent(DiscordShardedClient discordClient)
        {
            _client = discordClient;
            _client.Log += LogEvent;
        }

        public static Task LogEvent(LogMessage logMessage)
            => Utilities.WriteLogAsync(logMessage);
    }
}
