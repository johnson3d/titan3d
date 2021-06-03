#pragma once
#include "../IRenderContext.h"
#import <Metal/Metal.h>
#import <QuartzCore/CAMetalLayer.h>


NS_BEGIN

class MtlContext : public IRenderContext
{
public:
	MtlContext();
	~MtlContext();

	virtual ERHIType GetRHIType() override
	{
		return RHIType_Metal;
	}

	virtual ICommandList* CreateCommandList(const ICommandListDesc* pDesc) override;
	
	virtual IDrawCall* CreateDrawCall() override;

	virtual IVertexBuffer* CreateVertexBuffer(const IVertexBufferDesc* pDesc) override;
	virtual IIndexBuffer* CreateIndexBuffer(const IIndexBufferDesc* pDesc) override;
	virtual IGeometryMesh* CreateGeometryMesh() override;
	virtual IInputLayout* CreateInputLayout(const IInputLayoutDesc* pDesc) override;
	

	virtual ITexture2D* CreateTexture2D(const ITexture2DDesc* pDesc) override;
	virtual IRenderTargetView* CreateRenderTargetView(const IRenderTargetViewDesc* pDesc) override;
	virtual IDepthStencilView* CreateDepthRenderTargetView(const IDepthStencilViewDesc* pDesc) override;
	virtual IShaderResourceView* CreateShaderResourceView(const IShaderResourceViewDesc* pDesc) override;
	virtual IShaderResourceView* LoadShaderResourceView(const char* pFileName) override;
	virtual ISamplerState* CreateSamplerState(const ISamplerStateDesc* pDesc) override;
	virtual IFrameBuffers* CreateFrameBuffers(const IFrameBuffersDesc* pDesc) override;
	virtual ISwapChain* CreateSwapChain(const ISwapChainDesc* pDesc) override;

	virtual IRenderPipeline* CreateRenderPipeline(const IRenderPipelineDesc* pDesc) override;
	virtual IRasterizerState* CreateRasterizerState(const IRasterizerStateDesc* pDesc) override;
	virtual IDepthStencilState* CreateDepthStencilState(const IDepthStencilStateDesc* pDesc) override;
	virtual IBlendState* CreateBlendState(const IBlendStateDesc* pDesc) override;

	//shader
	virtual IShaderProgram* CreateShaderProgram(const IShaderProgramDesc* pDesc) override;
	virtual IVertexShader* CreateVertexShader(const IShaderDesc* pDesc) override;
	virtual IPixelShader* CreatePixelShader(const IShaderDesc* pDesc) override;
	virtual IComputeShader* CreateComputeShader(const IShaderDesc* pDesc) override;

	virtual IConstantBuffer* CreateConstantBuffer(const IConstantBufferDesc* pDesc) override;

public:
	bool Init(const IRenderContextDesc* pDesc);


public:
	id<MTLDevice> m_pDevice;
	id<MTLCommandQueue> m_pCmdQueue;
	id<MTLBuffer> m_pOneByteBuffer;
	id<MTLTexture> m_pOnePixelTex2D;
	id<CAMetalDrawable> m_refMtlDrawable; // the whole rhi system must only have one and only one mtl_drawable!
	CAMetalLayer* m_refMtlLayer;
};

NS_END