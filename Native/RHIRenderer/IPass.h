#pragma once
#include "IRenderResource.h"
#include "IShader.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "ITextureBase.h"
#include "IGeometryMesh.h"

NS_BEGIN

class IRenderContext;
class ICommandList;
class IRenderPipeline;
class IVertexShader;
class IIndexShader;
class IConstantBuffer;
class IVertexBuffer;
class IIndexBuffer;
class ITextureBase;
class IShaderResourceView;
class ISamplerState;
class IShaderProgram;
class GfxMeshPrimitives;

enum EPrimitiveType
{
	EPT_PointList = 1,
	EPT_LineList = 2,
	EPT_LineStrip = 3,
	EPT_TriangleList = 4,
	EPT_TriangleStrip = 5,
	EPT_TriangleFan = 6,
};

class IPass;

struct IShaderResources : public VIUnknown
{
	RTTI_DEF(IShaderResources, 0x25d4135e5b03e326, true);
	//std::map<BYTE, IShaderResourceView*>	VSResources;
	//std::map<BYTE, IShaderResourceView*>	PSResources;
	IShaderResourceView* VSResources[MaxTexOrSamplerSlot];
	IShaderResourceView* PSResources[MaxTexOrSamplerSlot];

	IShaderResources()
	{
		memset(VSResources, 0, sizeof(VSResources));
		memset(PSResources, 0, sizeof(PSResources));
	}
	~IShaderResources();
	void VSBindTexture(BYTE slot, IShaderResourceView* tex);
	void PSBindTexture(BYTE slot, IShaderResourceView* tex);
	IShaderResourceView* GetBindTextureVS(BYTE slot);
	IShaderResourceView* GetBindTexturePS(BYTE slot);
	UINT PSResourceNum()
	{
		UINT count = 0;
		for (int i = 0; i < MaxTexOrSamplerSlot; i++)
		{
			if (PSResources[i] != nullptr)
			{
				count++;
			}
		}
		return count;
		//return (UINT)PSResources.size();
	}
	UINT VSResourceNum()
	{
		UINT count = 0;
		for (int i = 0; i < MaxTexOrSamplerSlot; i++)
		{
			if (VSResources[i] != nullptr)
			{
				count++;
			}
		}
		return count;
		//return (UINT)VSResources.size();
	}
};

struct IShaderSamplers : public  VIUnknown
{
	RTTI_DEF(IShaderSamplers, 0x3192c6575b03e351, true);
	//std::map<BYTE, ISamplerState*>	VSSamplers;
	//std::map<BYTE, ISamplerState*>	PSSamplers;
	ISamplerState*				VSSamplers[MaxTexOrSamplerSlot];
	ISamplerState*				PSSamplers[MaxTexOrSamplerSlot];

	IShaderSamplers()
	{
		memset(VSSamplers, 0, sizeof(VSSamplers));
		memset(PSSamplers, 0, sizeof(PSSamplers));
	}
	~IShaderSamplers();
	void VSBindSampler(BYTE slot, ISamplerState* sampler);
	void PSBindSampler(BYTE slot, ISamplerState* sampler);
};

struct DrawPrimitiveDesc
{
	DrawPrimitiveDesc()
	{
		PrimitiveType = EPT_TriangleList;
		BaseVertexIndex = 0;
		StartIndex = 0;
		NumPrimitives = 0;
		NumInstances = 1;
	}
	EPrimitiveType PrimitiveType;
	UINT BaseVertexIndex;
	UINT StartIndex;
	UINT NumPrimitives;
	UINT NumInstances;
	bool IsIndexDraw() const{
		return StartIndex != 0xFFFFFFFF;
	}
};

struct IViewPort : public VIUnknown
{
	IViewPort()
	{
		TopLeftX = 0;
		TopLeftY = 0;
		Width = 0;
		Height = 0;
		MinDepth = 0;
		MaxDepth = 1.0F;
	}
	RTTI_DEF(IViewPort, 0x1e91cb6f5b04f4ef, true);
	float TopLeftX;
	float TopLeftY;
	float Width;
	float Height;
	float MinDepth;
	float MaxDepth;
	VDef_ReadWrite(float, TopLeftX, );
	VDef_ReadWrite(float, TopLeftY, );
	VDef_ReadWrite(float, Width, );
	VDef_ReadWrite(float, Height, );
	VDef_ReadWrite(float, MinDepth, );
	VDef_ReadWrite(float, MaxDepth, );
};

