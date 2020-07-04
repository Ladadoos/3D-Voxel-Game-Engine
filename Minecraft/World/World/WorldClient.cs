namespace Minecraft
{
    /// <summary>
    /// The representation of the world used on the client.
    /// </summary>
    class WorldClient : World
    {
        public WorldClient(Game game) : base(game)
        {
            OnBlockPlacedHandler += game.MasterRenderer.OnBlockPlaced;
            OnBlockRemovedHandler += game.MasterRenderer.OnBlockRemoved;
            OnChunkLoadedHandler += game.MasterRenderer.OnChunkLoaded;
            OnChunkUnloadedHandler += game.MasterRenderer.OnChunkUnloaded;
        }
    }
}
