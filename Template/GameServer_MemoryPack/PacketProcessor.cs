using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks.Dataflow;


namespace GameServer_MemoryPack;

class PacketProcessor
{
    bool _isThreadRunning = false;
    System.Threading.Thread _processThread = null;

    public Func<string, byte[], bool> NetSendFunc;
    
    //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
    //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
    BufferBlock<PacketRequestInfo> _packetBuffer = new ();

    
    Dictionary<int, Action<PacketRequestInfo>> _packetHandlerDict = new ();
    PKHCommon _commonPacketHandler = new ();
           

    public void CreateAndStart(ServerOption serverOpt)
    {   
        RegistPacketHandler();

        _isThreadRunning = true;
        _processThread = new System.Threading.Thread(this.Process);
        _processThread.Start();
    }
    
    public void Destory()
    {
        MainServer.s_MainLogger.Info("PacketProcessor::Destory - begin");

        _isThreadRunning = false;
        _packetBuffer.Complete();

        _processThread.Join();

        MainServer.s_MainLogger.Info("PacketProcessor::Destory - end");
    }
          
    public void InsertPacket(PacketRequestInfo data)
    {
        _packetBuffer.Post(data);
    }

    
    void RegistPacketHandler()
    {
        PKHandler.NetSendFunc = NetSendFunc;
        PKHandler.DistributeInnerPacket = InsertPacket;

        _commonPacketHandler.Init();
        _commonPacketHandler.RegistPacketHandler(_packetHandlerDict); 
    }

    void Process()
    {
        while (_isThreadRunning)
        {
            try
            {
                var packet = _packetBuffer.Receive();

                var header = new MemoryPackPacketHeader();
                header.Read(packet.Data);

                if (_packetHandlerDict.ContainsKey(header.Id))
                {
                    _packetHandlerDict[header.Id](packet);
                }
                /*else
                {
                    System.Diagnostics.Debug.WriteLine("세션 번호 {0}, PacketID {1}, 받은 데이터 크기: {2}", packet.SessionID, packet.PacketID, packet.BodyData.Length);
                }*/
            }
            catch (Exception ex)
            {
                if (_isThreadRunning)
                {
                    MainServer.s_MainLogger.Error(ex.ToString());
                }
            }
        }
    }


}
