using CommandLine;
using System;
using System.Timers;

namespace sendFailTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Send Fail Test");

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
                Console.WriteLine("[ERROR] 서버 네트워크 시작 실패");
                return;
            }


            var timer = new Timer(64);

            timer.Elapsed += (s, e) =>
            {
                var packet = TempPacket();

                foreach (var session in server.GetAllSessions())
                {
                    session.Send(packet);
                }
                Console.WriteLine(DateTime.Now);
            };

            timer.Start();

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

        static byte[] TempPacket()
        {
            var startNumber = 0;
            var endNumber = 9;
            var count = 1024;

            var SecureNumberRandom = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            var secureString = new System.Text.StringBuilder();

            for (int i = 0; i < count; ++i)
            {
                secureString.Append(SecureNumberRandom.Next(startNumber, endNumber).ToString());
            }

            var body = System.Text.Encoding.UTF8.GetBytes(secureString.ToString());
            //List<byte> dataSource = new List<byte>();
            //dataSource.AddRange(BitConverter.GetBytes(MsgLen));
            //dataSource.AddRange(Msg);
            return body;
        }
    }


    public class ServerOption
    {
        [Option("port", Required = true, HelpText = "Server Port Number")]
        public int Port { get; set; }

        [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber Count")]
        public int MaxConnectionNumber { get; set; } = 0;

        [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }

        [Option("sendTimeOut", Required = true, HelpText = "session sendTimeOut")]
        public int SendTimeOut { get; set; }

        [Option("sendingQueueSize", Required = true, HelpText = "session sendingQueueSize")]
        public int SendingQueueSize { get; set; }

        [Option("sendBufferSize", Required = true, HelpText = "session sendBufferSize")]
        public int SendBufferSize { get; set; }
    }
}
