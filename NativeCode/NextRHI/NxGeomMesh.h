#pragma once
#include "NxGpuDevice.h"
#include "NxBuffer.h"
#include "NxShader.h"
#include "NxRHIDefine.h"

#include "../Base/timekeys/TimeKeys.h"
#include "../Math/v3dxVector2.h"
#include "../Math/v3dxVector3.h"
#include "../Math/v3dxBox3.h"
#include "../Base/xnd/vfxxnd.h"

#include "../Bricks/Quark/DisjointSet.h"
#include "../Bricks/Quark/GraphPartitioner.h"
#include "../Bricks/Quark/Cluster.h"
#include "../Bricks/Quark/ClusterDAG.h"

struct VHitResult;

NS_BEGIN

namespace NxRHI
{
	class IInputLayout;
	
	enum TR_ENUM()
		EVertexShaderOutType
	{
		VOT_Position,
			VOT_Normal,//float3
			VOT_Color,
			VOT_UV,//float2
			VOT_WorldPos,//float3
			VOT_Tangent,
			VOT_Lightmap,

			VOT_F4_1,//uint4
			VOT_F4_2,
			VOT_F4_3,

			VOT_Custom0,
			VOT_Custom1,
			VOT_Custom2,
			VOT_Custom3,
			VOT_Custom4,

			VOT_Special,//uint4
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FMeshAtomDesc
	{
		FMeshAtomDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			PrimitiveType = EPT_TriangleList;
			BaseVertexIndex = 0;
			StartIndex = 0;
			NumPrimitives = 0;
			NumInstances = 1;

			DispatchMeshX = 0;
			DispatchMeshY = 0;
			DispatchMeshZ = 0;
		}
		EPrimitiveType PrimitiveType = EPT_TriangleList;
		UINT BaseVertexIndex = 0;
		UINT StartIndex = 0;
		UINT NumPrimitives = 0;
		UINT NumInstances = 1;

		UINT DispatchMeshX = 0;
		UINT DispatchMeshY = 0;
		UINT DispatchMeshZ = 0;
		bool IsIndexDraw() const {
			return StartIndex != 0xFFFFFFFF;
		}
		bool IsDispatchMesh()
		{
			return DispatchMeshX + DispatchMeshY + DispatchMeshZ > 0;
		}
	};
	class TR_CLASS()
		FVertexArray : public IGpuResource
	{
	public:
		virtual void Commit(ICommandList * cmdlist);
		virtual void BindVB(EVertexStreamType stream, IVbView* buffer);
		IVbView* GetVB(EVertexStreamType stream)
		{
			return VertexBuffers[stream];
		}
		void Reset()
		{
			for (int i = 0; i < VST_Number; i++)
			{
				VertexBuffers[i] = nullptr;
			}
		}
		static void GetStreamInfo(EVertexStreamType type, UINT* stride = nullptr, UINT* element = nullptr, int* varType = nullptr);
	public:
		AutoRef<IVbView>			VertexBuffers[VST_Number];
	};
	class TR_CLASS()
		FGeomMesh : public VIUnknown
	{
	public:
		void Reset(bool bClearBuffer);
		void Commit(ICommandList * cmdlist);
		UINT GetAtomNum() {
			return (UINT)Atoms.size();
		}
		void SetAtomNum(UINT size) {
			Atoms.resize(size);
		}
		FMeshAtomDesc* GetAtomDesc(UINT index, UINT lod) {
			return &Atoms[index][lod];
		}
		void SetAtomDesc(UINT index, UINT lod, const FMeshAtomDesc& desc) {
			Atoms[index][lod] = desc;
		}
		void PushAtomDesc(UINT index, const FMeshAtomDesc& desc) {
			Atoms[index].push_back(desc);
		}
		
		void BindVertexArray(FVertexArray* va) {
			VertexArray = va;
		}
		void BindIndexBuffer(IIbView* buffer);
		FVertexArray* GetVertexArray() {
			return VertexArray;
		}
		
		AutoRef<IVbView> GetVertexBuffer(EVertexStreamType type) {
			return VertexArray->VertexBuffers[type];
		}
		AutoRef<IIbView> GetIndexBuffer() {
			return IndexBuffer;
		}
	public:
		bool						IsIndex32 = false;
		AutoRef<FVertexArray>		VertexArray;
		AutoRef<IIbView>			IndexBuffer;
		std::vector<std::vector<FMeshAtomDesc>>	Atoms;
	};

	class TR_CLASS()
		FTransientBuffer : public IResourceBase
	{
	public:
		FTransientBuffer() {}
		void Initialize(IGpuDevice* device, UINT size, EBufferType type, EGpuUsage usage, ECpuAccess cpuAccess);
		UINT Alloc(IGpuDevice* device, UINT size, bool bGrow);
		IVbView* AllocVBV(IGpuDevice* device, UINT stride, UINT size, bool bGrow);
		IIbView* AllocIBV(IGpuDevice* device, UINT stride, UINT size, bool bGrow);
		void Reset()
		{
			mCurrentOffset = 0;
		}
		IBuffer* GetBuffer() {
			return mBuffer;
		}
	protected:
		AutoRef<IBuffer>			mBuffer;
		UINT						mCurrentOffset = 0;
	};

