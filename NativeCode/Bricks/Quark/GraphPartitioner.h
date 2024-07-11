
#pragma once

#include "DisjointSet.h"
#include "../../Math/v3dxBox3.h"

NS_BEGIN

template< typename ValueType, typename CountType, class SortKeyClass >
void RadixSort32(ValueType* Dst, ValueType* Src, CountType Num, SortKeyClass SortKey)
{
    CountType Histograms[1024 + 2048 + 2048];
    CountType* Histogram0 = Histograms + 0;
    CountType* Histogram1 = Histogram0 + 1024;
    CountType* Histogram2 = Histogram1 + 2048;

    memset(Histograms, 0, sizeof(Histograms));

    {
        // Parallel histogram generation pass
        const ValueType* s = (const ValueType*)Src;
        for (CountType i = 0; i < Num; i++)
        {
            UINT Key = SortKey(s[i]);
            Histogram0[(Key >> 0) & 1023]++;
            Histogram1[(Key >> 10) & 2047]++;
            Histogram2[(Key >> 21) & 2047]++;
        }
    }
    {
        // Prefix sum
        // Set each histogram entry to the sum of entries preceding it
        CountType Sum0 = 0;
        CountType Sum1 = 0;
        CountType Sum2 = 0;
        for (CountType i = 0; i < 1024; i++)
        {
            CountType t;
            t = Histogram0[i] + Sum0; Histogram0[i] = Sum0 - 1; Sum0 = t;
            t = Histogram1[i] + Sum1; Histogram1[i] = Sum1 - 1; Sum1 = t;
            t = Histogram2[i] + Sum2; Histogram2[i] = Sum2 - 1; Sum2 = t;
        }
        for (CountType i = 1024; i < 2048; i++)
        {
            CountType t;
            t = Histogram1[i] + Sum1; Histogram1[i] = Sum1 - 1; Sum1 = t;
            t = Histogram2[i] + Sum2; Histogram2[i] = Sum2 - 1; Sum2 = t;
        }
    }
    {
        // Sort pass 1
        const ValueType* s = (const ValueType*)Src;
        ValueType* d = Dst;
        for (CountType i = 0; i < Num; i++)
        {
            ValueType Value = s[i];
            UINT Key = SortKey(Value);
            d[++Histogram0[((Key >> 0) & 1023)]] = Value;
        }
    }
    {
        // Sort pass 2
        const ValueType* s = (const ValueType*)Dst;
        ValueType* d = Src;
        for (CountType i = 0; i < Num; i++)
        {
            ValueType Value = s[i];
            UINT Key = SortKey(Value);
            d[++Histogram1[((Key >> 10) & 2047)]] = Value;
        }
    }
    {
        // Sort pass 3
        const ValueType* s = (const ValueType*)Src;
        ValueType* d = Dst;
        for (CountType i = 0; i < Num; i++)
        {
            ValueType Value = s[i];
            UINT Key = SortKey(Value);
            d[++Histogram2[((Key >> 21) & 2047)]] = Value;
        }
    }
}

static inline UINT MortonCode3(UINT x)
{
    x &= 0x000003ff;
    x = (x ^ (x << 16)) & 0xff0000ff;
    x = (x ^ (x << 8)) & 0x0300f00f;
    x = (x ^ (x << 4)) & 0x030c30c3;
    x = (x ^ (x << 2)) & 0x09249249;
    return x;
}

class FGraphPartitioner
{
public:
	struct FGraphData
	{
		INT32	Offset;
		INT32	Num;

		std::vector< INT32 >	Adjacency;
		std::vector< INT32 >	AdjacencyCost;
		std::vector< INT32 >	AdjacencyOffset;
	};

	// Inclusive
	struct FRange
	{
		UINT	Begin;
		UINT	End;

		bool operator<( const FRange& Other) const { return Begin < Other.Begin; }
	};
	std::vector< FRange >	Ranges;
	std::vector< UINT >	Indexes;
	std::vector< UINT >	SortedTo;

public:
				FGraphPartitioner( UINT InNumElements );

	FGraphData*	NewGraph( UINT NumAdjacency ) const;

