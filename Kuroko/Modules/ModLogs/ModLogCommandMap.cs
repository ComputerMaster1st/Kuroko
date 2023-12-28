namespace Kuroko.Modules.ModLogs
{
    internal class ModLogCommandMap
    {
        public const string MENU = "ml_menu";

        #region Configuration

        public const string CHANNEL = "ml_channel";
        public const string CHANNEL_SAVE = "ml_channel_save";
        public const string CHANNEL_DELETE = "ml_channel_delete";
        public const string CHANNEL_RESUME = "ml_channel_resume";
        public const string CHANNEL_RESUME_SAVE = "ml_channel_resume_save";
        public const string CHANNEL_IGNORE = "ml_channel_ignore";
        public const string CHANNEL_IGNORE_RESET = "ml_channel_ignore_reset";
        public const string CHANNEL_IGNORE_SAVE = "ml_channel_ignore_save";

        #endregion
        #region Toggles

        // Select menu for setting up toggles
        public const string ENTRIES = "ml_entries";
        public const string AUDITLOG = "ml_auditlog";

        // Toggles
        public const string MESSAGE_DELETED = "ml_message_deleted";
        public const string MESSAGE_EDITED = "ml_message_edited";
        public const string JOIN = "ml_join";
        public const string LEAVE = "ml_leave";
        public const string TIMEOUT = "ml_timeout";
        public const string SERVERMUTE = "ml_servermute";
        public const string KICK = "ml_kick";
        public const string BAN = "ml_ban";

        #endregion
    }
}
