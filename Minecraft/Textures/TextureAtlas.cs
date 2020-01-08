using OpenTK;

namespace Minecraft
{
    class TextureAtlas : Texture
    {
        private float cellUVSize;

        public TextureAtlas(int textureId, int atlasSizeInPixels, int atlasCellSizeInPixels) : base(textureId, atlasSizeInPixels, atlasSizeInPixels)
        {
            int cellsPerRow = atlasSizeInPixels / atlasCellSizeInPixels;
            cellUVSize = 1.0F / cellsPerRow;
        }

        public float[] GetTextureCoords(float atlasGridX, float atlasGridY)
        {
            float xMin = atlasGridX * cellUVSize;
            float yMin = atlasGridY * cellUVSize;
            float xMax = atlasGridX * cellUVSize + cellUVSize;
            float yMax = atlasGridY * cellUVSize + cellUVSize;

            return new float[]{ xMax, yMax, xMin, yMax, xMin, yMin, xMax, yMin };
        }

        public float[] GetTextureCoords(Vector2 atlatGrid)
        {
            return GetTextureCoords(atlatGrid.X, atlatGrid.Y);
        }
    }
}
