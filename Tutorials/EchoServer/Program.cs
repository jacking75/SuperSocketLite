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
            var option = new ServerOption
            {
                Port = 32452,
                MaxConnectionNumber = 32,
                Name = "EchoServer"
            };

            return option;
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
        public int Port { get; set; }

        public int MaxConnectionNumber { get; set; } = 0;

        public string Name { get; set; }        
    }

}
