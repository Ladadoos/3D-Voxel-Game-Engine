using System;

namespace Minecraft
{
    class ClientNetHandler : INetHandler
    {
        private Game game;
        private Connection playerConnection;

        public ClientNetHandler(Game game, Connection playerConnection)
        {
            this.game = game;
            this.playerConnection = playerConnection;
        }

        public void ProcessPlaceBlockPacket(PlaceBlockPacket blockPacket)
        {
            game.world.AddBlockToWorld(blockPacket.state.position, blockPacket.state);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.world.QueueToRemoveBlockAt(removeBlockPacket.position);
        }

        public void ProcessChatPacket(ChatPacket chatPacket)
        {
            Logger.Info("Client received message " + chatPacket.message);
        }

        public void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket)
        {
            game.world.LoadChunk(chunkDataPacket.chunk);
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            Logger.Info(playerDataPacket.ToString());
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            Logger.Info("Player: " + playerJoinAcceptPacket.name + " connected.");
            playerConnection.state = ConnectionState.Accepted;
        }

        public void ProcessPlayerKickPacket(PlayerKickPacket playerKickPacket)
        {
            playerConnection.state = ConnectionState.Closed;
            Logger.Info("You were kicked for reason: " + playerKickPacket.reason + " Message: " + playerKickPacket.message);
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            BlockState state = game.world.GetBlockAt(playerInteractionPacket.intPosition);

        }
    }
}
