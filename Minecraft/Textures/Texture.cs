namespace Minecraft
{
    class Texture
    {
        public int ID { get; private set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }

        public Texture(int textureId, int pixelWidth, int pixelHeight)
        {
            ID = textureId;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }    
        
        public Texture(string pathToFile, int pixelWidth, int pixelHeight)
        {
            ID = TextureLoader.LoadTexture(pathToFile);
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }
    }
}
