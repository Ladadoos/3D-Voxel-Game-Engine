

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
            if(blocks == null)
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

    [ProtoContract(SkipConstructor = true)]
    class NetChunk
    {
        [ProtoMember(1)]
        public Dictionary<int, NetSection> sections = new Dictionary<int, NetSection>();
        [ProtoMember(2)]
        public int gridX;
        [ProtoMember(3)]
        public int gridZ;

        public Chunk ExtractChunk()
        {
            Chunk chunk = new Chunk(gridX, gridZ);
            chunk.sections = new Section[Constants.SECTIONS_IN_CHUNKS];
            foreach(KeyValuePair<int, NetSection> section in sections)
            {
                chunk.sections[section.Key] = section.Value.ExtractSection(gridX, gridZ);
            }
            return chunk;
        }

        public NetChunk(Chunk chunk)
        {
            gridX = chunk.gridX;
            gridZ = chunk.gridZ;
            for(int i = 0; i < Constants.SECTIONS_IN_CHUNKS; i++)
            {
                Section section = chunk.sections[i];
                if(section != null)
                {
                    sections.Add(i, new NetSection(section));
                }
            }
        }
    }
}
