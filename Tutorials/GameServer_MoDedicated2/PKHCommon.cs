﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;


namespace GameServer;

public class PKHCommon : PKHandler
{
    public void RegistPacketHandler(Dictionary<UInt16, Action<ServerPacketData>> packetHandlerMap)
    {            
        packetHandlerMap.Add((UInt16)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
        packetHandlerMap.Add((UInt16)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisConnectClient);

        packetHandlerMap.Add((UInt16)PACKETID.REQ_LOGIN, RequestLogin);
                                            
    }

    public void NotifyInConnectClient(ServerPacketData requestData)
    {
        MainServer.MainLogger.Debug($"Current Connected Session Count: {ServerNetwork.SessionCount}");
    }

    public void NotifyInDisConnectClient(ServerPacketData requestData)
    {
        var sessionIndex = requestData.SessionIndex;
        var user = UserMgr.GetUser(sessionIndex);
        
        if (user != null)
        {
            var roomNum = user.RoomNumber;

            if (roomNum != PacketDef.INVALID_ROOM_NUMBER)
            {
                var packet = new PKTInternalNtfRoomLeave()
                {
                    RoomNumber = roomNum,
                    UserID = user.ID(),
                };

                var packetBodyData = MessagePackSerializer.Serialize(packet);
                var internalPacket = new ServerPacketData();
                internalPacket.Assign("", sessionIndex, (UInt16)PACKETID.NTF_IN_ROOM_LEAVE, packetBodyData);

                ServerNetwork.Distribute(internalPacket);
            }

            UserMgr.RemoveUser(sessionIndex);
        }
                    
        MainServer.MainLogger.Debug($"Current Connected Session Count: {ServerNetwork.SessionCount}");
    }


    public void RequestLogin(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.MainLogger.Debug("로그인 요청 받음");

        try
        {
            if(UserMgr.GetUser(sessionIndex) != null)
            {
                ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, packetData.SessionID);
                return;
            }
                            
            var reqData = MessagePackSerializer.Deserialize< PKTReqLogin>(packetData.BodyData);
            var errorCode = UserMgr.AddUser(reqData.UserID, packetData.SessionID, packetData.SessionIndex);
            if (errorCode != ERROR_CODE.NONE)
            {
                ResponseLoginToClient(errorCode, packetData.SessionID);

                if (errorCode == ERROR_CODE.LOGIN_FULL_USER_COUNT)
                {
                    NotifyMustCloseToClient(ERROR_CODE.LOGIN_FULL_USER_COUNT, packetData.SessionID);
                }
                
                return;
            }

            ResponseLoginToClient(errorCode, packetData.SessionID);

            MainServer.MainLogger.Debug("로그인 요청 답변 보냄");

        }
        catch(Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.MainLogger.Error(ex.ToString());
        }
    }
            
    public void ResponseLoginToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PACKETID.RES_LOGIN, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }

    public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PACKETID.NTF_MUST_CLOSE, bodyData);

        ServerNetwork.SendData(sessionID, sendData);
    }


    
                  
}
