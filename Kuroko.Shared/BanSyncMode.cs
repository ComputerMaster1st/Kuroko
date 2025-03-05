namespace Kuroko.Shared;

public enum BanSyncMode
{
    /// <summary>
    /// BanSyncMode
    /// Disabled: No Sync
    /// Simplex: Read-Only
    /// HalfDuplex: Read & Send Warnings
    /// FullDuplex: Read & Send Warnings & Bans
    /// </summary>
    Disabled,
    Simplex,
    HalfDuplex,
    FullDuplex
}