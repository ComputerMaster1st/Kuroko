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
        public const string RoleRequestRemove = "rr_user_remove";

        #endregion

        #region Management

        public const string RoleRequestManageAdd = "rr_manage_add";
        public const string RoleRequestManageDelete = "rr_manage_delete";
        public const string RoleRequestManageRemove = "rr_manage_remove";
        public const string RoleRequestManageRemoveAll = "rr_manage_remove_all";
        public const string RoleRequestManageSave = "rr_manage_save";

        #endregion

        #endregion
    }
}
