using OpenTK;

namespace Minecraft
{
    class PlayerJoinAcceptPacket : Packet
    {
        public string name { get; private set; }
        public int playerId { get; private set; }
        public Vector3 spawnPosition { get; private set; }

        public PlayerJoinAcceptPacket(string name, int playerId, Vector3 spawnPosition)
            : base(PacketType.PlayerJoinAccept)
        {
            this.name = name;
            this.playerId = playerId;
            this.spawnPosition = spawnPosition;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinAcceptPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(playerId);
            bufferedStream.WriteUtf8String(name);
            bufferedStream.WriteFloat(spawnPosition.X);
            bufferedStream.WriteFloat(spawnPosition.Y);
            bufferedStream.WriteFloat(spawnPosition.Z);
        }
    }
}
