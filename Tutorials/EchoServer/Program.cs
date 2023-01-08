using System;

namespace EchoServer
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

            
            var server = new MainServer();
            server.InitConfig(serverOption);
            server.CreateServer();

            var IsResult = server.Start();

            if (IsResult)
            {
                MainServer.MainLogger.Info("서버 네트워크 시작");
            }
            else
            {
                Console.WriteLine("서버 네트워크 시작 실패");
                return;
            }
                        

            Console.WriteLine("key를 누르면 종료한다....");
            Console.ReadKey();

            server.Destory();
        }

        static ServerOption ParseCommandLine(string[] args)
        {
            var option = new ServerOption
            {
                Port = 32452,
                MaxConnectionNumber = 32,
                Name = "EchoServer"
            };

            return option;
        }               

    }

    public class ServerOption
    {
        public int Port { get; set; }

        public int MaxConnectionNumber { get; set; } = 0;

        public string Name { get; set; }        
    }

}
