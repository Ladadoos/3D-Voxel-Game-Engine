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
            byte[] bytes = DataConverter.ObjectToByteArray(chunk);
            bufferedStream.WriteInt32(bytes.Length);
            bufferedStream.WriteBytes(bytes);
        }
    }
}
