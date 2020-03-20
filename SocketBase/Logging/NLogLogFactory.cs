using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketBase.Logging
{
#if (__NOT_USE_NLOG__ != true)  //NLog를 사용하지 않는다면 __NOT_USE_NLOG__ 선언한다
    public class NLogLogFactory :  LogFactoryBase
    {
        public NLogLogFactory()
            : this("NLog.config")
        {
        }

        public NLogLogFactory(string nlogConfig)
            : base(nlogConfig)
        {
            if (!IsSharedConfig)
            {
                NLog.Config.XmlLoggingConfiguration.SetCandidateConfigFilePaths(new[] { ConfigFile });
            }
            else
            {                
            }
        }

        public override ILog GetLog(string name)
        {
            return new NLogLog(NLog.LogManager.GetLogger(name));
        }
    }
#endif
}
