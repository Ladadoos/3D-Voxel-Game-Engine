using OpenTK;

namespace Minecraft
{
    class PlayerJoinAcceptPacket : Packet
    {
        public string Name { get; private set; }
        public int PlayerID { get; private set; }
        public Vector3 SpawnPosition { get; private set; }

        public PlayerJoinAcceptPacket(string name, int playerId, Vector3 spawnPosition)
            : base(PacketType.PlayerJoinAccept)
        {
            Name = name;
            PlayerID = playerId;
            SpawnPosition = spawnPosition;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinAcceptPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(PlayerID);
            bufferedStream.WriteUtf8String(Name);
            bufferedStream.WriteVector3(SpawnPosition);
        }
    }
}
