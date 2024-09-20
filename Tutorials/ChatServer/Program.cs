using System;

namespace ChatServer;

class Program
{
    // 실행할 때는 아래 명령어로 실행한다.
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

        MainServer.s_MainLogger.Info("Press q to shut down the server");

        // q 키를 누르면 서버를 종료한다.
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
