﻿using System.Collections.Specialized;

namespace SuperSocketLite.SocketBase.Config;

/// <summary>
/// The root configuration interface
/// </summary>
public partial interface IRootConfig
{
    /// <summary>
    /// Gets the max working threads.
    /// </summary>
    int MaxWorkingThreads { get; }

    /// <summary>
    /// Gets the min working threads.
    /// </summary>
    int MinWorkingThreads { get; }

    /// <summary>
    /// Gets the max completion port threads.
    /// </summary>
    int MaxCompletionPortThreads { get; }

    /// <summary>
    /// Gets the min completion port threads.
    /// </summary>
    int MinCompletionPortThreads { get; }

            
    /// <summary>
    /// Gets the log factory name.
    /// </summary>
    /// <value>
    /// The log factory.
    /// </value>
    string LogFactory { get; }


    
    /// <summary>
    /// Gets the option elements.
    /// </summary>
    NameValueCollection OptionElements { get; }


    /// <summary>
    /// Gets the default culture for all server instances.
    /// </summary>
    /// <value>
    /// The default culture.
    /// </value>
    string DefaultCulture { get; }
}
