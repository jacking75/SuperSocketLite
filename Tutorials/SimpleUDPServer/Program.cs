using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleUDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Simple UDP Server !");

            var config = new ServerConfig
            {
                Port = 555,
                Ip = "Any",
                MaxConnectionNumber = 10,
                Mode = SocketMode.Udp,
                Name = "GPSServer"
            };

            var appServer = new UdpAppServer();
            appServer.Setup(new RootConfig(), config, logFactory: new SuperSocket.SocketBase.Logging.ConsoleLogFactory());


            appServer.Start();

            Console.WriteLine("key를 누르면 종료한다....");
            Console.ReadKey();
        }
    }


    public class MyUdpRequestInfo : UdpRequestInfo
    {
        public MyUdpRequestInfo(string key, string sessionID)
            : base(key, sessionID)
        {

        }

        public string Value { get; set; }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.AddRange(Encoding.ASCII.GetBytes(Key));
            data.AddRange(Encoding.ASCII.GetBytes(SessionID));

            int expectedLen = 36 + 4;
            int maxLen = expectedLen - data.Count;

            if (maxLen > 0)
            {
                for (var i = 0; i < maxLen; i++)
                {
                    data.Add(0x00);
                }
            }

            data.AddRange(Encoding.UTF8.GetBytes(Value));

            return data.ToArray();
        }
    }


    class UdpAppServer : AppServer<UdpTestSession, MyUdpRequestInfo>
    {
        public UdpAppServer()
            : base(new DefaultReceiveFilterFactory<MyReceiveFilter, MyUdpRequestInfo>())
        {

        }               
    }


    public class UdpTestSession : AppSession<UdpTestSession, MyUdpRequestInfo>
    {

    }

        

    class MyUdpProtocol : IReceiveFilterFactory<MyUdpRequestInfo>
    {
        public IReceiveFilter<MyUdpRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, System.Net.IPEndPoint remoteEndPoint)
        {
            return new MyReceiveFilter();
        }
    }



    class MyReceiveFilter : IReceiveFilter<MyUdpRequestInfo>
    {
        public MyUdpRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;

            if (length <= 40)
                return null;

            var key = Encoding.ASCII.GetString(readBuffer, offset, 4);
            var sessionID = Encoding.ASCII.GetString(readBuffer, offset + 4, 36);

            var data = Encoding.UTF8.GetString(readBuffer, offset + 40, length - 40);

            return new MyUdpRequestInfo(key, sessionID) { Value = data };
        }

        public int LeftBufferSize
        {
            get { return 0; }
        }

        public IReceiveFilter<MyUdpRequestInfo> NextReceiveFilter
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the filter state.
        /// </summary>
        /// <value>
        /// The filter state.
        /// </value>
        public FilterState State { get; private set; }

        public void Reset()
        {

        }
    }
}
