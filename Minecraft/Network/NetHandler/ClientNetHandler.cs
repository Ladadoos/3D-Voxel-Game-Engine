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

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placedBlockPacket)
        {
            game.world.AddBlockToWorld(placedBlockPacket.blockPos, placedBlockPacket.blockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.world.QueueToRemoveBlockAt(removeBlockPacket.blockPos);
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
            if(!game.world.playerEntities.TryGetValue(playerDataPacket.entityId, out Player player))
            {
                Logger.Error("Received positional data for unregistered player " + playerDataPacket.entityId);
                return;
            }
            if(!(player is OtherClientPlayer))
            {
                Logger.Error("Something else than other player stored in players map: " + player.GetType());
                return;
            }
            OtherClientPlayer otherPlayer = (OtherClientPlayer)player;
            otherPlayer.serverPosition = playerDataPacket.position;
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            Logger.Info("You: " + playerJoinAcceptPacket.name + " connected.");

            game.player.id = playerJoinAcceptPacket.playerId;
            playerConnection.state = ConnectionState.Accepted;
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            Player otherPlayer = new OtherClientPlayer(playerJoinPacket.playerId);
            game.world.playerEntities.Add(playerJoinPacket.playerId, otherPlayer);
        }

        public void ProcessPlayerKickPacket(PlayerKickPacket playerKickPacket)
        {
            playerConnection.state = ConnectionState.Closed;
            Logger.Info("You were kicked for reason: " + playerKickPacket.reason + " Message: " + playerKickPacket.message);
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            BlockState state = game.world.GetBlockAt(playerInteractionPacket.blockPos);
            state.GetBlock().OnInteract(state, game.world);
        }
    }
}
