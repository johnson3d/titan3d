#pragma once
#include "IRenderResource.h"

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

struct IRenderPipelineDesc
{
	//IVertexShader* VertexShader;
	//IPixelShader* PixelShader;
};

class IRenderPipeline : public IRenderResource
{
protected:
	IRasterizerState* mRasterizerState;
	IDepthStencilState* mDepthStencilState;
	IBlendState* mBlendState;
public:
	IRenderPipeline();
	~IRenderPipeline();

	void BindRasterizerState(IRasterizerState* State);
	void BindDepthStencilState(IDepthStencilState* State);
	void BindBlendState(IBlendState* State);

	
	IRasterizerState* GetRasterizerState() {
		return mRasterizerState;
	}
	IDepthStencilState* GetDepthStencilState() {
		return mDepthStencilState;
	}
	IBlendState* GetBindBlendState() {
		return mBlendState;
	}

	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout) = 0;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* Layout) = 0;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) = 0;
public:
	float		mBlendFactor[4];
	UINT		mSampleMask;
};

NS_END