#ifndef _ALGORITHM_RINGBUFFER_H_
#define _ALGORITHM_RINGBUFFER_H_

struct TtRawRingBuffer
{
    RWByteAddressBuffer RingBuffer;
    int Capacity;
    void Push(uint v)
    {
        uint index;
        RingBuffer.InterlockedAdd(0, 1, index);
        RingBuffer.Store((index % Capacity) * 4 + 8, v);
    }
    uint Pop()
    {
        if (IsFull())
            return -1;
        uint index;
        RingBuffer.InterlockedAdd(4, 1, index);
        return RingBuffer.Load((index % Capacity) * 4 + 8);
    }
    uint PopNoCheck()
    {
        uint index;
        RingBuffer.InterlockedAdd(4, 1, index);
        return RingBuffer.Load((index % Capacity) * 4 + 8);
    }
    int GetPopNumIndex(int num)
    {
        uint index;
        RingBuffer.InterlockedAdd(4, num, index);
        return index;
    }
    uint GetValue(int index)
    {
        return RingBuffer.Load((index % Capacity) * 4 + 8);
    }
    int GetCount()
    {
        return RingBuffer.Load(0) - RingBuffer.Load(4);
    }
    bool IsFull()
    {
        return GetCount() > Capacity;
    }
    bool IsEmpty()
    {
        return RingBuffer.Load(4) == RingBuffer.Load(0);
    }
};

struct TtRawArray
{
    RWByteAddressBuffer RawBuffer;
    int Capacity;
    int GetCount()
    {
        return RawBuffer.Load(0);
    }
    void Clear()
    {
        RawBuffer.Store(0, 0);
    }
    void Push(uint v)
    {
        uint index;
        RawBuffer.InterlockedAdd(0, 1, index);
        RawBuffer.Store((index % Capacity) * 4 + 4, v);
    }
    int GetPushNumIndex(int num)
    {
        uint index;
        RawBuffer.InterlockedAdd(0, num, index);
        return index;
    }
    bool IsFull()
    {
        return RawBuffer.Load(0) >= Capacity;
    }
    uint GetValue(int index)
    {
        return RawBuffer.Load((index % Capacity) * 4 + 4);
    }
    void SetValue(int index, uint v)
    {
        RawBuffer.Store((index % Capacity) * 4 + 4, v);
    }
};

struct TtReadonlyRawArray
{
    RWByteAddressBuffer RawBuffer;
    int Capacity;
    int GetCount()
    {
        return RawBuffer.Load(0);
    }
    uint GetValue(int index)
    {
        return RawBuffer.Load((index % Capacity) * 4 + 4);
    }
};

#endif//#ifndef _ALGORITHM_RINGBUFFER_H_