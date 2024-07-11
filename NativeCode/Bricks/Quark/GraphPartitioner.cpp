
#include "GraphPartitioner.h"
#include <algorithm>
#include "../../../3rd/native/metis/5.1.0/include/metis.h"

NS_BEGIN

template <class T>
static constexpr inline T DivideAndRoundUp(T Dividend, T Divisor)
{
    return (Dividend + Divisor - 1) / Divisor;
}

template <class T>
static constexpr inline T DivideAndRoundNearest(T Dividend, T Divisor)
{
    return (Dividend >= 0)
        ? (Dividend + Divisor / 2) / Divisor
        : (Dividend - Divisor / 2 + 1) / Divisor;
}

FGraphPartitioner::FGraphPartitioner( UINT InNumElements )
	: NumElements( InNumElements )
{
	Indexes.resize( NumElements );
	for( UINT i = 0; i < NumElements; i++ )
	{
		Indexes[i] = i;
	}
}

void FGraphPartitioner::AddAdjacency(FGraphData* Graph, UINT AdjIndex, INT32 Cost)
{
    Graph->Adjacency.push_back(SortedTo[AdjIndex]);
    Graph->AdjacencyCost.push_back(Cost);
}

void FGraphPartitioner::AddLocalityLinks(FGraphData* Graph, UINT Index, INT32 Cost)
{
    //for( auto Iter = LocalityLinks.CreateKeyIterator( Index ); Iter; ++Iter )
    auto Iter = LocalityLinks.find(Index);
    if (Iter != LocalityLinks.end())
    {
        auto values = Iter->second;
        for (int i = 0; i < values.size(); ++i)
        {
            UINT AdjIndex = values[i];
            Graph->Adjacency.push_back(SortedTo[AdjIndex]);
            Graph->AdjacencyCost.push_back(Cost);
        }
    }

}

void FGraphPartitioner::InsertToLocalityLinks(UINT k, UINT v)
{
    auto keyIter = LocalityLinks.find(k);
    if (keyIter != LocalityLinks.end())
    {
        auto values = keyIter->second;
        for (int i = 0; i < values.size(); i++)
        {
            if (values[i] == v)
                return;
        }
        values.push_back(v);
    }
}

FGraphPartitioner::FGraphData* FGraphPartitioner::NewGraph( UINT NumAdjacency ) const
{
	NumAdjacency += UINT(LocalityLinks.size());

	FGraphData* Graph = new FGraphPartitioner::FGraphData;
	Graph->Offset = 0;
	Graph->Num = NumElements;
	Graph->Adjacency.reserve( NumAdjacency );
	Graph->AdjacencyCost.reserve( NumAdjacency );
	Graph->AdjacencyOffset.resize( NumElements + 1 );
	return Graph;
}

void FGraphPartitioner::Partition( FGraphData* Graph, INT32 InMinPartitionSize, INT32 InMaxPartitionSize )
{
	ASSERT(false);
}

