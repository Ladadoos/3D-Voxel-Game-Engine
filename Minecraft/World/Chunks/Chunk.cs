using System.Collections.Generic;

namespace Minecraft
{
    class Chunk
    {
        public Dictionary<Vector3i, BlockState> tickableBlocks { get; set; } = new Dictionary<Vector3i, BlockState>();
        public Section[] sections { get; set; } = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX { get; private set; }
        public int gridZ { get; private set; }

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public void Tick(float deltaTime, World world)
        {
            foreach(KeyValuePair<Vector3i, BlockState> kp in tickableBlocks)
            {
                kp.Value.GetBlock().OnTick(kp.Value, world, kp.Key, deltaTime);
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
                sections[sectionHeight] = new Section(gridX, gridZ, (byte)sectionHeight);
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

                if (blockstate.GetBlock().isTickable && !tickableBlocks.ContainsKey(blockPos))
                {
                    tickableBlocks.Add(blockPos, blockstate);
                }               
            }         
        }
    }
}
