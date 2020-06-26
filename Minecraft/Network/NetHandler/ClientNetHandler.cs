using OpenTK;
using System;
using System.Linq;

namespace Minecraft
{
    class ClientNetHandler : INetHandler
    {
        private readonly Game game;
        private ClientSession session;

        public ClientNetHandler(Game game)
        {
            this.game = game;
        }

        public void AssignSession(ClientSession session) => this.session = session;

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placeBlockPacket)
        {
            game.World.QueueToAddBlockAt(placeBlockPacket.BlockPos, placeBlockPacket.BlockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.World.QueueToRemoveBlocksAt(removeBlockPacket.BlockPositions);
        }

        public void ProcessChatPacket(ChatPacket chatPacket)
        {
            game.MasterRenderer.IngameCanvas.AddUserMessage(chatPacket.Sender, chatPacket.Message);
        }

        public void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket)
        {
            game.World.AddPlayerPresenceToChunk(chunkDataPacket.Chunk);
        }

        public void ProcessChunkUnloadPacket(ChunkUnloadPacket unloadChunkPacket)
        {
            foreach(Vector2 chunkGridPosition in unloadChunkPacket.ChunkGridPositions)
            {
                if (game.World.LoadedChunks.TryGetValue(chunkGridPosition, out Chunk chunk))
                {
                    if(!game.World.RemovePlayerPresenceOfChunk(chunk))
                        throw new Exception();
                }
            }
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            Logger.Info(playerDataPacket.ToString());
            if(!game.World.LoadedEntities.TryGetValue(playerDataPacket.EntityID, out Entity player))
            {
                Logger.Error("Received positional data for unregistered player " + playerDataPacket.EntityID);
                return;
            }
            if(!(player is OtherClientPlayer))
            {
                Logger.Error("Something else than other player stored in players map: " + player.GetType());
                return;
            }
            OtherClientPlayer otherPlayer = (OtherClientPlayer)player;
            otherPlayer.ServerPosition = playerDataPacket.Position;
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            Logger.Info("You: " + playerJoinAcceptPacket.Name + " connected.");

            game.ClientPlayer.ID = playerJoinAcceptPacket.PlayerID;
            game.ClientPlayer.Name = playerJoinAcceptPacket.Name;
            game.ClientPlayer.Position = playerJoinAcceptPacket.SpawnPosition;
            session.State = SessionState.Accepted;

            game.World.Environment.CurrentTime = playerJoinAcceptPacket.CurrentTime;

            game.World.SpawnEntity(game.ClientPlayer);
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            OtherClientPlayer otherPlayer = new OtherClientPlayer(playerJoinPacket.PlayerID, playerJoinPacket.Name, game.World);
            UICanvasEntityName playerNameCanvas = new UICanvasEntityName(game, otherPlayer, playerJoinPacket.Name);
            game.MasterRenderer.AddCanvas(playerNameCanvas);
            game.World.SpawnEntity(otherPlayer);
        }

        public void ProcessPlayerLeavePacket(PlayerLeavePacket playerLeavePacket)
        {
            if(playerLeavePacket.ID == 0)
            {
                Logger.Info("You were disconnected for reason: " + playerLeavePacket.Reason + " Message: " + playerLeavePacket.Message);
                session.State = SessionState.Closed;
            } else
            {
                Logger.Info("Player " + playerLeavePacket.ID + " left for reason " + playerLeavePacket.Reason + " with message " + playerLeavePacket.Message);
            }
            game.World.DespawnEntity(playerLeavePacket.ID);
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            Vector3i blockPos = playerInteractionPacket.BlockPos;
            BlockState state = game.World.GetBlockAt(blockPos);
            state.GetBlock().OnInteract(state, blockPos, game.World);
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            throw new InvalidOperationException();
        }
    }
}
