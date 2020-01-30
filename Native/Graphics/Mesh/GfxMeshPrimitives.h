#pragma once
#include "../GfxPreHead.h"

NS_BEGIN

class GfxMdfQueue;
class GfxMeshDataProvider;
class GfxMeshPrimitives : public VIUnknown
{
	friend GfxMeshDataProvider;
public:
	RTTI_DEF(GfxMeshPrimitives, 0x159b85ee5b0e52b1, true);
	GfxMeshPrimitives();
	~GfxMeshPrimitives();
	virtual void Cleanup() override;

	vBOOL Init(IRenderContext* rc, const char* name, UINT atom);
	vBOOL InitFromGeomtryMesh(IRenderContext* rc, IGeometryMesh* mesh, UINT atom, const v3dxBox3* aabb);
	vBOOL LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad);
	void Save2Xnd(IRenderContext* rc, XNDNode* node);
	vBOOL RefreshResource(IRenderContext* rc, const char* name, XNDNode* node);
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
	IGeometryMesh* GetGeomtryMesh() const {
		return mGeometryMesh;
	}
	GfxMdfQueue* GetMdfQueue();

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
	VDef_ReadWrite(v3dxBox3, AABB, m);

	virtual IResourceState* GetResourceState() override {
		return &mResourceState;
	}

	static void CalcNormals32(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const UINT* triangles, UINT nTri);
	static void CalcNormals16(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const USHORT* triangles, UINT nTri);
private:
	AutoRef<IVertexBuffer> LoadVB(IRenderContext* rc, XNDAttrib* pAttr, UINT stride, TimeKeys& tkeys, UINT& resSize, EVertexSteamType stream);
	void SaveVB(IRenderContext* rc, XNDAttrib* pAttr, IVertexBuffer* vb, TimeKeys& tkeys, UINT stride);
protected:
	std::string				mName;
	AutoRef<IGeometryMesh>	mGeometryMesh;
	AutoRef<GfxMdfQueue>	mMdfQueue;
	std::vector<std::vector<DrawPrimitiveDesc>>	mAtoms;

	TObjectHandle<IRenderContext>	mContext;
	AutoRef<XNDNode>		mSrcNode;
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
	v3dxBox3				mOBB;
	v3dxMatrix4				mOBBMatrix;
	IResourceState			mResourceState;
};

class GfxMeshDataProvider : public VIUnknown
{
public:
	std::vector<std::vector<DrawPrimitiveDesc>>	mAtoms;
	v3dxBox3				mAABB;
	IBlobObject*			mVertexBuffers[VST_Number];
	IBlobObject*			IndexBuffer;
	EIndexBufferType		IBType;
	UINT					VertexNumber;
	UINT					TriangleNumber;
public:
	RTTI_DEF(GfxMeshDataProvider, 0x279344bc5c983c5b, true);
	GfxMeshDataProvider();
	~GfxMeshDataProvider();
	virtual void Cleanup() override;

	vBOOL InitFromMesh(IRenderContext* rc, GfxMeshPrimitives* mesh);
	vBOOL Init(DWORD streams, EIndexBufferType ibType, int atom);

	UINT GetVertexNumber() const;
	UINT GetTriangleNumber() const;
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
	int IntersectTriangle(const v3dxVector3* vStart, const v3dxVector3* vEnd, float* dist);
	void* GetVertexPtr(EVertexSteamType stream, UINT index);

	UINT GetStreamStride(EVertexSteamType stream);

	UINT AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color);
	vBOOL AddTriangle(UINT a, UINT b, UINT c);

	vBOOL ToMesh(IRenderContext* rc, GfxMeshPrimitives* mesh);
};

NS_END