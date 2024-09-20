using MessagePack; //https://github.com/neuecc/MessagePack-CSharp
using System;
using System.Collections.Generic;


namespace CSBaseLib;

public class PacketDef
{
    public const Int16 HeaderSize = 5;
    public const int MaxUserIDByteLength = 16;
    public const int MaxUserPWByteLength = 16;

    public const int InvalidRoomNumber = -1;
}

public class PacketToBytes
{
    public static byte[] Make(PacketId packetID, byte[] bodyData)
    {
        byte type = 0;
        var pktID = (Int16)packetID;
        Int16 bodyDataSize = 0;
        if (bodyData != null)
        {
            bodyDataSize = (Int16)bodyData.Length;
        }

        var packetSize = (Int16)(bodyDataSize + PacketDef.HeaderSize);
        
        
        var dataSource = new byte[packetSize];
        Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, dataSource, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(pktID), 0, dataSource, 2, 2);
        dataSource[4] = type;
        
        if (bodyData != null)
        {
            Buffer.BlockCopy(bodyData, 0, dataSource, 5, bodyDataSize);
        }

        return dataSource;
    }

    
}

// 로그인 요청
[MessagePackObject]
public class PKTReqLogin
{
    [Key(0)]
    public string UserID;
    [Key(1)]
    public string AuthToken;
}

[MessagePackObject]
public class PKTResLogin
{
    [Key(0)]
    public short Result;
}


[MessagePackObject]
public class PKNtfMustClose
{
    [Key(0)]
    public short Result;
}



[MessagePackObject]
public class PKTReqRoomEnter
{
    [Key(0)]
    public int RoomNumber;
}

[MessagePackObject]
public class PKTResRoomEnter
{
    [Key(0)]
    public short Result;
}

[MessagePackObject]
public class PKTNtfRoomUserList
{
    [Key(0)]
    public List<string> UserIDList = new List<string>();
}

[MessagePackObject]
public class PKTNtfRoomNewUser
{
    [Key(0)]
    public string UserID;
}


[MessagePackObject]
public class PKTReqRoomLeave
{
}

[MessagePackObject]
public class PKTResRoomLeave
{
    [Key(0)]
    public short Result;
}

[MessagePackObject]
public class PKTNtfRoomLeaveUser
{
    [Key(0)]
    public string UserID;
}


[MessagePackObject]
public class PKTReqRoomChat
{
    [Key(0)]
    public string ChatMessage;
}


[MessagePackObject]
public class PKTNtfRoomChat
{
    [Key(0)]
    public string UserID;

    [Key(1)]
    public string ChatMessage;
}
