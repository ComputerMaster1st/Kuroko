using Discord;
using Discord.WebSocket;
using Kuroko.Attributes;

namespace Kuroko.Events;

[PreInitialize, KurokoEvent]
public class DiscordLogEvent
{
    public DiscordLogEvent(DiscordShardedClient shardedClient)
        => shardedClient.Log += ShardedClientOnLog;

    private static Task ShardedClientOnLog(LogMessage arg)
        => Utilities.WriteLogAsync(arg);
}