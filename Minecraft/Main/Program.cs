namespace Minecraft
{
    class Program
    {
        static void Main(string[] args)
        {
            StartArgs startArgs = new ArgsParser().ParseProgramArgs(args);
            GameWindow window = new GameWindow(startArgs);
            window.Run();
        }
    }
}
