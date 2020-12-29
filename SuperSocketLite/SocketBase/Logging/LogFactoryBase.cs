using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SuperSocket.SocketBase.Logging
{
    /// <summary>
    /// LogFactory Base class
    /// </summary>
    public abstract class LogFactoryBase : ILogFactory
    {
        /// <summary>
        /// Gets the config file file path.
        /// </summary>
        protected string ConfigFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the server instance is running in isolation mode and the multiple server instances share the same logging configuration.
        /// </summary>
        protected bool IsSharedConfig { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogFactoryBase"/> class.
        /// </summary>
        /// <param name="configFile">The config file.</param>
        protected LogFactoryBase(string configFile)
        {
            if (Path.IsPathRooted(configFile))
            {
                ConfigFile = configFile;
                return;
            }

            if (Path.DirectorySeparatorChar != '\\')
            {
                // 원본에서는 윈도우와 비윈도우 간에 로그 파일을 다르게 하기 위해서 아래처럼 했음
                //configFile = Path.GetFileNameWithoutExtension(configFile) + ".unix" + Path.GetExtension(configFile);
                configFile = Path.GetFileNameWithoutExtension(configFile) + Path.GetExtension(configFile);
            }

            
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);

            if (File.Exists(filePath))
            {
                ConfigFile = filePath;
                return;
            }

            filePath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFile);

            if (File.Exists(filePath))
            {
                ConfigFile = filePath;
                return;
            }

            ConfigFile = configFile;
        }

        /// <summary>
        /// Gets the log by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public abstract ILog GetLog(string name);
    }
}
