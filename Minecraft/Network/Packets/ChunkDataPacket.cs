using ProtoBuf;

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
            NetChunk netChunk = new NetChunk(chunk);
            Serializer.SerializeWithLengthPrefix(bufferedStream.bufferedStream, netChunk, PrefixStyle.Base128);
        }
    }
}
