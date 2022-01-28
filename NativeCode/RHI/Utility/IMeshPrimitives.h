#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector2.h"
#include "../../Math/v3dxVector3.h"
#include "../../Math/v3dxBox3.h"
#include "IGeometryMesh.h"
#include "../IDrawCall.h"

struct VHitResult;

NS_BEGIN

class XndAttribute;
class XndNode;
class XndHolder;
class IRenderContext;
class IMeshDataProvider;

class TR_CLASS()
IMeshPrimitives : public IRenderResource
{
	friend IMeshDataProvider;
public:
	struct VModelDesc
	{
		DWORD				Flags;
		DWORD				UnUsed;
		UINT				VertexNumber;
		UINT				GeoTabeNumber;
		UINT				PolyNumber;
		UINT				AtomNumber;
	};	
	ENGINE_RTTI(IMeshPrimitives);
	
	IMeshPrimitives();
	~IMeshPrimitives();
	virtual void Cleanup() override;

	vBOOL Init(IRenderContext* rc, const char* name, UINT atom);
	vBOOL InitFromGeomtryMesh(IRenderContext* rc, IGeometryMesh* mesh, UINT atom, const v3dxBox3* aabb);
	vBOOL LoadXnd(IRenderContext* rc, const char* name, XndHolder* xnd, bool isLoad);
	void Save2Xnd(IRenderContext* rc, XndNode* node);

	vBOOL RefreshResource(IRenderContext* rc, const char* name, XndNode* node);
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
	IGeometryMesh* GetGeomtryMesh() const {
		return mGeometryMesh;
	}

	const char* GetName() const;
	UINT GetAtomNumber() const;
	vBOOL GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const;
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
	VModelDesc				mDesc;
	v3dxBox3				mAABB;
	TR_MEMBER(SV_NoBind)
	IResourceState			mResourceState;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IMeshDataProvider : public VIUnknown
{
public:
	std::vector<std::vector<DrawPrimitiveDesc>>	mAtoms;
	v3dxBox3				mAABB;
	IBlobObject*			mVertexBuffers[VST_Number];
	IBlobObject*			IndexBuffer;
	IBlobObject*			FaceBuffer;
	EIndexBufferType		IBType;
	UINT					VertexNumber;
	UINT					PrimitiveNumber;
public:
	ENGINE_RTTI(IMeshDataProvider);

	IMeshDataProvider();
	~IMeshDataProvider();
	virtual void Cleanup() override;

	void Reset() {
		Cleanup();
	}

	vBOOL InitFromMesh(IRenderContext* rc, IMeshPrimitives* mesh);
	vBOOL Init(DWORD streams, EIndexBufferType ibType, int atom);

	vBOOL LoadFromMeshPrimitive(XndNode* pNode, EVertexSteamType streams);
	
	void GetAABB(v3dxBox3* box) {
		*box = mAABB;
	}
	void SetAABB(v3dxBox3* box) {
		mAABB = *box;
	}
	UINT GetVertexNumber() const;
	UINT GetPrimitiveNumber() const;
	UINT GetAtomNumber() const;
	IBlobObject* GetStream(EVertexSteamType index);
	IBlobObject* GetIndices();

	vBOOL GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const;
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
	vBOOL GetTriangle(int index, UINT* vA, UINT* vB, UINT* vC);
	vBOOL GetAtomTriangle(UINT atom, UINT index, UINT* vA, UINT* vB, UINT* vC);
	int IntersectTriangle(const v3dxVector3* scale, const v3dxVector3* vStart, const v3dxVector3* vEnd, VHitResult* result);
	void* GetVertexPtr(EVertexSteamType stream, UINT index);

	UINT GetStreamStride(EVertexSteamType stream);

	UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color);
	
	//alternative interface for same mesh
	vBOOL AddTriangle(UINT a, UINT b, UINT c);
	vBOOL AddTriangle(UINT a, UINT b, UINT c, USHORT faceData);

	vBOOL AddLine(UINT a, UINT b);

	vBOOL ToMesh(IRenderContext* rc, IMeshPrimitives* mesh);

private:
	void LoadVB(XndAttribute* pAttr, UINT stride, EVertexSteamType stream);
};

NS_END