void FGraphPartitioner::BisectGraph( FGraphData* Graph, FGraphData* ChildGraphs[2] )
{
	ChildGraphs[0] = nullptr;
	ChildGraphs[1] = nullptr;

	auto AddPartition =
		[ this ]( INT32 Offset, INT32 Num )
		{
			FRange& Range = Ranges[ NumPartitions++ ];
			Range.Begin	= Offset;
			Range.End	= Offset + Num;
		};

	if( Graph->Num <= MaxPartitionSize )
	{
		AddPartition( Graph->Offset, Graph->Num );
		return;
	}

	const INT32 TargetPartitionSize = ( MinPartitionSize + MaxPartitionSize ) / 2;
	const INT32 TargetNumPartitions = std::max( 2, DivideAndRoundNearest( Graph->Num, TargetPartitionSize ) );

	ASSERT( Graph->AdjacencyOffset.size() == Graph->Num + 1 );

	INT32 NumConstraints = 1;
	INT32 NumParts = 2;
	INT32 EdgesCut = 0;

	float PartitionWeights[] = {
		float( TargetNumPartitions / 2 ) / TargetNumPartitions,
		1.0f - float( TargetNumPartitions / 2 ) / TargetNumPartitions
	};

	INT32 Options[ METIS_NOPTIONS ];
	METIS_SetDefaultOptions( Options );

	// Allow looser tolerance when at the higher levels. Strict balance isn't that important until it gets closer to partition sized.
	bool bLoose = TargetNumPartitions >= 128 || MaxPartitionSize / MinPartitionSize > 1;
	bool bSlow = Graph->Num < 4096;
	
	Options[ METIS_OPTION_UFACTOR ] = bLoose ? 200 : 1;
	//Options[ METIS_OPTION_NCUTS ] = Graph->Num < 1024 ? 8 : ( Graph->Num < 4096 ? 4 : 1 );
	//Options[ METIS_OPTION_NCUTS ] = bSlow ? 4 : 1;
	//Options[ METIS_OPTION_NITER ] = bSlow ? 20 : 10;
	//Options[ METIS_OPTION_IPTYPE ] = METIS_IPTYPE_RANDOM;
	//Options[ METIS_OPTION_MINCONN ] = 1;

	int r = METIS_PartGraphRecursive(
		&Graph->Num,
		&NumConstraints,			// number of balancing constraints
		&Graph->AdjacencyOffset[0],
		&Graph->Adjacency[0],
		NULL,						// Vert weights
		NULL,						// Vert sizes for computing the total communication volume
		&Graph->AdjacencyCost[0],	// Edge weights
		&NumParts,
		PartitionWeights,			// Target partition weight
		NULL,						// Allowed load imbalance tolerance
		Options,
		&EdgesCut,
		&PartitionIDs[0] + Graph->Offset
	);

	if( /*ensureAlways*/( r == METIS_OK ) )
	{
		// In place divide the array
		// Both sides remain sorted but back is reversed.
		INT32 Front = Graph->Offset;
		INT32 Back =  Graph->Offset + Graph->Num - 1;
		while( Front <= Back )
		{
			while( Front <= Back && PartitionIDs[ Front ] == 0 )
			{
				SwappedWith[ Front ] = Front;
				Front++;
			}
			while( Front <= Back && PartitionIDs[ Back ] == 1 )
			{
				SwappedWith[ Back ] = Back;
				Back--;
			}

			if( Front < Back )
			{
				//Swap( Indexes[ Front ], Indexes[ Back ] );
				auto temp = Indexes[Front];
				Indexes[Front] = Indexes[Back];
				Indexes[Back] = Indexes[Front];

				SwappedWith[ Front ] = Back;
				SwappedWith[ Back ] = Front;
				Front++;
				Back--;
			}
		}

		INT32 Split = Front;

		INT32 Num[2];
		Num[0] = Split - Graph->Offset;
		Num[1] = Graph->Offset + Graph->Num - Split;
				
		ASSERT( Num[0] > 1 );
		ASSERT( Num[1] > 1 );

		if( Num[0] <= MaxPartitionSize && Num[1] <= MaxPartitionSize )
		{
			AddPartition( Graph->Offset,	Num[0] );
			AddPartition( Split,			Num[1] );
		}
		else
		{
			for( INT32 i = 0; i < 2; i++ )
			{
				ChildGraphs[i] = new FGraphData;
				ChildGraphs[i]->Adjacency.reserve( Graph->Adjacency.size() >> 1 );
				ChildGraphs[i]->AdjacencyCost.reserve( Graph->Adjacency.size() >> 1 );
				ChildGraphs[i]->AdjacencyOffset.reserve( Num[i] + 1 );
				ChildGraphs[i]->Num = Num[i];
			}

			ChildGraphs[0]->Offset = Graph->Offset;
			ChildGraphs[1]->Offset = Split;

			for( INT32 i = 0; i < Graph->Num; i++ )
			{
				FGraphData* ChildGraph = ChildGraphs[ i >= ChildGraphs[0]->Num ];

				ChildGraph->AdjacencyOffset.push_back( INT32(ChildGraph->Adjacency.size()) );
				
				INT32 OrgIndex = SwappedWith[ Graph->Offset + i ] - Graph->Offset;
				for( INT32 AdjIndex = Graph->AdjacencyOffset[ OrgIndex ]; AdjIndex < Graph->AdjacencyOffset[ OrgIndex + 1 ]; AdjIndex++ )
				{
					INT32 Adj     = Graph->Adjacency[ AdjIndex ];
					INT32 AdjCost = Graph->AdjacencyCost[ AdjIndex ];

					// Remap to child
					Adj = SwappedWith[ Graph->Offset + Adj ] - ChildGraph->Offset;

					// Edge connects to node in this graph
					if( 0 <= Adj && Adj < ChildGraph->Num )
					{
						ChildGraph->Adjacency.push_back( Adj );
						ChildGraph->AdjacencyCost.push_back( AdjCost );
					}
				}
			}
			ChildGraphs[0]->AdjacencyOffset.push_back(INT32(ChildGraphs[0]->Adjacency.size()) );
			ChildGraphs[1]->AdjacencyOffset.push_back(INT32(ChildGraphs[1]->Adjacency.size()) );
		}
	}
}

void FGraphPartitioner::RecursiveBisectGraph( FGraphData* Graph )
{
	FGraphData* ChildGraphs[2];
	BisectGraph( Graph, ChildGraphs );
	delete Graph;

	if( ChildGraphs[0] && ChildGraphs[1] )
	{
		RecursiveBisectGraph( ChildGraphs[0] );
		RecursiveBisectGraph( ChildGraphs[1] );
	}
}

