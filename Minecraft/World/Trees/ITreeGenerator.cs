namespace Minecraft
{
    interface ITreeGenerator
    {
        void GenerateTreeAt(Chunk chunk, int worldX, int worldY, int worldZ);
    }
}
