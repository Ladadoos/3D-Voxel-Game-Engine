using System;

using ProtoBuf;

namespace Minecraft
{
    [ProtoContract]
    class Section
    {  
        [ProtoMember(1)]
        public byte height;
        [ProtoMember(2)]
        public BlockState[] blocks = new BlockState[Constants.CHUNK_SIZE * Constants.SECTION_HEIGHT * Constants.CHUNK_SIZE];
        [ProtoMember(3)]
        public int gridX;
        [ProtoMember(4)]
        public int gridZ;

        public Section(int gridX, int gridZ, byte height)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
            this.height = height;
        }

        public Section() { }

        public Section DeepCopy()
        {
            Section newSection = new Section(gridX, gridZ, height);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int r = x * 16 * 16 + y * 16 + z;
                        newSection.blocks[r] = blocks[r]?.ShallowCopy();
                    }
                }
            }
            return newSection;
        }

        public void AddBlock(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[localX * 16 * 16 + localY * 16 + localZ] = blockstate;
        }

        //Consider optimalization where you completely ignore a section if its fully opaque and surrounding section walls are full opaque
        public void RemoveBlock(int localX, int localY, int localZ)
        {
            blocks[localX * 16 * 16 + localY * 16 + localZ] = null;
        }

        public BlockState GetBlockAt(int localX, int localY, int localZ)
        {
            //Console.WriteLine(localX + "|" + localY + "|" + localZ + " mult " + (localX * 16 * 16 + localY * 16 + localZ) + " len " + blocks.Length);
            return blocks[localX * 16 * 16 + localY * 16 + localZ];
        }
    }
}