	class FMeshDataProvider;
	class TR_CLASS()
		FMeshPrimitives : public IResourceBase
	{
		friend FMeshDataProvider;
	public:
		struct FModelDesc
		{
			void SetDefault() {
				Flags = 0;
				UnUsed = 0;
				VertexNumber = 0;
				GeoTabeNumber = 0;
				PolyNumber = 0;
				AtomNumber = 0;
			}
			DWORD				Flags;
			DWORD				UnUsed;
			UINT				VertexNumber;
			UINT				GeoTabeNumber;
			UINT				PolyNumber;
			UINT				AtomNumber;
		};
		ENGINE_RTTI(FMeshPrimitives);

		FMeshPrimitives();
		~FMeshPrimitives();

		void Reset(bool bClearBuffer);

		virtual FResourceState* GetResourceState() override {
			return &mResourceState;
		}
		virtual void InvalidateResource() override;
		virtual bool RestoreResource(IWeakRefObject* pDevice) override;

		bool Init(IGpuDevice* device, const char* name, UINT atom);
		bool Init(IGpuDevice* device, FGeomMesh* mesh, const v3dxBox3 * aabb);
		void SetTransientVertexBuffer(FTransientBuffer* buffer)
		{
			mVertexBuffer = buffer;
		}
		void SetTransientIndexBuffer(FTransientBuffer* buffer)
		{
			mIndexBuffer = buffer;
		}
		bool LoadXnd(IGpuDevice* device, const char* name, XndHolder * xnd, bool isLoad);
		void Save2Xnd(IGpuDevice* device, XndNode * node);

		bool RefreshResource(IGpuDevice* device, const char* name, XndNode * node);
		
		FGeomMesh* GetGeomtryMesh() const {
			return mGeometryMesh;
		}
		UINT GetVertexNumber() const {
			return mDesc.VertexNumber;
		}
		UINT GetPrimitiveNumber() const {
			return mDesc.PolyNumber;
		}

		const char* GetName() const;
		UINT GetAtomNumber() const;
		void SetAtomNumber(UINT count) {
			mGeometryMesh->Atoms.resize(count);
		}
		FMeshAtomDesc* GetAtom(UINT index, UINT lod) const;
		void SetAtom(UINT index, UINT lod, const FMeshAtomDesc& desc);
		void PushAtom(UINT index, const FMeshAtomDesc& desc) {
			mGeometryMesh->PushAtomDesc(index, desc);
		}

		bool SetGeomtryMeshStream(ICommandList* cmd, EVertexStreamType stream, void* data, UINT size, UINT stride, ECpuAccess cpuAccess);
		bool SetGeomtryMeshIndex(ICommandList* cmd, void* data, UINT size, bool isBit32, ECpuAccess cpuAccess);
		void SetAABB(v3dxBox3 & aabb)
		{
			mAABB = aabb;
		}
		static void CalcNormals32(OUT std::vector<v3dxVector3>&normals, const v3dxVector3 * pos, UINT nVert, const UINT * triangles, UINT nTri);
		static void CalcNormals16(OUT std::vector<v3dxVector3>&normals, const v3dxVector3 * pos, UINT nVert, const USHORT * triangles, UINT nTri);

		void ValidAtomExtData()
		{
			mAtomExtData.resize(mGeometryMesh->Atoms.size());
		}
		void SetAtomExtData(UINT index, VIUnknown* data) {
			if (index >= (UINT)mAtomExtData.size())
				return;
			mAtomExtData[index] = data;
		}
		VIUnknown* GetAtomExtData(UINT index) {
			if (index >= (UINT)mAtomExtData.size())
				return nullptr;
			return mAtomExtData[index];
		}
		// cluster interfaces
		int ClusterizeTriangles(IGpuDevice* device);
		int BuildQuarkDAG(IGpuDevice* device);
		int BuildQuarkDAGEx(IGpuDevice* device, UINT maxGroupSize, UINT clusterSize = 128);
		bool SaveClusters(XndNode* pNode);
		int LoadClusters(XndHolder* xnd, IGpuDevice* device);
		QuarkCluster* GetCluster(int index);
		UINT GetClusterCount() const { return (UINT)mClusters.size(); }
		UINT GetDAGMipLevels() const { return mDAGMipLevels; }
		float GetClusterLODError(int index) const;
		int GetClusterMipLevel(int index) const;

		// BVH traversal LOD selection: returns the number of selected clusters
		// cameraPos: camera world position
		// screenHeight: viewport height in pixels
		// fov: camera field of view in radians
		// errorThreshold: screen-space error threshold in pixels
		// outClusterIndices: output buffer for selected cluster indices
		// maxCount: max capacity of outClusterIndices
		UINT SelectClustersForLOD(
			const v3dxVector3& cameraPos,
			float screenHeight, float fov,
			float errorThreshold,
			UINT* outClusterIndices, UINT maxCount);

		// GPU LOD selection: export flattened group hierarchy data
		void GetDAGExportSizes(UINT& outGroupCount, UINT& outChildrenTotal,
			UINT& outParentsTotal, UINT& outClusterCount, UINT& outRootGroupCount) const;
		UINT ExportDAGGroupsForGPU(
			void* outGroups, UINT maxGroups,
			UINT* outChildrenIndices, UINT maxChildren,
			UINT* outParentsIndices, UINT maxParents,
			UINT* outClusterGroupMap, UINT maxClusters,
			UINT* outRootGroupIndices, UINT maxRootGroups,
			UINT& outChildrenTotal, UINT& outParentsTotal, UINT& outRootGroupCount) const;
		UINT GetDAGGroupCount() const { return (UINT)mDAGGroups.size(); }
		
		float* GetClustersVB()
		{
			return &mClustersVB[0];
		}
		UINT* GetClustersIB()
		{
			return &mClustersIB[0];
		}
		UINT GetClustersVBCount()
		{
			UINT stride = GetClustersVBStride();
			return stride > 0 ? (UINT)(mClustersVB.size() / stride) : 0;
		}
		UINT GetClustersVBStride()
		{
			return mClusters.empty() ? 8 : mClusters[0].mVertStride;
		}
		bool GetClustersHasTangents()
		{
			return GetClustersVBStride() == 12;
		}
		UINT GetClustersIBCount()
		{
			return (UINT)mClustersIB.size();
		}
	private:
		AutoRef<IVbView> LoadVB(IGpuDevice* device, XndAttribute * pAttr, UINT stride, TimeKeys & tkeys, UINT & resSize, EVertexStreamType stream);
		void SaveVB(IGpuDevice* device, XndAttribute * pAttr, IVbView* vb, TimeKeys & tkeys, UINT stride);

		bool GetMeshBuffer(IGpuDevice* device, std::vector<v3dxVector3>& Verts, std::vector<UINT>& Indexes);
	protected:
		std::string				mName;
		AutoRef<FGeomMesh>		mGeometryMesh;
		std::vector<AutoRef<VIUnknown>> mAtomExtData;
		TimeKeys				mMopherKeys[VST_Number];

		AutoRef<XndHolder>		mXnd;
		FModelDesc				mDesc;
		v3dxBox3				mAABB;
		TR_MEMBER(SV_NoBind)
		FResourceState			mResourceState;

		// cluster relative
		std::vector<QuarkCluster> mClusters;
		std::vector<FClusterGroup> mDAGGroups; // DAG group hierarchy for BVH traversal
		std::vector<float> mClustersVB; // stride=8 floats (pos3+normal3+uv2) per vertex
		std::vector<UINT> mClustersIB;
		UINT mDAGMipLevels = 0;

        AutoRef<FVertexArray>		mClustersVertexArray;
        AutoRef<IIbView>			mClustersIndexView;

		AutoRef<FTransientBuffer>	mVertexBuffer;
		AutoRef<FTransientBuffer>	mIndexBuffer;
	public:
		struct FStreamTypeInfo
		{
			const char* XndName = nullptr;
			int Stride = 0;
		};
		static inline FStreamTypeInfo GetStreamTypeInfo(EVertexStreamType type)
		{
			FStreamTypeInfo result;
			switch (type)
			{
			case VST_Position:
			{
				result.XndName = "Position";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_Normal:
			{
				result.XndName = "Normal";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_Tangent:
			{
				result.XndName = "Tangent";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_Color:
			{
				result.XndName = "VertexColor";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_UV:
			{
				result.XndName = "DiffuseUV";
				result.Stride = sizeof(v3dxVector2);
				break;
			}
			case VST_LightMap:
			{
				result.XndName = "LightMapUV";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_SkinIndex:
			{
				result.XndName = "BlendIndex";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_SkinWeight:
			{
				result.XndName = "BlendWeight";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_TerrainIndex:
			{
				result.XndName = "Fix_VIDTerrain";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_TerrainGradient:
			{
				result.XndName = "TerrainGradient";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_InstPos:
			{
				result.XndName = nullptr;// "InstPos";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_InstQuat:
			{
				result.XndName = nullptr;// "InstQuat";
				result.Stride = sizeof(v3dxQuaternion);
				break;
			}
			case VST_InstScale:
			{
				result.XndName = nullptr;// "InstScale";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_F4_1:
			{
				result.XndName = nullptr;// "F41";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_F4_2:
			{
				result.XndName = nullptr;// "F42";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_F4_3:
			{
				result.XndName = nullptr;// "F43";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			default:
				break;
			}

			return result;
		}
	};
}

NS_END

