using OpenTK;

namespace Minecraft
{
    class PlayerDataPacket : Packet
    {
        public int EntityID { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }

        public PlayerDataPacket(int entityId, Vector3 position, Vector3 velocity) : base(PacketType.EntityPosition)
        {
            EntityID = entityId;
            Position = position;
            Velocity = velocity;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerDataPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(EntityID);
            bufferedStream.WriteVector3(Position);
            bufferedStream.WriteVector3(Velocity);
        }
    }
}
