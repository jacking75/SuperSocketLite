using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;


namespace PvPGameServer
{
    public class PKHCommon : PKHandler
    {
        public void RegistPacketHandler(Dictionary<int, Action<EFBinaryRequestInfo>> packetHandlerMap)
        {            
            packetHandlerMap.Add((int)PACKETID.NTF_IN_CONNECT_CLIENT, NotifyInConnectClient);
            packetHandlerMap.Add((int)PACKETID.NTF_IN_DISCONNECT_CLIENT, NotifyInDisConnectClient);

            packetHandlerMap.Add((int)PACKETID.REQ_LOGIN, RequestLogin);
                                                
        }

        public void NotifyInConnectClient(EFBinaryRequestInfo requestData)
        {
        }

        public void NotifyInDisConnectClient(EFBinaryRequestInfo requestData)
        {
            var sessionID = requestData.SessionID;
            var user = UserMgr.GetUser(sessionID);
            
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
                    internalPacket.Assign(sessionID, (Int16)PACKETID.NTF_IN_ROOM_LEAVE, packetBodyData);

                    DistributePacket(internalPacket);
                }

                UserMgr.RemoveUser(sessionID);
            }
        }


        public void RequestLogin(EFBinaryRequestInfo packetData)
        {
            var sessionID = packetData.SessionID;
            MainServer.MainLogger.Debug("로그인 요청 받음");

            try
            {
                if(UserMgr.GetUser(sessionID) != null)
                {
                    ResponseLoginToClient(ERROR_CODE.LOGIN_ALREADY_WORKING, packetData.SessionID);
                    return;
                }
                                
                var reqData = MessagePackSerializer.Deserialize< PKTReqLogin>(packetData.Data);
                var errorCode = UserMgr.AddUser(reqData.UserID, sessionID);
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

            var sendData = MessagePackSerializer.Serialize(resLogin);
            WriteHeaderInfo(PACKETID.RES_LOGIN, sendData);
  
            NetSendFunc(sessionID, sendData);
        }

        public void NotifyMustCloseToClient(ERROR_CODE errorCode, string sessionID)
        {
            var resLogin = new PKNtfMustClose()
            {
                Result = (short)errorCode
            };

            var bodyData = MessagePackSerializer.Serialize(resLogin);
            var sendData = PacketToBytes.Make(PACKETID.NTF_MUST_CLOSE, bodyData);

            NetSendFunc(sessionID, sendData);
        }


        
                      
    }
}
