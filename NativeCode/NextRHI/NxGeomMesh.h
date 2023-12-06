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
		}
		EPrimitiveType PrimitiveType = EPT_TriangleList;
		UINT BaseVertexIndex = 0;
		UINT StartIndex = 0;
		UINT NumPrimitives = 0;
		UINT NumInstances = 1;
		bool IsIndexDraw() const {
			return StartIndex != 0xFFFFFFFF;
		}
	};
	class TR_CLASS()
		FVertexArray : public VIUnknown
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
		virtual bool RestoreResource(IWeakReference* pDevice) override;

		bool Init(IGpuDevice* device, const char* name, UINT atom);
		bool Init(IGpuDevice* device, FGeomMesh* mesh, const v3dxBox3 * aabb);
		bool LoadXnd(IGpuDevice* device, const char* name, XndHolder * xnd, bool isLoad);
		void Save2Xnd(IGpuDevice* device, XndNode * node);

		bool RefreshResource(IGpuDevice* device, const char* name, XndNode * node);
		
		FGeomMesh* GetGeomtryMesh() const {
			return mGeometryMesh;
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
		bool SaveClusters(XndNode* pNode);
		int LoadClusters(XndHolder* xnd, IGpuDevice* device);
		QuarkCluster* GetCluster(int index);
		
		v3dxVector3* GetClustersVB()
		{
			return &mClustersVB[0];
		}
		UINT32* GetClustersIB()
		{
			return &mClustersIB[0];
		}
		UINT32 GetClustersVBCount()
		{
			return (UINT32)mClustersVB.size();
		}
		UINT32 GetClustersIBCount()
		{
			return (UINT32)mClustersIB.size();
		}
	private:
		AutoRef<IVbView> LoadVB(IGpuDevice* device, XndAttribute * pAttr, UINT stride, TimeKeys & tkeys, UINT & resSize, EVertexStreamType stream);
		void SaveVB(IGpuDevice* device, XndAttribute * pAttr, IVbView* vb, TimeKeys & tkeys, UINT stride);

		bool GetMeshBuffer(IGpuDevice* device, std::vector<v3dxVector3>& Verts, std::vector<UINT32>& Indexes);
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
		std::vector<v3dxVector3> mClustersVB;
		std::vector<UINT32> mClustersIB;

        AutoRef<FVertexArray>		mClustersVertexArray;
        AutoRef<IIbView>			mClustersIndexView;
	};
	
	struct TR_CLASS(SV_LayoutStruct = 8)
		FMeshVertex
	{
		v3dxVector3 Position;
		v3dxVector3 Normal;
		v3dVector4_t Tangent;
		DWORD Color;
		v3dxVector2 UV;
		v3dVector4_t LightMap;
		DWORD SkinIndex;
		v3dVector4_t SkinWeight;
	};
	class TR_CLASS()
		FMeshDataProvider : public IWeakReference
	{
	public:
		std::vector<std::vector<FMeshAtomDesc>>	mAtoms;
		std::vector<AutoRef<VIUnknown>>		mAtomExtDatas;
		v3dxBox3				mAABB;
		AutoRef<IBlobObject>	mVertexBuffers[VST_Number];
		AutoRef<IBlobObject>	IndexBuffer;
		AutoRef<IBlobObject>	FaceBuffer = nullptr;
		bool					IsIndex32 = false;
		UINT					VertexNumber = 0;
		UINT					PrimitiveNumber = 0;

		DWORD					StreamTypes = 0;
		int						AtomSize = 0;
	public:
		ENGINE_RTTI(FMeshDataProvider);

		FMeshDataProvider();
		~FMeshDataProvider();
		virtual void Cleanup() override;

		void Reset();

		bool InitFromMesh(IGpuDevice* device, FMeshPrimitives* mesh);
		bool Init(DWORD streams, bool isIndex32, int atom);
		bool Init();

		bool LoadFromMeshPrimitive(XndNode * pNode, EVertexStreamType streams);

		void GetAABB(v3dxBox3 * box) {
			*box = mAABB;
		}
		void SetAABB(v3dxBox3 * box) {
			mAABB = *box;
		}
		UINT GetVertexNumber() const;
		UINT GetPrimitiveNumber() const;
		UINT GetAtomNumber() const;
		IBlobObject* CreateStream(EVertexStreamType index);
		IBlobObject* GetStream(EVertexStreamType index);
		IBlobObject* GetIndices();

		FMeshAtomDesc* GetAtom(UINT index, UINT lod);
		VIUnknown* GetAtomExtData(UINT index) const{
			if (index >= mAtomExtDatas.size())
				return nullptr;
			return mAtomExtDatas[index];
		}
		void PushAtom(const FMeshAtomDesc* pDescLODs, UINT count, VIUnknown* ext);
		bool SetAtom(UINT index, UINT lod, const FMeshAtomDesc& desc);
		void PushAtomLOD(UINT index, const FMeshAtomDesc& desc);
		UINT GetAtomLOD(UINT index);
		bool GetTriangle(int index, UINT * vA, UINT * vB, UINT * vC);
		bool GetAtomTriangle(UINT atom, UINT index, UINT * vA, UINT * vB, UINT * vC);
		int IntersectTriangle(const v3dxVector3 * scale, const v3dxVector3 * vStart, const v3dxVector3 * vEnd, VHitResult * result);
		void* GetVertexPtr(EVertexStreamType stream, UINT index);

		UINT AddVertex(const v3dxVector3 * pos, const v3dxVector3 * nor, const v3dxVector2 * uv, DWORD color);
		UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector3* tangent, const v3dxVector2* uv, DWORD color);
		UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, const v3dxQuaternion* lighmapUV, DWORD color);
		UINT AddVertex(const FMeshVertex& vertex);
		void AddVertex(const FMeshVertex* pVertex, UINT num);
		
		bool AddVertex_Pos_UV_Color_Index(const void* pVertex, UINT num, bool bInvertY = false, float CanvasHeight = 0);

		void ResizeVertexBuffers(UINT size);

		//alternative interface for same mesh
		vBOOL AddTriangle(UINT a, UINT b, UINT c);
		vBOOL AddTriangle(UINT a, UINT b, UINT c, USHORT faceData);
		vBOOL AddTriangle(UINT* pTri, UINT numOfTri);

		vBOOL AddLine(UINT a, UINT b);

		bool ToMesh(ICommandList* cmd, FMeshPrimitives * mesh);
		FInputLayoutDesc* GetInputLayoutDesc();
	private:
		void LoadVB(XndAttribute * pAttr, UINT stride, EVertexStreamType stream);
	};
}

NS_END

