
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
		const std::vector< v3dxVector3 >& InNormals,
		const std::vector< float >& InTangents,   // float4 per vertex (xyz + sign), empty = no tangent
		const std::vector< float >& InUVs,
		const std::vector< UINT >& InIndexes,
		const std::vector< INT32 >& InMaterialIndexes,
		UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );

    QuarkCluster( QuarkCluster& SrcCluster, UINT TriBegin, UINT TriEnd, const FGraphPartitioner& Partitioner, const FAdjacency& Adjacency );
    QuarkCluster(const std::vector<QuarkCluster*> & MergeList );

	float		Simplify( UINT TargetNumTris, float TargetError = 0.0f, UINT LimitNumTris = 0, bool bForNaniteFallback = false );
	FAdjacency	BuildAdjacency() const;
	void		Split( FGraphPartitioner& Partitioner, const FAdjacency& Adjacency, UINT InClusterSize = ClusterSize ) const;
	void		Bound();

private:
	UINT		AddVert( const float* Vert, FHashTable& HashTable );

public:
	UINT			GetVertSize() const;
	v3dxVector3&		GetPosition( UINT VertIndex );
	v3dxVector3&		GetNormal( UINT VertIndex );
	float*				GetTangent( UINT VertIndex );       // offset 6, only valid when mVertStride==12
	float*				GetUVs( UINT VertIndex );           // offset 6 (stride=8) or 10 (stride=12)

	const v3dxVector3& GetPositionConst( UINT VertIndex ) const;
	const v3dxVector3&	GetNormalConst( UINT VertIndex ) const;
	const float*		GetTangentConst( UINT VertIndex ) const;
	const float*		GetUVsConst( UINT VertIndex ) const;

	void				SanitizeVertexData();

	//friend FArchive& operator<<(FArchive& Ar, QuarkCluster& Cluster);

	static const UINT	ClusterSize = 128;

	UINT		mVertStride = 8;  // 8 (no tangent) or 12 (with tangent)
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

	v3dxSphere	SphereBounds;
	v3dxSphere	LODBounds;

	UINT		GroupIndex = ~0u;
	UINT		GroupPartIndex = ~0u;
	UINT		GeneratingGroupIndex = ~0u;

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
    int VertexStart = 0;
    int VertexCount = 0;
    int IndexStart = 0;
    int IndexCount = 0;
    int PrimaryMaterialID = 0;
};

inline UINT QuarkCluster::GetVertSize() const
{
	return mVertStride; // 8 (pos3+normal3+uv2) or 12 (pos3+normal3+tangent4+uv2)
}

inline v3dxVector3& QuarkCluster::GetPosition( UINT VertIndex )
{
	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

inline const v3dxVector3& QuarkCluster::GetPositionConst( UINT VertIndex ) const
{
	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() ] );
}

inline v3dxVector3& QuarkCluster::GetNormal( UINT VertIndex )
{
	return *reinterpret_cast< v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
}

inline const v3dxVector3& QuarkCluster::GetNormalConst( UINT VertIndex ) const
{
	return *reinterpret_cast< const v3dxVector3* >( &Verts[ VertIndex * GetVertSize() + 3 ] );
}

// Tangent: float4 at offset 6 (only valid when mVertStride==12)
inline float* QuarkCluster::GetTangent( UINT VertIndex )
{
	return &Verts[ VertIndex * GetVertSize() + 6 ];
}

inline const float* QuarkCluster::GetTangentConst( UINT VertIndex ) const
{
	return &Verts[ VertIndex * GetVertSize() + 6 ];
}

// UV: offset 6 (stride=8, no tangent) or offset 10 (stride=12, after tangent4)
inline float* QuarkCluster::GetUVs( UINT VertIndex )
{
	UINT uvOffset = (mVertStride == 12) ? 10 : 6;
	return &Verts[ VertIndex * GetVertSize() + uvOffset ];
}

inline const float* QuarkCluster::GetUVsConst( UINT VertIndex ) const
{
	UINT uvOffset = (mVertStride == 12) ? 10 : 6;
	return &Verts[ VertIndex * GetVertSize() + uvOffset ];
}


class TR_CLASS()
    ClusterBuilder : public IWeakRefObject
{
public:
	ClusterBuilder() {}
};


NS_END