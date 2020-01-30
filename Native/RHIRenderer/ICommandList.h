#pragma once
#include "PreHead.h"
#include "../Core/thread/vfxcritical.h"

NS_BEGIN


struct ICommandListDesc
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
class IPass;
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

class GraphicsProfiler;

typedef void(WINAPI *FOnPassBuilt)(ICommandList* cmd, IPass* pass);

class ICommandList : public RHIUnknown
{
protected:
	TObjectHandle<IRenderContext>			mRHIContext;
	//1 dp=> 1 pass;
	std::vector<IPass*>		mPassArray;

	std::string				mDebugName;
public:
	int						mDrawCall;
	int						mDrawTriangle;
	AutoRef<GraphicsProfiler>	mProfiler;
	UINT					mCmdCount;
	
	FOnPassBuilt			OnPassBuilt;
public:
	ICommandList();
	~ICommandList();

	AutoRef<IRenderContext> GetContext();

	//��������ô˻��෽������֤Passes��ȷ����
	virtual void BeginCommand() = 0;
	virtual void EndCommand() = 0;

	void ClearMeshDrawPassArray();

	UINT GetPassNumber() const {
		return (UINT)mPassArray.size();
	}

	void SetPassBuiltCallBack(FOnPassBuilt fun) {
		OnPassBuilt = fun;
	}

	const char* GetDebugName() const{
		return mDebugName.c_str();
	}
	void SetDebugName(const char* name) {
		mDebugName = name;
	}
	
	VDef_ReadOnly(DrawCall);
	VDef_ReadOnly(DrawTriangle);
	VDef_ReadOnly(CmdCount);

	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) = 0;

	virtual void ClearMRT(const std::pair<BYTE,DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) = 0;*/
	
	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer) = 0;
	virtual void BuildRenderPass(vBOOL bImmCBuffer, int limitter, IPass** ppPass);
	virtual void EndRenderPass() = 0;

	virtual void Blit2DefaultFrameBuffer(IFrameBuffers* FrameBuffers, int dstWidth, int dstHeight) {}

	void PushPass(IPass* Pass);

	virtual void Commit(IRenderContext* pRHICtx) = 0;

	void SetGraphicsProfiler(GraphicsProfiler* profiler);

	virtual void SetRasterizerState(IRasterizerState* State){}
	virtual void SetDepthStencilState(IDepthStencilState* State) {}
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) {}

	virtual void SetComputeShader(IComputeShader* ComputerShader)
	{
		ASSERT(false);
	}
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
	{
		ASSERT(false);
	}
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts)
	{
		ASSERT(false);
	}
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
	{
		ASSERT(false);
	}
	virtual void CSDispatch(UINT x, UINT y, UINT z)
	{
		ASSERT(false);
	}

	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) = 0;
public:
	struct ContextState
	{
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