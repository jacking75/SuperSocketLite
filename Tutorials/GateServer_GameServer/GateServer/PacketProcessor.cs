using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

using CommonLib;

namespace GateServer
{
    class PacketProcessor
    {
        bool IsThreadRunning = false;
        System.Threading.Thread ProcessThread = null;

        //receive쪽에서 처리하지 않아도 Post에서 블럭킹 되지 않는다. 
        //BufferBlock<T>(DataflowBlockOptions) 에서 DataflowBlockOptions의 BoundedCapacity로 버퍼 가능 수 지정. BoundedCapacity 보다 크게 쌓이면 블럭킹 된다
        BufferBlock<ServerPacketData> MsgBuffer = new BufferBlock<ServerPacketData>();

        PKHandler PacketHandler = new PKHandler();

        public void CreateAndStart(Action<string, byte[]> sendPacket)
        {
            PacketHandler.Init(sendPacket);

            IsThreadRunning = true;
            ProcessThread = new System.Threading.Thread(this.Process);
            ProcessThread.Start();
        }
        
        public void Destory()
        {
            IsThreadRunning = false;
            MsgBuffer.Complete();
        }
              
        public void InsertPacket(ServerPacketData data)
        {
            MsgBuffer.Post(data);
        }
             
        void Process()
        {
            while (IsThreadRunning)
            {
                //System.Threading.Thread.Sleep(64); //테스트 용
                try
                {
                    var packet = MsgBuffer.Receive();

                    PacketHandler.Process(packet);                    
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => MainServer.MainLogger.Error(ex.ToString()));
                }
            }
        }


    }
}
