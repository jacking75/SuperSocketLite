﻿using System;

namespace SuperSocket.SocketBase.Config;

/// <summary>
/// Command assembly config
/// </summary>
[Serializable]
public class CommandAssemblyConfig : ICommandAssemblyConfig
{
    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    /// <value>
    /// The assembly.
    /// </value>
    public string Assembly { get; set; }
}
