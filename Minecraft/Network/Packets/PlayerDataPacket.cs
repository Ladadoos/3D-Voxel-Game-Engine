using OpenTK;

namespace Minecraft
{
    class PlayerDataPacket : Packet
    {
        public Vector3 Position { get; private set; }
        public int EntityID { get; private set; }

        public PlayerDataPacket(Vector3 position, int entityId) : base(PacketType.EntityPosition)
        {
            Position = position;
            EntityID = entityId;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerDataPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(EntityID);
            bufferedStream.WriteVector3(Position);
        }
    }
}
