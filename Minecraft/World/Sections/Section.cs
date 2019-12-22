using System.Collections.Generic;

namespace Minecraft
{
    class Section
    {  
        public byte height;
        public BlockState[,,] blocks = new BlockState[Constants.CHUNK_SIZE, Constants.SECTION_HEIGHT, Constants.CHUNK_SIZE];

        public Section(byte height)
        {
            this.height = height;
        }

        public void AddBlock(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[localX, localY, localZ] = blockstate;
        }

        //Consider optimalization where you completely ignore a section if its fully opaque and surrounding section walls are full opaque
        public void RemoveBlock(int localX, int localY, int localZ)
        {
            blocks[localX, localY, localZ] = null;
        }

        public void ToStream(NetBufferedStream bufferedStream)
        {
            List<BlockState> states = new List<BlockState>();
            for(int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        //NULL = AIR check
                        if(blocks[x, y, z] != null)
                        {
                            states.Add(blocks[x, y, z]);
                        }
                    }
                }
            }

            bufferedStream.WriteInt32(states.Count);
            foreach(BlockState state in states)
            {
                state.ToStream(bufferedStream);
            }
        }
    }
}
