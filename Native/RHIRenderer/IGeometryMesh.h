#pragma once
#include "IRenderResource.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "../Core/timekeys/TimeKeys.h"
#include "../Math/v3dxMatrix4.h"

NS_BEGIN

enum EVertexSteamType
{
	VST_Position,
	VST_Normal,
	VST_Tangent,
	VST_Color,
	VST_UV,
	VST_LightMap,
	VST_SkinIndex,
	VST_SkinWeight,
	VST_TerrainIndex,
	VST_TerrainGradient,//10
	VST_InstPos,
	VST_InstQuat,
	VST_InstScale,
	VST_F4_1,
	VST_F4_2,
	VST_F4_3,//16
	VST_Number,
};

class ICommandList;
class IPass;

class IVertexArray : public VIUnknown
{
public:
	RTTI_DEF(IVertexArray, 0x5d2268915cb1639f, true);
	IVertexBuffer*			VertexBuffers[VST_Number];
	UINT					mNumInstances;

	IVertexArray()
	{
		memset(VertexBuffers, 0, sizeof(VertexBuffers));
		mNumInstances = 0;
	}
	~IVertexArray();

	VDef_ReadWrite(UINT, NumInstances, m);
	void ApplyVBs(ICommandList* cmd, IPass* pass, vBOOL bImm);

	inline IVertexBuffer* GetVertexBuffer(EVertexSteamType index) {
		return VertexBuffers[index];
	}

	void BindVertexBuffer(EVertexSteamType index, IVertexBuffer* vb)
	{
		if (VertexBuffers[index] == vb)
			return;
		if (vb)
			vb->AddRef();
		Safe_Release(VertexBuffers[index]);
		VertexBuffers[index] = vb;
	}
};

class IGeometryMesh : public IRenderResource
{
protected:
	IVertexBuffer*			VertexBuffers[VST_Number];
	IIndexBuffer*			IndexBuffer;
public:
	vBOOL					mIsDirty;
	TimeKeys				MopherKeys[VST_Number];
public:
	RTTI_DEF(IGeometryMesh, 0x889c9cac5b2dd347, true);
	IGeometryMesh()
	{
		memset(VertexBuffers, 0, sizeof(VertexBuffers));
		IndexBuffer = NULL;
		mIsDirty = FALSE;
	}
	~IGeometryMesh();
	virtual void InvalidateResource() override;

	VDef_ReadWrite(vBOOL, IsDirty, m);

	inline IVertexBuffer* GetVertexBuffer(EVertexSteamType index) {
		return VertexBuffers[index];
	}
	inline IIndexBuffer* GetIndexBuffer() {
		return IndexBuffer;
	}

	virtual void BindVertexBuffer(EVertexSteamType index, IVertexBuffer* vb)
	{
		if (VertexBuffers[index] == vb)
			return;
		if (vb)
			vb->AddRef();
		Safe_Release(VertexBuffers[index]);
		VertexBuffers[index] = vb;
	}
	virtual void BindIndexBuffer(IIndexBuffer* ib)
	{
		if (IndexBuffer == ib)
			return;
		if (ib)
			ib->AddRef();
		Safe_Release(IndexBuffer);
		IndexBuffer = ib;
	}

	virtual vBOOL ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm);

	static IGeometryMesh* MergeGeoms(IRenderContext* rc, IGeometryMesh** meshArray, v3dxMatrix4* transform, int count, v3dxBox3* aabb);
};

NS_END