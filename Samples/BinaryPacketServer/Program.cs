using System;

namespace BinaryPacketServer
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.WriteLine("Hello SuperSocketLite");

            var server = new Server();
            server.Start();

            Console.WriteLine("key를 누르면 종료한다....");
            Console.ReadKey();
        }

        
    }

    class Server
    {
        System.Timers.Timer workProcessTimer;

        public void Start()
        {
            var server = new MainServer();
            server.InitConfig();
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

            workProcessTimer = new System.Timers.Timer(32);
            workProcessTimer.Elapsed += OnProcessTimedEvent;
            workProcessTimer.AutoReset = true;
            workProcessTimer.Enabled = true;
        }

        void OnProcessTimedEvent(object sender, EventArgs e)
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
}
