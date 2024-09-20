using MemoryPack;
using System;
using System.Collections.Generic;



namespace PvPGameServer;

public class PKHCommon : PKHandler
{
    public void RegistPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerDict)
    {            
        packetHandlerDict.Add((int)PacketId.NtfInConnectClient, HandleNotifyInConnectClient);
        packetHandlerDict.Add((int)PacketId.NtfInDisconnectClient, HandleNotifyInDisConnectClient);

        packetHandlerDict.Add((int)PacketId.ReqLogin, HandleRequestLogin);
                                            
    }

    public void HandleNotifyInConnectClient(MemoryPackBinaryRequestInfo requestData)
    {
    }

    public void HandleNotifyInDisConnectClient(MemoryPackBinaryRequestInfo requestData)
    {
        var sessionID = requestData.SessionID;
        var user = _userMgr.GetUser(sessionID);
        
        if (user != null)
        {
            var roomNum = user.RoomNumber;

            if (roomNum != Room.InvalidRoomNumber)
            {
                var internalPacket = InnerPakcetMaker.MakeNTFInnerRoomLeavePacket(sessionID, roomNum, user.ID());                
                DistributeInnerPacket(internalPacket);
            }

            _userMgr.RemoveUser(sessionID);
        }
    }


    public void HandleRequestLogin(MemoryPackBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            if(_userMgr.GetUser(sessionID) != null)
            {
                ResponseLoginToClient(ErrorCode.LoginAlreadyWorking, packetData.SessionID);
                return;
            }
                            
            var reqData = MemoryPackSerializer.Deserialize< PKTReqLogin>(packetData.Data);
            var errorCode = _userMgr.AddUser(reqData.UserID, sessionID);
            if (errorCode != ErrorCode.None)
            {
                ResponseLoginToClient(errorCode, packetData.SessionID);

                if (errorCode == ErrorCode.LoginFullUserCount)
                {
                    NotifyMustCloseToClient(ErrorCode.LoginFullUserCount, packetData.SessionID);
                }
                
                return;
            }

            ResponseLoginToClient(errorCode, packetData.SessionID);

            MainServer.s_MainLogger.Debug($"로그인 결과. UserID:{reqData.UserID}, {errorCode}");

        }
        catch(Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }
            
    public void ResponseLoginToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var sendData = MemoryPackSerializer.Serialize(resLogin);
        MemoryPackPacketHeader.Write(sendData, PacketId.ResLogin);

        NetSendFunc(sessionID, sendData);
    }

    public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var sendData = MemoryPackSerializer.Serialize(resLogin);
        MemoryPackPacketHeader.Write(sendData, PacketId.NtfMustClose);

        NetSendFunc(sessionID, sendData);
    }


    
                  
}
