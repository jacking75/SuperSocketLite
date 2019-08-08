using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServer
{
    public class ServerOption
    {
        [Option( "uniqueID", Required = true, HelpText = "Server UniqueID")]
        public int ChatServerUniqueID { get; set; }

        [Option("name", Required = true, HelpText = "Server Name")]
        public string Name { get; set; }

        [Option("maxConnectionNumber", Required = true, HelpText = "MaxConnectionNumber")]
        public int MaxConnectionNumber { get; set; }

        [Option("port", Required = true, HelpText = "Port")]
        public int Port { get; set; }

        [Option("maxRequestLength", Required = true, HelpText = "maxRequestLength")]
        public int MaxRequestLength { get; set; }

        [Option("receiveBufferSize", Required = true, HelpText = "receiveBufferSize")]
        public int ReceiveBufferSize { get; set; }

        [Option("sendBufferSize", Required = true, HelpText = "sendBufferSize")]
        public int SendBufferSize { get; set; }
       
    }    
}
