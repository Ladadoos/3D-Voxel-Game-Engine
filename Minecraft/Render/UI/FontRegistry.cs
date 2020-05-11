using System.Collections.Generic;

namespace Minecraft
{
    static class FontRegistry
    {
        private static ReadOnlyDictionary<FontType, Font> fonts;

        public static void Initialize()
        {
            RegisterFonts();
        }

        public static Font GetFont(FontType fontType)
        {
            if(!fonts.TryGetValue(fontType, out Font font))
            {
                throw new KeyNotFoundException();
            }
            return font;
        }

        private static void RegisterFonts()
        {
            Dictionary<FontType, Font> registry = new Dictionary<FontType, Font>
            {
                { FontType.Arial, new Font("../../Resources/arial.fnt", "../../Resources/arial.png", 512, 512) },
            };
            fonts = new ReadOnlyDictionary<FontType, Font>(registry);
        }
    }
}
