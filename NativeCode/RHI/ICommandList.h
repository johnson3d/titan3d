#pragma once
#include "PreHead.h"
#include "../Base/thread/vfxcritical.h"

NS_BEGIN


struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ICommandListDesc
{

};

class IRenderContext;
class IVertexBuffer;
class IIndexBuffer;
class IShader;
class IConstantBuffer;
class IRenderTargetView;
class IDepthStencilView;
class IShaderResourceView;
class IUnorderedAccessView;
class IDrawCall;
struct RenderPassDesc;
class IFrameBuffers;
class ISwapChain;
struct IViewPort;
struct IScissorRect;
class IShaderProgram;
class IRasterizerState;
class IDepthStencilState;
class IDepthStencilState;
class IBlendState;
class IInputLayout;
class IVertexShader;
class IPixelShader;
class IComputeShader;
class ISamplerState;
class ICommandList;
class ITexture2D;
class IGpuBuffer;
class IRenderPipeline;
class IFence;

class GraphicsProfiler;
class IClassInstance;

TR_CALLBACK(SV_NameSpace = EngineNS, SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*FOnPassBuilt)(ICommandList* cmd, const IDrawCall* pass);

VTypeHelperDefine(FOnPassBuilt, sizeof(void*));

StructBegin(FOnPassBuilt, EngineNS)
StructEnd(void)

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EPrimitiveType
{
	EPT_PointList = 1,
	EPT_LineList = 2,
	EPT_LineStrip = 3,
	EPT_TriangleList = 4,
	EPT_TriangleStrip = 5,
	EPT_TriangleFan = 6,
};

struct TR_CLASS()
IPipelineStat : public VIUnknown
{
	IPipelineStat()
	{
		Reset();
		mDrawLimit = INT_MAX;
	}
	inline void Reset()
	{
		mDrawCall = 0;
		mDrawTriangle = 0;
		mCmdCount = 0;

		mLastestDrawCall = nullptr;
	}
	int						mDrawCall;
	int						mDrawTriangle;
	UINT					mCmdCount;
	int						mDrawLimit;

	IDrawCall* mLastestDrawCall;
	inline bool IsOverLimit()
	{
		return mDrawCall >= mDrawLimit;
	}
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ICommandList : public VIUnknown
{
protected:
	TObjectHandle<IRenderContext>			mRHIContext;
	//1 dp=> 1 pass;
	std::vector<IDrawCall*>		mPassArray;

	std::string					mDebugName;
public:
	AutoRef<IPipelineStat>		mPipelineStat;
	AutoRef<GraphicsProfiler>	mProfiler;
	
	FOnPassBuilt			OnPassBuilt;
public:
	ICommandList();
	~ICommandList();

	AutoRef<IRenderContext> GetContext();

	void SetPipelineState(IPipelineStat* stat)
	{
		mPipelineStat.StrongRef(stat);
	}

	IPipelineStat* GetPipelineState() {
		return mPipelineStat;
	}

	TR_FUNCTION()
	virtual void BeginCommand() = 0;
	TR_FUNCTION()
	virtual void EndCommand() = 0;

	TR_FUNCTION()
	void ClearMeshDrawPassArray();

	TR_FUNCTION()
	UINT GetPassNumber() const {
		return (UINT)mPassArray.size();
	}

	TR_FUNCTION()
	void SetPassBuiltCallBack(FOnPassBuilt fun) {
		OnPassBuilt = fun;
	}

	TR_FUNCTION()
	const char* GetDebugName() const{
		return mDebugName.c_str();
	}
	TR_FUNCTION()
	void SetDebugName(const char* name) {
		mDebugName = name;
	}
	
	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) = 0;

	virtual void ClearMRT(const std::pair<BYTE,DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) = 0;*/
	
	TR_FUNCTION()
	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer) = 0;
	TR_FUNCTION()
	virtual void BuildRenderPass(vBOOL bImmCBuffer);
	TR_FUNCTION()
	virtual void EndRenderPass() = 0;

	TR_FUNCTION()
	virtual void Blit2DefaultFrameBuffer(IFrameBuffers* FrameBuffers, int dstWidth, int dstHeight) {}

	TR_FUNCTION()
	void PushDrawCall(IDrawCall* Pass);

	TR_FUNCTION()
	virtual void Commit(IRenderContext* pRHICtx) = 0;

	TR_FUNCTION()
	void SetGraphicsProfiler(GraphicsProfiler* profiler);

	TR_FUNCTION()
	virtual void SetRasterizerState(IRasterizerState* State){}
	TR_FUNCTION()
	virtual void SetDepthStencilState(IDepthStencilState* State) {}
	TR_FUNCTION(SV_NoStarToRef = blendFactor)
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) {}

	TR_FUNCTION()
	virtual void SetComputeShader(IComputeShader* ComputerShader)
	{
		ASSERT(false);
	}
	TR_FUNCTION()
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
	{
		ASSERT(false);
	}
	TR_FUNCTION()
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts)
	{
		ASSERT(false);
	}
	TR_FUNCTION()
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
	{
		ASSERT(false);
	}
	TR_FUNCTION()
	virtual void CSDispatch(UINT x, UINT y, UINT z)
	{
		ASSERT(false);
	}
	TR_FUNCTION()
	virtual void PSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
	{

	}
	TR_FUNCTION()
	virtual void PSSetSampler(UINT32 Index, ISamplerState* Sampler)
	{

	}
	TR_FUNCTION()
	virtual void SetScissorRect(IScissorRect* sr)
	{

	}
	TR_FUNCTION()
	virtual void SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
	{

	}
	TR_FUNCTION()
	virtual void SetIndexBuffer(IIndexBuffer* IndexBuffer)
	{

	}
	TR_FUNCTION()
	virtual void DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
	{

	}
	TR_FUNCTION()
	virtual void DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
	{

	}
	TR_FUNCTION()
	virtual void DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
	{

	}
	TR_FUNCTION()
	virtual void IASetInputLayout(IInputLayout* pInputLayout)
	{

	}
	TR_FUNCTION()
	virtual void VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances)
	{

	}
	TR_FUNCTION()
	virtual void PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances)
	{

	}
	TR_FUNCTION()
	virtual void SetViewport(IViewPort* vp)
	{

	}
	TR_FUNCTION()
	virtual void VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
	{

	}
	TR_FUNCTION()
	virtual void PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
	{

	}
	TR_FUNCTION()
	virtual void SetRenderPipeline(IRenderPipeline* pipeline)
	{

	}
	TR_FUNCTION()
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) = 0;

	virtual void Signal(IFence* fence, int value){}
