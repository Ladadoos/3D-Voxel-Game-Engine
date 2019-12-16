namespace Minecraft
{
    abstract class Texture
    {
        public int textureId { get; protected set; }

        public Texture(int textureId)
        {
            this.textureId = textureId;
        }      
    }
}
