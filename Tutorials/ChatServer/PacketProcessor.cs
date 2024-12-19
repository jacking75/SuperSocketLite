using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks.Dataflow;


namespace ChatServer;


// 패킷 처리 클래스
class PacketProcessor
{
    bool _isThreadRunning = false;
    System.Threading.Thread _processThread = null;

    //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
    //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
    BufferBlock<ServerPacketData> _packetBuffer = new BufferBlock<ServerPacketData>();

    UserManager _userMgr = new UserManager();

    Tuple<int,int> _roomNumberRange = new Tuple<int, int>(-1, -1);
    List<Room> _roomList = new ();

    Dictionary<int, Action<ServerPacketData>> _packetHandlerMap = new ();
    PKHCommon _commonPacketHandler = new ();
    PKHRoom _roomPacketHandler = new ();
            
        
    public void CreateAndStart(List<Room> roomList, MainServer mainServer)
    {
        var maxUserCount = MainServer.s_ServerOption.RoomMaxCount * MainServer.s_ServerOption.RoomMaxUserCount;
        _userMgr.Init(maxUserCount);

        _roomList = roomList;
        var minRoomNum = _roomList[0].Number;
        var maxRoomNum = _roomList[0].Number + _roomList.Count() - 1;
        _roomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);
        
        RegistPacketHandler(mainServer);

        _isThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }
    
    public void Destory()
    {
        _isThreadRunning = false;
        _packetBuffer.Complete();
    }
          
    public void InsertPacket(ServerPacketData data)
    {
        _packetBuffer.Post(data);
    }

    
    void RegistPacketHandler(MainServer serverNetwork)
    {            
        _commonPacketHandler.Init(serverNetwork, _userMgr);
        _commonPacketHandler.RegistPacketHandler(_packetHandlerMap);                
        
        _roomPacketHandler.Init(serverNetwork, _userMgr);
        _roomPacketHandler.SetRooomList(_roomList);
        _roomPacketHandler.RegistPacketHandler(_packetHandlerMap);
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
                if(_isThreadRunning)
                {
                    MainServer.s_MainLogger.Error(ex.ToString());
                }                
            }
        }
    }


}

