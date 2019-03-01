using System;

namespace Minecraft.Tools
{
    class Logger
    {
        public static void Log(string message, LogType type)
        {
            switch (type)
            {
                case LogType.INFORMATION:
                    Console.WriteLine("[INFO] " + message);
                    break;
                case LogType.WARNING:
                    Console.WriteLine("[WARN] " + message);
                    break;
                default:
                    Console.WriteLine(message);
                    break;
            }
        }
    }

    public enum LogType
    {
        INFORMATION, WARNING
    };
}
