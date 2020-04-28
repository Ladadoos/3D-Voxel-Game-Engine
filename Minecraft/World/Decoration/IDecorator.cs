namespace Minecraft
{
    interface IDecorator
    {
        void Decorate(Chunk chunk, int worldY, int localX, int localZ);
    }
}
