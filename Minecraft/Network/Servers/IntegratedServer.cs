namespace Minecraft
{
    class IntegratedServer : Server
    {
        protected override void InitializeWorld(Game game)
        {
            world = new ClientWorld(game);
        }
    }
}
