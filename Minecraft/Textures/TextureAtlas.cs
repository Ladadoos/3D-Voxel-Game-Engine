using System;

namespace Minecraft
{
    class TextureAtlas : Texture
    {
        //center offset variable
        //https://gamedev.stackexchange.com/questions/46963/how-to-avoid-texture-bleeding-in-a-texture-atlas


        public int atlasSize;
        public int textureSize;

        private int texturesPerRow;
        private float textureUVsize;
        private float texelSize;

        public TextureAtlas(int textureId, int atlasSize, int textureSize) : base(textureId)
        {
            this.atlasSize = atlasSize;
            this.textureSize = textureSize;

            texturesPerRow = atlasSize / textureSize;
            textureUVsize = 1.0F / texturesPerRow;
            texelSize = 0.5F / atlasSize;
        }

        public float[] GetCubeTextureCoords(int backX, int backY, int rightX, int rightY, int frontX, int frontY, int leftX, int leftY,
            int topX, int topY, int bottomX, int bottomY)
        {

            float[] textureCoords = new float[48];
            Array.Copy(GetTextureCoords(backX, backY), 0, textureCoords, 0, 8);
            Array.Copy(GetTextureCoords(rightX, rightY), 0, textureCoords, 8, 8);
            Array.Copy(GetTextureCoords(frontX, frontY), 0, textureCoords, 16, 8);
            Array.Copy(GetTextureCoords(leftX, leftY), 0, textureCoords, 24, 8);
            Array.Copy(GetTextureCoords(topX, topY), 0, textureCoords, 32, 8);
            Array.Copy(GetTextureCoords(bottomX, bottomY), 0, textureCoords, 40, 8);

            return textureCoords;
        }

        public float[] GetTextureCoords(int x, int y)
        {
            float xMin = x * textureUVsize + texelSize * 0.5F;
            float yMin = y * textureUVsize + texelSize * 0.5F;

            float xMax = x * textureUVsize + textureUVsize - texelSize * 0.5F;
            float yMax = y * textureUVsize + textureUVsize - texelSize * 0.5F;

            float[] textureCoords = {
                xMax, yMax,
                xMin, yMax,
                xMin, yMin,
                xMax, yMin
            };

            return textureCoords;
        }

    }
}
