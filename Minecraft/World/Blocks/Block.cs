namespace Minecraft.World.Blocks
{
    abstract class Block
    {
        public float[] textureCoordinates;

        public Block(float[] textureCoordinates)
        {
            this.textureCoordinates = textureCoordinates;
        }
    }
}
