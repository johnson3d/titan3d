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
struct IRenderPassDesc;
class IFrameBuffers;
class ISwapChain;
class IViewPort;
class IScissorRect;
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
class ISemaphore;
class IRenderPass;

class GraphicsProfiler;
class IClassInstance;

struct IRenderPassClears;

TR_CALLBACK(SV_NameSpace = EngineNS, SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*FOnPassBuilt)(ICommandList* cmd, const IDrawCall* pass);

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

class TR_CLASS()
	ICommandList : public VIUnknown
{
protected:
	TObjectHandle<IRenderContext>			mRHIContext;
	//1 dp=> 1 pass;
	std::vector<IDrawCall*>		mPassArray;

	std::wstring				mDebugName;
public:
	AutoRef<IPipelineStat>		mPipelineStat;
	AutoRef<GraphicsProfiler>	mProfiler;
	
	FOnPassBuilt				OnPassBuilt;

	std::vector<ISemaphore*>	mWaitSemaphores;
	AutoRef<ISemaphore>			mSemaphore;
public:
	ICommandList();
	~ICommandList();

	IRenderContext* GetContext();

	void SetPipelineState(IPipelineStat* stat)
	{
		mPipelineStat.StrongRef(stat);
	}

	IPipelineStat* GetPipelineState() {
		return mPipelineStat;
	}

	virtual bool BeginCommand() = 0;
	virtual void EndCommand() = 0;

	void ClearMeshDrawPassArray();

	UINT GetPassNumber() const {
		return (UINT)mPassArray.size();
	}

	void SetPassBuiltCallBack(FOnPassBuilt fun) {
		OnPassBuilt = fun;
	}

	void SetDebugName(const char* name);

	ISemaphore* GetSemaphore() {
		return mSemaphore;
	}
	void AddWaitSemaphore(ISemaphore* semaphore);
	void RemoveWaitSemaphore(ISemaphore* semaphore);
	
	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) = 0;

	virtual void ClearMRT(const std::pair<BYTE,DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) = 0;*/
	
	virtual bool BeginRenderPass(IFrameBuffers* pFrameBuffer, const IRenderPassClears* passClears, const char* debugName = nullptr) = 0;
	virtual void BuildRenderPass(vBOOL bImmCBuffer);
	virtual void EndRenderPass() = 0;

	virtual void Blit2DefaultFrameBuffer(IFrameBuffers* FrameBuffers, int dstWidth, int dstHeight) {}

	void PushDrawCall(IDrawCall* Pass);

	virtual void Commit(IRenderContext* pRHICtx) = 0;

	void SetGraphicsProfiler(GraphicsProfiler* profiler);

	virtual void SetRasterizerState(IRasterizerState* State) = 0;
	virtual void SetDepthStencilState(IDepthStencilState* State) = 0;
	TR_FUNCTION(SV_NoStarToRef = blendFactor)
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) = 0;

	virtual void SetComputeShader(IComputeShader* ComputerShader) = 0;
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture) = 0;
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts) = 0;
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer) = 0;
	virtual void CSDispatch(UINT x, UINT y, UINT z) = 0;
	virtual void CSDispatchIndirect(IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) = 0;
	virtual void SetScissorRect(IScissorRect* sr) = 0;
	virtual void SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) = 0;
	virtual void SetIndexBuffer(IIndexBuffer* IndexBuffer) = 0;
	virtual void DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) = 0;
	virtual void DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) = 0;
	virtual void IASetInputLayout(IInputLayout* pInputLayout) = 0;
	virtual void VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances) = 0;
	virtual void PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances) = 0;
	virtual void SetViewport(IViewPort* vp) = 0;
	virtual void VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer) = 0;
	virtual void VSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV) = 0;
	virtual void PSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV) = 0;
	virtual void VSSetSampler(UINT32 Index, ISamplerState* Sampler) = 0;
	virtual void PSSetSampler(UINT32 Index, ISamplerState* Sampler) = 0;
	virtual void SetRenderPipeline(IRenderPipeline* pipeline, EPrimitiveType dpType) = 0;
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) = 0;

	virtual void BeginEvent(const char* info) = 0;
	virtual void EndEvent() = 0;
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
		inline IViewPort* GetViewport()
		{
			return ViewPort;
		}
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