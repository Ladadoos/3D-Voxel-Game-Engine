using System;
using System.Collections.Generic;

namespace Minecraft
{
    struct StartArgs
    {
        public RunMode RunMode;
        public string IP;
        public int Port;
    }

    class ArgsParser
    {
        public StartArgs ParseProgramArgs(string[] args)
        {
            Dictionary<string, string> argsDic = new Dictionary<string, string>();
            foreach(string arg in args)
            {
                string[] keyValue = arg.Split('=');
                if(keyValue.Length != 2) continue;
                argsDic.Add(keyValue[0], keyValue[1]);
            }

            return new StartArgs()
            {
                RunMode = GetRunMode(argsDic),
                IP = GetIp(argsDic),
                Port = GetPort(argsDic)
            };
        }

        private RunMode GetRunMode(Dictionary<string, string> startArgs)
        {
            if(!startArgs.TryGetValue("mode", out string value))
            {
                throw new KeyNotFoundException("Missing run mode key in start args");
            }
            value = value.ToLower();
            if(value == "client") return RunMode.Client;
            if(value == "server") return RunMode.Server;
            if(value == "clientserver") return RunMode.ClientServer;
            throw new InvalidOperationException("Invalid value for runmode");
        }

        private string GetIp(Dictionary<string, string> startArgs)
        {
            if(!startArgs.TryGetValue("ip", out string value))
            {
                throw new KeyNotFoundException("Missing ip key in start args");
            }
            return value;
        }

        private int GetPort(Dictionary<string, string> startArgs)
        {
            if(!startArgs.TryGetValue("port", out string value))
            {
                throw new KeyNotFoundException("Missing port key in start args");
            }
            if(!int.TryParse(value, out int port))
            {
                throw new InvalidOperationException("Invalid type for port");
            }
            return port;
        }
    }
}
