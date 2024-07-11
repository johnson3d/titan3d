
#pragma once

#include "../../Base/IUnknown.h"

#include "TriangleUtil.h"
#include "../../Math/v3dxSphere.h"
#include "../../Math/v3dxBox3.h"

NS_BEGIN

class FGraphPartitioner;

struct FMaterialTriangle
{
	UINT Index0;
	UINT Index1;
	UINT Index2;
	UINT MaterialIndex;
	UINT RangeCount;
};

//struct FMaterialRange
//{
//	UINT RangeStart;
//	UINT RangeLength;
//	UINT MaterialIndex;
//	std::vector<uint8, TInlineAllocator<12>> BatchTriCounts;
//
//	friend FArchive& operator<<(FArchive& Ar, FMaterialRange& Range);
//};

//struct FStripDesc
//{
//	UINT Bitmasks[4][3];
//	UINT NumPrevRefVerticesBeforeDwords;
//	UINT NumPrevNewVerticesBeforeDwords;
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
		const std::vector< UINT >& InIndexes,
		//const std::vector < INT32 >& InMaterialIndexes,
		//UINT InNumTexCoords, bool bInHasColors, bool bInPreserveArea,
		UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );

    QuarkCluster( QuarkCluster& SrcCluster, UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );
    QuarkCluster(const std::vector<QuarkCluster*> & MergeList );

	float		Simplify( UINT TargetNumTris, float TargetError = 0.0f, UINT LimitNumTris = 0, bool bForNaniteFallback = false );
	FAdjacency	BuildAdjacency() const;
	void		Split( FGraphPartitioner& Partitioner, const FAdjacency& Adjacency ) const;
	void		Bound();

private:
	UINT		AddVert( const float* Vert, FHashTable& HashTable );

public:
	UINT				GetVertSize() const;
	v3dxVector3&			GetPosition( UINT VertIndex );
// 	float*				GetAttributes( UINT VertIndex );
// 	v3dxVector3&			GetNormal( UINT VertIndex );
// 	FLinearColor&		GetColor( UINT VertIndex );
// 	FVector2f*			GetUVs( UINT VertIndex );

	const v3dxVector3& GetPositionConst( UINT VertIndex ) const;
// 	const v3dxVector3&	GetNormal( UINT VertIndex ) const;
// 	const FLinearColor&	GetColor( UINT VertIndex ) const;
// 	const FVector2f*	GetUVs( UINT VertIndex ) const;

	void				SanitizeVertexData();

	//friend FArchive& operator<<(FArchive& Ar, QuarkCluster& Cluster);

	static const UINT	ClusterSize = 128;

	UINT		NumVerts = 0;
	UINT		NumTris = 0;
	UINT		NumTexCoords = 0;
	bool		bHasColors = false;
	bool		bPreserveArea = false;

	std::vector< float >		Verts;
	std::vector< UINT >	Indexes; // TODO: WORD replaced UINT
	std::vector< INT32 >		MaterialIndexes;
	std::vector< INT8 >		ExternalEdges;
	UINT				NumExternalEdges;

	std::map< UINT, UINT >	AdjacentClusters;

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

// 	UINT		GroupIndex			= MAX_uint32;
// 	UINT		GroupPartIndex		= MAX_uint32;
// 	UINT		GeneratingGroupIndex= MAX_uint32;
// 
// 	std::vector<FMaterialRange, TInlineAllocator<4>> MaterialRanges;
// 	std::vector<FIntVector>	QuantizedPositions;
// 
// 	FStripDesc		StripDesc;
// 	std::vector<uint8>	StripIndexData;

	// export members
    int VertexStart;
    int VertexCount;
    int IndexStart;
    int IndexCount;
};

inline UINT QuarkCluster::GetVertSize() const
{
	//return 6 + ( bHasColors ? 4 : 0 ) + NumTexCoords * 2; // 3 pos, 3 normal
	return 3;
}

inline v3dxVector3& QuarkCluster::GetPosition( UINT VertIndex )
{
	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

inline const v3dxVector3& QuarkCluster::GetPositionConst( UINT VertIndex ) const
{
	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

// inline float* QuarkCluster::GetAttributes( UINT VertIndex )
// {
// 	return &Verts[ VertIndex * GetVertSize() + 3 ];
// }
// 
// inline v3dxVector3& QuarkCluster::GetNormal( UINT VertIndex )
// {
// 	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
// }
// 
// inline const v3dxVector3& QuarkCluster::GetNormal( UINT VertIndex ) const
// {
// 	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
// }
// 
// inline FLinearColor& QuarkCluster::GetColor( UINT VertIndex )
// {
// 	return *reinterpret_cast< FLinearColor* >( &Verts[ VertIndex * GetVertSize() + 6 ] );
// }
// 
// inline const FLinearColor& QuarkCluster::GetColor( UINT VertIndex ) const
// {
// 	return *reinterpret_cast< const FLinearColor* >( &Verts[ VertIndex * GetVertSize() + 6 ] );
// }
// 
// inline FVector2f* QuarkCluster::GetUVs( UINT VertIndex )
// {
// 	return reinterpret_cast< FVector2f* >( &Verts[ VertIndex * GetVertSize() + 6 + ( bHasColors ? 4 : 0 ) ] );
// }
// 
// inline const FVector2f* QuarkCluster::GetUVs( UINT VertIndex ) const
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