using System.Collections.Generic;

namespace Minecraft
{
    class FontRegistry
    {
        public ReadOnlyDictionary<FontType, Font> Fonts { get; private set; }

        public FontRegistry()
        {
            RegisterFonts();
        }

        public Font GetValue(FontType fontType)
        {
            Fonts.TryGetValue(fontType, out Font font);
            return font;
        }

        private void RegisterFonts()
        {
            Dictionary<FontType, Font> registry = new Dictionary<FontType, Font>
            {
                { FontType.Arial, new Font("../../Resources/arial.fnt", "../../Resources/arial.png", 512, 512) },
            };
            Fonts = new ReadOnlyDictionary<FontType, Font>(registry);
        }
    }
}
