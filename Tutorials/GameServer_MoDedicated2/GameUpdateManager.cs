using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServer
{
    class GameUpdateManager
    {
        ConcurrentQueue<GameUpdateIndexInfo> GameUpdateIndexPool = new ConcurrentQueue<GameUpdateIndexInfo>();

        List<GameUpdate> GameUpdateList = new List<GameUpdate>();

        public void Init(int threadCount, UInt16 maxGameCountPerThread)
        {
            for (var i = 0; i < threadCount; ++i)
            {
                GameUpdateList.Add(new GameUpdate());
                GameUpdateList[i].Init(maxGameCountPerThread);
            }

            for (int i = 0; i < maxGameCountPerThread; ++i)
            {
                for (var j = 0; j < threadCount; ++j)
                {
                    GameUpdateIndexPool.Enqueue(new GameUpdateIndexInfo((UInt16)j, (UInt16)i));
                }
            }
        }

        public bool NewStartGame(GameLogic game)
        {
            if(GameUpdateIndexPool.TryDequeue(out var index))
            {
                GameUpdateList[index.UpdateIndex]
                return true;
            }

            return false;
        }

       
    }

    struct GameUpdateIndexInfo
    {
        public GameUpdateIndexInfo(UInt16 updateIndex, UInt16 elementIndex)
        {
            UpdateIndex = updateIndex;
            ElementIndex = elementIndex;
        }

        public UInt16 UpdateIndex;
        public UInt16 ElementIndex;

    }
}
