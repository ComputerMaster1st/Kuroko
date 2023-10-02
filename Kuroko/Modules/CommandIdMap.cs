namespace Kuroko.Modules
{
    // TODO: You can setup custom command ids here so there's no need to spam it everywhere. Helps to reduce spelling errors as Discord is finicky in this regard

    internal static class CommandIdMap
    {
        #region Global

        public const string Exit = "exit";

        #endregion

        #region RoleRequest

        public const string RoleRequestMenu = "rr_menu";

        #region Users

        public const string RoleRequestAssign = "rr_user_assign";
        public const string RoleRequestDelete = "rr_user_delete";
        public const string RoleRequestSave = "rr_user_save";
        public const string RoleRequestRemove = "rr_user_remove";

        #endregion

        #region Management

        public const string RoleRequestManageAdd = "rr_manage_add";
        public const string RoleRequestManageDelete = "rr_manage_delete";
        public const string RoleRequestManageRemove = "rr_manage_remove";
        public const string RoleRequestManageReset = "rr_manage_reset";
        public const string RoleRequestManageSave = "rr_manage_save";

        #endregion

        #endregion

        #region ModLogs

        public const string ModLogMenu = "ml_menu";
        public const string ModLogChannel = "ml_channel";
        public const string ModLogChannelSave = "ml_channel_save";
        public const string ModLogChannelDelete = "ml_channel_delete";
        public const string ModLogChannelResume = "ml_channel_resume";
        public const string ModLogChannelResumeSave = "ml_channel_resume_save";
        public const string ModLogChannelIgnore = "ml_channel_ignore";
        public const string ModLogChannelIgnoreReset = "ml_channel_ignore_reset";
        public const string ModLogChannelIgnoreSave = "ml_channel_ignore_save";
        public const string ModLogMessageDeleted = "ml_message_deleted";
        public const string ModLogMessageEdited = "ml_message_edited";
        public const string ModLogJoin = "ml_join";
        public const string ModLogLeave = "ml_leave";

        #endregion
    }
}
