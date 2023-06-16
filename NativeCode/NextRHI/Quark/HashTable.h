
#pragma once
#include "../Base/BaseHead.h"
#include "../Math/v3dxVector3.h"
#include <vector>
#include <map>

template <typename T>
static constexpr __forceinline bool IsPowerOfTwo(T Value)
{
    return ((Value & (Value - 1)) == (T)0);
}

static __forceinline UINT32 CountLeadingZeros(UINT32 Value)
{
    // return 32 if value is zero
    unsigned long BitIndex;
    _BitScanReverse64(&BitIndex, UINT64(Value) * 2 + 1);
    return 32 - BitIndex;
}
static __forceinline UINT32 CeilLogTwo(UINT32 Arg)
{
    // if Arg is 0, change it to 1 so that we return 0
    Arg = Arg ? Arg : 1;
    return 32 - CountLeadingZeros(Arg - 1);
}

static __forceinline UINT32 RoundUpToPowerOfTwo(UINT32 Arg)
{
    return 1 << CeilLogTwo(Arg);
}

class FHashTable
{
public:
    FHashTable(UINT32 InHashSize = 1024, UINT32 InIndexSize = 0);
    FHashTable(const FHashTable& Other);
    ~FHashTable();

    void			Clear();
    void			Clear(UINT32 InHashSize, UINT32 InIndexSize = 0);
    void			Free();
    void	Resize(UINT32 NewIndexSize);

    // Functions used to search
    UINT32			First(UINT32 Key) const;
    UINT32			Next(UINT32 Index) const;
    bool			IsValid(UINT32 Index) const;

    void			Add(UINT32 Key, UINT32 Index);
    void			Add_Concurrent(UINT32 Key, UINT32 Index);
    void			Remove(UINT32 Key, UINT32 Index);

    // Average # of compares per search
    //float	AverageSearch() const;

protected:
    // Avoids allocating hash until first add
    static UINT32	EmptyHash[1];

    UINT32			HashSize;
    UINT32			HashMask;
    UINT32			IndexSize;

    UINT32* Hash;
    UINT32* NextIndex;
};
