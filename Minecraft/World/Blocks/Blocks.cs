using System.Collections.Generic;

namespace Minecraft
{
    class Blocks
    {
        public static readonly Block Air = new BlockAir(0);
        public static readonly Block Dirt = new BlockDirt(1);
        public static readonly Block Stone = new BlockStone(2);
        public static readonly Block Flower = new BlockFlower(3);
        public static readonly Block Tnt = new BlockTNT(4);
        public static readonly Block Grass = new BlockGrass(5);

        private static List<Block> registeredBlocks;

        public static void RegisterBlocks()
        {
            registeredBlocks = new List<Block>()
            {
                Air,
                Dirt,
                Stone,
                Flower,
                Tnt,
                Grass
            };
        }

        public static Block GetBlockFromIdentifier(int id)
        {
            if (id < 0 || id >= registeredBlocks.Count)
            {
                throw new System.Exception("Invalid id: " + id);
            }
            return registeredBlocks[id];
        }
    }
}