	void		AddAdjacency( FGraphData* Graph, UINT AdjIndex, INT32 Cost );
	void		AddLocalityLinks( FGraphData* Graph, UINT Index, INT32 Cost );

//     bool GetCenter(std::vector<v3dxVector3>& Verts, std::vector<UINT>& Indexes, UINT TriIndex, v3dxVector3& Center)
//     {
//         if (Indexes[TriIndex * 3 + 0] >= Verts.size() ||
//             Indexes[TriIndex * 3 + 1] >= Verts.size() ||
//             Indexes[TriIndex * 3 + 2] >= Verts.size())
//         {
//             return false;
//         }
//         Center = Verts[Indexes[TriIndex * 3 + 0]];
//         Center += Verts[Indexes[TriIndex * 3 + 1]];
//         Center += Verts[Indexes[TriIndex * 3 + 2]];
//         Center *= (1.0f / 3.0f);
// 
//         return true;
//     }
	template< typename FGetCenter >
	void		BuildLocalityLinks( FDisjointSet& DisjointSet, const v3dxBox3& Bounds, const std::vector<INT32>& GroupIndexes, FGetCenter& GetCenter/*std::vector<v3dxVector3>& Verts, std::vector<UINT>& Indexes*/)
    {
        std::vector< UINT > SortKeys;
        SortKeys.resize(NumElements);
        SortedTo.resize(NumElements);

        const bool bElementGroups = !GroupIndexes.empty();	// Only create locality links between elements with the same group index

        // 	ParallelFor( TEXT("BuildLocalityLinks.PF"), NumElements, 4096,
        // 		[&]( UINT Index )
        for (UINT Index = 0; Index < NumElements; ++Index)
        {
            v3dxVector3 Center;
            bool isValid = GetCenter(Index, Center);
            if (!isValid)
                continue;

            v3dxVector3 CenterLocal = (Center - Bounds.Min()) / v3dxVector3(Bounds.Max() - Bounds.Min()).getMax();

            UINT Morton;
            Morton = MortonCode3(UINT(CenterLocal.X * 1023));
            Morton |= MortonCode3(UINT(CenterLocal.Y * 1023)) << 1;
            Morton |= MortonCode3(UINT(CenterLocal.Z * 1023)) << 2;
            SortKeys[Index] = Morton;
        }
        //);

        RadixSort32(&SortedTo[0], &Indexes[0], NumElements,
            [&](UINT Index)
            {
                return SortKeys[Index];
            });

        SortKeys.clear();

        //Swap( Indexes, SortedTo );
        {
            std::vector< UINT > temp;
            temp.resize(Indexes.size());
            if (Indexes.size() > 0)
            {
                memcpy(&temp[0], &Indexes[0], sizeof(UINT) * Indexes.size());
            }

            Indexes.resize(SortedTo.size());
            if (SortedTo.size() > 0)
            {
                memcpy(&Indexes[0], &SortedTo[0], sizeof(UINT) * SortedTo.size());
            }

            SortedTo.resize(temp.size());
            if (temp.size() > 0)
            {
                memcpy(&SortedTo[0], &temp[0], sizeof(UInt32) * temp.size());
            }
        }

        for (UINT i = 0; i < NumElements; i++)
        {
            SortedTo[Indexes[i]] = i;
        }

        std::vector< FRange > IslandRuns;
        IslandRuns.resize(NumElements);

        // Run length acceleration
        // Range of identical IslandID denoting that elements are connected.
        // Used for jumping past connected elements to the next nearby disjoint element.
        {
            UINT RunIslandID = 0;
            UINT RunFirstElement = 0;

            for (UINT i = 0; i < NumElements; i++)
            {
                UINT IslandID = DisjointSet.Find(Indexes[i]);

                if (RunIslandID != IslandID)
                {
                    // We found the end so rewind to the beginning of the run and fill.
                    for (UINT j = RunFirstElement; j < i; j++)
                    {
                        IslandRuns[j].End = i - 1;
                    }

                    // Start the next run
                    RunIslandID = IslandID;
                    RunFirstElement = i;
                }

                IslandRuns[i].Begin = RunFirstElement;
            }
            // Finish the last run
            for (UINT j = RunFirstElement; j < NumElements; j++)
            {
                IslandRuns[j].End = NumElements - 1;
            }
        }

        for (UINT i = 0; i < NumElements; i++)
        {
            UINT Index = Indexes[i];

            UINT RunLength = IslandRuns[i].End - IslandRuns[i].Begin + 1;
            if (RunLength < 128)
            {
                UINT IslandID = DisjointSet[Index];
                INT32 GroupID = bElementGroups ? GroupIndexes[Index] : 0;

                v3dxVector3 Center;
                bool isValid = GetCenter(Index, Center);
                if (!isValid)
                    continue;

                const UINT MaxLinksPerElement = 5;

                UINT ClosestIndex[MaxLinksPerElement];
                float  ClosestDist2[MaxLinksPerElement];
                for (INT32 k = 0; k < MaxLinksPerElement; k++)
                {
                    ClosestIndex[k] = ~0u;
                    ClosestDist2[k] = Math::POS_INFINITY;
                }

                for (int Direction = 0; Direction < 2; Direction++)
                {
                    UINT Limit = Direction ? NumElements - 1 : 0;
                    UINT Step = Direction ? 1 : -1;

                    UINT Adj = i;
                    for (INT32 Iterations = 0; Iterations < 16; Iterations++)
                    {
                        if (Adj == Limit)
                            break;
                        Adj += Step;

                        UINT AdjIndex = Indexes[Adj];
                        UINT AdjIslandID = DisjointSet[AdjIndex];
                        INT32 AdjGroupID = bElementGroups ? GroupIndexes[AdjIndex] : 0;
                        if (IslandID == AdjIslandID || (GroupID != AdjGroupID))
                        {
                            // Skip past this run
                            if (Direction)
                                Adj = IslandRuns[Adj].End;
                            else
                                Adj = IslandRuns[Adj].Begin;
                        }
                        else
                        {
                            // Add to sorted list
                            v3dxVector3 temp;
                            bool isValid = GetCenter(AdjIndex, temp);
                            if (!isValid)
                                continue;

                            float AdjDist2 = (Center - temp).getLengthSq();
                            for (int k = 0; k < MaxLinksPerElement; k++)
                            {
                                if (AdjDist2 < ClosestDist2[k])
                                {
                                    //Swap( AdjIndex, ClosestIndex[k] );
                                    //Swap( AdjDist2, ClosestDist2[k] );

                                    UINT temp = AdjIndex;
                                    AdjIndex = ClosestIndex[k];
                                    ClosestIndex[k] = temp;

                                    temp = UINT(AdjDist2);
                                    AdjDist2 = ClosestDist2[k];
                                    ClosestDist2[k] = AdjDist2;
                                }
                            }
                        }
                    }
                }

                for (int k = 0; k < MaxLinksPerElement; k++)
                {
                    if (ClosestIndex[k] != ~0u)
                    {
                        // Add both directions
                        //LocalityLinks.AddUnique( Index, ClosestIndex[k] );
                        //LocalityLinks.AddUnique( ClosestIndex[k], Index );
                        InsertToLocalityLinks(Index, ClosestIndex[k]);
                        InsertToLocalityLinks(ClosestIndex[k], Index);

                    }
                }
            }
        }
    }

	void		Partition( FGraphData* Graph, INT32 InMinPartitionSize, INT32 InMaxPartitionSize );
	void		PartitionStrict( FGraphData* Graph, INT32 InMinPartitionSize, INT32 InMaxPartitionSize, bool bThreaded );

	void		InsertToLocalityLinks(UINT k, UINT v);
private:
	void		BisectGraph( FGraphData* Graph, FGraphData* ChildGraphs[2] );
	void		RecursiveBisectGraph( FGraphData* Graph );

	UINT		NumElements;
	INT32		MinPartitionSize = 0;
	INT32		MaxPartitionSize = 0;

	std::atomic< UINT >	NumPartitions;

	std::vector< INT32 >		PartitionIDs;
	std::vector< INT32 >		SwappedWith;

	typedef std::vector<UINT> LinkValues;
	std::map< UINT, LinkValues >	LocalityLinks;
};

NS_END