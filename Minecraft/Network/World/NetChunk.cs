using ProtoBuf;
using System.Collections.Generic;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class NetChunk
    {
        [ProtoMember(1)]
        public Dictionary<int, NetSection> sections = new Dictionary<int, NetSection>();
        [ProtoMember(2)]
        public Dictionary<Vector3i, BlockState> tickableBlocks = new Dictionary<Vector3i, BlockState>();
        [ProtoMember(3)]
        public int gridX;
        [ProtoMember(4)]
        public int gridZ;

        public Chunk ExtractChunk()
        {
            Chunk chunk = new Chunk(gridX, gridZ);
            chunk.sections = new Section[Constants.SECTIONS_IN_CHUNKS];
            foreach(KeyValuePair<int, NetSection> section in sections)
            {
                chunk.sections[section.Key] = section.Value.ExtractSection(gridX, gridZ);
            }
            chunk.tickableBlocks = tickableBlocks;
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
            tickableBlocks = chunk.tickableBlocks;
        }
    }
}
