namespace Minecraft
{
    class ChunkDataPacket : Packet
    {
        public Chunk chunk { get; private set; }

        public ChunkDataPacket(Chunk chunk) : base(PacketType.ChunkData)
        {     
            this.chunk = chunk;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkDataPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(chunk.gridX);
            bufferedStream.WriteInt32(chunk.gridZ);
            bufferedStream.WriteInt32(GetChunkPayloadByteSize());

            for(int i = 0; i < 16; i++)
            {
                Section section = chunk.sections[i];
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
            for(int i = 0; i < 16; i++)
            {
                Section section = chunk.sections[i];
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
