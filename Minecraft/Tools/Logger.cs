using System;

namespace Minecraft
{
    class Logger
    {
        private static LogLevel logLevel = LogLevel.Info;

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
            if(logLevel >= LogLevel.Info)
            {
                Print(message, LogLevel.Info);
            }
        }

        public static void Warn(string message)
        {
            if (logLevel >= LogLevel.Warn)
            {
                Print(message, LogLevel.Warn);
            }
        }

        public static void Error(string message)
        {
            if (logLevel >= LogLevel.Error)
            {
                Print(message, LogLevel.Error);
            }
        }
    }

    public enum LogLevel
    {
        Info, Warn, Error
    };
}
