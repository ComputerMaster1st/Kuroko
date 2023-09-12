using Discord;

namespace Kuroko.Modules.RoleRequest
{
    public static class RRMenu
    {
        public static MessageComponent BuildUserMenu()
        {
            var builder = new ComponentBuilder()
                .WithButton("Assign", CommandIdMap.RoleRequestAssign + ":0", ButtonStyle.Success, row: 0)
                .WithButton("Remove", CommandIdMap.RoleRequestRemove + ":0", ButtonStyle.Danger, row: 0)
                .WithButton("Exit", CommandIdMap.Exit, ButtonStyle.Secondary, row: 0);

            return builder.Build();
        }
    }
}
