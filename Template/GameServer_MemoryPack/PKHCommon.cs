using MemoryPack;
using System;
using System.Collections.Generic;



namespace GameServer_MemoryPack;

public class PKHCommon : PKHandler
{
    public void RegistPacketHandler(Dictionary<int, Action<PacketRequestInfo>> packetHandlerDict)
    {
        packetHandlerDict.Add((int)PacketId.ReqResTestEcho, HandleRequestEcho);
        packetHandlerDict.Add((int)PacketId.ReqLogin, HandleRequestLogin);
    }
        

    public void HandleRequestEcho(PacketRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("Ehco 요청 받음");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqResEcho>(packetData.Data);
            MainServer.s_MainLogger.Debug($"Ehco: {reqData.DummyData}");

            var response = new PKTReqResEcho()
            {
                DummyData = reqData.DummyData
            };

            var sendData = MemoryPackSerializer.Serialize(response);
            MemoryPackPacketHeader.Write(sendData, PacketId.ReqResTestEcho);

            NetSendFunc(sessionID, sendData);

        }
        catch (Exception ex)
        {
            // 패킷 해제에 의해서 로그가 남지 않도록 로그 수준을 Debug로 한다.
            MainServer.s_MainLogger.Error(ex.ToString());
        }
    }

    public void HandleRequestLogin(PacketRequestInfo packetData)
    {
        var sessionID = packetData.SessionID;
        MainServer.s_MainLogger.Debug("로그인 요청 받음");

        try
        {
            var reqData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packetData.Data);
            
                        

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

    
                  
}
