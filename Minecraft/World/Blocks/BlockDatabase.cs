using System.Collections.Generic;
using System;

namespace Minecraft
{
    class BlockDatabase
    {
        public TextureAtlas textureAtlas;
        public Dictionary<Block, float[]> staticBlockTextures = new Dictionary<Block, float[]>();

        public BlockDatabase()
        {
            int textureId = Game.textureLoader.LoadTexture("../../Resources/texturePack2.png");
            textureAtlas = new TextureAtlas(textureId, 256, 16);
        }

        public void RegisterBlocks()
        {
            RegisterStaticBlock(Block.Air, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            RegisterStaticBlock(Block.Dirt, 4, 1, 4, 1, 4, 1, 4, 1, 5, 1, 5, 1);
            /*Register(BlockType.Grass, 3, 0, 3, 0, 3, 0, 3, 0, 0, 0, 2, 0);
            Register(BlockType.Cobblestone, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1);
            Register(BlockType.Pumpkin, 8, 7, 6, 7, 6, 7, 6, 7, 6, 6, 6, 7);
            Register(BlockType.Brick, 7, 0, 7, 0, 7, 0, 7, 0, 7, 0, 7, 0);
            Register(BlockType.Redstone_Ore, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3);
            Register(BlockType.Dirt, 2, 0, 2, 0, 2, 0, 2, 0, 2, 0, 2, 0);
            Register(BlockType.Stone, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0);
            Register(BlockType.Sand, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1);
            Register(BlockType.Leaves, 5, 3, 5, 3, 5, 3, 5, 3, 5, 3, 5, 3);
            Register(BlockType.Snow, 2, 4, 2, 4, 2, 4, 2, 4, 2, 4, 2, 4);
            Register(BlockType.Gravel, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1, 3, 1);*/
        }

        private void RegisterStaticBlock(Block block, int baX, int baY, int rX, int rY, int fX, int fY, int lX, int lY, int tX, int tY, int boX, int boY)
        {
            //Back, right, front, left, top, bottom
            staticBlockTextures.Add(block, textureAtlas.GetCubeTextureCoords(baX, baY, rX, rY, fX, fY, lX, lY, tX, tY, boX, baY));
        }

    }
}
