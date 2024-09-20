using System;
using System.Collections.Concurrent;
using System.Threading;


namespace GameServer;

// 게임 로직을 주기적으로 업데이트 하는 클래스
class GameUpdater
{
    bool _isThreadRunning = false;
    Thread _processThread = null;

    UInt16 _maxGameCount = 0;
    GameLogic[] _gameLogics = null;

    ConcurrentQueue<InOutGameElement> _newGameQueue = new ConcurrentQueue<InOutGameElement>();


    public void Init(UInt16 maxGameCount)
    {
        _maxGameCount = maxGameCount;
        _gameLogics = new GameLogic[maxGameCount];

        _isThreadRunning = true;
        _processThread = new Thread(this.Process);
        _processThread.Start();
    }

    public void Stop()
    {
        if (_isThreadRunning == false)
        {
            return;
        }

        _isThreadRunning = false;
        _processThread.Join();
    }

    public void NewGame(UInt16 index, GameLogic game)
    {
        _newGameQueue.Enqueue(new InOutGameElement { Index = index, GameObj = game })  ;
    }

    // 업데이트 주기를 최대한 원하는 시간에 맞추기 위해서 Sleep을 사용
    void Process()
    {
        while (_isThreadRunning)
        {
            if(_newGameQueue.TryDequeue(out var newGame))
            {
                _gameLogics[newGame.Index] = newGame.GameObj;
            }

            for(var i = 0; i < _maxGameCount; ++ i)
            {
                if (_gameLogics[i] == null)
                {
                    continue;
                }

                if(_gameLogics[i].IsStop)
                {
                    _gameLogics[i] = null;
                }

                _gameLogics[i].Update();
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
