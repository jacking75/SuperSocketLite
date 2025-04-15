﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocketLite.SocketBase.Config;

/// <summary>
/// Certificate config model class
/// </summary>
[Serializable]
public class CertificateConfig : ICertificateConfig
{
    /// <summary>
    /// Gets/sets the file path.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets/sets the password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets/sets the the store where certificate locates.
    /// </summary>
    /// <value>
    /// The name of the store.
    /// </value>
    public string StoreName { get; set; }

    /// <summary>
    /// Gets/sets the store location of the certificate.
    /// </summary>
    /// <value>
    /// The store location.
    /// </value>
    public StoreLocation StoreLocation { get; set; }

    /// <summary>
    /// Gets/sets the thumbprint.
    /// </summary>
    public string Thumbprint { get; set; }

    /// <summary>
    /// Gets/sets a value indicating whether [client certificate required].
    /// </summary>
    /// <value>
    /// <c>true</c> if [client certificate required]; otherwise, <c>false</c>.
    /// </value>
    public bool ClientCertificateRequired { get; set; }

    /// <summary>
    /// Gets/sets a value that will be used to instantiate the X509Certificate2 object in the CertificateManager
    /// </summary>
    public X509KeyStorageFlags KeyStorageFlags { get; set; }

}
