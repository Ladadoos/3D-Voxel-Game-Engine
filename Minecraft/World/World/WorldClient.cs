namespace Minecraft
{
    class WorldClient : World
    {
        public ChunkProvider chunkProvider { get; private set; }

        public WorldClient(Game game) : base(game)
        {
            chunkProvider = new ChunkProvider(game, this);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            chunkProvider.CheckForNewChunks(this);
        }

        public static void AddHooks(Game game, World world)
        {
            world.OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            world.OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            world.OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
        }
    }
}
