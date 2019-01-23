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

        [Option("redisAddress", Separator = ',')]
        public IEnumerable<string> RedisAddress { get; set; }

        [Option("maxUserCount", Required = true, HelpText = "MaxUserCount")]
        public int MaxUserCount { get; set; } = 0;
    }

    // 제거하기
    public class ChatServerEnvironment
    {
        public static int ChatServerUniqueID    = 0;

        public static int RoomMaxCountPerThread = 0;
        public static int RoomThreadCount      = 0;
        public static int RoomMaxUserCount = 0;
        public static int RoomStartNumber = 0;

        public static int DBWorkerThreadCount = 0;

        public static string RedisAddress;

        public static int MaxUserCount = 0;


        public static void Setting()
        {
            ChatServerUniqueID  = 1;
            
            RoomMaxCountPerThread = 16;
            RoomThreadCount    = 4;
            RoomMaxUserCount = 4;
            RoomStartNumber = 0;
            DBWorkerThreadCount = 4;
            RedisAddress = "192.168.0.10";

            SetMaxUserCount();
        }

        static void SetMaxUserCount()
        {
            MaxUserCount = (RoomMaxCountPerThread * RoomMaxCountPerThread) * RoomMaxUserCount;
        }
    }
}
