﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProtocolBuffer;

public class ProtocolBufferHeaderParser
{
    // 프로토콜 버퍼 와이어 타입
    private const int WIRETYPE_FIXED32 = 5;

    static public PacketHeader ParseHeaderOnly(byte[] serialized)
    {
        var header = new PacketHeader();

        const int TOTAL_SIZE_POSITION = 3; // 직렬화 메타 정보가 들어가 있음
        var readPos = TOTAL_SIZE_POSITION;
        
        var bytesSpan1 = serialized.AsSpan(readPos, 4);
        header.TotalSize = BinaryPrimitives.ReadUInt32LittleEndian(bytesSpan1);
        readPos += 5; // 1바이트 태그를 건너뛰기 위해 4가 아닌 5를 더함

        var bytesSpan2 = serialized.AsSpan(readPos, 4);
        header.Id = BinaryPrimitives.ReadUInt32LittleEndian(bytesSpan2);
        readPos += 5;

        var bytesSpan3 = serialized.AsSpan(readPos, 4);
        header.Value = BinaryPrimitives.ReadUInt32LittleEndian(bytesSpan3);

        // totalSize 값을 직접 쓰기
        //BinaryPrimitives.WriteUInt32LittleEndian(bytesSpan, totalSize);

        /*int position = 0;

        while (position < data.Length)
        {
            // 필드 번호와 와이어 타입 읽기
            int tag = ReadVarint(data, ref position);
            int fieldNumber = tag >> 3;
            int wireType = tag & 0x7;

            // fixed32 타입일 경우
            if (wireType == WIRETYPE_FIXED32)
            {
                // fieldNumber에 따라 적절한 필드에 값 할당
                switch (fieldNumber)
                {
                    case 1: // total_size
                        header.TotalSize = ReadFixed32(data, ref position);
                        break;
                    case 2: // id
                        header.Id = ReadFixed32(data, ref position);
                        break;
                    case 3: // value
                        header.Value = (byte)ReadFixed32(data, ref position);
                        break;
                }
            }
            else
            {
                // 다른 필드는 건너뛰기
                SkipField(data, wireType, ref position);
            }

            // 헤더 필드를 모두 읽었다면 종료
            if (fieldNumber > 3)
                break;
        }*/

        return header;
    }

    static private uint ReadFixed32(byte[] data, ref int position)
    {
        uint value = BitConverter.ToUInt32(data, position);
        position += 4;
        return value;
    }

    static private int ReadVarint(byte[] data, ref int position)
    {
        int value = 0;
        int shift = 0;

        while (true)
        {
            byte b = data[position++];
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0)
                break;
            shift += 7;
        }

        return value;
    }

    static private void SkipField(byte[] data, int wireType, ref int position)
    {
        switch (wireType)
        {
            case 0: // varint
                while ((data[position++] & 0x80) != 0) { }
                break;
            case 1: // fixed64
                position += 8;
                break;
            case 2: // length-delimited
                int length = ReadVarint(data, ref position);
                position += length;
                break;
            case 5: // fixed32
                position += 4;
                break;
        }
    }


    static public void WritePacketHeaderTotalSize(byte[] serialized, UInt32 totalSize)
    {        
        const int TOTAL_SIZE_POSITION = 3; // 직렬화 메타 정보가 들어가 있음
        var bytesSpan = serialized.AsSpan(TOTAL_SIZE_POSITION, 4);
        
        // totalSize 값을 직접 쓰기
        BinaryPrimitives.WriteUInt32LittleEndian(bytesSpan, totalSize);        
    }
}
