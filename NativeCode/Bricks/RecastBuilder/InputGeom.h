#pragma once

#include "Recast.h"
#include "ChunkyTriMesh.h"
#include "MeshLoaderObj.h"

#include "RecastCommon.h"

NS_BEGIN

static const int MAX_CONVEXVOL_PTS = 12;
struct ConvexVolume
{
	float verts[MAX_CONVEXVOL_PTS*3];
	float hmin, hmax;
	int nverts;
	int area;
};

class TR_CLASS()
	InputGeom : public VIUnknown
{
public:
	std::vector<v3dVector3_t> WalkDatas;
private:
	rcChunkyTriMesh* m_chunkyMesh;
	rcMeshLoaderObj* m_mesh;
	float m_meshBMin[3], m_meshBMax[3];
	
	/// @name Off-Mesh connections.
	///@{
	static const int MAX_OFFMESH_CONNECTIONS = 256;
	float m_offMeshConVerts[MAX_OFFMESH_CONNECTIONS*3*2];
	float m_offMeshConRads[MAX_OFFMESH_CONNECTIONS];
	unsigned char m_offMeshConDirs[MAX_OFFMESH_CONNECTIONS];
	unsigned char m_offMeshConAreas[MAX_OFFMESH_CONNECTIONS];
	unsigned short m_offMeshConFlags[MAX_OFFMESH_CONNECTIONS];
	unsigned int m_offMeshConId[MAX_OFFMESH_CONNECTIONS];
	int m_offMeshConCount;
	///@}

	/// @name Convex Volumes.
	///@{
	static const int MAX_VOLUMES = 256;
	ConvexVolume m_volumes[MAX_VOLUMES];
	int m_volumeCount;
	///@}

	//int m_areaType;
	float m_polyOffset;
	float m_boxHeight;
	float m_boxDescent;

	static const int MAX_PTS = 12;
	float m_pts[MAX_PTS * 3];
	int m_npts;
	int m_hull[MAX_PTS];
	int m_nhull;

public:
	ENGINE_RTTI(InputGeom);

	enum EAreaType
	{
		NoWalk = 0,
			Walk = 64,
	};

	InputGeom();
	~InputGeom();
	bool LoadMesh(NxRHI::FMeshDataProvider* mesh, float scale);

	bool loadMesh(rcContext* ctx, NxRHI::FMeshDataProvider* mesh, float scale);
	bool IsInWalkArea(const v3dxVector3& min, const v3dxVector3& max);
	void CreateConvexVolumes(EAreaType areatype, IBlobObject* blob, const v3dxVector3& min, const v3dxVector3& max);
	void DeleteConvexVolumesByArea(EAreaType areatype);
	void ClearConvexVolumes();
	
	//bool load(class rcContext* ctx, const std::string& filepath);
	//bool saveGeomSet(const BuildSettings* settings);
	
	/// Method to return static mesh data.
	const rcMeshLoaderObj* getMesh() const { return m_mesh; }
	const float* getMeshBoundsMin() const { return m_meshBMin; }
	const float* getMeshBoundsMax() const { return m_meshBMax; }
	const float* getNavMeshBoundsMin() const { return m_meshBMin; }
	const float* getNavMeshBoundsMax() const { return m_meshBMax; }
	const rcChunkyTriMesh* getChunkyMesh() const { return m_chunkyMesh; }
	bool raycastMesh(float* src, float* dst, float& tmin);

	/// @name Off-Mesh connections.
	///@{
	int getOffMeshConnectionCount() const { return m_offMeshConCount; }
	const float* getOffMeshConnectionVerts() const { return m_offMeshConVerts; }
	const float* getOffMeshConnectionRads() const { return m_offMeshConRads; }
	const unsigned char* getOffMeshConnectionDirs() const { return m_offMeshConDirs; }
	const unsigned char* getOffMeshConnectionAreas() const { return m_offMeshConAreas; }
	const unsigned short* getOffMeshConnectionFlags() const { return m_offMeshConFlags; }
	const unsigned int* getOffMeshConnectionId() const { return m_offMeshConId; }
	void addOffMeshConnection(const float* spos, const float* epos, const float rad,
							  unsigned char bidir, unsigned char area, unsigned short flags);
	void deleteOffMeshConnection(int i);

	void CSAddOffMeshConnection(v3dxVector3 startpos, v3dxVector3 endpos, float radius, int dir);
	///@}

	/// @name Box Volumes.
	///@{
	int getConvexVolumeCount() const { return m_volumeCount; }
	const ConvexVolume* getConvexVolumes() const { return m_volumes; }
	void addConvexVolume(const float* verts, const int nverts,
						 const float minh, const float maxh, unsigned char area);
	void deleteConvexVolume(int i);
	///@}
	
private:
	// Explicitly disabled copy constructor and copy assignment operator.
	InputGeom(const InputGeom&);
	InputGeom& operator=(const InputGeom&);
};

NS_END