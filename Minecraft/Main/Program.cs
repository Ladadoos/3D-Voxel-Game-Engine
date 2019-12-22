namespace Minecraft
{
    class Program
    {
        static void Main(string[] args)
        {
            GameWindow window = new GameWindow(GetRunMode(args[0]));
            window.Run();
        }

        static RunMode GetRunMode(string arg)
        {
            if (arg == "client") return RunMode.Client;
            if (arg == "server") return RunMode.Server;
            return RunMode.ClientServer;
        }
    }
}
