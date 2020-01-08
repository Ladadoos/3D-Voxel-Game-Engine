namespace Minecraft
{
    class Texture
    {
        public int textureId { get; private set; }
        public int pixelWidth { get; private set; }
        public int pixelHeight { get; private set; }

        public Texture(int textureId, int pixelWidth, int pixelHeight)
        {
            this.textureId = textureId;
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
        }      
    }
}