public:
	struct ContextState
	{
		friend ICommandList;
		ContextState()
		{
			Reset();
		}
		void Reset()
		{
			ViewPort = nullptr;
			ScissorRect = nullptr;
			ShaderProgram = nullptr;
			RasterizerState = nullptr;
			DepthStencilState = nullptr;
			BlendState = nullptr;
			InputLayout = nullptr;
			VertexShader = nullptr;
			PixelShader = nullptr;
			memset(CBuffers, 0, sizeof(CBuffers));
			memset(VSSampler, 0, sizeof(VSSampler));
			memset(PSSampler, 0, sizeof(PSSampler));
			memset(PSTextures, 0, sizeof(PSTextures));
		}
	private:
		IViewPort*			ViewPort;
		IScissorRect*		ScissorRect;
		IShaderProgram*		ShaderProgram;
		IRasterizerState*	RasterizerState;
		IDepthStencilState* DepthStencilState;
		IBlendState*		BlendState;
		IInputLayout*		InputLayout;
		IVertexShader*		VertexShader;
		IPixelShader*		PixelShader;
		IConstantBuffer*	CBuffers[16];
		ISamplerState*		VSSampler[16];
		ISamplerState*		PSSampler[16];
		IShaderResourceView*	PSTextures[16];
		static bool IsCMPState;
		bool IsSameScissors(IScissorRect* lh, IScissorRect* rh);
	public:
		bool TrySet_ViewPort(IViewPort* v)
		{
			if (IsCMPState == false)
				return true;
			if (ViewPort == v)
				return false;
			ViewPort = v;
			return true;
		}
		bool TrySet_ScissorRect(IScissorRect* v)
		{
			if (IsCMPState == false)
				return true;
			if (ScissorRect == v)
				return false;
			if (IsSameScissors(ScissorRect, v))
				return false;
			ScissorRect = v;
			return true;
		}
		bool TrySet_ShaderProgram(IShaderProgram* v)
		{
			if (IsCMPState == false)
				return true;
			if (ShaderProgram == v)
				return false;
			ShaderProgram = v;
			return true;
		}
		bool TrySet_RasterizerState(IRasterizerState* v)
		{
			if (IsCMPState == false)
				return true;
			if (RasterizerState == v)
				return false;
			RasterizerState = v;
			return true;
		}
		bool TrySet_DepthStencilState(IDepthStencilState* v)
		{
			if (IsCMPState == false)
				return true;
			if (DepthStencilState == v)
				return false;
			DepthStencilState = v;
			return true;
		}
		bool TrySet_BlendState(IBlendState* v)
		{
			if (IsCMPState == false)
				return true;
			if (BlendState == v)
				return false;
			BlendState = v;
			return true;
		}
		bool TrySet_InputLayout(IInputLayout* v)
		{
			if (IsCMPState == false)
				return true;
			if (InputLayout == v)
				return false;
			InputLayout = v;
			return true;
		}
		bool TrySet_VertexShader(IVertexShader* v)
		{
			if (IsCMPState == false)
				return true;
			if (VertexShader == v)
				return false;
			VertexShader = v;
			return true;
		}
		bool TrySet_PixelShader(IPixelShader* v)
		{
			if (IsCMPState == false)
				return true;
			if (PixelShader == v)
				return false;
			PixelShader = v;
			return true;
		}
		bool TrySet_CBuffers(UINT index, IConstantBuffer* v)
		{
			if (IsCMPState == false)
				return true;
			if (CBuffers[index] == v)
				return false;
			CBuffers[index] = v;
			return true;
		}
		bool TrySet_VSSampler(UINT index, ISamplerState* v)
		{
			if (IsCMPState == false)
				return true;
			if (VSSampler[index] == v)
				return false;
			VSSampler[index] = v;
			return true;
		}
		bool TrySet_PSSampler(UINT index, ISamplerState* v)
		{
			if (IsCMPState == false)
				return true;
			if (PSSampler[index] == v)
				return false;
			PSSampler[index] = v;
			return true;
		}
		bool TrySet_PSTextures(UINT index, IShaderResourceView* v)
		{
			if (IsCMPState == false)
				return true;
			if (PSTextures[index] == v)
				return false;
			PSTextures[index] = v;
			return true;
		}
	};

	ContextState		mCurrentState;
};

NS_END