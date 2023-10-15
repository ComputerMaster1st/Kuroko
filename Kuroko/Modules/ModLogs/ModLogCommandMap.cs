namespace Kuroko.Modules.ModLogs
{
    internal class ModLogCommandMap
    {
        public const string ModLogMenu = "ml_menu";

        #region "Configuration

        public const string ModLogChannel = "ml_channel";
        public const string ModLogChannelSave = "ml_channel_save";
        public const string ModLogChannelDelete = "ml_channel_delete";
        public const string ModLogChannelResume = "ml_channel_resume";
        public const string ModLogChannelResumeSave = "ml_channel_resume_save";
        public const string ModLogChannelIgnore = "ml_channel_ignore";
        public const string ModLogChannelIgnoreReset = "ml_channel_ignore_reset";
        public const string ModLogChannelIgnoreSave = "ml_channel_ignore_save";

        #endregion
        #region "Toggles"

        public const string ModLogMessageDeleted = "ml_message_deleted";
        public const string ModLogMessageEdited = "ml_message_edited";
        public const string ModLogJoin = "ml_join";
        public const string ModLogLeave = "ml_leave";
        public const string ModLogDownloadAttachment = "ml_download";

        #endregion
    }
}
