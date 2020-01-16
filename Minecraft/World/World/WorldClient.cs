namespace Minecraft
{
    class WorldClient : World
    {
        public ChunkProvider chunkProvider { get; private set; }

        public WorldClient(Game game) : base(game)
        {
            chunkProvider = new ChunkProvider(game, this);

            OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            chunkProvider.CheckForNewChunks(this);
        }
    }
}
