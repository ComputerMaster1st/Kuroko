using Discord;
using Discord.WebSocket;
using Kuroko.Core.Attributes;

// TODO: Example of event handling. Use [PreInitialize] to create the event handler and have it auto-load on initialization.

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
