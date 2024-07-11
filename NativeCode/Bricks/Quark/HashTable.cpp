
#include "HashTable.h"

NS_BEGIN

UINT FHashTable::EmptyHash[1] = { ~0u };
//namespace Nanite
//{
FHashTable::FHashTable(UINT InHashSize, UINT InIndexSize)
    : HashSize(InHashSize)
    , HashMask(0)
    , IndexSize(InIndexSize)
    , Hash(EmptyHash)
    , NextIndex(nullptr)
{
    ASSERT(HashSize > 0);
    ASSERT(IsPowerOfTwo(HashSize));

    if (IndexSize)
    {
        HashMask = HashSize - 1;

        Hash = new UINT[HashSize];
        NextIndex = new UINT[IndexSize];

        memset(Hash, 0xff, HashSize * 4);
    }
}

FHashTable::FHashTable(const FHashTable& Other)
    : HashSize(Other.HashSize)
    , HashMask(Other.HashMask)
    , IndexSize(Other.IndexSize)
    , Hash(EmptyHash)
{
    if (IndexSize)
    {
        Hash = new UINT[HashSize];
        NextIndex = new UINT[IndexSize];

        memcpy(Hash, Other.Hash, HashSize * 4);
        memcpy(NextIndex, Other.NextIndex, IndexSize * 4);
    }
}

FHashTable::~FHashTable()
{
    Free();
}

void FHashTable::Clear()
{
    if (IndexSize)
    {
        memset(Hash, 0xff, HashSize * 4);
    }
}

void FHashTable::Clear(UINT InHashSize, UINT InIndexSize)
{
    Free();

    HashSize = InHashSize;
    IndexSize = InIndexSize;

    ASSERT(HashSize > 0);
    ASSERT(IsPowerOfTwo(HashSize));

    if (IndexSize)
    {
        HashMask = HashSize - 1;

        Hash = new UINT[HashSize];
        NextIndex = new UINT[IndexSize];

        memset(Hash, 0xff, HashSize * 4);
    }
}

void FHashTable::Free()
{
    if (IndexSize)
    {
        HashMask = 0;
        IndexSize = 0;

        delete[] Hash;
        Hash = EmptyHash;

        delete[] NextIndex;
        NextIndex = nullptr;
    }
}

// First in hash chain
UINT FHashTable::First(UINT Key) const
{
    Key &= HashMask;
    return Hash[Key];
}

// Next in hash chain
UINT FHashTable::Next(UINT Index) const
{
    ASSERT(Index < IndexSize);
    ASSERT(NextIndex[Index] != Index); // check for corrupt tables
    return NextIndex[Index];
}

bool FHashTable::IsValid(UINT Index) const
{
    return Index != ~0u;
}

void FHashTable::Resize(UINT NewIndexSize)
{
    if (NewIndexSize == IndexSize)
    {
        return;
    }

    if (NewIndexSize == 0)
    {
        Free();
        return;
    }

    if (IndexSize == 0)
    {
        HashMask = (UINT16)(HashSize - 1);
        Hash = new UINT[HashSize];
        memset(Hash, 0xff, HashSize * 4);
    }

    UINT* NewNextIndex = new UINT[NewIndexSize];

    if (NextIndex)
    {
        memcpy(NewNextIndex, NextIndex, IndexSize * 4);
        delete[] NextIndex;
    }

    IndexSize = NewIndexSize;
    NextIndex = NewNextIndex;
}
void FHashTable::Add(UINT Key, UINT Index)
{
    if (Index >= IndexSize)
    {
        Resize(std::max(32u, RoundUpToPowerOfTwo(Index + 1)));
    }

    Key &= HashMask;
    NextIndex[Index] = Hash[Key];
    Hash[Key] = Index;
}

// Safe for many threads to add concurrently.
// Not safe to search the table while other threads are adding.
// Will not resize. Only use for presized tables.
void FHashTable::Add_Concurrent(UINT Key, UINT Index)
{
    ASSERT(Index < IndexSize);

    Key &= HashMask;
    NextIndex[Index] = (int)::_InterlockedExchange((long*)&Hash[Key], (long)Index);
}

inline void FHashTable::Remove(UINT Key, UINT Index)
{
    if (Index >= IndexSize)
    {
        return;
    }

    Key &= HashMask;

    if (Hash[Key] == Index)
    {
        // Head of chain
        Hash[Key] = NextIndex[Index];
    }
    else
    {
        for (UINT i = Hash[Key]; IsValid(i); i = NextIndex[i])
        {
            if (NextIndex[i] == Index)
            {
                // Next = Next->Next
                NextIndex[i] = NextIndex[Index];
                break;
            }
        }
    }
}

NS_END