using OpenTK;

namespace Minecraft
{
    class PlayerJoinAcceptPacket : Packet
    {
        public string Name { get; private set; }
        public int PlayerID { get; private set; }
        public Vector3 SpawnPosition { get; private set; }
        public float CurrentTime { get; private set; }

        public PlayerJoinAcceptPacket(string name, int playerId, Vector3 spawnPosition, float currentTime)
            : base(PacketType.PlayerJoinAccept)
        {
            Name = name;
            PlayerID = playerId;
            SpawnPosition = spawnPosition;
            CurrentTime = currentTime;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinAcceptPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(PlayerID);
            bufferedStream.WriteUtf8String(Name);
            bufferedStream.WriteVector3(SpawnPosition);
            bufferedStream.WriteFloat(CurrentTime);
        }
    }
}
