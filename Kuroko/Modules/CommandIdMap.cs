namespace Kuroko.Modules
{
    // TODO: You can setup custom command ids here so there's no need to spam it everywhere. Helps to reduce spelling errors as Discord is finicky in this regard

    internal static class CommandIdMap
    {
        #region Global

        public const string Exit = "normal_exit";

        #endregion

        #region RoleRequest

        public const string RoleRequestAssign = "rr_user_assign";
        public const string RoleRequestRemove = "rr_user_remove";

        #endregion
    }
}
