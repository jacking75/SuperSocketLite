using System;

namespace ChatServer
{
    class Program
    {
        //dotnet ChatServer.dll --uniqueID 1 --roomMaxCountPerThread 16 --roomThreadCount 4 --roomMaxUserCount 4 --roomStartNumber 1 --dbWorkerThreadCount 4 --redisAddress 192.168.0.10 --maxUserCount 100
        static void Main(string[] args)
        {
            var serverOption = ParseCommandLine(args);
            if(serverOption == null)
            {
                return;
            }

            var workProcessTimer = new System.Timers.Timer(32); 
            workProcessTimer.Elapsed += (s, e) => OnProcessTimedEvent(s,e);
            workProcessTimer.Start();

            var ServerApp = new MainServer();
            ServerApp.CreateStartServer();
            var error = ServerApp.CreateComponent();

            (error == CSBaseLib.ERROR_CODE.NONE).IfFalse(() =>
            {
                var errorMsg = string.Format("서버 컴포넌트 생성 실패. {0}: {1}", error, error.ToString());
                MainServer.WriteLog(errorMsg, CommonServerLib.LOG_LEVEL.INFO);
                CommonServerLib.DevLog.Write(errorMsg, CommonServerLib.LOG_LEVEL.ERROR);
            });

            while (true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("Server Terminate ~~~");
                        ServerApp.StopServer();
                        break;
                    }
                }
                                
            }
        }

        static ChatServerOption ParseCommandLine(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<ChatServerOption>(args) as CommandLine.Parsed<ChatServerOption>;

            if (result == null)
            {
                System.Console.WriteLine("Failed Command Line");
                return null;
            }

            return result.Value;
        }

        static void OnProcessTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
       {
           ProcessLog();
           ProcessInnerMessage();
       }

        static void ProcessLog()
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (true)
            {
                string msg;

                if (CommonServerLib.DevLog.GetLog(out msg))
                {
                    ++logWorkCount;

                    Console.WriteLine(msg);
                }
                else
                {
                    break;
                }

                if (logWorkCount > 32)
                {
                    break;
                }
            }
        }

        static void ProcessInnerMessage()
        {
            int workedCount = 0;

            while (true)
            {
                CommonServerLib.InnerMsg msg;

                if (CommonServerLib.InnerMessageHostProgram.GetMsg(out msg))
                {
                    ++workedCount;

                    switch (msg.Type)
                    {
                        case CommonServerLib.InnerMsgType.SERVER_START:
                            var values = msg.Value1.Split("_");
                            Console.WriteLine($"[ServerID, Address]: {values[0]},  {values[1]}");
                            break;
                        case CommonServerLib.InnerMsgType.CREATE_COMPONENT:
                            Console.WriteLine($"[RoomThreadCount]: {ChatServerEnvironment.RoomThreadCount}");
                            Console.WriteLine($"[RoomMaxCountPerThread]: {ChatServerEnvironment.RoomMaxCountPerThread}");
                            Console.WriteLine($"[RoomStartNumber]: {ChatServerEnvironment.RoomStartNumber}");
                            Console.WriteLine($"[RoomMaxUserCount]: {ChatServerEnvironment.RoomMaxUserCount}");
                            break;
                        case CommonServerLib.InnerMsgType.CURRENT_CONNECT_COUNT:
                            Console.WriteLine($"[CurrentUserCount]: {msg.Value1}");
                            break;
                    }
                }
                else
                {
                    break;
                }

                if (workedCount > 32)
                {
                    break;
                }
            }
        }

    } // end Class
}
