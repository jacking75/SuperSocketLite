using System;
using System.Collections.Generic;
using System.Text;

namespace EchoServerEx
{
    public class NLogLog : SuperSocket.SocketBase.Logging.ILog
    {
        private NLog.ILogger Log;

        public NLogLog(NLog.ILogger log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            Log = log;
        }

        public bool IsDebugEnabled
        {
            get { return Log.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return Log.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return Log.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return Log.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return Log.IsWarnEnabled; }
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }                         
        
        public void Error(string message)
        {
            Log.Error(message);
        }
        
        public void Error(string message, Exception exception)
        {
            Log.Error($"msg:{message}, exception:{exception.ToString()}");
        }
                
        public void Fatal(string message)
        {
            Log.Fatal(message);
        }
        
        public void Fatal(string message, Exception exception)
        {
            Log.Fatal($"msg:{message}, exception:{exception.ToString()}");
        }
              
        public void Info(string message)
        {
            Log.Info(message);
        }
                 
        public void Warn(string message)
        {
            Log.Warn(message);
        }

        
        
    }

}
