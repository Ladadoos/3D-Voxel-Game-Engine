namespace Minecraft
{
    class WorldClient : World
    {
        public WorldClient(Game game) : base(game)
        {

        }

        public static void AddHooks(Game game, World world)
        {
            world.OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            world.OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            world.OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
        }
    }
}
