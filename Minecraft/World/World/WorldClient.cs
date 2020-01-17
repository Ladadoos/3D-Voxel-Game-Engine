namespace Minecraft
{
    class WorldClient : World
    {
        public ChunkProvider chunkProvider { get; private set; }

        public WorldClient(Game game) : base(game)
        {
            chunkProvider = new ChunkProvider(game, this);

            OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
            OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            OnChunkUnloadedHandler += game.masterRenderer.OnChunkUnloaded;
        }
    }
}
