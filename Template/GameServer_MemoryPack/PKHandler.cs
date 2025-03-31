using System;


namespace GameServer_MemoryPack;

public class PKHandler
{
    public static Func<string, byte[], bool> NetSendFunc;
    public static Action<PacketRequestInfo> DistributeInnerPacket;

    
    public void Init()
    {
    }           
            
    
}
