namespace Minecraft
{
    class ForestBiome : Biome
    {
        private TreeGenerator treeGenerator;

        public ForestBiome()
        {
            treeGenerator = new TreeGenerator();
        }

        public override void Decorate(Chunk chunk, int x, int y, int z)
        {
            treeGenerator.GenerateTree(chunk, x, y, z);
        }
    }
}
