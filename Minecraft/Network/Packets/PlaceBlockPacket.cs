﻿namespace Minecraft
{
    class PlaceBlockPacket : Packet
    {
        public BlockState BlockState { get; private set; }
        public Vector3i BlockPos { get; private set; }

        public PlaceBlockPacket(BlockState blockState, Vector3i blockPos) : base(PacketType.PlaceBlock)
        {
            BlockState = blockState;
            BlockPos = blockPos;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlaceBlockPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteVector3i(BlockPos);
            bufferedStream.WriteInt32(BlockState.PayloadSize());
            BlockState.ToStream(bufferedStream);
        }

        public override string ToString()
        {
            return "[" + GetType() + "] -> " + BlockState.GetBlock().GetType() + " at " + BlockPos;
        }
    }
}
