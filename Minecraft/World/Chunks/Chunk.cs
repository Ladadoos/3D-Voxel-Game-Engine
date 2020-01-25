using System.Collections.Generic;

namespace Minecraft
{
    class Chunk
    {
        public Dictionary<Vector3i, BlockState> tickableBlocks = new Dictionary<Vector3i, BlockState>();
        public Section[] sections = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX { get; private set; }
        public int gridZ { get; private set; }

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public Chunk DeepClone()
        {
            Chunk newChunk = new Chunk(gridX, gridZ);
            Section[] newSections = new Section[Constants.SECTIONS_IN_CHUNKS];

            for (int i = 0; i < 16; i++)
            {
                newSections[i] = sections[i]?.DeepCopy();
                if (newSections[i] != null)
                {
                    newSections[i].gridX = gridX;
                    newSections[i].gridZ = gridZ;
                }

            }
            newChunk.sections = newSections;
            return newChunk;
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
