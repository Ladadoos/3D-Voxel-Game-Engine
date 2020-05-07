﻿namespace Minecraft
{
    class ServerSession : Session
    {
        private readonly ChunkProvider chunkProvider;

        public ServerSession(Connection connection, INetHandler netHandler)
            : base(connection, netHandler)
        {
            chunkProvider = new ChunkProvider(this, playerSettings);
        }

        public bool IsBlockPositionInViewRange(Vector3i blockPos)
        {
            return chunkProvider.currentlyVisibleChunks.Contains(World.GetChunkPosition(blockPos.X, blockPos.Z));
        }

        public void Update()
        {
            chunkProvider.Update();
        }
    }
}
