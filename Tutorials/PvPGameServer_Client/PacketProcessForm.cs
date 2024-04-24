using MemoryPack;
using System;
using System.Collections.Generic;


namespace csharp_test_client
{
    public partial class mainForm
    {
        Dictionary<PACKET_ID, Action<byte[]>> PacketFuncDic = new Dictionary<PACKET_ID, Action<byte[]>>();

        void SetPacketHandler()
        {
            PacketFuncDic.Add(PACKET_ID.PACKET_ID_ECHO, PacketProcess_Echo);
            PacketFuncDic.Add(PACKET_ID.RES_LOGIN, PacketProcess_LoginResponse);
            PacketFuncDic.Add(PACKET_ID.RES_ROOM_ENTER, PacketProcess_RoomEnterResponse);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_USER_LIST, PacketProcess_RoomUserListNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_NEW_USER, PacketProcess_RoomNewUserNotify);
            PacketFuncDic.Add(PACKET_ID.RES_ROOM_LEAVE, PacketProcess_RoomLeaveResponse);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_LEAVE_USER, PacketProcess_RoomLeaveUserNotify);
            PacketFuncDic.Add(PACKET_ID.NTF_ROOM_CHAT, PacketProcess_RoomChatNotify);
            //PacketFuncDic.Add(PACKET_ID.PACKET_ID_ROOM_RELAY_NTF, PacketProcess_RoomRelayNotify);
        }

        void PacketProcess(byte[] packet)
        {
            var header = new MemoryPackPacketHeadInfo();
            header.Read(packet);

            var packetType = (PACKET_ID)header.Id;
            //DevLog.Write("Packet Error:  PacketID:{packet.PacketID.ToString()},  Error: {(ERROR_CODE)packet.Result}");
            //DevLog.Write("RawPacket: " + packet.PacketID.ToString() + ", " + PacketDump.Bytes(packet.BodyData));

            if (PacketFuncDic.ContainsKey(packetType))
            {
                PacketFuncDic[packetType](packet);
            }
            else
            {
                DevLog.Write("Unknown Packet Id: " + packetType);
            }         
        }

        void PacketProcess_Echo(byte[] bodyData)
        {
            DevLog.Write($"Echo 받음:  {bodyData.Length}");
        }
                

        void PacketProcess_LoginResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResLogin>(packetData);

            DevLog.Write($"로그인 결과:  {(ERROR_CODE)responsePkt.Result}");
        }


        void PacketProcess_RoomEnterResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomEnter>(packetData);

            DevLog.Write($"방 입장 결과:  {(ERROR_CODE)responsePkt.Result}");
        }

        void PacketProcess_RoomUserListNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomUserList>(packetData);

            for (int i = 0; i < notifyPkt.UserIDList.Count; ++i)
            {
                AddRoomUserList(notifyPkt.UserIDList[i]);
            }

            DevLog.Write($"방의 기존 유저 리스트 받음");
        }

        void PacketProcess_RoomNewUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomNewUser>(packetData);

            AddRoomUserList(notifyPkt.UserID);
            
            DevLog.Write($"방에 새로 들어온 유저 받음");
        }


        void PacketProcess_RoomLeaveResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomLeave>(packetData);

            DevLog.Write($"방 나가기 결과:  {(ERROR_CODE)responsePkt.Result}");
        }

        void PacketProcess_RoomLeaveUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packetData);

            RemoveRoomUserList(notifyPkt.UserID);

            DevLog.Write($"방에서 나간 유저 받음");
        }

                
        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomChat>(packetData);

            AddRoomChatMessageList(notifyPkt.UserID, notifyPkt.ChatMessage);
        }

        void AddRoomChatMessageList(string userID, string msgssage)
        {
            var msg = $"{userID}:  {msgssage}";

            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add(msg);
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

       
    }
}
