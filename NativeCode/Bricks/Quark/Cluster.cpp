

#include "HashTable.h"
#include "Cluster.h"
#include "GraphPartitioner.h"

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
	const std::vector< UINT32 >& InIndexes,
	//const std::vector< INT32 >& InMaterialIndexes,
	//UINT32 InNumTexCoords, bool bInHasColors, bool bInPreserveArea,
	UINT32 TriBegin, UINT32 TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency )
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

	std::map< UINT32, UINT32 > OldToNewIndex;
	//OldToNewIndex.reserve( NumTris );

	for( UINT32 i = TriBegin; i < TriEnd; i++ )
	{
		UINT32 TriIndex = Partitioner.Indexes[i];

		for( UINT32 k = 0; k < 3; k++ )
		{
			UINT32 OldIndex = InIndexes[ TriIndex * 3 + k ];
            UINT32 NewIndex = ~0u;

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
                //for( UINT32 UVIndex = 0; UVIndex < NumTexCoords; UVIndex++ )
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
					UINT32 AdjTri = Partitioner.SortedTo[ AdjIndex / 3 ];
					if( AdjTri < TriBegin || AdjTri >= TriEnd )
						AdjCount++;
				} );

			ExternalEdges.push_back( AdjCount );
			NumExternalEdges += AdjCount != 0 ? 1 : 0;
		}

		//MaterialIndexes.push_back( InMaterialIndexes[ TriIndex ] );
	}

	SanitizeVertexData();

    //for( UINT32 VertexIndex = 0; VertexIndex < NumVerts; VertexIndex++ )
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
QuarkCluster::QuarkCluster( QuarkCluster& SrcCluster, UINT32 TriBegin, UINT32 TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency )
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

	std::map< UINT32, UINT32 > OldToNewIndex;
	//OldToNewIndex.Reserve( NumTris );

	for( UINT32 i = TriBegin; i < TriEnd; i++ )
	{
		UINT32 TriIndex = Partitioner.Indexes[i];

		for( UINT32 k = 0; k < 3; k++ )
		{
			UINT32 OldIndex = SrcCluster.Indexes[ TriIndex * 3 + k ];
            UINT32 NewIndex = ~0u;

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
					UINT32 AdjTri = Partitioner.SortedTo[ AdjIndex / 3 ];
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
	ASSERT(false);
}

float QuarkCluster::Simplify( UINT32 TargetNumTris, float TargetError, UINT32 LimitNumTris, bool bForNaniteFallback )
{
	ASSERT(false);
	return 0.0;
}

void QuarkCluster::Split( FGraphPartitioner& Partitioner, const FAdjacency& Adjacency ) const
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

	auto GetCenter = [ this ]( UINT32 TriIndex, v3dxVector3& Center)
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

	for( UINT32 i = 0; i < NumTris; i++ )
	{
		Graph->AdjacencyOffset[i] = INT32(Graph->Adjacency.size());

		UINT32 TriIndex = Partitioner.Indexes[i];

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

	Partitioner.PartitionStrict( Graph, ClusterSize - 4, ClusterSize, false );
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

UINT32 QuarkCluster::AddVert( const float* Vert, FHashTable& HashTable )
{
	const UINT32 VertSize = GetVertSize();
	const v3dxVector3& Position = *reinterpret_cast< const v3dxVector3* >( Vert );

	UINT32 Hash = HashPosition( Position );
	UINT32 NewIndex;
	for( NewIndex = HashTable.First( Hash ); HashTable.IsValid( NewIndex ); NewIndex = HashTable.Next( NewIndex ) )
	{
		UINT32 i;
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
	
	std::vector< v3dxVector3> Positions;
	Positions.resize(NumVerts);

	for( UINT32 i = 0; i < NumVerts; i++ )
	{
		Positions[i] = GetPosition(i);
		Bounds += Positions[i];
	}
	//SphereBounds = v3dxSphere( Positions.GetData(), Positions.size() );
	//LODBounds = SphereBounds;
	
	float MaxEdgeLength2 = 0.0f;
	for( int i = 0; i < Indexes.size(); i += 3 )
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

	for( UINT32 VertexIndex = 0; VertexIndex < NumVerts; VertexIndex++ )
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
// 		for( UINT32 UvIndex = 0; UvIndex < NumTexCoords; UvIndex++ )
// 		{
// 			SanitizeFloat( UVs[ UvIndex ].X, -FltThreshold, FltThreshold, 0.0f );
// 			SanitizeFloat( UVs[ UvIndex ].Y, -FltThreshold, FltThreshold, 0.0f );
// 		}
	}
}

NS_END