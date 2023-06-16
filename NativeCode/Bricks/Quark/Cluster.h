
#pragma once

#include "../../Base/IUnknown.h"

#include "TriangleUtil.h"
#include "../../Math/v3dxSphere.h"
#include "../../Math/v3dxBox3.h"

NS_BEGIN

class FGraphPartitioner;

struct FMaterialTriangle
{
	UINT32 Index0;
	UINT32 Index1;
	UINT32 Index2;
	UINT32 MaterialIndex;
	UINT32 RangeCount;
};

//struct FMaterialRange
//{
//	UINT32 RangeStart;
//	UINT32 RangeLength;
//	UINT32 MaterialIndex;
//	std::vector<uint8, TInlineAllocator<12>> BatchTriCounts;
//
//	friend FArchive& operator<<(FArchive& Ar, FMaterialRange& Range);
//};

//struct FStripDesc
//{
//	UINT32 Bitmasks[4][3];
//	UINT32 NumPrevRefVerticesBeforeDwords;
//	UINT32 NumPrevNewVerticesBeforeDwords;
//
//	friend FArchive& operator<<(FArchive& Ar, FStripDesc& Desc);
//};

class TR_CLASS()
    QuarkCluster 
{
public:
    //ENGINE_RTTI(QuarkCluster);

	QuarkCluster() {}
	QuarkCluster(
		const std::vector< v3dxVector3 >& InVerts,
		const std::vector< UINT32 >& InIndexes,
		//const std::vector < INT32 >& InMaterialIndexes,
		//UINT32 InNumTexCoords, bool bInHasColors, bool bInPreserveArea,
		UINT32 TriBegin, UINT32 TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );

    QuarkCluster( QuarkCluster& SrcCluster, UINT32 TriBegin, UINT32 TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );
    QuarkCluster(const std::vector<QuarkCluster*> & MergeList );

	float		Simplify( UINT32 TargetNumTris, float TargetError = 0.0f, UINT32 LimitNumTris = 0, bool bForNaniteFallback = false );
	FAdjacency	BuildAdjacency() const;
	void		Split( FGraphPartitioner& Partitioner, const FAdjacency& Adjacency ) const;
	void		Bound();

private:
	UINT32		AddVert( const float* Vert, FHashTable& HashTable );

public:
	UINT32				GetVertSize() const;
	v3dxVector3&			GetPosition( UINT32 VertIndex );
// 	float*				GetAttributes( UINT32 VertIndex );
// 	v3dxVector3&			GetNormal( UINT32 VertIndex );
// 	FLinearColor&		GetColor( UINT32 VertIndex );
// 	FVector2f*			GetUVs( UINT32 VertIndex );

	const v3dxVector3& GetPositionConst( UINT32 VertIndex ) const;
// 	const v3dxVector3&	GetNormal( UINT32 VertIndex ) const;
// 	const FLinearColor&	GetColor( UINT32 VertIndex ) const;
// 	const FVector2f*	GetUVs( UINT32 VertIndex ) const;

	void				SanitizeVertexData();

	//friend FArchive& operator<<(FArchive& Ar, QuarkCluster& Cluster);

	static const UINT32	ClusterSize = 128;

	UINT32		NumVerts = 0;
	UINT32		NumTris = 0;
	UINT32		NumTexCoords = 0;
	bool		bHasColors = false;
	bool		bPreserveArea = false;

	std::vector< float >		Verts;
	std::vector< UINT32 >	Indexes; // TODO: WORD replaced UINT32
	std::vector< INT32 >		MaterialIndexes;
	std::vector< INT8 >		ExternalEdges;
	UINT32				NumExternalEdges;

	std::map< UINT32, UINT32 >	AdjacentClusters;

	v3dxBox3	Bounds;
	//uint64		GUID = 0;
	INT32		MipLevel = 0;

	//FIntVector	QuantizedPosStart		= { 0u, 0u, 0u };
	//INT32		QuantizedPosPrecision	= 0u;
	//FIntVector  QuantizedPosBits		= { 0u, 0u, 0u };

	float		EdgeLength = 0.0f;
	float		LODError = 0.0f;
	float		SurfaceArea = 0.0f;
	
    //v3dxSphere	SphereBounds;
    //v3dxSphere	LODBounds;

// 	UINT32		GroupIndex			= MAX_uint32;
// 	UINT32		GroupPartIndex		= MAX_uint32;
// 	UINT32		GeneratingGroupIndex= MAX_uint32;
// 
// 	std::vector<FMaterialRange, TInlineAllocator<4>> MaterialRanges;
// 	std::vector<FIntVector>	QuantizedPositions;
// 
// 	FStripDesc		StripDesc;
// 	std::vector<uint8>	StripIndexData;
};

FORCEINLINE UINT32 QuarkCluster::GetVertSize() const
{
	//return 6 + ( bHasColors ? 4 : 0 ) + NumTexCoords * 2; // 3 pos, 3 normal
	return 3;
}

FORCEINLINE v3dxVector3& QuarkCluster::GetPosition( UINT32 VertIndex )
{
	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

FORCEINLINE const v3dxVector3& QuarkCluster::GetPositionConst( UINT32 VertIndex ) const
{
	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

// FORCEINLINE float* QuarkCluster::GetAttributes( UINT32 VertIndex )
// {
// 	return &Verts[ VertIndex * GetVertSize() + 3 ];
// }
// 
// FORCEINLINE v3dxVector3& QuarkCluster::GetNormal( UINT32 VertIndex )
// {
// 	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
// }
// 
// FORCEINLINE const v3dxVector3& QuarkCluster::GetNormal( UINT32 VertIndex ) const
// {
// 	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
// }
// 
// FORCEINLINE FLinearColor& QuarkCluster::GetColor( UINT32 VertIndex )
// {
// 	return *reinterpret_cast< FLinearColor* >( &Verts[ VertIndex * GetVertSize() + 6 ] );
// }
// 
// FORCEINLINE const FLinearColor& QuarkCluster::GetColor( UINT32 VertIndex ) const
// {
// 	return *reinterpret_cast< const FLinearColor* >( &Verts[ VertIndex * GetVertSize() + 6 ] );
// }
// 
// FORCEINLINE FVector2f* QuarkCluster::GetUVs( UINT32 VertIndex )
// {
// 	return reinterpret_cast< FVector2f* >( &Verts[ VertIndex * GetVertSize() + 6 + ( bHasColors ? 4 : 0 ) ] );
// }
// 
// FORCEINLINE const FVector2f* QuarkCluster::GetUVs( UINT32 VertIndex ) const
// {
// 	return reinterpret_cast< const FVector2f* >( &Verts[ VertIndex * GetVertSize() + 6 + ( bHasColors ? 4 : 0 ) ] );
// }


class TR_CLASS()
    ClusterBuilder : public IWeakReference
{
public:
	ClusterBuilder() {}
};


NS_END