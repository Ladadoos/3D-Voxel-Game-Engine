using System.Collections.Generic;

namespace Minecraft
{
    class FontRegistry
    {
        public ReadOnlyDictionary<FontType, Font> fonts;

        public FontRegistry()
        {
            RegisterFonts();
        }

        public Font GetValue(FontType fontType)
        {
            fonts.TryGetValue(fontType, out Font font);
            return font;
        }

        private void RegisterFonts()
        {
            Dictionary<FontType, Font> registry = new Dictionary<FontType, Font>
            {
                { FontType.Arial, new Font("../../Resources/arial.fnt", "../../Resources/arial.png", 512, 512) },
            };
            fonts = new ReadOnlyDictionary<FontType, Font>(registry);
        }
    }
}
