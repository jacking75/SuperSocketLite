﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Common;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Security;


namespace SuperSocket.SocketBase;

/// <summary>
/// AppServer base class
/// </summary>
/// <typeparam name="TAppSession">The type of the app session.</typeparam>
/// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
public abstract partial class AppServerBase<TAppSession, TRequestInfo> : IAppServer<TAppSession, TRequestInfo>, IRawDataProcessor<TAppSession>, IRequestHandler<TRequestInfo>, ISocketServerAccessor, IRemoteCertificateValidator, IActiveConnector, ISystemEndPoint, IDisposable
    where TRequestInfo : class, IRequestInfo
    where TAppSession : AppSession<TAppSession, TRequestInfo>, IAppSession, new()
{
    /// <summary>
    /// Null appSession instance
    /// </summary>
    protected readonly TAppSession NullAppSession = default(TAppSession);

    /// <summary>
    /// Gets the server's config.
    /// </summary>
    public IServerConfig Config { get; private set; }

    //Server instance name
    private string m_Name;

    /// <summary>
    /// the current state's code
    /// </summary>
    private int m_StateCode = ServerStateConst.NotInitialized;

    /// <summary>
    /// Gets the current state of the work item.
    /// </summary>
    /// <value>
    /// The state.
    /// </value>
    public ServerState State
    {
        get
        {
            return (ServerState)m_StateCode;
        }
    }

    /// <summary>
    /// Gets the certificate of current server.
    /// </summary>
    public X509Certificate Certificate { get; private set; }

    /// <summary>
    /// Gets or sets the receive filter factory.
    /// </summary>
    /// <value>
    /// The receive filter factory.
    /// </value>
    public virtual IReceiveFilterFactory<TRequestInfo> ReceiveFilterFactory { get; protected set; }

    /// <summary>
    /// Gets the Receive filter factory.
    /// </summary>
    object IAppServer.ReceiveFilterFactory
    {
        get { return this.ReceiveFilterFactory; }
    }
      

    private ISocketServerFactory m_SocketServerFactory;

    /// <summary>
    /// Gets the basic transfer layer security protocol.
    /// </summary>
    public SslProtocols BasicSecurity { get; private set; }

    /// <summary>
    /// Gets the root config.
    /// </summary>
    protected IRootConfig RootConfig { get; private set; }

    /// <summary>
    /// Gets the logger assosiated with this object.
    /// </summary>
    public ILog Logger { get; private set; }
            
    private static bool m_ThreadPoolConfigured = false;

    private List<IConnectionFilter> m_ConnectionFilters;

    private long m_TotalHandledRequests = 0;

    /// <summary>
    /// Gets the total handled requests number.
    /// </summary>
    protected long TotalHandledRequests
    {
        get { return m_TotalHandledRequests; }
    }

    private ListenerInfo[] m_Listeners;

    /// <summary>
    /// Gets or sets the listeners inforamtion.
    /// </summary>
    /// <value>
    /// The listeners.
    /// </value>
    public ListenerInfo[] Listeners
    {
        get { return m_Listeners; }
    }

    /// <summary>
    /// Gets the started time of this server instance.
    /// </summary>
    /// <value>
    /// The started time.
    /// </value>
    public DateTime StartedTime { get; private set; }


    /// <summary>
    /// Gets or sets the log factory.
    /// </summary>
    /// <value>
    /// The log factory.
    /// </value>
    public ILogFactory LogFactory { get; private set; }


    /// <summary>
    /// Gets the default text encoding.
    /// </summary>
    /// <value>
    /// The text encoding.
    /// </value>
    public Encoding TextEncoding { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
    /// </summary>
    public AppServerBase()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppServerBase&lt;TAppSession, TRequestInfo&gt;"/> class.
    /// </summary>
    /// <param name="receiveFilterFactory">The Receive filter factory.</param>
    public AppServerBase(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory)
    {
        this.ReceiveFilterFactory = receiveFilterFactory;
    }

            
    /// <summary>
    /// Setups the specified root config.
    /// </summary>
    /// <param name="rootConfig">The root config.</param>
    /// <param name="config">The config.</param>
    /// <returns></returns>
    protected virtual bool Setup(IRootConfig rootConfig, IServerConfig config)
    {
        return true;
    }

    partial void SetDefaultCulture(IRootConfig rootConfig, IServerConfig config);

    private void SetupBasic(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory)
    {
        if (rootConfig == null)
            throw new ArgumentNullException("rootConfig");

        RootConfig = rootConfig;

        if (config == null)
            throw new ArgumentNullException("config");

        if (!string.IsNullOrEmpty(config.Name))
            m_Name = config.Name;
        else
            m_Name = string.Format("{0}-{1}", this.GetType().Name, Math.Abs(this.GetHashCode()));

        Config = config;

        SetDefaultCulture(rootConfig, config);

        if (!m_ThreadPoolConfigured)
        {
            if (!TheadPoolEx.ResetThreadPool(rootConfig.MaxWorkingThreads >= 0 ? rootConfig.MaxWorkingThreads : new Nullable<int>(),
                    rootConfig.MaxCompletionPortThreads >= 0 ? rootConfig.MaxCompletionPortThreads : new Nullable<int>(),
                    rootConfig.MinWorkingThreads >= 0 ? rootConfig.MinWorkingThreads : new Nullable<int>(),
                    rootConfig.MinCompletionPortThreads >= 0 ? rootConfig.MinCompletionPortThreads : new Nullable<int>()))
            {
                throw new Exception("Failed to configure thread pool!");
            }

            m_ThreadPoolConfigured = true;
        }

        if (socketServerFactory == null)
        {
//                var socketServerFactoryType = Type.GetType("SuperSocket.SocketEngine.SocketServerFactory, SuperSocket.SocketEngine", true);
            var socketServerFactoryType = Type.GetType("SuperSocket.SocketEngine.SocketServerFactory, SuperSocketLite", true);
            socketServerFactory = (ISocketServerFactory)Activator.CreateInstance(socketServerFactoryType);
        }

        m_SocketServerFactory = socketServerFactory;

        //Read text encoding from the configuration
        if (!string.IsNullOrEmpty(config.TextEncoding))
            TextEncoding = Encoding.GetEncoding(config.TextEncoding);
        else
            TextEncoding = new ASCIIEncoding();
    }

    private bool SetupMedium(IReceiveFilterFactory<TRequestInfo> receiveFilterFactory, IEnumerable<IConnectionFilter> connectionFilters)
    {
        if (receiveFilterFactory != null)
            ReceiveFilterFactory = receiveFilterFactory;

        if (connectionFilters != null && connectionFilters.Any())
        {
            if (m_ConnectionFilters == null)
                m_ConnectionFilters = new List<IConnectionFilter>();

            m_ConnectionFilters.AddRange(connectionFilters);
        }
         
        return true;
    }

    private bool SetupAdvanced(IServerConfig config)
    {
        if (!SetupSecurity(config))
            return false;

        if (!SetupListeners(config))
            return false;
                                
        return true;
    }


    internal abstract IReceiveFilterFactory<TRequestInfo> CreateDefaultReceiveFilterFactory();

    private bool SetupFinal()
    {
        //Check receiveFilterFactory
        if (ReceiveFilterFactory == null)
        {
            ReceiveFilterFactory = CreateDefaultReceiveFilterFactory();

            if (ReceiveFilterFactory == null)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("receiveFilterFactory is required!");

                return false;
            }
        }

        var plainConfig = Config as ServerConfig;

        if (plainConfig == null)
        {
            //Using plain config model instead of .NET configuration element to improve performance
            plainConfig = new ServerConfig(Config);

            if (string.IsNullOrEmpty(plainConfig.Name))
                plainConfig.Name = Name;

            Config = plainConfig;
        }
        
        return SetupSocketServer();
    }

    /// <summary>
    /// Setups with the specified port.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns>return setup result</returns>
    public bool Setup(int port)
    {
        return Setup("Any", port);
    }

    private void TrySetInitializedState()
    {
        if (Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Initializing, ServerStateConst.NotInitialized)
                != ServerStateConst.NotInitialized)
        {
            throw new Exception("The server has been initialized already, you cannot initialize it again!");
        }
    }


    /// <summary>
    /// Setups with the specified config.
    /// </summary>
    /// <param name="config">The server config.</param>
    /// <param name="socketServerFactory">The socket server factory.</param>
    /// <param name="receiveFilterFactory">The receive filter factory.</param>
    /// <param name="logFactory">The log factory.</param>
    /// <param name="connectionFilters">The connection filters.</param>
    /// <param name="commandLoaders">The command loaders.</param>
    /// <returns></returns>
    public bool Setup(IServerConfig config, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, ILogFactory logFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
    {
        return Setup(new RootConfig(), config, socketServerFactory, receiveFilterFactory, logFactory, connectionFilters);
    }

    /// <summary>
    /// Setups the specified root config, this method used for programming setup
    /// </summary>
    /// <param name="rootConfig">The root config.</param>
    /// <param name="config">The server config.</param>
    /// <param name="socketServerFactory">The socket server factory.</param>
    /// <param name="receiveFilterFactory">The Receive filter factory.</param>
    /// <param name="logFactory">The log factory.</param>
    /// <param name="connectionFilters">The connection filters.</param>
    /// <param name="commandLoaders">The command loaders.</param>
    /// <returns></returns>
    public bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, ILogFactory logFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
    {
        TrySetInitializedState();

        SetupBasic(rootConfig, config, socketServerFactory);

        SetupLogFactory(logFactory);

        Logger = CreateLogger(this.Name);

        if (!SetupMedium(receiveFilterFactory, connectionFilters))
            return false;

        if (!SetupAdvanced(config))
            return false;

        if (!Setup(rootConfig, config))
            return false;

        if (!SetupFinal())
            return false;

        m_StateCode = ServerStateConst.NotStarted;
        return true;
    }

    /// <summary>
    /// Setups with the specified ip and port.
    /// </summary>
    /// <param name="ip">The ip.</param>
    /// <param name="port">The port.</param>
    /// <param name="socketServerFactory">The socket server factory.</param>
    /// <param name="receiveFilterFactory">The Receive filter factory.</param>
    /// <param name="logFactory">The log factory.</param>
    /// <param name="connectionFilters">The connection filters.</param>
    /// <returns>return setup result</returns>
    public bool Setup(string ip, int port, ISocketServerFactory socketServerFactory = null, IReceiveFilterFactory<TRequestInfo> receiveFilterFactory = null, ILogFactory logFactory = null, IEnumerable<IConnectionFilter> connectionFilters = null)
    {
        return Setup(new ServerConfig
                        {
                            Ip = ip,
                            Port = port
                        },
                      socketServerFactory,
                      receiveFilterFactory,
                      logFactory,
                      connectionFilters);
    }
           
    private bool SetupLogFactory(ILogFactory logFactory)
    {
        if (logFactory != null)
        {
            LogFactory = logFactory;
            return true;
        }

        //ConsoleLogFactory is default log factory
        if (LogFactory == null)
        {
            LogFactory = new ConsoleLogFactory();
        }

        return true;
    }


    /// <summary>
    /// Creates the logger for the AppServer.
    /// </summary>
    /// <param name="loggerName">Name of the logger.</param>
    /// <returns></returns>
    protected virtual ILog CreateLogger(string loggerName)
    {
        return LogFactory.GetLog(loggerName);
    }

    /// <summary>
    /// Setups the security option of socket communications.
    /// </summary>
    /// <param name="config">The config of the server instance.</param>
    /// <returns></returns>
    private bool SetupSecurity(IServerConfig config)
    {
        if (!string.IsNullOrEmpty(config.Security))
        {
            SslProtocols configProtocol;
            if (!config.Security.TryParseEnum<SslProtocols>(true, out configProtocol))
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error($"Failed to parse '{config.Security}' to SslProtocol!");

                return false;
            }

            BasicSecurity = configProtocol;
        }
        else
        {
            BasicSecurity = SslProtocols.None;
        }

        try
        {
            var certificate = GetCertificate(config.Certificate);

            if (certificate != null)
            {
                Certificate = certificate;
            }
            else if(BasicSecurity != SslProtocols.None)
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("Certificate is required in this security mode!");

                return false;
            }
            
        }
        catch (Exception e)
        {
            if (Logger.IsErrorEnabled)
                Logger.Error("Failed to initialize certificate!", e);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the certificate from server configuguration.
    /// </summary>
    /// <param name="certificate">The certificate config.</param>
    /// <returns></returns>
    protected virtual X509Certificate GetCertificate(ICertificateConfig certificate)
    {
        if (certificate == null)
        {
            if (BasicSecurity != SslProtocols.None && Logger.IsErrorEnabled)
                Logger.Error("There is no certificate configured!");
            return null;
        }

        if (string.IsNullOrEmpty(certificate.FilePath) && string.IsNullOrEmpty(certificate.Thumbprint))
        {
            if (BasicSecurity != SslProtocols.None && Logger.IsErrorEnabled)
                Logger.Error("You should define certificate node and either attribute 'filePath' or 'thumbprint' is required!");

            return null;
        }

        return CertificateManager.Initialize(certificate, GetFilePath);
    }

    bool IRemoteCertificateValidator.Validate(IAppSession session, object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return ValidateClientCertificate((TAppSession)session, sender, certificate, chain, sslPolicyErrors);
    }

    /// <summary>
    /// Validates the client certificate. This method is only used if the certificate configuration attribute "clientCertificateRequired" is true.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="sender">The sender.</param>
    /// <param name="certificate">The certificate.</param>
    /// <param name="chain">The chain.</param>
    /// <param name="sslPolicyErrors">The SSL policy errors.</param>
    /// <returns>return the validation result</returns>
    protected virtual bool ValidateClientCertificate(TAppSession session, object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return sslPolicyErrors == SslPolicyErrors.None;
    }

    /// <summary>
    /// Setups the socket server.instance
    /// </summary>
    /// <returns></returns>
    private bool SetupSocketServer()
    {
        try
        {
            m_SocketServer = m_SocketServerFactory.CreateSocketServer<TRequestInfo>(this, m_Listeners, Config);
            return m_SocketServer != null;
        }
        catch (Exception e)
        {
            if (Logger.IsErrorEnabled)
                Logger.Error(e.ToString());

            return false;
        }
    }

    private IPAddress ParseIPAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip) || "Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            return IPAddress.Any;
        else if ("IPv6Any".Equals(ip, StringComparison.OrdinalIgnoreCase))
            return IPAddress.IPv6Any;
        else
           return IPAddress.Parse(ip);
    }

    /// <summary>
    /// Setups the listeners base on server configuration
    /// </summary>
    /// <param name="config">The config.</param>
    /// <returns></returns>
    private bool SetupListeners(IServerConfig config)
    {
        var listeners = new List<ListenerInfo>();

        try
        {
            if (config.Port > 0)
            {
                listeners.Add(new ListenerInfo
                {
                    EndPoint = new IPEndPoint(ParseIPAddress(config.Ip), config.Port),
                    BackLog = config.ListenBacklog,
                    Security = BasicSecurity
                });
            }
            else
            {
                //Port is not configured, but ip is configured
                if (!string.IsNullOrEmpty(config.Ip))
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("Port is required in config!");

                    return false;
                }
            }

            //There are listener defined
            if (config.Listeners != null && config.Listeners.Any())
            {
                //But ip and port were configured in server node
                //We don't allow this case
                if (listeners.Any())
                {
                    if (Logger.IsErrorEnabled)
                        Logger.Error("If you configured Ip and Port in server node, you cannot defined listener in listeners node any more!");

                    return false;
                }

                foreach (var l in config.Listeners)
                {
                    SslProtocols configProtocol;

                    if (string.IsNullOrEmpty(l.Security))
                    {
                        configProtocol = BasicSecurity;
                    }
                    else if (!l.Security.TryParseEnum<SslProtocols>(true, out configProtocol))
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error($"Failed to parse '{config.Security}' to SslProtocol!");

                        return false;
                    }

                    if (configProtocol != SslProtocols.None && (Certificate == null))
                    {
                        if (Logger.IsErrorEnabled)
                            Logger.Error("There is no certificate loaded, but there is a secure listener defined!");
                        return false;
                    }

                    listeners.Add(new ListenerInfo
                    {
                        EndPoint = new IPEndPoint(ParseIPAddress(l.Ip), l.Port),
                        BackLog = l.Backlog,
                        Security = configProtocol
                    });
                }
            }

            if (!listeners.Any())
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("No listener defined!");

                return false;
            }

            m_Listeners = listeners.ToArray();

            return true;
        }
        catch (Exception e)
        {
            if (Logger.IsErrorEnabled)
                Logger.Error(e.ToString());

            return false;
        }
    }

    /// <summary>
    /// Gets the name of the server instance.
    /// </summary>
    public string Name
    {
        get { return m_Name; }
    }

    private ISocketServer m_SocketServer;

    /// <summary>
    /// Gets the socket server.
    /// </summary>
    /// <value>
    /// The socket server.
    /// </value>
    ISocketServer ISocketServerAccessor.SocketServer
    {
        get { return m_SocketServer; }
    }

    /// <summary>
    /// Starts this server instance.
    /// </summary>
    /// <returns>
    /// return true if start successfull, else false
    /// </returns>
    public virtual bool Start()
    {
        var origStateCode = Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Starting, ServerStateConst.NotStarted);

        if (origStateCode != ServerStateConst.NotStarted)
        {
            if (origStateCode < ServerStateConst.NotStarted)
                throw new Exception("You cannot start a server instance which has not been setup yet.");

            if (Logger.IsErrorEnabled)
                Logger.Error($"This server instance is in the state {(ServerState)origStateCode}, you cannot start it now.");

            return false;
        }

        if (!m_SocketServer.Start())
        {
            m_StateCode = ServerStateConst.NotStarted;
            return false;
        }

        StartedTime = DateTime.Now;
        m_StateCode = ServerStateConst.Running;
                    
        try
        {
            //Will be removed in the next version
#pragma warning disable 0612, 618
            OnStartup();
#pragma warning restore 0612, 618

            OnStarted();
        }
        catch (Exception e)
        {
            if (Logger.IsErrorEnabled)
            {
                Logger.Error("One exception wa thrown in the method 'OnStartup()'.", e);
            }
        }
        finally
        {
            if (Logger.IsInfoEnabled)
                Logger.Info(string.Format("The server instance {0} has been started!", Name));
        }

        return true;
    }

    /// <summary>
    /// Called when [startup].
    /// </summary>
    [Obsolete("Use OnStarted() instead")]
    protected virtual void OnStartup()
    {

    }

    /// <summary>
    /// Called when [started].
    /// </summary>
    protected virtual void OnStarted()
    {

    }

    /// <summary>
    /// Called when [stopped].
    /// </summary>
    protected virtual void OnStopped()
    {

    }

    /// <summary>
    /// Stops this server instance.
    /// </summary>
    public virtual void Stop()
    {
        if (Interlocked.CompareExchange(ref m_StateCode, ServerStateConst.Stopping, ServerStateConst.Running)
                != ServerStateConst.Running)
        {
            return;
        }

        m_SocketServer.Stop();

        m_StateCode = ServerStateConst.NotStarted;

        OnStopped();
                    
        if (Logger.IsInfoEnabled)
            Logger.Info(string.Format("The server instance {0} has been stopped!", Name));
    }


    private Func<TAppSession, byte[], int, int, bool> m_RawDataReceivedHandler;

    /// <summary>
    /// Gets or sets the raw binary data received event handler.
    /// TAppSession: session
    /// byte[]: receive buffer
    /// int: receive buffer offset
    /// int: receive lenght
    /// bool: whether process the received data further
    /// </summary>
    event Func<TAppSession, byte[], int, int, bool> IRawDataProcessor<TAppSession>.RawDataReceived
    {
        add { m_RawDataReceivedHandler += value; }
        remove { m_RawDataReceivedHandler -= value; }
    }

    /// <summary>
    /// Called when [raw data received].
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="length">The length.</param>
    internal bool OnRawDataReceived(IAppSession session, byte[] buffer, int offset, int length)
    {
        var handler = m_RawDataReceivedHandler;
        if (handler == null)
            return true;

        return handler((TAppSession)session, buffer, offset, length);
    }

    private RequestHandler<TAppSession, TRequestInfo> m_RequestHandler;

    /// <summary>
    /// Occurs when a full request item received.
    /// </summary>
    public virtual event RequestHandler<TAppSession, TRequestInfo> NewRequestReceived
    {
        add { m_RequestHandler += value; }
        remove { m_RequestHandler -= value; }
    }


    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    protected virtual void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
    {            
        session.CurrentCommand = requestInfo.Key;

        try
        {
            m_RequestHandler(session, requestInfo);
        }
        catch (Exception e)
        {
            session.InternalHandleExcetion(e);
        }

        session.PrevCommand = requestInfo.Key;
        session.LastActiveTime = DateTime.Now;

        if (Config.LogCommand && Logger.IsInfoEnabled)
        {
            //Logger.Info(session, string.Format("Command - {0}", requestInfo.Key));
            var message = string.Format("Command - {0}", requestInfo.Key);
            Logger.Info(string.Format("Session: {0}/{1}", session.SessionID, session.RemoteEndPoint) + Environment.NewLine + message);
        }

        Interlocked.Increment(ref m_TotalHandledRequests);
    }


    /// <summary>
    /// Executes the command for the session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    internal void ExecuteCommand(IAppSession session, TRequestInfo requestInfo)
    {
        this.ExecuteCommand((TAppSession)session, requestInfo);
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    void IRequestHandler<TRequestInfo>.ExecuteCommand(IAppSession session, TRequestInfo requestInfo)
    {
        this.ExecuteCommand((TAppSession)session, requestInfo);
    }

    /// <summary>
    /// Gets or sets the server's connection filter
    /// </summary>
    /// <value>
    /// The server's connection filters
    /// </value>
    public IEnumerable<IConnectionFilter> ConnectionFilters
    {
        get { return m_ConnectionFilters; }
    }

    /// <summary>
    /// Executes the connection filters.
    /// </summary>
    /// <param name="remoteAddress">The remote address.</param>
    /// <returns></returns>
    private bool ExecuteConnectionFilters(IPEndPoint remoteAddress)
    {
        if (m_ConnectionFilters == null)
            return true;

        for (var i = 0; i < m_ConnectionFilters.Count; i++)
        {
            var currentFilter = m_ConnectionFilters[i];
            if (!currentFilter.AllowConnect(remoteAddress))
            {
                if (Logger.IsInfoEnabled)
                    Logger.Info($"A connection from {remoteAddress} has been refused by filter {currentFilter.Name}!");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Creates the app session.
    /// </summary>
    /// <param name="socketSession">The socket session.</param>
    /// <returns></returns>
    IAppSession IAppServer.CreateAppSession(ISocketSession socketSession)
    {
        if (!ExecuteConnectionFilters(socketSession.RemoteEndPoint))
            return NullAppSession;

        var appSession = CreateAppSession(socketSession);
        
        appSession.Initialize(this, socketSession);

        return appSession;
    }

    /// <summary>
    /// create a new TAppSession instance, you can override it to create the session instance in your own way
    /// </summary>
    /// <param name="socketSession">the socket session.</param>
    /// <returns>the new created session instance</returns>
    protected virtual TAppSession CreateAppSession(ISocketSession socketSession)
    {
        return new TAppSession();
    }

    /// <summary>
    /// Registers the new created app session into the appserver's session container.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns></returns>
    bool IAppServer.RegisterSession(IAppSession session)
    {
        var appSession = session as TAppSession;

        if (!RegisterSession(appSession.SessionID, appSession))
            return false;

        appSession.SocketSession.Closed += OnSocketSessionClosed;

        //if (Config.LogBasicSessionActivity && Logger.IsInfoEnabled)
            //Logger.Info(session, "A new session connected!");

        OnNewSessionConnected(appSession);
        return true;
    }

    /// <summary>
    /// Registers the session into session container.
    /// </summary>
    /// <param name="sessionID">The session ID.</param>
    /// <param name="appSession">The app session.</param>
    /// <returns></returns>
    protected virtual bool RegisterSession(string sessionID, TAppSession appSession)
    {
        return true;
    }


    private SessionHandler<TAppSession> m_NewSessionConnected;

    /// <summary>
    /// The action which will be executed after a new session connect
    /// </summary>
    public event SessionHandler<TAppSession> NewSessionConnected
    {
        add { m_NewSessionConnected += value; }
        remove { m_NewSessionConnected -= value; }
    }

    /// <summary>
    /// Called when [new session connected].
    /// </summary>
    /// <param name="session">The session.</param>
    protected virtual void OnNewSessionConnected(TAppSession session)
    {
        var handler = m_NewSessionConnected;
        if (handler == null)
        {
            return;
        }

        Task.Run(() => handler(session));            
        //var handler = m_NewSessionConnected;
        //if (handler == null)
        //    return;

        //handler.BeginInvoke(session, OnNewSessionConnectedCallback, handler);
    }

    private void OnNewSessionConnectedCallback(IAsyncResult result)
    {
        try
        {
            var handler = (SessionHandler<TAppSession>)result.AsyncState;
            handler.EndInvoke(result);
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }

    /// <summary>
    /// Resets the session's security protocol.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="security">The security protocol.</param>
    public void ResetSessionSecurity(IAppSession session, SslProtocols security)
    {
        m_SocketServer.ResetSessionSecurity(session, security);
    }

    /// <summary>
    /// Called when [socket session closed].
    /// </summary>
    /// <param name="session">The socket session.</param>
    /// <param name="reason">The reason.</param>
    private void OnSocketSessionClosed(ISocketSession session, CloseReason reason)
    {
        //Even if LogBasicSessionActivity is false, we also log the unexpected closing because the close reason probably be useful
        //if (Logger.IsInfoEnabled && (Config.LogBasicSessionActivity || (reason != CloseReason.ServerClosing && reason != CloseReason.ClientClosing && reason != CloseReason.ServerShutdown && reason != CloseReason.SocketError)))
            //Logger.Info(session, string.Format("This session was closed for {0}!", reason));

        var appSession = session.AppSession as TAppSession;
        appSession.Connected = false;
        OnSessionClosed(appSession, reason);
    }

    private SessionHandler<TAppSession, CloseReason> m_SessionClosed;
    /// <summary>
    /// Gets/sets the session closed event handler.
    /// </summary>
    public event SessionHandler<TAppSession, CloseReason> SessionClosed
    {
        add { m_SessionClosed += value; }
        remove { m_SessionClosed -= value; }
    }

    /// <summary>
    /// Called when [session closed].
    /// </summary>
    /// <param name="session">The appSession.</param>
    /// <param name="reason">The reason.</param>
    protected virtual void OnSessionClosed(TAppSession session, CloseReason reason)
    {
        var handler = m_SessionClosed;

        if (handler != null)
        {
            Task.Run(() => handler(session, reason)); 
        }

        session.OnSessionClosed(reason);
        //var handler = m_SessionClosed;

        //if (handler != null)
        //{
        //    handler.BeginInvoke(session, reason, OnSessionClosedCallback, handler);
        //}

        //session.OnSessionClosed(reason);
    }

    private void OnSessionClosedCallback(IAsyncResult result)
    {
        try
        {
            var handler = (SessionHandler<TAppSession, CloseReason>)result.AsyncState;
            handler.EndInvoke(result);
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString());
        }
    }

    /// <summary>
    /// Gets the app session by ID.
    /// </summary>
    /// <param name="sessionID">The session ID.</param>
    /// <returns></returns>
    public abstract TAppSession GetSessionByID(string sessionID);

    /// <summary>
    /// Gets the app session by ID.
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    IAppSession IAppServer.GetSessionByID(string sessionID)
    {
        return this.GetSessionByID(sessionID);
    }

    /// <summary>
    /// Gets the matched sessions from sessions snapshot.
    /// </summary>
    /// <param name="critera">The prediction critera.</param>
    public virtual IEnumerable<TAppSession> GetSessions(Func<TAppSession, bool> critera)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Gets all sessions in sessions snapshot.
    /// </summary>
    public virtual IEnumerable<TAppSession> GetAllSessions()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Gets the total session count.
    /// </summary>
    public abstract int SessionCount { get; }

    /// <summary>
    /// Gets the physical file path by the relative file path,
    /// search both in the appserver's root and in the supersocket root dir if the isolation level has been set other than 'None'.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path.</param>
    /// <returns></returns>
    public string GetFilePath(string relativeFilePath)
    {
        var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeFilePath);                        
        return filePath;
    }

    

    /// <summary>
    /// Connect the remote endpoint actively.
    /// </summary>
    /// <param name="targetEndPoint">The target end point.</param>
    /// <param name="localEndPoint">The local end point.</param>
    /// <returns></returns>
    /// <exception cref="System.Exception">This server cannot support active connect.</exception>
    Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint, EndPoint localEndPoint)
    {
        var activeConnector = m_SocketServer as IActiveConnector;

        if (activeConnector == null)
            throw new Exception("This server cannot support active connect.");

        return activeConnector.ActiveConnect(targetEndPoint, localEndPoint);
    }

    /// <summary>
    /// Connect the remote endpoint actively.
    /// </summary>
    /// <param name="targetEndPoint">The target end point.</param>
    /// <returns></returns>
    /// <exception cref="System.Exception">This server cannot support active connect.</exception>
    Task<ActiveConnectResult> IActiveConnector.ActiveConnect(EndPoint targetEndPoint)
    {
        return ((IActiveConnector)this).ActiveConnect(targetEndPoint, null);
    }

    

    
    /// <summary>
    /// Transfers the system message
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="messageData">The message data.</param>
    void ISystemEndPoint.TransferSystemMessage(string messageType, object messageData)
    {
        OnSystemMessageReceived(messageType, messageData);
    }

    /// <summary>
    /// Called when [system message received].
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="messageData">The message data.</param>
    protected virtual void OnSystemMessageReceived(string messageType, object messageData)
    {
    }
           
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    public void Dispose()
    {
        if (m_StateCode == ServerStateConst.Running)
            Stop();
    }

    


    partial void SetDefaultCulture(IRootConfig rootConfig, IServerConfig config)
    {
        var defaultCulture = config.DefaultCulture;

        //default culture has been set for this server instance
        if (!string.IsNullOrEmpty(defaultCulture))
        {
            Logger.Warn($"The default culture cannot be set, because you cannot set default culture for one server instance if the Isolation is None!");
            return;
        }
        else if (!string.IsNullOrEmpty(rootConfig.DefaultCulture))
        {
            defaultCulture = rootConfig.DefaultCulture;
            return;
        }

        if (string.IsNullOrEmpty(defaultCulture))
            return;

        try
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(defaultCulture);
        }
        catch (Exception e)
        {
            Logger.Error(string.Format("Failed to set default culture '{0}'.", defaultCulture), e);
        }
    }
}
