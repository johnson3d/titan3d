#ifndef _ALGORITHM_QUEUE_H_
#define _ALGORITHM_QUEUE_H_

struct TtQueue
{
    RWByteAddressBuffer DataBuffer;
    int HeadSize;
    int WriteOffset;
    int ReadOffset;
    uint BufferCapacity;
    void Init()
    {
        HeadSize = 2;
        WriteOffset = 0;
        ReadOffset = 1;
        BufferCapacity = 0;
    }
    
    bool Enqueue(uint data)
    {
        int pos = 0;
        DataBuffer.InterlockedAdd(WriteOffset, 1, pos);
        if (pos >= BufferCapacity)
        {
            return false;
        }
        DataBuffer.Store((HeadSize + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint2 data)
    {
        int pos = 0;
        DataBuffer.InterlockedAdd(WriteOffset, 2, pos);
        if (pos >= BufferCapacity)
        {
            return false;
        }
        DataBuffer.Store2((HeadSize + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint3 data)
    {
        int pos = 0;
        DataBuffer.InterlockedAdd(WriteOffset, 3, pos);
        if (pos >= BufferCapacity)
        {
            return false;
        }
        DataBuffer.Store3((HeadSize + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint4 data)
    {
        int pos = 0;
        DataBuffer.InterlockedAdd(WriteOffset, 4, pos);
        if (pos >= BufferCapacity)
        {
            return false;
        }
        DataBuffer.Store4((HeadSize + pos) * 4, data);
        return true;
    }
    bool Dequeue(inout uint data)
    {
        int readPos = 0;
        DataBuffer.InterlockedAdd(ReadOffset, 1, readPos);
        int writePos = 0;
        DataBuffer.InterlockedAdd(WriteOffset, 1, writePos);
        if (readPos < writePos)
        {
            data = DataBuffer.Load((HeadSize + readPos) * 4);
            return true;
        }
        return false;
    }
    uint Dequeue1()
    {
        int readPos = 0;
        DataBuffer.InterlockedAdd(ReadOffset, 1, readPos);
        return DataBuffer.Load((HeadSize + readPos) * 4);
    }
    uint2 Dequeue2()
    {
        int readPos = 0;
        DataBuffer.InterlockedAdd(ReadOffset, 2, readPos);
        return DataBuffer.Load2((HeadSize + readPos) * 4);
    }
    uint3 Dequeue3()
    {
        int readPos = 0;
        DataBuffer.InterlockedAdd(ReadOffset, 3, readPos);
        return DataBuffer.Load3((HeadSize + readPos) * 4);
    }
    uint4 Dequeue4()
    {
        int readPos = 0;
        DataBuffer.InterlockedAdd(ReadOffset, 4, readPos);
        return DataBuffer.Load4((HeadSize + readPos) * 4);
    }
};

interface IQueueReadProxy
{
    void GetReadPos(int count, out int readPos);
};

struct TtQueueReadProxy : IQueueReadProxy
{
    ByteAddressBuffer DataBuffer;
    int HeadSize;
    void Init()
    {
        HeadSize = 2;
    }
    void GetReadPos(int count, out int readPos);
    uint Dequeue1()
    {
        int readPos = 0;
        GetReadPos(1, readPos);
        return DataBuffer.Load((HeadSize + readPos) * 4);
    }
    uint2 Dequeue2()
    {
        int readPos = 0;
        GetReadPos(2, readPos);
        return DataBuffer.Load2((HeadSize + readPos) * 4);
    }
    uint3 Dequeue3()
    {
        int readPos = 0;
        GetReadPos(3, readPos);
        return DataBuffer.Load3((HeadSize + readPos) * 4);
    }
    uint4 Dequeue4()
    {
        int readPos = 0;
        GetReadPos(4, readPos);
        return DataBuffer.Load4((HeadSize + readPos) * 4);
    }
};

struct TtQueueReadProxyTest// : TtQueueReadProxy
{
    RWByteAddressBuffer DataBufferWrite;
    void GetReadPos(int count, out int readPos)
    {
        DataBufferWrite.InterlockedAdd(0, count, readPos);
    }
};

template<typename TPosProvider>
struct TtQueueReadProxy2
{
    TPosProvider PosProvider;
    ByteAddressBuffer DataBuffer;
    int HeadSize;
    void Init(TPosProvider provider)
    {
        PosProvider = provider;
        HeadSize = 2;
    }
    uint Dequeue1()
    {
        int readPos = 0;
        PosProvider.GetReadPos(1, readPos);
        return DataBuffer.Load((HeadSize + readPos) * 4);
    }
    uint2 Dequeue2()
    {
        int readPos = 0;
        PosProvider.GetReadPos(2, readPos);
        return DataBuffer.Load2((HeadSize + readPos) * 4);
    }
    uint3 Dequeue3()
    {
        int readPos = 0;
        PosProvider.GetReadPos(3, readPos);
        return DataBuffer.Load3((HeadSize + readPos) * 4);
    }
    uint4 Dequeue4()
    {
        int readPos = 0;
        PosProvider.GetReadPos(4, readPos);
        return DataBuffer.Load4((HeadSize + readPos) * 4);
    }
};

#endif//#ifndef _ALGORITHM_QUEUE_H_