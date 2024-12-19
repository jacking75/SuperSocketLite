using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

using DB;
using CSBaseLib;


namespace ChatServer;

class PacketProcessor
{
    bool _공용_프로세서 = false;

    bool _isThreadRunning = false;
    System.Threading.Thread _processThread = null;

    //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
    //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
    BufferBlock<ServerPacketData> _packetBuffer = new BufferBlock<ServerPacketData>();

    Tuple<int,int> _roomNumberRange = new Tuple<int, int>(-1, -1);
    List<Room> _roomList = new List<Room>();

    Dictionary<int, Action<ServerPacketData>> _packetHandlerMap = new Dictionary<int, Action<ServerPacketData>>();
    PKHCommon _commonPacketHandler = new PKHCommon();
    PKHRoom _roomPacketHandler = new PKHRoom();
            

    //TODO MainServer를 인자로 주지말고, func을 인자로 넘겨주는 것이 좋다
    public void CreateAndStart(bool IsCommon, List<Room> roomList, MainServer mainServer, ConnectSessionManager sessionMgr)
    {
        _공용_프로세서 = IsCommon;

        if (IsCommon == false)
        {
            _roomList = roomList;

            var minRoomNum = _roomList[0].Number;
            var maxRoomNum = _roomList[0].Number + _roomList.Count() - 1;
            _roomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);
        }

        RegistPacketHandler(mainServer, sessionMgr);

        _isThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }
    
    public void Destory()
    {
        _isThreadRunning = false;
        _packetBuffer.Complete();
    }

    public bool 관리중인_Room(int roomNumber)
    {
        // roomNumber가 _roomNumberRange 안에 포함 되는지 확인
        return roomNumber >= _roomNumberRange.Item1 && roomNumber <= _roomNumberRange.Item2;
    }

    public void InsertMsg(bool isClientRequest, ServerPacketData data)
    {
        if (isClientRequest && 
            (data.PacketID <= (short)PacketId.CsBegin || data.PacketID >= (short)PacketId.CsEnd))
        {
            return;
        }

        _packetBuffer.Post(data);
        //DevLog.Write($"[InsertMsg] - PktID: {data.PacketID}", LOG_LEVEL.DEBUG);
    }

    
    void RegistPacketHandler(MainServer serverNetwork, ConnectSessionManager sessionManager)
    {
        if (_공용_프로세서)
        {
            var maxUserCount = (MainServer.s_ServerOption.RoomMaxCountPerThread * MainServer.s_ServerOption.RoomThreadCount) * MainServer.s_ServerOption.RoomMaxUserCount;
            _commonPacketHandler.Init(serverNetwork, sessionManager);
            _commonPacketHandler.SetConfig(maxUserCount);
            _commonPacketHandler.RegistPacketHandler(_packetHandlerMap);                
        }
        else
        {
            _roomPacketHandler.Init(serverNetwork, sessionManager);
            _roomPacketHandler.Init(_roomList);
            _roomPacketHandler.RegistPacketHandler(_packetHandlerMap);
        }          
    }

    void Process()
    {
        while (_isThreadRunning)
        {
            //System.Threading.Thread.Sleep(64); //테스트 용
            try
            {
                var packet = _packetBuffer.Receive();

                if (_packetHandlerMap.ContainsKey(packet.PacketID))
                {
                    _packetHandlerMap[packet.PacketID](packet);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                }
            }
            catch (Exception ex)
            {
                if (_isThreadRunning)
                {
                    MainServer.s_MainLogger.Error(ex.ToString());
                }
            }
        }
    }


}
