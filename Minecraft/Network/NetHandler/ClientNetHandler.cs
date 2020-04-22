using OpenTK;
using System;

namespace Minecraft
{
    class ClientNetHandler : INetHandler
    {
        private Game game;
        private ClientSession session;

        public ClientNetHandler(Game game)
        {
            this.game = game;
        }

        public void AssignSession(ClientSession session) => this.session = session;

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
            game.world.AddPlayerPresenceToChunk(chunkDataPacket.chunk);
        }

        public void ProcessChunkUnloadPacket(ChunkUnloadPacket unloadChunkPacket)
        {
            foreach(Vector2 chunkGridPosition in unloadChunkPacket.chunkGridPositions)
            {
                if (game.world.loadedChunks.TryGetValue(chunkGridPosition, out Chunk chunk))
                {
                    game.world.RemovePlayerPresenceOfChunk(chunk);
                }
            }
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            Logger.Info(playerDataPacket.ToString());
            if(!game.world.loadedEntities.TryGetValue(playerDataPacket.entityId, out Entity player))
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
            game.player.playerName = playerJoinAcceptPacket.name;
            session.state = SessionState.Accepted;

            game.world.SpawnEntity(game.player);
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            OtherClientPlayer otherPlayer = new OtherClientPlayer(playerJoinPacket.playerId, playerJoinPacket.name);
            UICanvasEntityName playerNameCanvas = new UICanvasEntityName(game, otherPlayer, playerJoinPacket.name);
            game.masterRenderer.AddCanvas(playerNameCanvas);
            game.world.SpawnEntity(otherPlayer);
        }

        public void ProcessPlayerLeavePacket(PlayerLeavePacket playerLeavePacket)
        {
            if(playerLeavePacket.id == 0)
            {
                Logger.Info("You were disconnected for reason: " + playerLeavePacket.reason + " Message: " + playerLeavePacket.message);
                session.state = SessionState.Closed;
            } else
            {
                Logger.Info("Player " + playerLeavePacket.id + " left for reason " + playerLeavePacket.reason + " with message " + playerLeavePacket.message);
            }
            game.world.DespawnEntity(playerLeavePacket.id);
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            Vector3i blockPos = playerInteractionPacket.blockPos;
            BlockState state = game.world.GetBlockAt(blockPos);
            state.GetBlock().OnInteract(state, blockPos, game.world);
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            throw new InvalidOperationException();
        }
    }
}
