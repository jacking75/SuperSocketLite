using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;


namespace PvPGameServer
{
    class PacketProcessor
    {
        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        public Func<string, byte[], bool> NetSendFunc;
        public Action<EFBinaryRequestInfo> DistributePacket;

        //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        BufferBlock<EFBinaryRequestInfo> MsgBuffer = new BufferBlock<EFBinaryRequestInfo>();

        UserManager UserMgr = new UserManager();

        Tuple<int,int> RoomNumberRange = new Tuple<int, int>(-1, -1);
        List<Room> RoomList = new List<Room>();

        Dictionary<int, Action<EFBinaryRequestInfo>> PacketHandlerMap = new Dictionary<int, Action<EFBinaryRequestInfo>>();
        PKHCommon CommonPacketHandler = new PKHCommon();
        PKHRoom RoomPacketHandler = new PKHRoom();
                

        public void CreateAndStart(List<Room> roomList, ServerOption serverOpt)
        {
            var maxUserCount = serverOpt.RoomMaxCount * serverOpt.RoomMaxUserCount;
            UserMgr.Init(maxUserCount);

            RoomList = roomList;
            var minRoomNum = RoomList[0].Number;
            var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
            RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);
            
            RegistPacketHandler();

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }
        
        public void Destory()
        {
            MainServer.MainLogger.Info("PacketProcessor::Destory - begin");

            IsThreadRunning = false;
            MsgBuffer.Complete();

            ProcessThread.Join();

            MainServer.MainLogger.Info("PacketProcessor::Destory - end");
        }
              
        public void InsertPacket(EFBinaryRequestInfo data)
        {
            MsgBuffer.Post(data);
        }

        
        void RegistPacketHandler()
        {
            PKHandler.NetSendFunc = NetSendFunc;
            CommonPacketHandler.Init(UserMgr);
            CommonPacketHandler.RegistPacketHandler(PacketHandlerMap);                
            
            RoomPacketHandler.Init(UserMgr);
            RoomPacketHandler.SetRooomList(RoomList);
            RoomPacketHandler.RegistPacketHandler(PacketHandlerMap);
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                //System.Threading.Thread.Sleep(64); //테스트 용
                try
                {
                    var packet = MsgBuffer.Receive();

                    var header = new MsgPackPacketHeadInfo();
                    header.Read(packet.Data);

                    if (PacketHandlerMap.ContainsKey(header.Id))
                    {
                        PacketHandlerMap[header.Id](packet);
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                    }
                }
                catch (Exception ex)
                {
                    if (IsThreadRunning)
                    {
                        MainServer.MainLogger.Error(ex.ToString());
                    }
                }
            }
        }


    }
}
