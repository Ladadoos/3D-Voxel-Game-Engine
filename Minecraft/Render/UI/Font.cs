using System.Linq;

namespace Minecraft
{
    class Font
    {
        public Texture fontMapTexture { get; private set; }
        public int desiredPixelLineHeight { get; private set; }
        public ReadOnlyDictionary<int, Character> fontChars;

        public Font(string fontFilePath, string fontMapFilePath, int fontMapWidth, int fontMapHeight)
        {
            int fontMapTextureid = TextureLoader.LoadTexture(fontMapFilePath);
            fontMapTexture = new Texture(fontMapTextureid, fontMapWidth, fontMapHeight);

            CharacterBuilder charBuilder = new CharacterBuilder();
            fontChars = new ReadOnlyDictionary<int, Character>(charBuilder.BuildFont(fontMapTexture, fontFilePath));
            desiredPixelLineHeight = fontChars.Aggregate((l, r) => l.Value.height > r.Value.height ? l : r).Value.height;
        }
    }
}
