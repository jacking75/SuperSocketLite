using System;
using System.Collections.Generic;

using MessagePack;

using CSBaseLib;
using DB;


namespace ChatServer;

public class PKHCommon : PKHandler
{
    UserManager _userMgr = new ();


    public void SetConfig(int maxUserCount)
    {
        _userMgr.Init(maxUserCount);
    }

    public void RegistPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerMap)
    {            
        packetHandlerMap.Add((int)PacketId.NtfInConnectClient, NotifyInConnectClient);
        packetHandlerMap.Add((int)PacketId.NtfInDisconnectClient, NotifyInDisConnectClient);

        packetHandlerMap.Add((int)PacketId.ReqLogin, RequestLogin);
        packetHandlerMap.Add((int)PacketId.ReqRoomEnter, RequestRoomEnter);            

        packetHandlerMap.Add((int)PacketId.ResInRoomEnter, ResponseRoomEnterInternal);
                    
        packetHandlerMap.Add((int)PacketId.ReqDbLogin, ResponseLoginFromDB);
    }

    public void NotifyInConnectClient(ServerPacketData requestData)
    {
        MainServer.s_MainLogger.Debug($"Current Connected Session Count: {_serverNetwork.SessionCount}");
    }

    public void NotifyInDisConnectClient(ServerPacketData requestData)
    {
        var sessionIndex = requestData.SessionIndex;
        var roomNum = _sessionMgr.GetRoomNumber(sessionIndex);
        var user = _userMgr.GetUser(sessionIndex);

        if (roomNum != PacketDef.InvalidRoomNumber && user != null)
        {                
            SendInternalRoomLeavePacket(roomNum, user.ID());
        }

        if (user != null)
        {
            _userMgr.RemoveUser(sessionIndex);
        }

        _sessionMgr.SetClear(sessionIndex);
        MainServer.s_MainLogger.Debug($"Current Connected Session Count: {_serverNetwork.SessionCount}");
    }

    public void RequestLogin(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            if( _sessionMgr.EnableReuqestLogin(sessionIndex) == false)
            {
                ResponseLoginToClient(ErrorCode.LoginAlreadyWorking, packetData.SessionID);
                return;
            }
                            
            var reqData = MessagePackSerializer.Deserialize< PKTReqLogin>(packetData.BodyData);

            // 세션의 상태를 바꾼다
            _sessionMgr.SetPreLogin(sessionIndex);
            
            // DB 작업 의뢰한다.
            var dbReqLogin = new DBReqLogin()
            {
                UserID = reqData.UserID,
                AuthToken = reqData.AuthToken
            };
            var jobDatas = MessagePackSerializer.Serialize(dbReqLogin);
            
            var dbQueue = MakeDBQueue(PacketId.ReqDbLogin, sessionID, sessionIndex, jobDatas);
            RequestDBJob(_serverNetwork.GetPacketDistributor(), dbQueue);

            MainServer.s_MainLogger.Debug("DB에 로그인 요청 보냄");
        }
        catch(Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    public void ResponseLoginFromDB(ServerPacketData packetData)
    {
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("DB에서 로그인 답변 받음");

        try
        {
            var resData = MessagePackSerializer.Deserialize<DBResLogin>(packetData.BodyData);
            
            // DB 처리 성공/실패에 대한 처리를 한다.
            var errorCode = ErrorCode.None;

            if (resData.Result != ErrorCode.None)
            {
                errorCode = ErrorCode.LoginInvalidAuthToken;
                ResponseLoginToClient(errorCode, packetData.SessionID);
                _sessionMgr.SetStateNone(sessionIndex);
                return;
            }

            errorCode = _userMgr.AddUser(resData.UserID, packetData.SessionID, packetData.SessionIndex);
            if(errorCode != ErrorCode.None)
            {                    
                ResponseLoginToClient(errorCode, packetData.SessionID);
              
                if(errorCode == ErrorCode.LoginFullUserCount)
                {
                    NotifyMustCloseToClient(ErrorCode.LoginFullUserCount, packetData.SessionID);
                    _sessionMgr.SetDisable(sessionIndex);
                }
                else
                {
                    _sessionMgr.SetStateNone(sessionIndex);
                }

                return;
            }

            _sessionMgr.SetLogin(sessionIndex, resData.UserID);

            ResponseLoginToClient(errorCode, packetData.SessionID);

            MainServer.s_MainLogger.Debug("로그인 요청 답변 보냄");
        }
        catch (Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Dbug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    public void ResponseLoginToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKTResLogin()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PacketId.ResLogin, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }

    public void NotifyMustCloseToClient(ErrorCode errorCode, string sessionID)
    {
        var resLogin = new PKNtfMustClose()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resLogin);
        var sendData = PacketToBytes.Make(PacketId.NtfMustClose, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }


    public void RequestRoomEnter(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("RequestRoomEnter");

        try
        {
            var user = _userMgr.GetUser(sessionIndex);
            
            if(user == null || user.IsConfirm(sessionID) == false)
            {
                ResponseEnterRoomToClient(ErrorCode.RoomEnterInvalidUser, sessionID);
                return;
            }

            if (_sessionMgr.EnableReuqestEnterRoom(sessionIndex) == false)
            {
                ResponseEnterRoomToClient(ErrorCode.RoomEnterInvalidState, sessionID);
                return;
            }

            var reqData = MessagePackSerializer.Deserialize<PKTReqRoomEnter>(packetData.BodyData);

            var internalRoomEnter = MakeInternalRoomEnterPacket(reqData.RoomNumber, user.ID(), sessionID, sessionIndex);
            if (SendInternalRoomProcessor(true, reqData.RoomNumber, internalRoomEnter) == false)
            {
                ResponseEnterRoomToClient(ErrorCode.RoomEnterErrorSystem, sessionID);
                return;
            }

            _sessionMgr.SetPreRoomEnter(sessionIndex, reqData.RoomNumber);

            MainServer.s_MainLogger.Debug("패킷 분배");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    void ResponseEnterRoomToClient(ErrorCode errorCode,  string sessionID)
    {
        var resRoomEnter = new PKTResRoomEnter()
        {
            Result = (short)errorCode
        };

        var bodyData = MessagePackSerializer.Serialize(resRoomEnter);
        var sendData = PacketToBytes.Make(PacketId.ResRoomEnter, bodyData);

        _serverNetwork.SendData(sessionID, sendData);
    }

    ServerPacketData MakeInternalRoomEnterPacket(int roomNumber, string userID, string sessionID, int sessionIndex)
    {
        var packet = new PKTInternalReqRoomEnter()
        {
            RoomNumber = roomNumber,
            UserID = userID,
         };

        var packetBodyData = MessagePackSerializer.Serialize(packet);

        var internalPacket = new ServerPacketData();
        internalPacket.Assign(sessionID, sessionIndex, (Int16)PacketId.ReqInRoomEnter, packetBodyData);
        return internalPacket;
    }


    public void ResponseRoomEnterInternal(ServerPacketData packetData)
    {
        var sessionID = packetData.SessionID;
        var sessionIndex = packetData.SessionIndex;
        MainServer.s_MainLogger.Debug("ResponseRoomEnterInternal");

        try
        {
            var resData = MessagePackSerializer.Deserialize<PKTInternalResRoomEnter>(packetData.BodyData);

            var user = _userMgr.GetUser(sessionIndex);

            if(user == null || user.ID() != resData.UserID)
            {
                if (resData.Result == ErrorCode.None)
                {
                    SendInternalRoomLeavePacket(resData.RoomNumber, resData.UserID);
                }
                return;
            }

            if (resData.Result != ErrorCode.None)
            {                    
                _sessionMgr.SetStateLogin(sessionIndex);
            }
            else
            {
                _sessionMgr.SetRoomEntered(sessionIndex, resData.RoomNumber);
            }

            ResponseEnterRoomToClient(resData.Result, sessionID);

            MainServer.s_MainLogger.Debug("ResponseRoomEnterInternal - Success");
        }
        catch (Exception ex)
        {
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    void SendInternalRoomLeavePacket(int roomNumber, string userID)
    {
        // 구현하기
        var packet = new PKTInternalNtfRoomLeave()
        {
            RoomNumber = roomNumber,
            UserID = userID,
        };

        var packetBodyData = MessagePackSerializer.Serialize(packet);

        var internalPacket = new ServerPacketData();
        internalPacket.Assign("", -1, (Int16)PacketId.NtfInRoomLeave, packetBodyData);

        SendInternalRoomProcessor(false, roomNumber, internalPacket);
    }
                  
}
