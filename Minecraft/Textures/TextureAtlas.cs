using OpenTK;

namespace Minecraft
{
    class TextureAtlas : Texture
    {
        private readonly float cellUVSize;

        public TextureAtlas(int textureId, int atlasSizeInPixels, int atlasCellSizeInPixels) : base(textureId, atlasSizeInPixels, atlasSizeInPixels)
        {
            int cellsPerRow = atlasSizeInPixels / atlasCellSizeInPixels;
            cellUVSize = 1.0F / cellsPerRow;
        }

        public Vector2[] GetTextureCoords(float atlasGridX, float atlasGridY)
        {
            float xMin = atlasGridX * cellUVSize;
            float yMin = atlasGridY * cellUVSize;
            float xMax = atlasGridX * cellUVSize + cellUVSize;
            float yMax = atlasGridY * cellUVSize + cellUVSize;

            return new Vector2[]
            {
                new Vector2(xMax, yMax),
                new Vector2(xMin, yMax),
                new Vector2(xMin, yMin),
                new Vector2(xMax, yMin)
            };
        }

        public Vector2[] GetTextureCoords(Vector2 atlatGrid)
        {
            return GetTextureCoords(atlatGrid.X, atlatGrid.Y);
        }
    }
}
