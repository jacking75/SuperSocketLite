using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;


namespace EchoServerEx
{
    public enum LOG_LEVEL
    {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR
    }

    public class DevLog
    {
        static System.Collections.Concurrent.ConcurrentQueue<string> logMsgQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();

        static LOG_LEVEL 출력가능_로그레벨 = new LOG_LEVEL();

        static public void Init(LOG_LEVEL logLevel)
        {
            출력가능_로그레벨 = logLevel;
        }

        static public void ChangeLogLevel(LOG_LEVEL logLevel)
        {
            출력가능_로그레벨 = logLevel;
        }

        static public void Write(string msg, LOG_LEVEL logLevel = LOG_LEVEL.TRACE,
                                [CallerFilePath] string fileName = "",
                                [CallerMemberName] string methodName = "",
                                [CallerLineNumber] int lineNumber = 0)
        {
            if (출력가능_로그레벨 <= logLevel)
            {
                logMsgQueue.Enqueue(string.Format("{0}:{1}| {2}", DateTime.Now, methodName, msg));
            }
        }

        static public bool GetLog(out string msg)
        {
            if (logMsgQueue.TryDequeue(out msg))
            {
                return true;
            }

            return false;
        }

    }
}
