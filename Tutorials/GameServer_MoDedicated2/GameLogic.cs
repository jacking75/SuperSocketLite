using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class GameLogic
    {
        UInt32 Index = 0;
        ConcurrentQueue<GameMessage> MsgQueue = new ConcurrentQueue<GameMessage>();

        UInt16 UpdateIntervalMilliSec = 0;
        DateTime PrevUpdateTime = DateTime.Now;

        public void Init(UInt32 index, UInt16 intervalMSec)
        {
            Index = index;
            UpdateIntervalMilliSec = intervalMSec;
        }

        public void Clear()
        {

        }

        public void Start()
        {
            PrevUpdateTime = DateTime.Now.AddMilliseconds(-UpdateIntervalMilliSec);
        }
                
        public void AddMessage(UInt16 msgId, byte[] msgData)
        {
            MsgQueue.Enqueue(new GameMessage(msgId, msgData));
        }

        public bool Update()
        {
            var curTime = DateTime.Now;
            var diffTime = curTime - PrevUpdateTime;

            if(diffTime.TotalMilliseconds < UpdateIntervalMilliSec)
            {
                return false;
            }

            PrevUpdateTime = curTime;

            MainServer.MainLogger.Debug($"[GameLogic-Update] Call. Index:{Index}, [{curTime.Millisecond}]");

            if (MsgQueue.TryDequeue(out var gameMsg))
            {
                MainServer.MainLogger.Debug($"[GameLogic-Update] id: {gameMsg.MsgId}. Index:{Index}");

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

}
