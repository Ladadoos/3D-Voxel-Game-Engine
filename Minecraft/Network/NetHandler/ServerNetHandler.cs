using OpenTK;
using System;

namespace Minecraft
{
    class ServerNetHandler : INetHandler
    {
        private Game game;
        private Connection playerConnection;

        public ServerNetHandler(Game game, Connection playerConnection)
        {
            this.game = game;
            this.playerConnection = playerConnection;
        }

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placedBlockpacket)
        {
            game.server.world.AddBlockToWorld(placedBlockpacket.blockPos, placedBlockpacket.blockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.server.world.QueueToRemoveBlockAt(removeBlockPacket.blockPos);
        }

        public void ProcessChatPacket(ChatPacket chatPacket)
        {
            Logger.Info("Server received message " + chatPacket.message);
            game.server.BroadcastPacket(chatPacket);
        }

        public void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessChunkUnloadPacket(ChunkUnloadPacket unloadChunkPacket)
        {
            Vector2 chunkGridPosition = new Vector2(unloadChunkPacket.gridX, unloadChunkPacket.gridZ);
            if (game.server.world.loadedChunks.TryGetValue(chunkGridPosition, out Chunk chunk))
            { 
                game.server.world.RemovePlayerPresenceOfChunk(chunk);
            }
        }

        public void ProcessChunkDataRequestPacket(ChunkDataRequestPacket packet)
        {
            Vector2 gridPosition = new Vector2(packet.gridX, packet.gridZ);
            if(!game.server.world.loadedChunks.TryGetValue(gridPosition, out Chunk chunk))
            {
                chunk = game.server.world.GenerateBlocksForChunk(packet.gridX, packet.gridZ);
                game.server.world.AddPlayerPresenceToChunk(chunk);
            }
            if(game.server.isOpen && !game.server.IsHost(playerConnection))
            {
                playerConnection.WritePacket(new ChunkDataPacket(chunk));
            }
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            playerConnection.player.position = playerDataPacket.position;
            game.server.BroadcastPacketExceptTo(playerConnection, playerDataPacket);
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            string playerName = playerJoinRequestPacket.name.Trim();
            if (playerName == string.Empty || playerName == "Player")
            {
                playerConnection.WritePacket(new PlayerLeavePacket(0, LeaveReason.Banned, "You are not allowed on this server."));
                playerConnection.state = ConnectionState.Closed;
                return;
            }
            int playerId = game.server.world.entityIdTracker.GenerateId();
            string serverPlayerName = "server_" + playerName + "_id_" + playerId;
            ServerPlayer player = new ServerPlayer(playerId, serverPlayerName, new Vector3(10, 100, 10));
            playerConnection.player = player;

            game.server.world.SpawnEntity(player);
            playerConnection.WritePacket(new PlayerJoinAcceptPacket(serverPlayerName, playerId)); // Accept join
            playerConnection.state = ConnectionState.Accepted;
            //Let all the online players know about the new player
            game.server.BroadcastPacketExceptTo(playerConnection, new PlayerJoinPacket(serverPlayerName, playerId));

            //let the new player know about all the already only players
            foreach (Connection client in game.server.clients)
            {
                if (client.player == player) continue;
                playerConnection.WritePacket(new PlayerJoinPacket(client.player.playerName, client.player.id));
            }
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessPlayerLeavePacket(PlayerLeavePacket playerKickPacket)
        {
            throw new NotImplementedException();
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            BlockState state = game.server.world.GetBlockAt(playerInteractionPacket.blockPos);
            //state.GetBlock().OnInteract(state, game.world);
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            game.server.UpdateKeepAliveFor(playerConnection);
        }
    }
}
