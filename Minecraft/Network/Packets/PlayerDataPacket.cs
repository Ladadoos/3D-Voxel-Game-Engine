using OpenTK;

namespace Minecraft
{
    class PlayerDataPacket : Packet
    {
        public Vector3 position { get; private set; }
        public int entityId { get; private set; }

        public PlayerDataPacket(Vector3 position, int entityId) : base(PacketType.EntityPosition)
        {
            this.position = position;
            this.entityId = entityId;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerDataPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(entityId);
            bufferedStream.WriteFloat(position.X);
            bufferedStream.WriteFloat(position.Y);
            bufferedStream.WriteFloat(position.Z);
        }
    }
}
