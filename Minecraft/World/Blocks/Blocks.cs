using System.Collections.Generic;

namespace Minecraft
{
    class Blocks
    {
        public static BlockState GetState(Block block)
        {
            if(block.HasCustomState)
                return block.GetNewDefaultState();
            int arrayId = block.ID - 1;
            return defaultStates[arrayId];
        }

        public static readonly Block Air = new BlockAir(1);
        public static readonly Block Dirt = new BlockDirt(2);
        public static readonly Block Stone = new BlockStone(3);
        public static readonly Block Flower = new BlockFlower(4);
        public static readonly Block Tnt = new BlockTNT(5);
        public static readonly Block Grass = new BlockGrass(6);
        public static readonly Block Sand = new BlockSand(7);
        public static readonly Block SugarCane = new BlockSugarCane(8);
        public static readonly Block Wheat = new BlockWheat(9);
        public static readonly Block SandStone = new BlockSandstone(10);
        public static readonly Block GrassBlade = new BlockGrassBlade(11);
        public static readonly Block DeadBush = new BlockDeadBush(12);
        public static readonly Block Cactus = new BlockCactus(13);
        public static readonly Block OakLog = new BlockOakLog(14);
        public static readonly Block OakLeaves = new BlockOakLeaves(15);
        public static readonly Block Gravel = new BlockGravel(16);

        public static int Count { get { return registeredBlocks.Length; } }
        private static Block[] registeredBlocks;
        private static BlockState[] defaultStates;

        public static void RegisterBlocks()
        {
            List<Block> blocks = new List<Block>()
            {
                Air,
                Dirt,
                Stone,
                Flower,
                Tnt,
                Grass,
                Sand,
                SugarCane,
                Wheat,
                SandStone,
                GrassBlade,
                DeadBush,
                Cactus,
                OakLog,
                OakLeaves,
                Gravel
            };

            registeredBlocks = blocks.ToArray();
            defaultStates = new BlockState[registeredBlocks.Length];
            for(int i = 0; i < registeredBlocks.Length; i++)
                defaultStates[i] = blocks[i].GetNewDefaultState();
        }

        public static Block GetBlockFromIdentifier(int id)
        {
            id--;
            if (id < 0 || id >= registeredBlocks.Length)
                throw new System.Exception("Invalid id: " + id);

            return registeredBlocks[id];
        }
    }
}

