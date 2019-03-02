﻿
using OpenTK;

using Minecraft.World.Blocks;
using Minecraft.Tools;
using Minecraft.World.Sections;

namespace Minecraft.World
{
    class Chunk
    {
        public Model model;

        public Section[] sections = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX;
        public int gridZ;

        public Matrix4 transformationMatrix;

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;

            transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridX * Constants.CHUNK_SIZE, 0, gridZ * Constants.CHUNK_SIZE), 0, 0, 0, 1, 1, 1);
        }

        public void AddBlock(int x, int y, int z, BlockType block)
        {
            int h = y / Constants.SECTION_HEIGHT;
            if(sections[h] == null)
            {
                sections[h] = new Section((sbyte)h);
            }

            int localY = y - h * Constants.SECTION_HEIGHT;
            if(block == BlockType.Air)
            {
                sections[h].RemoveBlock(x, localY, z);
            }
            else
            {
                sections[h].AddBlock(x, localY, z, block);
            }
            
        }
    }
}
