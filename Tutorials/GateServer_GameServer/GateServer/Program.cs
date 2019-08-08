using System;

namespace GateServer
{
    class Program
    {
        //dotnet ChatServer.dll --uniqueID 1 --roomMaxCount 16 --roomMaxUserCount 4 --roomStartNumber 1 --maxUserCount 100
        static void Main(string[] args)
        {
            var serverOption = ParseCommandLine(args);
            if(serverOption == null)
            {
                return;
            }

           
            var serverApp = new MainServer();
            serverApp.InitConfig(serverOption);
            serverApp.CreateStartServer();


            var gameServerProxy = new GameServerProxy();
            gameServerProxy.InitConfig();
            gameServerProxy.CreateStartServer();
            gameServerProxy.StartAllConnectTo();

            MainServer.MainLogger.Info("Press q to shut down the server");

            while (true)
            {
                System.Threading.Thread.Sleep(128);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("Server Terminate ~~~");
                        gameServerProxy.StopServer();
                        serverApp.StopServer();
                        break;
                    }
                }
                                
            }
        }

        static ServerOption ParseCommandLine(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

            if (result == null)
            {
                System.Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }                  

    } // end Class
}
