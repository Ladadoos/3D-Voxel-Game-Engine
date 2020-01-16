using ProtoBuf;
using System.Collections.Generic;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class NetSection
    {
        [ProtoMember(1)]
        public byte height;
        [ProtoMember(2)]
        public Dictionary<int, BlockState> blocks = new Dictionary<int, BlockState>();

        public Section ExtractSection(int gridX, int gridZ)
        {
            Section section = new Section(gridX, gridZ, height);
            if (blocks == null)
            {
                Logger.Warn("Section " + gridX + "/" + gridZ + " at " + height + " was uninitialized.");
                blocks = new Dictionary<int, BlockState>();
            }
            foreach (KeyValuePair<int, BlockState> block in blocks)
            {
                section.blocks[block.Key] = block.Value;
            }
            return section;
        }

        public NetSection(Section section)
        {
            height = section.height;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        BlockState state = section.GetBlockAt(x, y, z);
                        if (state != null)
                        {
                            int r = x * 16 * 16 + y * 16 + z;
                            blocks.Add(r, state);
                        }
                    }
                }
            }
        }
    }
}
