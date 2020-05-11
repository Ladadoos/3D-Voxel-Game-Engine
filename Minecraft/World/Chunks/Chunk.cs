using System.Collections.Generic;

namespace Minecraft
{
    class Chunk
    {
        public Dictionary<Vector3i, BlockState> TickableBlocks { get; set; } = new Dictionary<Vector3i, BlockState>();
        public Section[] Sections { get; set; } = new Section[Constants.NUM_SECTIONS_IN_CHUNKS];
        public int GridX { get; private set; }
        public int GridZ { get; private set; }

        public Chunk(int gridX, int gridZ)
        {
            GridX = gridX;
            GridZ = gridZ;
        }

        public void Tick(float deltaTime, World world)
        {
            foreach(KeyValuePair<Vector3i, BlockState> kp in TickableBlocks)
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

            int sectionHeight = worldY / 16;
            if(Sections[sectionHeight] == null)
            {
                Sections[sectionHeight] = new Section(GridX, GridZ, (byte)sectionHeight);
            }

            Vector3i blockPos = new Vector3i(localX + GridX * 16, worldY, localZ + GridZ * 16);
            int sectionLocalY = worldY - sectionHeight * 16;
            if(blockstate.GetBlock() == Blocks.Air)
            {
                Sections[sectionHeight].RemoveBlockAt(localX, sectionLocalY, localZ);
                if(TickableBlocks.TryGetValue(blockPos, out BlockState tickable))
                {
                    TickableBlocks.Remove(blockPos);
                }
            }
            else
            {
                Sections[sectionHeight].AddBlockAt(localX, sectionLocalY, localZ, blockstate);

                if (blockstate.GetBlock().IsTickable)
                {
                    TickableBlocks.Add(blockPos, blockstate);
                }               
            }         
        }

        public int GetPayloadSize()
        {
            int size = 0;
            for(int i = 0; i < Constants.NUM_SECTIONS_IN_CHUNKS; i++)
            {
                Section section = Sections[i];
                size++;
                if(section == null)
                {
                    continue;
                }
                for(int x = 0; x < 16; x++)
                {
                    for(int y = 0; y < 16; y++)
                    {
                        for(int z = 0; z < 16; z++)
                        {
                            BlockState state = section.GetBlockAt(x, y, z);
                            size += 2;
                            if(state != null)
                            {
                                size += state.PayloadSize();
                            }
                        }
                    }
                }
            }
            return size;
        }
    }
}
