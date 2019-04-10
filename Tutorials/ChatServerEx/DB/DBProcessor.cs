using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks.Dataflow;

using CSBaseLib;

namespace DB
{
    public class DBProcessor
    {
        public SuperSocket.SocketBase.Logging.ILog MainLogger;
        bool IsThreadRunning = false;
        List<System.Threading.Thread> ThreadList = new List<System.Threading.Thread>();

        BufferBlock<DBQueue> MsgBuffer = new BufferBlock<DBQueue>();

        Dictionary<PACKETID, Func<DBQueue, DBResultQueue>> DBWorkHandlerMap = new Dictionary<PACKETID, Func<DBQueue, DBResultQueue>>();
        DBJobWorkHandler DBWorkHandler = null;

        Action<DBResultQueue> DBWorkResultFunc = null;

        RedisLib RedisWraper = new RedisLib();


        public ERROR_CODE CreateAndStart(int threadCount, Action<DBResultQueue> dbWorkResultFunc, string redisAddress)
        {
            MainLogger.Info("DB Init Start");

            RedisWraper.Init(redisAddress);

            DBWorkResultFunc = dbWorkResultFunc;
            var error = RegistPacketHandler();

            if (error.Item1 != ERROR_CODE.NONE)
            {
                return error.Item1;
            }


            IsThreadRunning = true;

            for (int i = 0; i < threadCount; ++i)
            {
                var processThread = new System.Threading.Thread(this.Process);
                processThread.Start();

                ThreadList.Add(processThread);
            }

            MainLogger.Info("DB Init Success");
            return ERROR_CODE.NONE;
        }

        public void Destory()
        {
            IsThreadRunning = false;
            MsgBuffer.Complete();
        }

        public void InsertMsg(DBQueue dbQueue)
        {
            MsgBuffer.Post(dbQueue);
        }


        Tuple<ERROR_CODE, string> RegistPacketHandler()
        {
            DBWorkHandler = new DBJobWorkHandler();
            var error = DBWorkHandler.Init(RedisWraper);

            if (error.Item1 != ERROR_CODE.NONE)
            {
                return error;
            }


            DBWorkHandlerMap.Add(PACKETID.REQ_DB_LOGIN, DBWorkHandler.RequestLogin);

            return new Tuple<ERROR_CODE, string>(ERROR_CODE.NONE, "");
        }

        void Process()
        {
            while (IsThreadRunning)
            {
                try
                {
                    var dbJob = MsgBuffer.Receive();

                    if (DBWorkHandlerMap.ContainsKey(dbJob.PacketID))
                    {
                        var result = DBWorkHandlerMap[dbJob.PacketID](dbJob);
                        DBWorkResultFunc(result);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("세션 번호 {0}, DBWorkID {1}", dbJob.SessionID, dbJob.PacketID);
                    }
                }
                catch (Exception ex)
                {
                    IsThreadRunning.IfTrue(() => MainLogger.Error(ex.ToString()));
                }
            }
        }
    }

    
}