struct IScissorRect : public VIUnknown
{
	RTTI_DEF(IScissorRect, 0x486b7d125d70a687, true);
	struct SRRect
	{
		int MinX;
		int MinY;
		int MaxX;
		int MaxY;
	};
	std::vector<SRRect> Rects;
	
	void SetRectNumber(UINT num);
	UINT GetRectNumber() {
		return (UINT)Rects.size();
	}
	void SetSCRect(UINT idx, int left, int top, int right, int bottom);
	void GetSCRect(UINT idx, SRRect* pRect);
};

struct IPassDesc
{

};

class GraphicsProfiler;
class IGpuBuffer;
class IPass : public IRenderResource
{
	friend GraphicsProfiler;
protected:
	DWORD									mUserFlags;
	VSLLockNoSleep							mLocker;
	IRenderPipeline *						m_pPipelineState;
	IShaderProgram*							m_pGpuProgram;

	IConstantBuffer*						CBuffersVS[MaxCB];
	IConstantBuffer*						CBuffersPS[MaxCB];

	GfxMeshPrimitives*						MeshPrimitives;
	AutoRef<IGpuBuffer>						IndirectDrawArgsBuffer;
	UINT									IndirectDrawOffsetForArgs;

	UINT									AtomIndex;
	UINT									LodLevel;
	UINT									NumInstances;
	IShaderResources*						m_pShaderTexBinder;
	IShaderSamplers*						m_pShaderSamplerBinder;
	IViewPort*								m_pViewport;
	IScissorRect*							ScissorRect;

	AutoRef<IVertexArray>					AttachVBs;
	AutoRef<IIndexBuffer>					AttachIndexBuffer;
	AutoRef<IShaderResources>				AttachSRVs;
public:
	IPass();
	~IPass();

	VDef_ReadWrite(DWORD, UserFlags, m);

	IConstantBuffer* FindCBufferVS(const char* name);
	IConstantBuffer* FindCBufferPS(const char* name);
	virtual void BuildPass(ICommandList* cmd, vBOOL bImmCBuffer);
	virtual vBOOL ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer);

	IRenderPipeline* GetPipeline() {
		return m_pPipelineState;
	}
	virtual void BindPipeline(IRenderPipeline* pipeline);
	
	IShaderProgram* GetGpuProgram()
	{
		return m_pGpuProgram;
	}

	virtual void BindGpuProgram(IShaderProgram* pGpuPrgram);

	virtual void BindGeometry(GfxMeshPrimitives* mesh, UINT atom, float lod);
	void SetLod(float lod);
	float GetLod();
	void BindAttachVBs(IVertexArray* vbs) {
		AttachVBs.StrongRef(vbs);
	}
	void BindAttachIndexBuffer(IIndexBuffer* ib) {
		AttachIndexBuffer.StrongRef(ib);
	}
	void BindAttachSRVs(IShaderResources* srvs) {
		AttachSRVs.StrongRef(srvs);
	}

	IShaderResources* GetShaderResurces() {
		return m_pShaderTexBinder;
	}
	virtual void BindShaderResouces(IShaderResources* res)
	{
		if (res)
			res->AddRef();
		Safe_Release(m_pShaderTexBinder);
		m_pShaderTexBinder = res;
	}
	virtual void BindShaderSamplers(IShaderSamplers* samps)
	{
		if (samps)
			samps->AddRef();
		Safe_Release(m_pShaderSamplerBinder);
		m_pShaderSamplerBinder = samps;
	}
	virtual void BindViewPort(IViewPort* vp)
	{
		if (vp)
			vp->AddRef();
		Safe_Release(m_pViewport);
		m_pViewport = vp;
	}
	virtual void BindScissor(IScissorRect* sr)
	{
		if (sr)
			sr->AddRef();
		Safe_Release(ScissorRect);
		ScissorRect = sr;
	}

	void GetDrawPrimitive(DrawPrimitiveDesc* desc);
	void SetInstanceNumber(int instNum);
	void SetIndirectDraw(IGpuBuffer* pBuffer, UINT offset);
	
	virtual void SetViewport(ICommandList* cmd, IViewPort* vp) = 0;
	virtual void SetScissorRect(ICommandList* cmd, IScissorRect* sr) = 0;

	virtual void SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline) = 0;
	virtual void SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) = 0;
	virtual void SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer) = 0;

	virtual void VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	virtual void PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	virtual void VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	virtual void PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	
	virtual void DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) = 0;

	//����CBuffer
	virtual void BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer);
	virtual void BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer);

	void BindCBuffAll(IShaderProgram* shaderProgram, UINT cbIndex, IConstantBuffer* CBuffer);
};

NS_END