namespace Minecraft
{
    class ChunkDataPacket : Packet
    {
        public Chunk Chunk { get; private set; }

        public ChunkDataPacket(Chunk chunk) : base(PacketType.ChunkData)
        {     
            Chunk = chunk;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkDataPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(Chunk.GridX);
            bufferedStream.WriteInt32(Chunk.GridZ);
            bufferedStream.WriteInt32(GetChunkPayloadByteSize());

            for(int i = 0; i < Constants.NUM_SECTIONS_IN_CHUNKS; i++)
            {
                Section section = Chunk.Sections[i];
                if(section == null)
                {
                    bufferedStream.WriteBool(false);
                } else
                {
                    bufferedStream.WriteBool(true);
                    for(int x = 0; x < 16; x++)
                    {
                        for(int y = 0; y < 16; y++)
                        {
                            for(int z = 0; z < 16; z++)
                            {
                                BlockState state = section.GetBlockAt(x, y, z);
                                if(state == null)
                                {
                                    bufferedStream.WriteUInt16(0);
                                } else
                                {
                                    state.ToStream(bufferedStream);
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetChunkPayloadByteSize()
        {
            int size = 0;
            for(int i = 0; i < Constants.NUM_SECTIONS_IN_CHUNKS; i++)
            {
                Section section = Chunk.Sections[i];
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
