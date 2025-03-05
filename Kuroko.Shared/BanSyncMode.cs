namespace Kuroko.Shared;

public enum BanSyncMode
{
    /// <summary>
    /// No Sync
    /// </summary>
    Disabled,
    /// <summary>
    /// Host Default Setting
    /// </summary>
    Default,
    /// <summary>
    /// Read Host Banlist Only
    /// </summary>
    Simplex,
    /// <summary>
    /// Read Host Banlist & Send Warnings To Host
    /// </summary>
    HalfDuplex,
    /// <summary>
    /// Read Host Banlist & Send Warnings To Host & Execute Ban On Host
    /// WARNING: ONLY TO BE USED FOR GROUPED COMMUNITY SERVERS
    /// </summary>
    FullDuplex
}