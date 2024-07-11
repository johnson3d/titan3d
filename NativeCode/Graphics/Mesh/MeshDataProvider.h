#pragma once
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

namespace NxRHI
{
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

		bool BuildTangent();

		bool LoadFromMeshPrimitive(XndNode* pNode, EVertexStreamType streams);

		void GetAABB(v3dxBox3* box) {
			*box = mAABB;
		}
		void SetAABB(v3dxBox3* box) {
			mAABB = *box;
		}
		UINT GetVertexNumber() const;
		UINT GetPrimitiveNumber() const;
		UINT GetAtomNumber() const;
		IBlobObject* CreateStream(EVertexStreamType index);
		IBlobObject* GetStream(EVertexStreamType index);
		IBlobObject* GetIndices();

		FMeshAtomDesc* GetAtom(UINT index, UINT lod);
		VIUnknown* GetAtomExtData(UINT index) const {
			if (index >= mAtomExtDatas.size())
				return nullptr;
			return mAtomExtDatas[index];
		}
		void PushAtom(const FMeshAtomDesc* pDescLODs, UINT count, VIUnknown* ext);
		bool SetAtom(UINT index, UINT lod, const FMeshAtomDesc& desc);
		void PushAtomLOD(UINT index, const FMeshAtomDesc& desc);
		UINT GetAtomLOD(UINT index);
		bool GetTriangle(int index, UINT* vA, UINT* vB, UINT* vC);
		bool GetAtomTriangle(UINT atom, UINT index, UINT* vA, UINT* vB, UINT* vC);
		int IntersectTriangle(const v3dxVector3* scale, const v3dxVector3* vStart, const v3dxVector3* vEnd, VHitResult* result);
		void* GetVertexPtr(EVertexStreamType stream, UINT index);

		UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color);
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

		bool ToMesh(ICommandList* cmd, FMeshPrimitives* mesh);
		FInputLayoutDesc* GetInputLayoutDesc();
	private:
		void LoadVB(XndAttribute* pAttr, UINT stride, EVertexStreamType stream);
	};
}

NS_END

