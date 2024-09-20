using System;
using System.Collections.Generic;

using MessagePack;

using CSBaseLib;


namespace ChatServer;

public class PKHCommon : PKHandler
{
    public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {            
        packetHandlerMap.Add((int)PacketId.NtfInConnectClient, HandleNotifyInConnectClient);
        packetHandlerMap.Add((int)PacketId.NtfInDisconnectClient, HandleNotifyInDisConnectClient);

        packetHandlerMap.Add((int)PacketId.ReqLogin, HandleRequestLogin);
                                            
    }

    public void HandleNotifyInConnectClient(ServerPacketData requestData)
    {
        MainServer.s_MainLogger.Debug($"Current Connected Session Count: {_serverNetwork.SessionCount}");
    }

    public void HandleNotifyInDisConnectClient(ServerPacketData requestData)
    {
        var sessionID = requestData.SessionID;
        var user = _userMgr.GetUser(sessionID);
        
        if (user != null)
        {
            var roomNum = user.RoomNumber;

            if (roomNum != PacketDef.InvalidRoomNumber)
            {
                var packet = new PKTInternalNtfRoomLeave()
                {
                    RoomNumber = roomNum,
                    UserID = user.ID(),
                };

                var packetBodyData = MessagePackSerializer.Serialize(packet);
                var internalPacket = new ServerPacketData();
                internalPacket.SetPacketData(sessionID, (Int16)PacketId.NtfInRoomLeave, packetBodyData);

                _serverNetwork.Distribute(internalPacket);
            }

            _userMgr.RemoveUser(sessionID);
        }
                    
        MainServer.s_MainLogger.Debug($"Current Connected Session Count: {_serverNetwork.SessionCount}");
    }


    public void HandleRequestLogin(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            if(_userMgr.GetUser(sessionID) != null)
            {
                SendResponseLoginToClient(ErrorCode.LoginAlreadyWorking, packetData.SessionID);
                return;
            }
                            
            var reqData = MessagePackSerializer.Deserialize< PKTReqLogin>(packetData.BodyData);

            var errorCode = _userMgr.AddUser(reqData.UserID, sessionID);
            if (errorCode != ErrorCode.None)
            {
                SendResponseLoginToClient(errorCode, packetData.SessionID);

                if (errorCode == ErrorCode.LoginFullUserCount)
                {
                    SendNotifyMustCloseToClient(ErrorCode.LoginFullUserCount, packetData.SessionID);
                }
                
                return;
            }

            SendResponseLoginToClient(errorCode, packetData.SessionID);

            MainServer.s_MainLogger.Debug("로그인 요청 답변 보냄");

        }
        catch(Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
            
    public void SendResponseLoginToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PacketId.ResLogin, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }

    public void SendNotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PacketId.NtfMustClose, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }


    
                  
}
