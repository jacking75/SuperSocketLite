using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameServer
{
    public class GameUpdaterManager
    {
        ConcurrentQueue<GameUpdateIndexInfo> UnUseUpdateSlotPool = new ConcurrentQueue<GameUpdateIndexInfo>();

        List<GameUpdater> GameUpdaterList = new List<GameUpdater>();

        public void Init(int threadCount, UInt16 maxGameCountPerThread)
        {
            for (var i = 0; i < threadCount; ++i)
            {
                GameUpdaterList.Add(new GameUpdate());
                GameUpdaterList[i].Init(maxGameCountPerThread);
            }

            for (int i = 0; i < maxGameCountPerThread; ++i)
            {
                for (var j = 0; j < threadCount; ++j)
                {
                    UnUseUpdateSlotPool.Enqueue(new GameUpdateIndexInfo((UInt16)j, (UInt16)i));
                }
            }
        }

        public bool NewStartGame(GameLogic game)
        {
            game.Start();

            if (UnUseUpdateSlotPool.TryDequeue(out var index))
            {
                GameUpdaterList[index.UpdaterIndex].NewGame(index.ElementIndex, game);
                return true;
            }

            return false;
        }

        public void AllStop()
        {
            foreach(var gameUpdate in GameUpdaterList)
            {
                gameUpdate.Stop();
            }
        }
               
    }

    struct GameUpdateIndexInfo
    {
        public GameUpdateIndexInfo(UInt16 updaterIndex, UInt16 elementIndex)
        {
            UpdaterIndex = updaterIndex;
            ElementIndex = elementIndex;
        }

        public UInt16 UpdaterIndex;
        public UInt16 ElementIndex;

    }
}
