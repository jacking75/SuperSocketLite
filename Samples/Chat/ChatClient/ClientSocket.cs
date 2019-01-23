using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.IO;
using System.Net;

namespace ChatClient
{
    public class ClientSocket
    {
        //초기화
        public Socket socket = null;    //server 연결을 위한 ClientSock

        public string LatestErrorMsg;
        public string LatestReceiveMsg;

        //소켓연결        
        public bool conn(string IP, int PORT)
        {
            IPAddress serverIP = IPAddress.Parse(IP);
            int serverPort = PORT;

            //Socket 생성(생성 안되면, SocketException 발생!!!!!!)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Socket 연결(연결 안되면, SocketException 발생!!!!!!)
            this.socket.Connect(new IPEndPoint(serverIP, serverPort));

            if (socket == null || socket.Connected == false)
            {
                return false;
            }

            return true;
        }

        //스트림에서 읽어오기(소켓 연결확인은 버튼을 누르면...! form 에서...)
        public Tuple<int, byte[]> s_read()
        {
            try
            {
                byte[] getbyte = new byte[4096];
                var nRecv = socket.Receive(getbyte, 0, getbyte.Length, SocketFlags.None);
                return new Tuple<int, byte[]>(nRecv, getbyte);
            }
            catch (SocketException se)
            {
                LatestErrorMsg = se.ToString();
            }

            return null;
        }

        //스트림에 쓰기
        public void s_write(byte[] senddata)
        {
            try
            {
                if (socket != null && socket.Connected) //연결상태 유무 확인
                {
                    socket.Send(senddata, 0, senddata.Length, SocketFlags.None);
                }
                else
                {
                    LatestErrorMsg = "먼저 채팅서버에 접속하세요!";
                }
            }
            catch (SocketException se)
            {
                LatestErrorMsg = se.Message.ToString();
            }
        }

        //소켓과 스트림 닫기
        public void close()
        {
            socket.Close();
        }
    }
}
