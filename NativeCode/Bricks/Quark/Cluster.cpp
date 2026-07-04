

#include "HashTable.h"
#include "Cluster.h"
#include "GraphPartitioner.h"
#include "../MeshSimplify/IMeshSimplify.h"
#include "meshoptimizer.h"

NS_BEGIN



void CorrectAttributes( float* Attributes )
{
	//v3dxVector3& Normal = *reinterpret_cast< v3dxVector3* >( Attributes );
	//Normal.Normalize();
}

void CorrectAttributesColor( float* Attributes )
{
	CorrectAttributes( Attributes );
	
    //FLinearColor& Color = *reinterpret_cast< FLinearColor* >( Attributes + 3 );
    //Color = Color.GetClamped();
}


QuarkCluster::QuarkCluster(
	const std::vector< v3dxVector3 >& InVerts,
	const std::vector< UINT >& InIndexes,
	//const std::vector< INT32 >& InMaterialIndexes,
	//UINT InNumTexCoords, bool bInHasColors, bool bInPreserveArea,
	UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency )
{
	//GUID = (uint64(TriBegin) << 32) | TriEnd;
	
	NumTris = TriEnd - TriBegin;
	//ensure(NumTriangles <= QuarkCluster::ClusterSize);
	
    //bHasColors = bInHasColors;
    //bPreserveArea = bInPreserveArea;
    //NumTexCoords = InNumTexCoords;

	Verts.reserve( NumTris * GetVertSize() );
	Indexes.reserve( 3 * NumTris );
	MaterialIndexes.reserve( NumTris );
	ExternalEdges.reserve( 3 * NumTris );
	NumExternalEdges = 0;

	//ASSERT(InMaterialIndexes.size() * 3 == InIndexes.size());

	std::map< UINT, UINT > OldToNewIndex;
	//OldToNewIndex.reserve( NumTris );

	for( UINT i = TriBegin; i < TriEnd; i++ )
	{
		UINT TriIndex = Partitioner.Indexes[i];

		for( UINT k = 0; k < 3; k++ )
		{
			UINT OldIndex = InIndexes[ TriIndex * 3 + k ];
            UINT NewIndex = ~0u;

            auto NewIndexPtr = OldToNewIndex.find(OldIndex);
            if (NewIndexPtr != OldToNewIndex.end())
			{
				NewIndex = NewIndexPtr->second;
			}

			if( NewIndex == ~0u )
			{
				auto size = Verts.size();
				Verts.resize( GetVertSize() + size);
				NewIndex = NumVerts++;
				OldToNewIndex.insert(std::make_pair(OldIndex, NewIndex));
				
				const v3dxVector3& InVert = InVerts[ OldIndex ];

				GetPosition( NewIndex ) = InVert;
                //GetNormal( NewIndex ) = InVert.TangentZ;
                //
                //if( bHasColors )
                //{
                //	GetColor( NewIndex ) = InVert.Color.ReinterpretAsLinear();
                //}
                //
                //FVector2f* UVs = GetUVs( NewIndex );
                //for( UINT UVIndex = 0; UVIndex < NumTexCoords; UVIndex++ )
                //{
                //	UVs[ UVIndex ] = InVert.UVs[ UVIndex ];
                //}
			}

			Indexes.push_back( NewIndex );

			INT32 EdgeIndex = TriIndex * 3 + k;
			INT32 AdjCount = 0;
			
			Adjacency.ForAll( EdgeIndex,
				[ &AdjCount, TriBegin, TriEnd, &Partitioner ]( INT32 EdgeIndex, INT32 AdjIndex )
				{
					UINT AdjTri = Partitioner.SortedTo[ AdjIndex / 3 ];
					if( AdjTri < TriBegin || AdjTri >= TriEnd )
						AdjCount++;
				} );

			ExternalEdges.push_back( AdjCount );
			NumExternalEdges += AdjCount != 0 ? 1 : 0;
		}

		//MaterialIndexes.push_back( InMaterialIndexes[ TriIndex ] );
	}

	SanitizeVertexData();

    //for( UINT VertexIndex = 0; VertexIndex < NumVerts; VertexIndex++ )
    //{
    //	float* Attributes = GetAttributes( VertexIndex );
    //
    //	// Make sure this vertex is valid from the start
    //	if( bHasColors )
    //		CorrectAttributesColor( Attributes );
    //	else
    //		CorrectAttributes( Attributes );
    //}

	Bound();
}

