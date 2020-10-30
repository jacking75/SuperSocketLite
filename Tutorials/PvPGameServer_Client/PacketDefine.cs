using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csharp_test_client
{
    class PacketDef
    {
      
        //public const Int16 PACKET_HEADER_SIZE = 5;
        public const int MAX_USER_ID_BYTE_LENGTH = 16;
        public const int MAX_USER_PW_BYTE_LENGTH = 16;
    }

    public enum PACKET_ID : ushort
    {
        PACKET_ID_ECHO = 101,

        // Ping(Heart-beat)
        PACKET_ID_PING_REQ = 201,
        PACKET_ID_PING_RES = 202,

        PACKET_ID_ERROR_NTF = 203,


        REQ_LOGIN = 1002,
        RES_LOGIN = 1003,
        NTF_MUST_CLOSE = 1005,

        REQ_ROOM_ENTER = 1015,
        RES_ROOM_ENTER = 1016,
        NTF_ROOM_USER_LIST = 1017,
        NTF_ROOM_NEW_USER = 1018,

        REQ_ROOM_LEAVE = 1021,
        RES_ROOM_LEAVE = 1022,
        NTF_ROOM_LEAVE_USER = 1023,

        REQ_ROOM_CHAT = 1026,
        NTF_ROOM_CHAT = 1027,


        REQ_ROOM_DEV_ALL_ROOM_START_GAME = 1091,
        RES_ROOM_DEV_ALL_ROOM_START_GAME = 1092,

        REQ_ROOM_DEV_ALL_ROOM_END_GAME = 1093,
        RES_ROOM_DEV_ALL_ROOM_END_GAME = 1094,
    }


    public enum ERROR_CODE : Int16
    {
        ERROR_NONE = 0,



        ERROR_CODE_USER_MGR_INVALID_USER_UNIQUEID = 112,

        ERROR_CODE_PUBLIC_CHANNEL_IN_USER = 114,

        ERROR_CODE_PUBLIC_CHANNEL_INVALIDE_NUMBER = 115,
    }
}
