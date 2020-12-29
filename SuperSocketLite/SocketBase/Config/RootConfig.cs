using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using System.Collections.Specialized;
using System.Configuration;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Root configuration model
    /// </summary>
    [Serializable]
    public partial class RootConfig : IRootConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootConfig"/> class.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        public RootConfig(IRootConfig rootConfig)
        {
            rootConfig.CopyPropertiesTo(this);
            this.OptionElements = rootConfig.OptionElements;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootConfig"/> class.
        /// </summary>
        public RootConfig()
        {
            int maxWorkingThread, maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThread, out maxCompletionPortThreads);
            MaxWorkingThreads = maxWorkingThread;
            MaxCompletionPortThreads = maxCompletionPortThreads;

            int minWorkingThread, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkingThread, out minCompletionPortThreads);
            MinWorkingThreads = minWorkingThread;
            MinCompletionPortThreads = minCompletionPortThreads;            
        }

        

        /// <summary>
        /// Gets/Sets the max working threads.
        /// </summary>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the min working threads.
        /// </summary>
        public int MinWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the max completion port threads.
        /// </summary>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets/sets the min completion port threads.
        /// </summary>
        public int MinCompletionPortThreads { get; set; }

        
        /// <summary>
        /// Gets/sets the log factory name.
        /// </summary>
        /// <value>
        /// The log factory.
        /// </value>
        public string LogFactory { get; set; }

        /// <summary>
        /// Gets/sets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; set; }

        
        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        /// <value>
        /// The default culture.
        /// </value>
        public string DefaultCulture { get; set; }
    }
}