void FGraphPartitioner::PartitionStrict( FGraphData* Graph, INT32 InMinPartitionSize, INT32 InMaxPartitionSize, bool bThreaded )
{
	MinPartitionSize = InMinPartitionSize;
	MaxPartitionSize = InMaxPartitionSize;

	auto newSize = PartitionIDs.size() + NumElements;
	PartitionIDs.resize(newSize);

	newSize = SwappedWith.size() + NumElements;
	SwappedWith.resize(newSize);

	// Adding to atomically so size big enough to not need to grow.
	INT32 NumPartitionsExpected = DivideAndRoundUp( Graph->Num, MinPartitionSize );

	newSize = Ranges.size() + NumPartitionsExpected * 2;
	Ranges.resize( newSize );
	NumPartitions = 0;

// 	if( bThreaded && NumPartitionsExpected > 4 )
// 	{	
// 		extern CORE_API INT32 GUseNewTaskBackend;
// 		if (GUseNewTaskBackend)
// 		{
// 			TLocalWorkQueue<FGraphData> LocalWork(Graph);
// 			LocalWork.Run(MakeYCombinator([this, &LocalWork](auto Self, FGraphData* Graph) -> void
// 			{
// 				FGraphData* ChildGraphs[2];
// 				BisectGraph( Graph, ChildGraphs );
// 				delete Graph;
// 
// 				if( ChildGraphs[0] && ChildGraphs[1] )
// 				{
// 					// Only spawn add a worker thread if remaining work is expected to be large enough
// 					if (ChildGraphs[0]->Num > 256)
// 					{
// 						LocalWork.AddTask(ChildGraphs[0]);
// 						LocalWork.AddWorkers(1);
// 					}
// 					else
// 					{
// 						Self(ChildGraphs[0]);
// 					}
// 					Self(ChildGraphs[1]);
// 				}
// 			}));
// 		}
// 		else
// 		{
// 			const ENamedThreads::Type DesiredThread = IsInGameThread() ? ENamedThreads::AnyThread : ENamedThreads::AnyBackgroundThreadNormalTask;
// 
// 			class FBuildTask
// 			{
// 			public:
// 				FBuildTask( FGraphPartitioner* InPartitioner, FGraphData* InGraph, ENamedThreads::Type InDesiredThread)
// 					: Partitioner( InPartitioner )
// 					, Graph( InGraph )
// 					, DesiredThread( InDesiredThread )
// 				{}
// 
// 				void DoTask( ENamedThreads::Type CurrentThread, const FGraphEventRef& MyCompletionEvent )
// 				{
// 					FGraphData* ChildGraphs[2];
// 					Partitioner->BisectGraph( Graph, ChildGraphs );
// 					delete Graph;
// 
// 					if( ChildGraphs[0] && ChildGraphs[1] )
// 					{
// 						if( ChildGraphs[0]->Num > 256 )
// 						{
// 							FGraphEventRef Task = TGraphTask< FBuildTask >::CreateTask().ConstructAndDispatchWhenReady( Partitioner, ChildGraphs[0], DesiredThread);
// 							MyCompletionEvent->DontCompleteUntil( Task );
// 						}
// 						else
// 						{
// 							FBuildTask( Partitioner, ChildGraphs[0], DesiredThread).DoTask( CurrentThread, MyCompletionEvent );
// 						}
// 
// 						FBuildTask( Partitioner, ChildGraphs[1], DesiredThread).DoTask( CurrentThread, MyCompletionEvent );
// 					}
// 				}
// 
// 				static inline TStatId GetStatId()
// 				{
// 					RETURN_QUICK_DECLARE_CYCLE_STAT(FBuildTask, STATGROUP_ThreadPoolAsyncTasks);
// 				}
// 
// 				static inline ESubsequentsMode::Type	GetSubsequentsMode()	{ return ESubsequentsMode::TrackSubsequents; }
// 
// 				inline ENamedThreads::Type GetDesiredThread() const
// 				{
// 					return DesiredThread;
// 				}
// 
// 			private:
// 				FGraphPartitioner*  Partitioner;
// 				FGraphData*         Graph;
// 				ENamedThreads::Type DesiredThread;
// 			};
// 
// 			FGraphEventRef BuildTask = TGraphTask< FBuildTask >::CreateTask( nullptr ).ConstructAndDispatchWhenReady( this, Graph, DesiredThread);
// 			FTaskGraphInterface::Get().WaitUntilTaskCompletes( BuildTask );
// 		}
// 	}
//	else
	{
		RecursiveBisectGraph( Graph );
	}

	Ranges.resize( NumPartitions );

// 	if( bThreaded )
// 	{
// 		// Force a deterministic order
// 		Ranges.Sort();
// 	}

	PartitionIDs.clear();
	SwappedWith.clear();

	for( UINT i = 0; i < NumElements; i++ )
	{
		SortedTo[ Indexes[i] ] = i;
	}
}

NS_END