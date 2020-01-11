using System.Collections.Generic;
using System;

namespace Minecraft
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> argsDic = new Dictionary<string, string>();
            foreach(string arg in args)
            {
                string[] keyValue = arg.Split('=');
                if (keyValue.Length != 2) continue;
                argsDic.Add(keyValue[0], keyValue[1]);
            }

            StartArgs startArgs = new StartArgs()
            {
                runMode = GetRunMode(argsDic),
                ip = GetIp(argsDic),
                port = GetPort(argsDic)
            };
            GameWindow window = new GameWindow(startArgs);
            window.Run();
        }

        static RunMode GetRunMode(Dictionary<string, string> startArgs)
        {
            if(!startArgs.TryGetValue("mode", out string value))
            {
                throw new KeyNotFoundException("Missing run mode key");
            }
            if (value == "client") return RunMode.Client;
            if (value == "server") return RunMode.Server;
            if (value == "clientserver") return RunMode.ClientServer;
            throw new InvalidOperationException("Invalid value for runmode");
        }

        static string GetIp(Dictionary<string, string> startArgs)
        {
            if (!startArgs.TryGetValue("ip", out string value))
            {
                return "127.0.0.1";
            }
            return value;
        }

        static int GetPort(Dictionary<string, string> startArgs)
        {
            if (!startArgs.TryGetValue("port", out string value))
            {
                return 50000;
            }
            if (!int.TryParse(value, out int port))
            {
                throw new InvalidOperationException("Invalid type for port");
            }
            return port;
        }
    }
}
