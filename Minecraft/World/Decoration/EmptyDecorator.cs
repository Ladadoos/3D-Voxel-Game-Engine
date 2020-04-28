namespace Minecraft
{
    class EmptyDecorator : IDecorator
    {
        public void Decorate(Chunk chunk, int worldY, int localX, int localZ)
        {
        }
    }
}
