using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkUnloadPacket : Packet
    {
        public List<Vector2> chunkGridPositions { get; private set; }

        public ChunkUnloadPacket(List<Vector2> chunkGridPositions) : base(PacketType.ChunkUnload)
        {
            this.chunkGridPositions = chunkGridPositions;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkUnloadPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(chunkGridPositions.Count);
            foreach(Vector2 gridPos in chunkGridPositions)
            {
                bufferedStream.WriteInt32((int)gridPos.X);
                bufferedStream.WriteInt32((int)gridPos.Y);
            }
        }
    }
}
