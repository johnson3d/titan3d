#pragma once
#include "IRenderResource.h"
#include "../Math/v3dxColor4.h"

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
class IRasterizerState;
class IDepthStencilState;
class IBlendState;
class IShaderProgram;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IRenderPipelineDesc
{
	IRenderPipelineDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		GpuProgram = nullptr;
		Rasterizer = nullptr;
		DepthStencil = nullptr;
		Blend = nullptr;
		SampleMask = 0xFFFFFFFF;
		BlendFactor.fromArgb(0, 0, 0, 0);
	}
	IShaderProgram*			GpuProgram;
	IRasterizerState*		Rasterizer;
	IDepthStencilState*		DepthStencil;
	IBlendState*			Blend;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4				BlendFactor;
	UINT					SampleMask;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IRenderPipeline : public IRenderResource
{
protected:
	AutoRef<IRasterizerState>		mRasterizerState;
	AutoRef<IDepthStencilState>		mDepthStencilState;
	AutoRef<IBlendState>			mBlendState;
	AutoRef<IShaderProgram>			mGpuProgram;
public:
	IRenderPipeline();
	~IRenderPipeline();
	
	TR_FUNCTION()
	void BindRasterizerState(IRasterizerState* State);
	TR_FUNCTION()
	void BindDepthStencilState(IDepthStencilState* State);
	TR_FUNCTION()
	void BindBlendState(IBlendState* State);	
	TR_FUNCTION()
	void BindGpuProgram(IShaderProgram* pGpuPrgram);

	TR_FUNCTION()
	IRasterizerState* GetRasterizerState() {
		return mRasterizerState;
	}
	TR_FUNCTION()
	IDepthStencilState* GetDepthStencilState() {
		return mDepthStencilState;
	}
	TR_FUNCTION()
	IBlendState* GetBindBlendState() {
		return mBlendState;
	}
	TR_FUNCTION()
	IShaderProgram* GetGpuProgram()
	{
		return mGpuProgram;
	}
	
	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout) = 0;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* Layout) = 0;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) = 0;
public:
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4		mBlendFactor;
	TR_MEMBER()
	UINT			mSampleMask;
};

NS_END