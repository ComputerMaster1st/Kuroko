using Discord;

namespace Kuroko.Modules.Globals
{
    public static class Pagination
    {
        public static (bool HasOptions, MessageComponent Components) SelectMenu(
            SelectMenuBuilder builder,
            int startIndex,
            IUser user,
            string commandId,
            string returningCommandId,
            bool isSingleSelect = false)
        {
            var componentBuilder = new ComponentBuilder();
            (bool HasOptions, MessageComponent Components) output = new();

            if (builder.Options.Count > 0)
            {
                if (isSingleSelect)
                    builder.WithMaxValues(1);
                else
                    builder.WithMaxValues(builder.Options.Count);

                componentBuilder.WithSelectMenu(builder);
                output.HasOptions = true;
            }
            else
                output.HasOptions = false;

            if (startIndex > 0)
                componentBuilder.WithButton("<<--", $"{commandId}:{user.Id},{startIndex - 25}", ButtonStyle.Primary);

            if (builder.Options.Count >= 25)
                componentBuilder.WithButton("-->>", $"{commandId}:{user.Id},{startIndex + 25}", ButtonStyle.Primary);

            componentBuilder.WithButton("Back To Menu", $"{returningCommandId}:{user.Id}", ButtonStyle.Secondary);

            output.Components = componentBuilder.Build();

            return output;
        }
    }
}
