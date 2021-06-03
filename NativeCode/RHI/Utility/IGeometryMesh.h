#pragma once
#include "../IRenderResource.h"
#include "../IVertexBuffer.h"
#include "../IIndexBuffer.h"
#include "../../Base/timekeys/TimeKeys.h"
#include "../../Math/v3dxMatrix4.h"

NS_BEGIN

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EVertexSteamType
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

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
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

class ICommandList;
class IDrawCall;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IVertexArray : public VIUnknown
{
public:
	RTTI_DEF(IVertexArray, 0x5d2268915cb1639f, true);
	IVertexBuffer*			VertexBuffers[VST_Number];
	UINT					mNumInstances;

	TR_CONSTRUCTOR()
	IVertexArray()
	{
		memset(VertexBuffers, 0, sizeof(VertexBuffers));
		mNumInstances = 0;
	}
	~IVertexArray();

	TR_FUNCTION()
	void Reset();

	void ApplyVBs(ICommandList* cmd, IDrawCall* pass, vBOOL bImm);

	TR_FUNCTION()
	inline IVertexBuffer* GetVertexBuffer(EVertexSteamType index) {
		return VertexBuffers[index];
	}

	TR_FUNCTION()
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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IGeometryMesh : public IRenderResource
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

	TR_FUNCTION()
	inline IVertexBuffer* GetVertexBuffer(EVertexSteamType index) {
		return VertexBuffers[index];
	}
	TR_FUNCTION()
	inline IIndexBuffer* GetIndexBuffer() {
		return IndexBuffer;
	}

	TR_FUNCTION()
	virtual void BindVertexBuffer(EVertexSteamType index, IVertexBuffer* vb)
	{
		if (VertexBuffers[index] == vb)
			return;
		if (vb)
			vb->AddRef();
		Safe_Release(VertexBuffers[index]);
		VertexBuffers[index] = vb;
	}
	TR_FUNCTION()
	virtual void BindIndexBuffer(IIndexBuffer* ib)
	{
		if (IndexBuffer == ib)
			return;
		if (ib)
			ib->AddRef();
		Safe_Release(IndexBuffer);
		IndexBuffer = ib;
	}

	virtual vBOOL ApplyGeometry(ICommandList* cmd, IDrawCall* pass, vBOOL bImm);

	TR_FUNCTION()
	static IGeometryMesh* MergeGeoms(IRenderContext* rc, IGeometryMesh** meshArray, v3dxMatrix4* transform, int count, v3dxBox3* aabb);
};

NS_END