namespace Kuroko.Commands;

public static class CommandMap
{
    // Global Command IDs
    public const string EXIT_WITH_PERM = "exit";
    public const string EXIT_WITH_UID = "exit_with_id";
    
    // BanSync Command IDs
    public const string BANSYNC_ENABLE = "bansync_enable";
    public const string BANSYNC_ALLOWREQUEST = "bansync_allowrequest";
    public const string BANSYNC_SYNCMODE_HOST = "bansync_syncmode_host";
    public const string BANSYNC_SYNCMODE_CLIENT = "bansync_syncmode_client";
    public const string BANSYNC_CHANNEL = "bansync_channel";
    public const string BANSYNC_CLIENTREQUEST = "bansync_clientrequest";
    public const string BANSYNC_CLIENTREQUEST_UUID = "bansync_clientrequest_uuid";
    public const string BANSYNC_CLIENTREQUEST_REASON = "bansync_clientrequest_reason";
    public const string BANSYNC_CLIENTREQUEST_ACCEPT = "bansync_clientrequest_accept";
}