using System;
using System.Threading.Tasks.Dataflow;

using CommonLib;


namespace GateServer;

class PacketProcessor
{
    bool _isThreadRunning = false;
    System.Threading.Thread _processThread = null;

    //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
    //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
    BufferBlock<ServerPacketData> _packetBuffer = new ();

    PKHandler _packetHandler = new ();


    public void CreateAndStart(Action<string, byte[]> sendPacket)
    {
        _packetHandler.Init(sendPacket);

        _isThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }
    
    public void Destory()
    {
        _isThreadRunning = false;
        _packetBuffer.Complete();
    }
          
    public void InsertPacket(ServerPacketData data)
    {
        _packetBuffer.Post(data);
    }
         
    void Process()
    {
        while (_isThreadRunning)
        {
            //System.Threading.Thread.Sleep(64); //테스트 용
            try
            {
                var packet = _packetBuffer.Receive();

                _packetHandler.Process(packet);                    
            }
            catch (Exception ex)
            {
                if(_isThreadRunning)
                {
                    MainServer.s_MainLogger.Error(ex.ToString());
                }
            }
        }
    }


}
