using System;


namespace SuperSocket.Common;

//TODO 유닛테스트 필요
public class ReuseLockBaseBuffer
{
    Int32 ReadPos = 0;
    Int32 WritePos = 0;
    Int32 BufferSize = 0;
    byte[] mBuffer = null;

    Int32 MinumBufferSize = 0;

    public ReuseLockBaseBuffer(int bufferSize)
    {
        BufferSize = bufferSize;
        mBuffer = new byte[bufferSize];
        MinumBufferSize = BufferSize / 4;
    }

    public void Clear()
    {
        lock (mBuffer)
        {
            ReadPos = 0;
            WritePos = 0;
        }
    }

    public bool Copy(byte[] source, int pos, int count)
    {
        lock(mBuffer)
        {
            var expectedLength = WritePos + count;
            if(BufferSize <= expectedLength)
            {
                return false;
            }

            Buffer.BlockCopy(source, pos, mBuffer, WritePos, count);
        }

        return true;
    }

    // 1개의 스레드에서만 호출해야 한다.
    public ArraySegment<byte> GetData()
    {
        lock (mBuffer)
        {
            var size = WritePos - ReadPos;
            return new ArraySegment<byte>(mBuffer, ReadPos, size);
        }
    }

    public void Commit(int size)
    {
        lock (mBuffer)
        {
            var currenDataSize = WritePos - ReadPos;

            if (currenDataSize == size)
            {
                ReadPos = 0;
                WritePos = 0;
                return;
            }


            ReadPos += size;
            currenDataSize = WritePos - ReadPos;

            if(currenDataSize < ReadPos)
            {
                Buffer.BlockCopy(mBuffer, ReadPos, mBuffer, 0, currenDataSize);
                ReadPos = 0;
                WritePos = currenDataSize;
            }
            else
            {
                var remainingBufferLength = BufferSize - WritePos;
                if (MinumBufferSize < remainingBufferLength)
                {
                    var temp = new byte[currenDataSize];
                    Buffer.BlockCopy(mBuffer, ReadPos, temp, 0, currenDataSize);
                    Buffer.BlockCopy(temp, 0, mBuffer, 0, currenDataSize);

                    ReadPos = 0;
                    WritePos = currenDataSize;
                }
            }
        }
    }
}
