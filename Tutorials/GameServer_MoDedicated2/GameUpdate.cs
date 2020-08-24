using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace GameServer
{
    class GameUpdate
    {
        bool IsThreadRunning = false;
        Thread ProcessThread = null;

        UInt16 MaxGameCount = 0;
        GameLogic[] GameLogics = null;

        ConcurrentQueue<InOutGameElement> InOutGameQueue = new ConcurrentQueue<InOutGameElement>();

        public void Init(UInt16 maxGameCount)
        {
            MaxGameCount = maxGameCount;
            GameLogics = new GameLogic[maxGameCount];

            IsThreadRunning = true;
            ProcessThread = new Thread(this.Process);
            ProcessThread.Start();
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                if(InOutGameQueue.TryDequeue(out var newGame))
                {
                    GameLogics[newGame.index] = newGame.GameObj;
                }

                foreach(var game in GameLogics)
                {
                    if(game == null)
                    {
                        continue;
                    }

                    game.Update();
                }

                Thread.Sleep(1);
            }
        }
    }

    class InOutGameElement
    {
        public UInt16 index;
        public GameLogic GameObj;
    }
}
