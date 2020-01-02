namespace Minecraft
{
    class ClientWorldHook : IEventHook
    {
        private Game game;

        public ClientWorldHook(Game game)
        {
            this.game = game;
        }

        public void AddEventHooksFor(IEventAnnouncer obj)
        {
            World world = (World)obj;
            world.OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            world.OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
            world.OnBlockRemovedHandler += game.masterRenderer.OnBlockRemoved;
        }
    }
}
