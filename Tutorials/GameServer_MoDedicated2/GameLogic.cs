using System;
using System.Collections.Concurrent;


namespace GameServer;

public class GameLogic
{
    UInt32 _index = 0;
    ConcurrentQueue<GameMessage> _msgQueue = new ConcurrentQueue<GameMessage>();

    UInt16 _updateIntervalMilliSec = 0;
    DateTime _prevUpdateTime = DateTime.Now;

    public bool IsStop { get; private set; } = false;


    public void Init(UInt32 index, UInt16 intervalMSec)
    {
        _index = index;
        _updateIntervalMilliSec = intervalMSec;
    }

    public void Clear()
    {
    }

    public void Start()
    {
        IsStop = false;
        _prevUpdateTime = DateTime.Now.AddMilliseconds(-_updateIntervalMilliSec);
    }

    public void Stop()
    {
        IsStop = true;
    }
            
    public void AddMessage(UInt16 msgId, byte[] msgData)
    {
        _msgQueue.Enqueue(new GameMessage(msgId, msgData));
    }

    public bool Update()
    {
        var curTime = DateTime.Now;
        var diffTime = curTime - _prevUpdateTime;

        if(diffTime.TotalMilliseconds < _updateIntervalMilliSec)
        {
            return false;
        }

        _prevUpdateTime = curTime;

        MainServer.MainLogger.Debug($"[GameLogic-Update] Call. Index:{_index}, [{curTime.Millisecond}]");

        if (_msgQueue.TryDequeue(out var gameMsg))
        {
            MainServer.MainLogger.Debug($"[GameLogic-Update] id: {gameMsg.MsgId}. Index:{_index}");

            if (gameMsg.MsgId == 0)
            {
                return false;
            }
        }
                    
        return true;
    }
    
}


public struct GameMessage
{
    public GameMessage(UInt16 msgId, byte[] msgData)
    {
        MsgId = msgId;
        MsgData = msgData;
    }

    public UInt16 MsgId;
    public byte[] MsgData;
}
