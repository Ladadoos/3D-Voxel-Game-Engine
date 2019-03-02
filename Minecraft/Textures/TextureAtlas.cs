using System;

namespace Minecraft.Textures
{
    class TextureAtlas : Texture
    {
        //center offset variable
        //https://gamedev.stackexchange.com/questions/46963/how-to-avoid-texture-bleeding-in-a-texture-atlas


        public int atlasSize;
        public int textureSize;

        private int texturesPerRow;
        private float unitSize;
        private float centerOffset;

        public TextureAtlas(int textureId, int atlasSize, int textureSize) : base(textureId)
        {
            this.atlasSize = atlasSize;
            this.textureSize = textureSize;

            texturesPerRow = atlasSize / textureSize;
            unitSize = 1.0F / texturesPerRow;
            centerOffset = 1.0F / (atlasSize * 2);
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
            float xMin = x * unitSize + centerOffset;
            float yMin = y * unitSize + centerOffset;

            float xMax = x * unitSize  + unitSize - centerOffset;
            float yMax = y * unitSize  + unitSize - centerOffset;

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
