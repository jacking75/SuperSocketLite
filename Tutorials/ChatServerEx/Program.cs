using System;

namespace ChatServer
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

            var error = serverApp.CreateComponent();

            (error == CSBaseLib.ERROR_CODE.NONE).IfFalse(() =>
            {
                var errorMsg = string.Format("서버 컴포넌트 생성 실패. {0}: {1}", error, error.ToString());
                MainServer.MainLogger.Error(errorMsg);                
            });

            MainServer.MainLogger.Info("Press q to shut down the server");

            while (true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("Server Terminate ~~~");
                        serverApp.StopServer();
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

    } // end Class
}
