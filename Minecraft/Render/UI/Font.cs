namespace Minecraft
{
    class Font
    {
        private static string defaultFont = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-=_+[]{}\\|;:'\".,<>/?`~ ";

        public Texture fontMap { get; private set; }
        private int charWidth, charHeight;

        public Font(Texture fontMap, int charWidth)
        {
            this.fontMap = fontMap;
            this.charWidth = charWidth;
            this.charHeight = fontMap.pixelHeight;
        }

        public float[] GetVerticesScreenSpace(UIText textComponent, UICanvas canvas)
        {
            float h = charHeight / (float)(canvas.window.Height / 2);
            float w = charWidth / (float)(canvas.window.Width / 2);

            float offset = 0;
            float[] allVertices = new float[textComponent.text.Length * 6 * 3];
            for(int i = 0; i < allVertices.Length; i+=18)
            {
                allVertices[i + 0]  = offset;     allVertices[i + 1]  = h; allVertices[i + 2]  = 0;
                allVertices[i + 3]  = offset + w; allVertices[i + 4]  = h; allVertices[i + 5]  = 0;
                allVertices[i + 6]  = offset + w; allVertices[i + 7]  = 0; allVertices[i + 8]  = 0;
                allVertices[i + 9]  = offset;     allVertices[i + 10] = h; allVertices[i + 11] = 0;
                allVertices[i + 12] = offset + w; allVertices[i + 13] = 0; allVertices[i + 14] = 0;
                allVertices[i + 15] = offset;     allVertices[i + 16] = 0; allVertices[i + 17] = 0;
                offset += w;
            }
            return allVertices;
        }

        public float[] GetTexturesScreenSpace(UIText textComponent, UICanvas canvas)
        {
            float charDim = charWidth / (float)fontMap.pixelWidth;

            float[] allTextures = new float[textComponent.text.Length * 6 * 2];
            for(int i = 0; i < allTextures.Length; i+=12)
            {
                char c = textComponent.text[i / 12];
                int j = defaultFont.IndexOf(c);
                float x1 = charDim * j;
                float x2 = charDim * (j + 1);

                allTextures[i + 0] = x1;  allTextures[i + 1] = 0;
                allTextures[i + 2] = x2;  allTextures[i + 3] = 0;
                allTextures[i + 4] = x2;  allTextures[i + 5] = 1;
                allTextures[i + 6] = x1;  allTextures[i + 7] = 0;
                allTextures[i + 8] = x2;  allTextures[i + 9] = 1;
                allTextures[i + 10] = x1; allTextures[i + 11] = 1;
            }

            return allTextures;
        }

    }
}
