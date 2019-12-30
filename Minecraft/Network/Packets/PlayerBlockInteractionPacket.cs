using OpenTK;

namespace Minecraft
{
    class PlayerBlockInteractionPacket : Packet
    {
        public Vector3 intPosition { get; private set; }

        public PlayerBlockInteractionPacket(Vector3 intPosition) : base(PacketType.PlayerBlockInteraction)
        {
            this.intPosition = intPosition;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerBlockInteractionpacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteFloat(intPosition.X);
            bufferedStream.WriteFloat(intPosition.Y);
            bufferedStream.WriteFloat(intPosition.Z);
        }
    }
}
