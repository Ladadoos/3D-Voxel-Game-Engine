using System;
using System.Collections.Generic;

namespace Minecraft
{
    [Serializable]
    class Chunk
    {
        [NonSerialized]
        private Dictionary<Vector3i, BlockState> tickableBlocks = new Dictionary<Vector3i, BlockState>();
        public Section[] sections = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX;
        public int gridZ;

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public void Tick(World world, float deltaTime)
        {
            foreach(BlockState state in tickableBlocks.Values)
            {
                state.GetBlock().OnTick(state, world, deltaTime);
            }
        }

        public void AddBlock(int localX, int worldY, int localZ, BlockState blockstate)
        {
            if (worldY < 0)
            {
                worldY = 0;
            } else if (worldY > Constants.MAX_BUILD_HEIGHT - 1)
            {
                worldY = Constants.MAX_BUILD_HEIGHT - 1;
            }

            int sectionHeight = worldY / Constants.SECTION_HEIGHT;
            if(sections[sectionHeight] == null)
            {
                sections[sectionHeight] = new Section(this, (byte)sectionHeight);
            }

            Vector3i blockPos = new Vector3i(localX + gridX * 16, worldY, localZ + gridZ * 16);
            int sectionLocalY = worldY - sectionHeight * Constants.SECTION_HEIGHT;
            if(blockstate.GetBlock() == Blocks.Air)
            {
                sections[sectionHeight].RemoveBlock(localX, sectionLocalY, localZ);
                if(tickableBlocks.TryGetValue(blockPos, out BlockState tickable))
                {
                    tickableBlocks.Remove(blockPos);
                }
            }
            else
            {
                sections[sectionHeight].AddBlock(localX, sectionLocalY, localZ, blockstate);

                if (blockstate.GetBlock().isTickable)
                {
                    tickableBlocks.Add(blockPos, blockstate);
                }               
            }         
        }
    }
}
