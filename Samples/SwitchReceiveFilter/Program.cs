using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System;

namespace SwitchReceiveFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start SwitchReceiveFilter!");

            var config = new ServerConfig
            {
                Port = 555,
                Ip = "Any",
                MaxConnectionNumber = 10,
                Mode = SocketMode.Tcp,
                Name = "GPSServer"
            };

            var appServer = new MyAppServer();
            appServer.Setup(new RootConfig(), config, logFactory: new SuperSocket.SocketBase.Logging.ConsoleLogFactory());


            appServer.Start();

            Console.WriteLine("key를 누르면 종료한다....");
            Console.ReadKey();
        }


        // 클라이언트 동작 예
        //public void TestSwitcher()
        //{
        //    EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_Config.Port);

        //    Random rd = new Random();

        //    using (Socket socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
        //    {
        //        socket.Connect(serverAddress);

        //        for (int i = 0; i < 100; i++)
        //        {
        //            var line = Guid.NewGuid().ToString();
        //            var commandLine = "ECHO " + line;

        //            var data = new byte[line.Length + 3 + 5];
        //            var len = 0;

        //            if (rd.Next(0, 100) / 2 == 0)
        //            {
        //                data[0] = (byte)'*';
        //                len = Encoding.ASCII.GetBytes(commandLine, 0, commandLine.Length, data, 1);
        //                data[len + 1] = (byte)'#';
        //                len += 2;
        //            }
        //            else
        //            {
        //                data[0] = (byte)'Y';
        //                len = Encoding.ASCII.GetBytes(commandLine, 0, commandLine.Length, data, 1);
        //                data[len + 1] = 0x00;
        //                data[len + 2] = 0xff;
        //                len += 3;
        //            }

        //            socket.Send(data, 0, len, SocketFlags.None);

        //            var task = Task.Factory.StartNew<string>(() =>
        //            {
        //                byte[] response = new byte[line.Length + 2]; //2 for \r\n

        //                int read = 0;

        //                while (read < response.Length)
        //                {
        //                    read += socket.Receive(response, read, response.Length - read, SocketFlags.None);
        //                }

        //                return Encoding.ASCII.GetString(response, 0, response.Length - 2);//trim \r\n
        //            });

        //            if (!task.Wait(2000))
        //            {
        //                Assert.Fail("Timeout");
        //                return;
        //            }

        //            Assert.AreEqual(line, task.Result);
        //        }
        //    }
        //}
    }
}
