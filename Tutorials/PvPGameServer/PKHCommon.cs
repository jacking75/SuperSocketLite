using MemoryPack;
using System;
using System.Collections.Generic;



namespace PvPGameServer;

public class PKHCommon : PKHandler
{
    public void RegistPacketHandler(Dictionary<int, Action<MemoryPackBinaryRequestInfo>> packetHandlerMap)
    {            
        packetHandlerMap.Add((int)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
        packetHandlerMap.Add((int)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisConnectClient);

        packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
                                            
    }

    public void NotifyInConnectClient(MemoryPackBinaryRequestInfo requestData)
    {
    }

    public void NotifyInDisConnectClient(MemoryPackBinaryRequestInfo requestData)
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


    public void RequestLogin(MemoryPackBinaryRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.MainLogger.Debug("로그인 요청 받음");

        try
        {
            if(_userMgr.GetUser(sessionID) != null)
            {
                ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, packetData.SessionID);
                return;
            }
                            
            var reqData = MemoryPackSerializer.Deserialize< PKTReqLogin>(packetData.Data);
            var errorCode = _userMgr.AddUser(reqData.UserID, sessionID);
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

            MainServer.MainLogger.Debug($"로그인 결과. UserID:{reqData.UserID}, {errorCode}");

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

        var sendData = MemoryPackSerializer.Serialize(resLogin);
        MemoryPackPacketHeadInfo.Write(sendData, PACKETID.RES_LOGIN);

        NetSendFunc(sessionID, sendData);
    }

    public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var sendData = MemoryPackSerializer.Serialize(resLogin);
        MemoryPackPacketHeadInfo.Write(sendData, PACKETID.NTF_MUST_CLOSE);

        NetSendFunc(sessionID, sendData);
    }


    
                  
}
