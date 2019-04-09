using CommandLine;
using System;

namespace EchoServerEx
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
                DevLog.Write(string.Format("서버 네트워크 시작"), LOG_LEVEL.INFO);
            }
            else
            {
                DevLog.Write(string.Format("[ERROR] 서버 네트워크 시작 실패"), LOG_LEVEL.ERROR);
                return;
            }

            var workProcessTimer = new System.Timers.Timer(32);
            workProcessTimer.Elapsed += OnProcessTimedEvent;
            workProcessTimer.AutoReset = true;
            workProcessTimer.Enabled = true;


            Console.WriteLine("key를 누르면 종료한다....");
            Console.ReadKey();
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

        static void OnProcessTimedEvent(object sender, EventArgs e)
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (true)
            {
                string msg;

                if (DevLog.GetLog(out msg))
                {
                    ++logWorkCount;

                    Console.WriteLine(msg);
                }
                else
                {
                    break;
                }

                if (logWorkCount > 7)
                {
                    break;
                }
            }
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
    }

}
