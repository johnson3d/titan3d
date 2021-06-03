#pragma once
#include "IRenderResource.h"
#include "IShader.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "ITextureBase.h"
#include "ICommandList.h"
#include "ISamplerState.h"
#include "IShaderResourceView.h"
#include "Utility/IGeometryMesh.h"

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
class IMeshPrimitives;

class IDrawCall;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderResources : public VIUnknown
{
	RTTI_DEF(IShaderResources, 0x25d4135e5b03e326, true);
	typedef std::vector<std::pair<USHORT, AutoRef<IShaderResourceView>>> RSVVector;
	RSVVector					VSResources;
	RSVVector					PSResources;

	TR_CONSTRUCTOR()
	IShaderResources() {}

	~IShaderResources();
	TR_FUNCTION()
	void VSBindTexture(UINT slot, IShaderResourceView* tex);
	TR_FUNCTION()
	void PSBindTexture(UINT slot, IShaderResourceView* tex);
	TR_FUNCTION()
	IShaderResourceView* GetBindTextureVS(UINT slot);
	TR_FUNCTION()
	IShaderResourceView* GetBindTexturePS(UINT slot);
	UINT PSResourceNum()
	{	return (UINT)PSResources.size();
	}
	UINT VSResourceNum()
	{
		return (UINT)VSResources.size();
	}
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderSamplers : public  VIUnknown
{
	RTTI_DEF(IShaderSamplers, 0x3192c6575b03e351, true);
	typedef std::vector<std::pair<USHORT, AutoRef<ISamplerState>>> SamplerVector;
	SamplerVector				VSSamplers;
	SamplerVector				PSSamplers;

	TR_CONSTRUCTOR()
	IShaderSamplers()
	{
		
	}
	~IShaderSamplers();
	TR_FUNCTION()
	void VSBindSampler(UINT slot, ISamplerState* sampler);
	TR_FUNCTION()
	void PSBindSampler(UINT slot, ISamplerState* sampler);
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
DrawPrimitiveDesc
{
	DrawPrimitiveDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
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

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IViewPort : public VIUnknown
{
	TR_CONSTRUCTOR()
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

	TR_MEMBER()
	float TopLeftX;
	TR_MEMBER()
	float TopLeftY;
	TR_MEMBER()
	float Width;
	TR_MEMBER()
	float Height;
	TR_MEMBER()
	float MinDepth;
	TR_MEMBER()
	float MaxDepth;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
SRRect
{
	int MinX;
	int MinY;
	int MaxX;
	int MaxY;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IScissorRect : public VIUnknown
{
	RTTI_DEF(IScissorRect, 0x486b7d125d70a687, true);
	TR_CONSTRUCTOR()
	IScissorRect()
	{

	}
	std::vector<SRRect> Rects;
	
	TR_FUNCTION()
	void SetRectNumber(UINT num);
	TR_FUNCTION()
	UINT GetRectNumber() {
		return (UINT)Rects.size();
	}
	TR_FUNCTION()
	void SetSCRect(UINT idx, int left, int top, int right, int bottom);
	TR_FUNCTION()
	void GetSCRect(UINT idx, SRRect* pRect);
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IDrawCallDesc
{

};

class GraphicsProfiler;
class IGpuBuffer;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IDrawCall : public IRenderResource
{
	friend GraphicsProfiler;
protected:
	AutoRef<IRenderPipeline>				m_pPipelineState;
	
	typedef std::vector<std::pair<USHORT, AutoRef<IConstantBuffer>>> CBufferVector;

	CBufferVector							CBuffersVS;
	CBufferVector							CBuffersPS;

	AutoRef<IMeshPrimitives>				MeshPrimitives;
	AutoRef<IGpuBuffer>						IndirectDrawArgsBuffer;
	UINT									IndirectDrawOffsetForArgs;

	UINT									AtomIndex;
	UINT									LodLevel;
	UINT									NumInstances;
	AutoRef<IShaderResources>				m_pShaderTexBinder;
	AutoRef<IShaderSamplers>				m_pShaderSamplerBinder;
	
	AutoRef<IVertexArray>					AttachVBs;
	AutoRef<IIndexBuffer>					AttachIndexBuffer;
	AutoRef<IShaderResources>				AttachSRVs;

	IManagedObjectHolder					TagHandle;
public:
	IDrawCall();
	~IDrawCall();

	inline void operator=(const IDrawCall& rh) {
		assert(false);
	}
	TR_FUNCTION()
	IConstantBuffer* FindCBufferVS(const char* name);
	TR_FUNCTION()
	IConstantBuffer* FindCBufferPS(const char* name);
	TR_FUNCTION()
	virtual void BuildPass(ICommandList* cmd, vBOOL bImmCBuffer);
	TR_FUNCTION()
	virtual vBOOL ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer);

	TR_FUNCTION()
	IRenderPipeline* GetPipeline() {
		return m_pPipelineState;
	}
	TR_FUNCTION()
	virtual void BindPipeline(IRenderPipeline* pipeline);
	
	TR_FUNCTION()
	virtual void BindGeometry(IMeshPrimitives* mesh, UINT atom, float lod);
	TR_FUNCTION()
	void SetLod(float lod);
	TR_FUNCTION()
	float GetLod();
	TR_FUNCTION()
	void BindAttachVBs(IVertexArray* vbs) {
		AttachVBs.StrongRef(vbs);
	}
	TR_FUNCTION()
	void BindAttachIndexBuffer(IIndexBuffer* ib) {
		AttachIndexBuffer.StrongRef(ib);
	}
	TR_FUNCTION()
	void BindAttachSRVs(IShaderResources* srvs) {
		AttachSRVs.StrongRef(srvs);
	}
	TR_FUNCTION()
	IShaderResources* GetShaderResources() {
		return m_pShaderTexBinder;
	}
	TR_FUNCTION()
	virtual void BindShaderResources(IShaderResources* res)
	{
		m_pShaderTexBinder.StrongRef(res);
	}
	TR_FUNCTION()
	virtual void BindShaderSamplers(IShaderSamplers* samps)
	{
		m_pShaderSamplerBinder.StrongRef(samps);
	}
	TR_FUNCTION()
	void GetDrawPrimitive(DrawPrimitiveDesc* desc);
	TR_FUNCTION()
	void SetInstanceNumber(int instNum);
	TR_FUNCTION()
	void SetIndirectDraw(IGpuBuffer* pBuffer, UINT offset);
	
	TR_FUNCTION()
	virtual void SetViewport(ICommandList* cmd, IViewPort* vp) = 0;
	TR_FUNCTION()
	virtual void SetScissorRect(ICommandList* cmd, IScissorRect* sr) = 0;

	TR_FUNCTION()
	virtual void SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline) = 0;
	TR_FUNCTION()
	virtual void SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) = 0;
	TR_FUNCTION()
	virtual void SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer) = 0;

	TR_FUNCTION()
	virtual void VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	TR_FUNCTION()
	virtual void PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) = 0;
	TR_FUNCTION()
	virtual void VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	TR_FUNCTION()
	virtual void PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) = 0;
	TR_FUNCTION()
	virtual void VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	TR_FUNCTION()
	virtual void PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) = 0;
	
	TR_FUNCTION()
	virtual void DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	TR_FUNCTION()
	virtual void DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	TR_FUNCTION()
	virtual void DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) = 0;

	//CBuffer
	TR_FUNCTION()
	virtual void BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer);
	TR_FUNCTION()
	virtual void BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer);

	TR_FUNCTION()
	UINT FindCBufferIndex(const char* name);
	TR_FUNCTION()
	void BindCBufferAll(UINT cbIndex, IConstantBuffer* CBuffer);

	TR_FUNCTION()
	UINT FindSRVIndex(const char* name);
	TR_FUNCTION()
	void BindSRVAll(UINT Index, IShaderResourceView* srv);

	TR_FUNCTION()
	bool GetSRVBindInfo(const char* name, TSBindInfo* info, int dataSize);

	TR_FUNCTION()
	bool GetSamplerBindInfo(const char* name, TSBindInfo* info, int dataSize);

	TR_FUNCTION()
	void SetTagHandle(void* handle) {
		TagHandle.SetHandle(handle);
	}
	TR_FUNCTION()
	void* GetTagHandle() {
		return TagHandle.mHandle;
	}
};

NS_END