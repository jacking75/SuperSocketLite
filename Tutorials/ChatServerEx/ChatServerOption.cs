using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatServerOption
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

        [Option("roomMaxCountPerThread", Required = true, HelpText = "RoomMaxCountPerThread Count")]
        public int RoomMaxCountPerThread { get; set; } = 0;

        [Option("roomThreadCount", Required = true, HelpText = "RoomThreadCount")]
        public int RoomThreadCount { get; set; } = 0;

        [Option("roomMaxUserCount", Required = true, HelpText = "RoomMaxUserCount")]
        public int RoomMaxUserCount { get; set; } = 0;

        [Option("roomStartNumber", Required = true, HelpText = "RoomStartNumber")]
        public int RoomStartNumber { get; set; } = 0;

        [Option("dbWorkerThreadCount", Required = true, HelpText = "DBWorkerThreadCount")]
        public int DBWorkerThreadCount { get; set; } = 0;

        [Option("redisAddres", Required = true, HelpText = "Redis Addres")]
        public string RedisAddres { get; set; }
        //[Option("redisAddress", Separator = ',')]
        //public IEnumerable<string> RedisAddress { get; set; }   

    }

    
}
