using System.Collections.Concurrent;

using CommonLib;


namespace GateServer;

public class ClientSessionManager
{
    static public int s_MaxSessionCount { get; private set; } = 0;
    static ConcurrentBag<int> s_indexPool = new ();

    ConcurrentDictionary<int, ClientSession> _sessionDict = new ();


    public void Init(int maxCount)
    {
        for (int i = 0; i < maxCount; ++i)
        {
            s_indexPool.Add(i);
        }

        s_MaxSessionCount = maxCount;
    }
     
    public bool NewSession(ClientSession session)
    {
        var index = PopIndex();
        if(index == -1)
        {
            return false;
        }

        session.SetSessionIndex(index);
        _sessionDict.TryAdd(index, session);
        return true;
    }

    public void ColesdSession(ClientSession session)
    {
        PushIndex(session.SessionIndex);
        _sessionDict.TryRemove(session.SessionIndex, out var _);
    }

    int PopIndex()
    {
        if (s_indexPool.TryTake(out var result))
        {
            return result;
        }

        return -1;
    }

    void PushIndex(int index)
    {
        if (index >= 0)
        {
            s_indexPool.Add(index);
        }
    }
            
    


}



