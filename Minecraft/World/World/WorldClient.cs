namespace Minecraft
{
    class WorldClient : World
    {
        public WorldClient(Game game) : base(game)
        {
            OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
            OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            OnChunkUnloadedHandler += game.masterRenderer.OnChunkUnloaded;
        }
    }
}
