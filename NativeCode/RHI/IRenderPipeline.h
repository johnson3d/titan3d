#pragma once
#include "IRenderResource.h"
#include "../Math/v3dxColor4.h"
#include <unordered_map>

NS_BEGIN

class IRenderContext;
class IVertexBuffer;
class IIndexBuffer;
class IShader;
class IVertexShader;
class IPixelShader;
class IInputLayout;
class IConstantBuffer;
class ICommandList;
class IRenderPass;
class IRasterizerState;
class IDepthStencilState;
class IBlendState;
class IShaderProgram;

struct TR_CLASS(SV_LayoutStruct = 8)
IRenderPipelineDesc
{
	IRenderPipelineDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		RenderPass = nullptr;
		GpuProgram = nullptr;
		Rasterizer = nullptr;
		DepthStencil = nullptr;
		Blend = nullptr;
		SampleMask = 0xFFFFFFFF;
		PrimitiveType = EPT_TriangleList;
	}
	IRenderPass*			RenderPass;
	IShaderProgram*			GpuProgram;
	IRasterizerState*		Rasterizer;
	IDepthStencilState*		DepthStencil;
	IBlendState*			Blend;
	EPrimitiveType			PrimitiveType;
	UINT					SampleMask;
};

class TR_CLASS()
IRenderPipeline : public IRenderResource
{
protected:
	AutoRef<IRenderPass>			mRenderPass;
	AutoRef<IRasterizerState>		mRasterizerState;
	AutoRef<IDepthStencilState>		mDepthStencilState;
	AutoRef<IBlendState>			mBlendState;
	AutoRef<IShaderProgram>			mGpuProgram;
public:
	IRenderPipeline();
	~IRenderPipeline();
	
	void BindRenderPass(IRenderPass * State);
	void BindRasterizerState(IRasterizerState* State);
	void BindDepthStencilState(IDepthStencilState* State);
	void BindBlendState(IBlendState* State);	
	void BindGpuProgram(IShaderProgram* pGpuPrgram);

	inline IRenderPass* GetRenderPass() {
		return mRenderPass;
	}
	inline IRasterizerState* GetRasterizerState() {
		return mRasterizerState;
	}
	inline IDepthStencilState* GetDepthStencilState() {
		return mDepthStencilState;
	}
	inline IBlendState* GetBindBlendState() {
		return mBlendState;
	}
	inline IShaderProgram* GetGpuProgram()
	{
		return mGpuProgram;
	}
	
	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout) = 0;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* Layout) = 0;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) = 0;
public:	
	UINT			mSampleMask;
	
	bool			mIsDirty;
};

NS_END