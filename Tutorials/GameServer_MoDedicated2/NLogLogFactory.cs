using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer;

#if (__NOT_USE_NLOG__ != true)  //NLog를 사용하지 않는다면 __NOT_USE_NLOG__ 선언한다
public class NLogLogFactory : SuperSocketLite.SocketBase.Logging.LogFactoryBase
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
            LogManager.Setup().LoadConfigurationFromFile(new[] { ConfigFile });
            // 2023.11.28 최흥배 비추천이 되어서 위의 코드로 변경
            //NLog.Config.XmlLoggingConfiguration.SetCandidateConfigFilePaths(new[] { ConfigFile });
        }
        else
        {                
        }
    }

    public override SuperSocketLite.SocketBase.Logging.ILog GetLog(string name)
    {
        return new NLogLog(NLog.LogManager.GetLogger(name));
    }
}
#endif
