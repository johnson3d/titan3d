
#pragma once
#include "../../Base/BaseHead.h"
#include "../../Math/v3dxVector3.h"
#include <vector>
#include <map>

NS_BEGIN

template <typename T>
static constexpr inline bool IsPowerOfTwo(T Value)
{
    return ((Value & (Value - 1)) == (T)0);
}

static inline UINT CountLeadingZeros(UINT Value)
{
    // return 32 if value is zero
    DWORD BitIndex;
    _BitScanReverse64(&BitIndex, UINT64(Value) * 2 + 1);
    return 32 - BitIndex;
}
static inline UINT CeilLogTwo(UINT Arg)
{
    // if Arg is 0, change it to 1 so that we return 0
    Arg = Arg ? Arg : 1;
    return 32 - CountLeadingZeros(Arg - 1);
}

static inline UINT RoundUpToPowerOfTwo(UINT Arg)
{
    return 1 << CeilLogTwo(Arg);
}

class FHashTable
{
public:
    FHashTable(UINT InHashSize = 1024, UINT InIndexSize = 0);
    FHashTable(const FHashTable& Other);
    ~FHashTable();

    void			Clear();
    void			Clear(UINT InHashSize, UINT InIndexSize = 0);
    void			Free();
    void	Resize(UINT NewIndexSize);

    // Functions used to search
    UINT			First(UINT Key) const;
    UINT			Next(UINT Index) const;
    bool			IsValid(UINT Index) const;

    void			Add(UINT Key, UINT Index);
    void			Add_Concurrent(UINT Key, UINT Index);
    void			Remove(UINT Key, UINT Index);

    // Average # of compares per search
    //float	AverageSearch() const;

protected:
    // Avoids allocating hash until first add
    static UINT	EmptyHash[1];

    UINT			HashSize;
    UINT			HashMask;
    UINT			IndexSize;

    UINT* Hash;
    UINT* NextIndex;
};

NS_END