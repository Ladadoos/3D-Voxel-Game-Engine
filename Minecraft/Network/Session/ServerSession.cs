﻿namespace Minecraft
{
    class ServerSession : Session
    {
        private readonly ChunkProvider chunkProvider;

        public ServerSession(Connection connection, INetHandler netHandler)
            : base(connection, netHandler)
        {
            chunkProvider = new ChunkProvider(this);
        }

        public bool IsBlockPositionInViewRange(Vector3i blockPos)
        {
            return chunkProvider.CurrentlyVisibleChunks.Contains(World.GetChunkPosition(blockPos.X, blockPos.Z));
        }

        public void Update()
        {
            chunkProvider.Update();
        }
    }
}
