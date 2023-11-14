using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Definitions.Licensing;
using MimeDetective.Engine;

namespace Kuroko.Shared
{
    public static class FileMimeType
    {
        private static ContentInspector _inspector = null;

        private static ContentInspector Inspector
        {
            get
            {
                _inspector ??= new ContentInspectorBuilder()
                {
                    Definitions = new ExhaustiveBuilder()
                    {
                        UsageType = UsageType.PersonalNonCommercial
                    }.Build()
                }.Build();

                return _inspector;
            }
        }

        public static IEnumerable<MimeTypeMatch> GetFromBytes(byte[] bytes)
        {
            var results = Inspector.Inspect(bytes);
            return results.ByMimeType();
        }
    }
}