// Split
QuarkCluster::QuarkCluster( QuarkCluster& SrcCluster, UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency )
	: MipLevel( SrcCluster.MipLevel )
{
	//GUID = MurmurFinalize64(SrcCluster.GUID) ^ ((uint64(TriBegin) << 32) | TriEnd);

	NumTexCoords	= SrcCluster.NumTexCoords;
	bHasColors		= SrcCluster.bHasColors;
	bPreserveArea	= SrcCluster.bPreserveArea;
	
	NumTris = TriEnd - TriBegin;

	Verts.reserve( NumTris * GetVertSize() );
	Indexes.reserve( 3 * NumTris );
	MaterialIndexes.reserve( NumTris );
	ExternalEdges.reserve( 3 * NumTris );
	NumExternalEdges = 0;

	std::map< UINT, UINT > OldToNewIndex;
	//OldToNewIndex.Reserve( NumTris );

	for( UINT i = TriBegin; i < TriEnd; i++ )
	{
		UINT TriIndex = Partitioner.Indexes[i];

		for( UINT k = 0; k < 3; k++ )
		{
			UINT OldIndex = SrcCluster.Indexes[ TriIndex * 3 + k ];
            UINT NewIndex = ~0u;

            auto NewIndexPtr = OldToNewIndex.find(OldIndex);
            if (NewIndexPtr != OldToNewIndex.end())
            {
                NewIndex = NewIndexPtr->second;
            }

			if( NewIndex == ~0u )
			{
				auto size = Verts.size();
				Verts.resize( GetVertSize() + size );

				NewIndex = NumVerts++;
				OldToNewIndex.insert( std::make_pair(OldIndex, NewIndex) );

				memcpy( &GetPosition( NewIndex ), &SrcCluster.GetPosition( OldIndex ), GetVertSize() * sizeof( float ) );
			}

			Indexes.push_back( NewIndex );

			INT32 EdgeIndex = TriIndex * 3 + k;
			INT32 AdjCount = SrcCluster.ExternalEdges[ EdgeIndex ];
			
			Adjacency.ForAll( EdgeIndex,
				[ &AdjCount, TriBegin, TriEnd, &Partitioner ]( INT32 EdgeIndex, INT32 AdjIndex )
				{
					UINT AdjTri = Partitioner.SortedTo[ AdjIndex / 3 ];
					if( AdjTri < TriBegin || AdjTri >= TriEnd )
						AdjCount++;
				} );

			ExternalEdges.push_back( AdjCount );
			NumExternalEdges += AdjCount != 0 ? 1 : 0;
		}

		MaterialIndexes.push_back( SrcCluster.MaterialIndexes[ TriIndex ] );
	}

	Bound();
}

// Merge
QuarkCluster::QuarkCluster(const std::vector<QuarkCluster*>& MergeList)
{
	NumVerts = 0;
	NumTris = 0;
	NumTexCoords = 0;
	bHasColors = false;
	bPreserveArea = false;
	NumExternalEdges = 0;

	// Calculate total sizes
	UINT TotalVerts = 0;
	UINT TotalTris = 0;
	for (auto* Cluster : MergeList)
	{
		TotalVerts += Cluster->NumVerts;
		TotalTris += Cluster->NumTris;
	}

	Verts.reserve(TotalVerts * GetVertSize());
	Indexes.reserve(3 * TotalTris);
	MaterialIndexes.reserve(TotalTris);
	ExternalEdges.reserve(3 * TotalTris);

	// Use hash table to weld shared boundary vertices during merge
	FHashTable HashTable(RoundUpToPowerOfTwo(TotalVerts));

	// Merge all clusters with vertex deduplication
	for (auto* Cluster : MergeList)
	{
		// Build old->new index mapping for this cluster
		std::vector<UINT> OldToNew(Cluster->NumVerts);
		for (UINT i = 0; i < Cluster->NumVerts; i++)
		{
			OldToNew[i] = AddVert((const float*)&Cluster->GetPosition(i), HashTable);
		}

		// Copy indices with remapping
		for (UINT i = 0; i < (UINT)Cluster->Indexes.size(); i++)
		{
			Indexes.push_back(OldToNew[Cluster->Indexes[i]]);
		}

		// Copy material indexes
		for (UINT i = 0; i < (UINT)Cluster->MaterialIndexes.size(); i++)
		{
			MaterialIndexes.push_back(Cluster->MaterialIndexes[i]);
		}

		// Copy external edges
		for (UINT i = 0; i < (UINT)Cluster->ExternalEdges.size(); i++)
		{
			ExternalEdges.push_back(Cluster->ExternalEdges[i]);
			NumExternalEdges += Cluster->ExternalEdges[i] != 0 ? 1 : 0;
		}

		NumTris += Cluster->NumTris;
	}

	// Take MipLevel from the first cluster
	if (!MergeList.empty())
	{
		MipLevel = MergeList[0]->MipLevel;
	}

	Bound();
}

