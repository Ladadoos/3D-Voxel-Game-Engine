using System;

namespace Minecraft
{
    public enum LogLevel
    {
        Packet, Info, Warn, Error
    };

    static class Logger
    {
        private static LogLevel logLevel = LogLevel.Packet;

        public static void SetLogLevel(LogLevel logLevel)
        {
            Logger.logLevel = logLevel;
        }

        private static void Print(string message, LogLevel level)
        {
            Console.WriteLine(string.Format("[{0:HH:mm:ss}][" + level.ToString() + "] " + message, DateTime.Now));
        }

        public static void Info(string message)
        {
            if(LogLevel.Info >= logLevel)
            {
                Print(message, LogLevel.Info);
            }
        }

        public static void Warn(string message)
        {
            if (LogLevel.Warn >= logLevel)
            {
                Print(message, LogLevel.Warn);
            }
        }

        public static void Error(string message)
        {
            if (LogLevel.Error >= logLevel)
            {
                Print(message, LogLevel.Error);
            }
        }

        public static void Packet(string message)
        {
            if (LogLevel.Packet >= logLevel)
            {
                Print(message, LogLevel.Packet);
            }
        }
    }
}
