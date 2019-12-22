using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class Chunk
    {
        private Dictionary<Vector3, BlockState> tickableBlocks = new Dictionary<Vector3, BlockState>();
        public Section[] sections = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX;
        public int gridZ;

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(gridX);
            bufferedStream.WriteInt32(gridZ);

            List<Section> validSections = new List<Section>();
            foreach (Section section in sections)
            {
                if (section != null)
                {
                    validSections.Add(section);
                }
            }

            bufferedStream.WriteInt32(validSections.Count);
            foreach (Section section in validSections)
            {
                section.ToStream(bufferedStream);
            }
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
                sections[sectionHeight] = new Section((byte)sectionHeight);
            }

            int sectionLocalY = worldY - sectionHeight * Constants.SECTION_HEIGHT;
            if(blockstate.GetBlock() == Blocks.Air)
            {
                sections[sectionHeight].RemoveBlock(localX, sectionLocalY, localZ);
                if(tickableBlocks.TryGetValue(blockstate.position, out BlockState tickable))
                {
                    tickableBlocks.Remove(blockstate.position);
                }
            }
            else
            {
                blockstate.position = new Vector3(localX + gridX * 16, worldY, localZ + gridZ * 16);
                sections[sectionHeight].AddBlock(localX, sectionLocalY, localZ, blockstate);

                if (blockstate.GetBlock().isTickable)
                {
                    tickableBlocks.Add(blockstate.position, blockstate);
                }               
            }         
        }
    }
}
