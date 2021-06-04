#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector2.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxBox3.h"
#include "IGeometryMesh.h"
#include "../IDrawCall.h"
#include "../../Bricks/Animation/Skeleton/IPartialSkeleton.h"

NS_BEGIN

class XndAttribute;
class XndNode;
class XndHolder;
class IRenderContext;
class IMeshDataProvider;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IMeshPrimitives : public VIUnknown
{
	friend IMeshDataProvider;
public:
	RTTI_DEF(IMeshPrimitives, 0x159b85ee5b0e52b1, true);
	TR_CONSTRUCTOR()
	IMeshPrimitives();
	~IMeshPrimitives();
	virtual void Cleanup() override;

	TR_FUNCTION()
	vBOOL Init(IRenderContext* rc, const char* name, UINT atom);
	TR_FUNCTION()
	vBOOL InitFromGeomtryMesh(IRenderContext* rc, IGeometryMesh* mesh, UINT atom, const v3dxBox3* aabb);
	TR_FUNCTION()
	vBOOL LoadXnd(IRenderContext* rc, const char* name, XndHolder* xnd, bool isLoad);
	TR_FUNCTION()
	void Save2Xnd(IRenderContext* rc, XndNode* node);
	vBOOL RefreshResource(IRenderContext* rc, const char* name, XndNode* node);
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
	IGeometryMesh* GetGeomtryMesh() const {
		return mGeometryMesh;
	}
	IPartialSkeleton* GetPartialSkeleton() const
	{
		return mPartialSkeleton;
	}
	void SetPartialSkeleton(IPartialSkeleton* value)
	{
		mPartialSkeleton = value;
	}

	TR_FUNCTION()
	const char* GetName() const;
	TR_FUNCTION()
	UINT GetAtomNumber() const;
	TR_FUNCTION()
	vBOOL GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const;
	TR_FUNCTION()
	const DrawPrimitiveDesc* GetAtom(UINT index, UINT lod) const {
		if (index >= (UINT)mAtoms.size())
			return nullptr;
		if (lod >= (UINT)mAtoms[index].size())
			return nullptr;
		return &mAtoms[index][lod];
	}
	vBOOL SetAtom(UINT index, UINT lod, const DrawPrimitiveDesc* desc);
	void PushAtomLOD(UINT index, const DrawPrimitiveDesc* desc);
	UINT GetAtomLOD(UINT index);
	UINT GetLodLevel(UINT index, float lod);
	vBOOL SetGeomtryMeshStream(IRenderContext* rc, EVertexSteamType stream, void* data, UINT size, UINT stride, UINT cpuAccess);
	vBOOL SetGeomtryMeshIndex(IRenderContext* rc, void* data, UINT size, EIndexBufferType type, UINT cpuAccess);
	void SetAABB(v3dxBox3& aabb)
	{
		mAABB = aabb;
	}
	virtual IResourceState* GetResourceState() override {
		return &mResourceState;
	}

	static void CalcNormals32(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const UINT* triangles, UINT nTri);
	static void CalcNormals16(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const USHORT* triangles, UINT nTri);
private:
	AutoRef<IVertexBuffer> LoadVB(IRenderContext* rc, XndAttribute* pAttr, UINT stride, TimeKeys& tkeys, UINT& resSize, EVertexSteamType stream);
	void SaveVB(IRenderContext* rc, XndAttribute* pAttr, IVertexBuffer* vb, TimeKeys& tkeys, UINT stride);
protected:
	std::string				mName;
	AutoRef<IGeometryMesh>	mGeometryMesh;
	std::vector<std::vector<DrawPrimitiveDesc>>	mAtoms;

	TObjectHandle<IRenderContext>	mContext;
	AutoRef<XndHolder>		mXnd;
	struct VModelDesc
	{
		DWORD				Flags;
		DWORD				UnUsed;
		UINT				VertexNumber;
		UINT				GeoTabeNumber;
		UINT				PolyNumber;
		UINT				AtomNumber;
	};
	VModelDesc				mDesc;
	v3dxBox3				mAABB;
	IResourceState			mResourceState;

	AutoRef<IPartialSkeleton> mPartialSkeleton;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IMeshDataProvider : public VIUnknown
{
public:
	std::vector<std::vector<DrawPrimitiveDesc>>	mAtoms;
	v3dxBox3				mAABB;
	IBlobObject*			mVertexBuffers[VST_Number];
	IBlobObject*			IndexBuffer;
	EIndexBufferType		IBType;
	UINT					VertexNumber;
	UINT					PrimitiveNumber;
public:
	RTTI_DEF(IMeshDataProvider, 0x279344bc5c983c5b, true);

	TR_CONSTRUCTOR()
	IMeshDataProvider();
	~IMeshDataProvider();
	virtual void Cleanup() override;

	TR_FUNCTION()
	void Reset() {
		Cleanup();
	}

	TR_FUNCTION()
	vBOOL InitFromMesh(IRenderContext* rc, IMeshPrimitives* mesh);
	TR_FUNCTION()
	vBOOL Init(DWORD streams, EIndexBufferType ibType, int atom);

	TR_FUNCTION()
	void GetAABB(v3dxBox3* box) {
		*box = mAABB;
	}
	TR_FUNCTION()
	void SetAABB(v3dxBox3* box) {
		mAABB = *box;
	}
	TR_FUNCTION()
	UINT GetVertexNumber() const;
	TR_FUNCTION()
	UINT GetPrimitiveNumber() const;
	TR_FUNCTION()
	UINT GetAtomNumber() const;
	TR_FUNCTION()
	IBlobObject* GetStream(EVertexSteamType index);
	TR_FUNCTION()
	IBlobObject* GetIndices();

	TR_FUNCTION()
	vBOOL GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const;
	TR_FUNCTION()
	const DrawPrimitiveDesc* GetAtom(UINT index, UINT lod) const {
		if (index >= (UINT)mAtoms.size())
			return nullptr;
		if (lod >= (UINT)mAtoms[index].size())
			return nullptr;
		return &mAtoms[index][lod];
	}
	TR_FUNCTION()
	vBOOL SetAtom(UINT index, UINT lod, const DrawPrimitiveDesc* desc);
	TR_FUNCTION()
	void PushAtomLOD(UINT index, const DrawPrimitiveDesc* desc);
	TR_FUNCTION()
	UINT GetAtomLOD(UINT index);
	TR_FUNCTION()
	vBOOL GetTriangle(int index, UINT* vA, UINT* vB, UINT* vC);
	TR_FUNCTION()
	vBOOL GetAtomTriangle(UINT atom, UINT index, UINT* vA, UINT* vB, UINT* vC);
	TR_FUNCTION()
	int IntersectTriangle(const v3dxVector3* vStart, const v3dxVector3* vEnd, float* dist);
	TR_FUNCTION()
	void* GetVertexPtr(EVertexSteamType stream, UINT index);

	TR_FUNCTION()
	UINT GetStreamStride(EVertexSteamType stream);

	TR_FUNCTION()
	UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color);
	TR_FUNCTION()
	vBOOL AddTriangle(UINT a, UINT b, UINT c);

	vBOOL AddLine(UINT a, UINT b);

	TR_FUNCTION()
	vBOOL ToMesh(IRenderContext* rc, IMeshPrimitives* mesh);
};

NS_END