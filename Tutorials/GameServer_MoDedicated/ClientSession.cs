using System;
using System.Collections.Concurrent;

using SuperSocketLite.SocketBase;


namespace GameServer;

public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
{
    static public int s_MaxSessionCount { get; private set; } = 0;
    static ConcurrentBag<int> s_indexPool = new ();
   
    public int SessionIndex { get; private set; } = -1;

    
    public static void CreateIndexPool(int maxCount)
    {
        for(int i = 0; i < maxCount; ++i)
        {
            s_indexPool.Add(i);
        }

        s_MaxSessionCount = maxCount;
    }

    public static int PopIndex()
    {
        if (s_indexPool.TryTake(out var result))
        {
            return result;
        }

        return -1;
    }

    public static void PushIndex(int index)
    {
        if (index >= 0)
        {
            s_indexPool.Add(index);
        }
    }

    public void AllocSessionIndex()
    {
        SessionIndex = PopIndex();
    }

    public void FreeSessionIndex(int index)
    {
        PushIndex(index);
    }
  
}
