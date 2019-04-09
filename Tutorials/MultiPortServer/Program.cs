using CommandLine;
using System;

namespace MultiPortServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello SuperSocketLite");

            var serverOption = ParseCommandLine(args);
            if (serverOption == null)
            {
                return;
            }

            
            var server1 = new MainServer();
            server1.InitConfig(serverOption.Name1, serverOption.Port1, serverOption.MaxConnectionNumber1);
            server1.CreateServer();

            var IsResult = server1.Start();

            if (IsResult)
            {
                MainServer.MainLogger.Info("C2S 서버 네트워크 시작");
            }
            else
            {
                Console.WriteLine("[ERROR] C2S 서버 네트워크 시작 실패");
                return;
            }


            var server2 = new MainServer();
            server2.InitConfig(serverOption.Name2, serverOption.Port2, serverOption.MaxConnectionNumber2);
            server2.CreateServer();

            IsResult = server2.Start();

            if (IsResult)
            {
                MainServer.MainLogger.Info("S2S 서버 네트워크 시작");
            }
            else
            {
                Console.WriteLine("[ERROR] S2S 서버 네트워크 시작 실패");
                return;
            }


            MainServer.MainLogger.Info("key를 누르면 종료한다....");
            Console.ReadKey();
        }

        static ServerOption ParseCommandLine(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<ServerOption>(args) as CommandLine.Parsed<ServerOption>;

            if (result == null)
            {
                Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }        
    }

    public class ServerOption
    {
        [Option("name1", Required = true, HelpText = "Server Name1")]
        public string Name1 { get; set; }

        [Option("port1", Required = true, HelpText = "Server Port1 Number")]
        public int Port1 { get; set; }

        [Option("maxConnectionNumber1", Required = true, HelpText = "MaxConnectionNumber1 Count")]
        public int MaxConnectionNumber1 { get; set; } = 0;

        [Option("name2", Required = true, HelpText = "Server Name2")]
        public string Name2 { get; set; }

        [Option("port2", Required = true, HelpText = "Server Port2 Number")]
        public int Port2 { get; set; }

        [Option("maxConnectionNumber2", Required = true, HelpText = "MaxConnectionNumber2 Count")]
        public int MaxConnectionNumber2 { get; set; } = 0;

    }

}
