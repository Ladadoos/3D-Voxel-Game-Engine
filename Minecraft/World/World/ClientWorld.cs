namespace Minecraft
{
    class ClientWorld : World
    {
        public ClientWorld(Game game) : base(game)
        {
            OnBlockPlacedHandler += game.masterRenderer.OnBlockPlaced;
            OnChunkLoadedHandler += game.masterRenderer.OnChunkLoaded;
        }
    }
}