float QuarkCluster::Simplify( UINT TargetNumTris, float TargetError, UINT LimitNumTris, bool bForNaniteFallback )
{
	if (NumTris <= TargetNumTris)
		return 0.0f;

	// Use meshopt_simplify for high-quality topology-preserving simplification
	size_t target_index_count = (size_t)TargetNumTris * 3;
	float meshopt_error = 0.0f;

	// Compute mesh scale for error metric (before simplification)
	const UINT VertSize = GetVertSize();
	float scale = meshopt_simplifyScale((const float*)&Verts[0], (size_t)NumVerts, VertSize * sizeof(float));

	std::vector<unsigned int> destination(Indexes.size()); // worst case: same as input

	size_t result_index_count = meshopt_simplify(
		destination.data(),
		Indexes.data(),
		Indexes.size(),
		(const float*)&Verts[0],
		(size_t)NumVerts,
		VertSize * sizeof(float),
		target_index_count,
		0.05f,  // 5% relative error tolerance
		meshopt_SimplifyLockBorder,  // Lock border vertices to prevent gaps at open edges
		&meshopt_error);

	if (result_index_count == 0)
		return 0.0f;

	UINT NewNumTris = (UINT)(result_index_count / 3);

	// Compact unused vertices
	std::vector<bool> vertUsed(NumVerts, false);
	for (size_t i = 0; i < result_index_count; i++)
	{
		vertUsed[destination[i]] = true;
	}

	std::vector<UINT> vertRemap(NumVerts, ~0u);
	UINT NewNumVerts = 0;
	std::vector<float> NewVerts;
	NewVerts.reserve(NumVerts * VertSize);

	for (UINT i = 0; i < NumVerts; i++)
	{
		if (vertUsed[i])
		{
			vertRemap[i] = NewNumVerts;
			for (UINT k = 0; k < VertSize; k++)
				NewVerts.push_back(Verts[i * VertSize + k]);
			NewNumVerts++;
		}
	}

	// Rebuild indices with compacted vertex mapping
	Indexes.clear();
	Indexes.reserve(result_index_count);
	for (size_t i = 0; i < result_index_count; i++)
	{
		Indexes.push_back(vertRemap[destination[i]]);
	}

	// Update cluster data
	Verts = std::move(NewVerts);
	NumVerts = NewNumVerts;
	NumTris = NewNumTris;

	// Rebuild external edges (mark all as potentially external after simplification)
	ExternalEdges.clear();
	ExternalEdges.resize(NumTris * 3, 1);
	NumExternalEdges = NumTris * 3;

	// Rebuild material indices
	MaterialIndexes.clear();
	MaterialIndexes.resize(NumTris, 0);

	// Recalculate bounds
	Bound();

	return meshopt_error * scale;
}

