#ifndef _ALGORITHM_QUEUE_H_
#define _ALGORITHM_QUEUE_H_

template<typename TPosProvider>
struct TtQueueWriter
{
    RWByteAddressBuffer DataBuffer;
    int Offset;
    uint BufferCapacity;
    TPosProvider Provider;
    void Init(TPosProvider posProvider, RWByteAddressBuffer buffer, int offset, int capacity)
    {
        Provider = posProvider;
        DataBuffer = buffer;
        Offset = offset;
        BufferCapacity = capacity;
    }
    
    bool GetWritePos(out int pos, int num)
    {
        return Provider.GetWritePos(pos, num);
    }
    
    bool Enqueue(uint data)
    {
        int pos = 0;
        GetWritePos(pos, 1);
        if (pos >= BufferCapacity)
            return false;
        DataBuffer.Store((Offset + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint2 data)
    {
        int pos = 0;
        GetWritePos(pos, 2);
        if (pos >= BufferCapacity)
            return false;
        DataBuffer.Store2((Offset + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint3 data)
    {
        int pos = 0;
        GetWritePos(pos, 3);
        if (pos >= BufferCapacity)
            return false;
        DataBuffer.Store3((Offset + pos) * 4, data);
        return true;
    }
    bool Enqueue(uint4 data)
    {
        int pos = 0;
        GetWritePos(pos, 4);
        if (pos >= BufferCapacity)
            return false;
        DataBuffer.Store4((Offset + pos) * 4, data);
        return true;
    }
};

struct TtQueueWriterTest
{
    RWByteAddressBuffer DataBufferWrite;
    int MaxSize;
    void GetWritePos(out int pos, int num)
    {
        DataBufferWrite.InterlockedAdd(0, num, pos);
    }
};

template<typename TPosProvider, typename ByteAddressBufferType>
struct TtQueueReader
{
    TPosProvider PosProvider;
    ByteAddressBufferType DataBuffer;
    int Offset;
    void Init(TPosProvider provider, ByteAddressBufferType buffer)
    {
        DataBuffer = buffer;
        PosProvider = provider;
        Offset = PosProvider.GetReadOffset();
    }
    uint Dequeue1()
    {
        int readPos = 0;
        PosProvider.GetReadPos(1, readPos);
        return DataBuffer.Load(Offset * 4);
    }
    uint2 Dequeue2()
    {
        int readPos = 0;
        PosProvider.GetReadPos(2, readPos);
        return DataBuffer.Load2((Offset + readPos) * 4);
    }
    uint3 Dequeue3()
    {
        int readPos = 0;
        PosProvider.GetReadPos(3, readPos);
        return DataBuffer.Load3((Offset + readPos) * 4);
    }
    uint4 Dequeue4()
    {
        int readPos = 0;
        PosProvider.GetReadPos(4, readPos);
        return DataBuffer.Load4((Offset + readPos) * 4);
    }
};

struct TtQueueReaderTest
{
    RWByteAddressBuffer DataBufferWrite;
    void GetReadPos(int count, out int readPos)
    {
        DataBufferWrite.InterlockedAdd(0, count, readPos);
    }
    int GetReadOffset()
    {
        return 2;
    }
};

#endif//#ifndef _ALGORITHM_QUEUE_H_