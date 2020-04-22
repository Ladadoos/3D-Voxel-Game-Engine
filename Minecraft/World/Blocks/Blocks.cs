using System.Collections.Generic;

namespace Minecraft
{
    class Blocks
    {
        public static readonly Block Air = new BlockAir(1);
        public static readonly Block Dirt = new BlockDirt(2);
        public static readonly Block Stone = new BlockStone(3);
        public static readonly Block Flower = new BlockFlower(4);
        public static readonly Block Tnt = new BlockTNT(5);
        public static readonly Block Grass = new BlockGrass(6);
        public static readonly Block Sand = new BlockSand(7);

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
                Grass,
                Sand
            };
        }

        public static Block GetBlockFromIdentifier(int id)
        {
            id -= 1;
            if (id < 0 || id >= registeredBlocks.Count)
            {
                throw new System.Exception("Invalid id: " + id);
            }
            if(id == 0)
            {
                //System.Console.WriteLine("got air" );
            }
            return registeredBlocks[id];
        }
    }
}

