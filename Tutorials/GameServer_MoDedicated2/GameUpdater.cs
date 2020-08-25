using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace GameServer
{
    class GameUpdater
    {
        bool IsThreadRunning = false;
        Thread ProcessThread = null;

        UInt16 MaxGameCount = 0;
        GameLogic[] GameLogics = null;

        ConcurrentQueue<InOutGameElement> NewGameQueue = new ConcurrentQueue<InOutGameElement>();

        public void Init(UInt16 maxGameCount)
        {
            MaxGameCount = maxGameCount;
            GameLogics = new GameLogic[maxGameCount];

            IsThreadRunning = true;
            ProcessThread = new Thread(this.Process);
            ProcessThread.Start();
        }

        public void Stop()
        {
            if (IsThreadRunning == false)
            {
                return;
            }

            IsThreadRunning = false;
            ProcessThread.Join();
        }

        public void NewGame(UInt16 index, GameLogic game)
        {
            NewGameQueue.Enqueue(new InOutGameElement { Index = index, GameObj = game })  ;
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                if(NewGameQueue.TryDequeue(out var newGame))
                {
                    GameLogics[newGame.Index] = newGame.GameObj;
                }

                for(var i = 0; i < MaxGameCount; ++ i)
                {
                    if (GameLogics[i] == null)
                    {
                        continue;
                    }

                    if(GameLogics[i].IsStop)
                    {
                        GameLogics[i] = null;
                    }

                    GameLogics[i].Update();
                }                

                Thread.Sleep(1);
            }
        }
    }

    class InOutGameElement
    {
        public UInt16 Index;
        public GameLogic GameObj;
    }
}