void QuarkCluster::Split( FGraphPartitioner& Partitioner, const FAdjacency& Adjacency, UINT InClusterSize ) const
{
	FDisjointSet DisjointSet( NumTris );
	for( INT32 EdgeIndex = 0; EdgeIndex < Indexes.size(); EdgeIndex++ )
	{
		Adjacency.ForAll( EdgeIndex,
			[ &DisjointSet ]( INT32 EdgeIndex0, INT32 EdgeIndex1 )
			{
				if( EdgeIndex0 > EdgeIndex1 )
					DisjointSet.UnionSequential( EdgeIndex0 / 3, EdgeIndex1 / 3 );
			} );
	}

	auto GetCenter = [ this ]( UINT TriIndex, v3dxVector3& Center)
	{
        if (Indexes[TriIndex * 3 + 0] >= Verts.size() ||
            Indexes[TriIndex * 3 + 1] >= Verts.size() ||
            Indexes[TriIndex * 3 + 2] >= Verts.size())
        {
            return false;
        }

		Center  = GetPositionConst( Indexes[ TriIndex * 3 + 0 ] );
		Center += GetPositionConst( Indexes[ TriIndex * 3 + 1 ] );
		Center += GetPositionConst( Indexes[ TriIndex * 3 + 2 ] );
		Center *= (1.0f / 3.0f);
		return true;
	};

	Partitioner.BuildLocalityLinks( DisjointSet, Bounds, MaterialIndexes, GetCenter );

	auto  Graph = Partitioner.NewGraph( NumTris * 3 );

	for( UINT i = 0; i < NumTris; i++ )
	{
		Graph->AdjacencyOffset[i] = INT32(Graph->Adjacency.size());

		UINT TriIndex = Partitioner.Indexes[i];

		// Add shared edges
		for( int k = 0; k < 3; k++ )
		{
			Adjacency.ForAll( 3 * TriIndex + k,
				[ &Partitioner, Graph ]( INT32 EdgeIndex, INT32 AdjIndex )
				{
					Partitioner.AddAdjacency( Graph, AdjIndex / 3, 4 * 65 );
				} );
		}

		Partitioner.AddLocalityLinks( Graph, TriIndex, 1 );
	}
	Graph->AdjacencyOffset[ NumTris ] = INT32(Graph->Adjacency.size());

	Partitioner.PartitionStrict( Graph, InClusterSize - 4, InClusterSize, false );
}

FAdjacency QuarkCluster::BuildAdjacency() const
{
	FAdjacency Adjacency( INT32(Indexes.size()) );
	FEdgeHash EdgeHash( INT32(Indexes.size()) );

	for( INT32 EdgeIndex = 0; EdgeIndex < Indexes.size(); EdgeIndex++ )
	{
		Adjacency.Direct[ EdgeIndex ] = -1;

		EdgeHash.ForAllMatching( EdgeIndex, true,
			[ this ]( INT32 CornerIndex, v3dxVector3& result)
			{
				if (Verts.size() > Indexes[CornerIndex])
				{
					result = GetPositionConst(Indexes[CornerIndex]);
					return true;
				}
				return false;
			},
			[&]( INT32 EdgeIndex, INT32 OtherEdgeIndex )
			{
				Adjacency.Link( EdgeIndex, OtherEdgeIndex );
			} );
	}

	return Adjacency;
}

UINT QuarkCluster::AddVert( const float* Vert, FHashTable& HashTable )
{
	const UINT VertSize = GetVertSize();
	const v3dxVector3& Position = *reinterpret_cast< const v3dxVector3* >( Vert );

	UINT Hash = HashPosition( Position );
	UINT NewIndex;
	for( NewIndex = HashTable.First( Hash ); HashTable.IsValid( NewIndex ); NewIndex = HashTable.Next( NewIndex ) )
	{
		UINT i;
		for( i = 0; i < VertSize; i++ )
		{
			if( Vert[i] != Verts[ NewIndex * VertSize + i ] )
				break;
		}
		if( i == VertSize )
			break;
	}
	if( !HashTable.IsValid( NewIndex ) )
	{
		auto size = Verts.size();
		Verts.resize( VertSize + size);

		NewIndex = NumVerts++;
		HashTable.Add( Hash, NewIndex );

		memcpy( &GetPosition( NewIndex ), Vert, GetVertSize() * sizeof( float ) );
	}

	return NewIndex;
}

