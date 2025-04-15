﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

using CSBaseLib;

namespace GameServer;

class PacketProcessor
{
    bool IsThreadRunning = false;
    System.Threading.Thread ProcessThread = null;

    //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
    //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
    BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();

    UserManager UserMgr = new UserManager();

    GameUpdaterManager GameUpdateMgr = new GameUpdaterManager();

    Tuple<int,int> RoomNumberRange = new Tuple<int, int>(-1, -1);
    List<Room> RoomList = new List<Room>();

    Dictionary<UInt16, Action<ServerPacketData>> PacketHandlerMap = new Dictionary<UInt16, Action<ServerPacketData>>();
    PKHCommon CommonPacketHandler = new PKHCommon();
    PKHRoom RoomPacketHandler = new PKHRoom();
            

    //TODO MainServer를 인자로 주지말고, func을 인자로 넘겨주는 것이 좋다
    public void CreateAndStart(List<Room> roomList, MainServer mainServer)
    {
        var maxRoomCount = MainServer.ServerOption.RoomMaxCount;
        var maxUserCount = maxRoomCount * MainServer.ServerOption.RoomMaxUserCount;
        UserMgr.Init(maxUserCount);

        GameUpdateMgr.Init(4, (UInt16)(maxRoomCount/4));

        RoomList = roomList;
        var minRoomNum = RoomList[0].Number;
        var maxRoomNum = RoomList[0].Number + RoomList.Count() - 1;
        RoomNumberRange = new Tuple<int, int>(minRoomNum, maxRoomNum);
        
        RegistPacketHandler(mainServer);

        IsThreadRunning = true;
        ProcessThread = new System.Threading.Thread(this.Process);
        ProcessThread.Start();
    }
    
    public void Destory()
    {
        GameUpdateMgr.AllStop();

        IsThreadRunning = false;
        MsgBuffer.Complete();

        MainServer.MainLogger.Info("[PacketProcessor.Destory] End");
    }
          
    public void InsertPacket(ServerPacketData data)
    {
        MsgBuffer.Post(data);
    }

    
    void RegistPacketHandler(MainServer serverNetwork)
    {            
        CommonPacketHandler.Init(serverNetwork, UserMgr);
        CommonPacketHandler.RegistPacketHandler(PacketHandlerMap);                
        
        RoomPacketHandler.Init(serverNetwork, UserMgr);
        RoomPacketHandler.SetObject(RoomList, GameUpdateMgr);
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

                if (PacketHandlerMap.ContainsKey(packet.PacketID))
                {
                    PacketHandlerMap[packet.PacketID](packet);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                }
            }
            catch (Exception ex)
            {
                if(IsThreadRunning)
                {
                    MainServer.MainLogger.Error(ex.ToString());
                }
            }
        }
    }


}
