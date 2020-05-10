using System.Linq;

namespace Minecraft
{
    class Font
    {
        public Texture FontMapTexture { get; private set; }
        public int DesiredPixelLineHeight { get; private set; }
        public ReadOnlyDictionary<int, Character> FontChars { get; private set; }

        public Font(string fontFilePath, string fontMapFilePath, int fontMapWidth, int fontMapHeight)
        {
            int fontMapTextureid = TextureLoader.LoadTexture(fontMapFilePath);
            FontMapTexture = new Texture(fontMapTextureid, fontMapWidth, fontMapHeight);

            CharacterBuilder charBuilder = new CharacterBuilder();
            FontChars = new ReadOnlyDictionary<int, Character>(charBuilder.BuildFont(FontMapTexture, fontFilePath));
            DesiredPixelLineHeight = FontChars.Aggregate((l, r) => l.Value.Height > r.Value.Height ? l : r).Value.Height;
        }
    }
}