void QuarkCluster::Bound()
{
	Bounds = v3dxBox3();
	SurfaceArea = 0.0f;

	if (NumVerts == 0)
	{
		SphereBounds = v3dxSphere(v3dxVector3::ZERO, 0.0f);
		LODBounds = SphereBounds;
		return;
	}

	std::vector< v3dxVector3> Positions;
	Positions.resize(NumVerts);

	v3dxVector3 Centroid(0, 0, 0);
	for( UINT i = 0; i < NumVerts; i++ )
	{
		Positions[i] = GetPosition(i);
		Bounds += Positions[i];
		Centroid += Positions[i];
	}
	Centroid /= (float)NumVerts;

	// Compute bounding sphere using Ritter's algorithm
	// Start with centroid, find farthest point, then expand
	float MaxRadiusSq = 0.0f;
	for (UINT i = 0; i < NumVerts; i++)
	{
		float DistSq = (Positions[i] - Centroid).getLengthSq();
		MaxRadiusSq = std::max(MaxRadiusSq, DistSq);
	}
	SphereBounds = v3dxSphere(Centroid, Math::Sqrt(MaxRadiusSq));
	LODBounds = SphereBounds;

	float MaxEdgeLength2 = 0.0f;
	for( int i = 0; i < (int)Indexes.size(); i += 3 )
	{
		v3dxVector3 v[3];
		v[0] = GetPosition( Indexes[ i + 0 ] );
		v[1] = GetPosition( Indexes[ i + 1 ] );
		v[2] = GetPosition( Indexes[ i + 2 ] );

		v3dxVector3 Edge01 = v[1] - v[0];
		v3dxVector3 Edge12 = v[2] - v[1];
		v3dxVector3 Edge20 = v[0] - v[2];

		MaxEdgeLength2 = std::max( MaxEdgeLength2, Edge01.getLengthSq() );
		MaxEdgeLength2 = std::max( MaxEdgeLength2, Edge12.getLengthSq() );
		MaxEdgeLength2 = std::max( MaxEdgeLength2, Edge20.getLengthSq() );

		float TriArea = 0.5f * Edge01.crossProduct(Edge20).getLength();
		SurfaceArea += TriArea;
	}
	EdgeLength = Math::Sqrt( MaxEdgeLength2 );
}

static void SanitizeFloat( float& X, float MinValue, float MaxValue, float DefaultValue )
{
	if( X >= MinValue && X <= MaxValue )
		;
	else if( X < MinValue )
		X = MinValue;
	else if( X > MaxValue )
		X = MaxValue;
	else
		X = DefaultValue;
}

void QuarkCluster::SanitizeVertexData()
{
	const float FltThreshold = 1e12f;	// Fairly arbitrary threshold for sensible float values.
										// Should be large enough for all practical purposes, while still leaving enough headroom
										// so that overflows shouldn't be a concern.
										// With a 1e12 threshold, even x^3 fits comfortable in float range.

	for( UINT VertexIndex = 0; VertexIndex < NumVerts; VertexIndex++ )
	{
		v3dxVector3& Position = GetPosition( VertexIndex );
		SanitizeFloat( Position.X, -FltThreshold, FltThreshold, 0.0f );
		SanitizeFloat( Position.Y, -FltThreshold, FltThreshold, 0.0f );
		SanitizeFloat( Position.Z, -FltThreshold, FltThreshold, 0.0f );

//         v3dxVector3& Normal = GetNormal(VertexIndex);
//         if (!(Normal.X >= -FltThreshold && Normal.X <= FltThreshold &&
//             Normal.Y >= -FltThreshold && Normal.Y <= FltThreshold &&
//             Normal.Z >= -FltThreshold && Normal.Z <= FltThreshold))	// Don't flip condition. Intentionally written like this to be NaN-safe
//         {
//             Normal = v3dxVector3::UpVector;
//         }
// 		
// 		if( bHasColors )
// 		{
// 			FLinearColor& Color = GetColor( VertexIndex );
// 			SanitizeFloat( Color.R, 0.0f, 1.0f, 1.0f );
// 			SanitizeFloat( Color.G, 0.0f, 1.0f, 1.0f );
// 			SanitizeFloat( Color.B, 0.0f, 1.0f, 1.0f );
// 			SanitizeFloat( Color.A, 0.0f, 1.0f, 1.0f );
// 		}
// 
// 		FVector2f* UVs = GetUVs( VertexIndex );
// 		for( UINT UvIndex = 0; UvIndex < NumTexCoords; UvIndex++ )
// 		{
// 			SanitizeFloat( UVs[ UvIndex ].X, -FltThreshold, FltThreshold, 0.0f );
// 			SanitizeFloat( UVs[ UvIndex ].Y, -FltThreshold, FltThreshold, 0.0f );
// 		}
	}
}

NS_END