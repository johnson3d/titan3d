#pragma once
#include "../IRenderContext.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderSystem;

class INullRenderContext : public IRenderContext
{
public:
	INullRenderContext();
	~INullRenderContext();

	virtual ERHIType GetRHIType() override
	{
		return RHT_VULKAN;
	}

	virtual ISwapChain* CreateSwapChain(const ISwapChainDesc* desc) override;
	virtual ICommandList* CreateCommandList(const ICommandListDesc* desc) override;
	
	virtual IDrawCall* CreateDrawCall() override;

	virtual IRenderPipeline* CreateRenderPipeline(const IRenderPipelineDesc* desc) override;

	virtual IVertexBuffer* CreateVertexBuffer(const IVertexBufferDesc* desc) override;
	virtual IIndexBuffer* CreateIndexBuffer(const IIndexBufferDesc* desc) override;
	virtual IInputLayout* CreateInputLayout(const IInputLayoutDesc* desc) override;
	virtual IGeometryMesh* CreateGeometryMesh() override;

	virtual IIndexBuffer* CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer) override;
	virtual IVertexBuffer* CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer) override;

	virtual IFrameBuffers* CreateFrameBuffers(const IFrameBuffersDesc* desc) override;
	virtual IRenderTargetView* CreateRenderTargetView(const IRenderTargetViewDesc* desc) override;
	virtual IDepthStencilView* CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc) override;
	virtual ITexture2D* CreateTexture2D(const ITexture2DDesc* desc) override;
	virtual IShaderResourceView* CreateShaderResourceView(const IShaderResourceViewDesc* desc) override;
	virtual IShaderResourceView* CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc) override;
	virtual IGpuBuffer* CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData) override;
	virtual IUnorderedAccessView* CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc) override;
	virtual IShaderResourceView* LoadShaderResourceView(const char* file) override;
	virtual ISamplerState* CreateSamplerState(const ISamplerStateDesc* desc) override;
	virtual IRasterizerState* CreateRasterizerState(const IRasterizerStateDesc* desc) override;
	virtual IDepthStencilState* CreateDepthStencilState(const IDepthStencilStateDesc* desc) override;
	virtual IBlendState* CreateBlendState(const IBlendStateDesc* desc) override;
	//shader
	virtual IShaderProgram* CreateShaderProgram(const IShaderProgramDesc* desc) override;
	virtual IVertexShader* CreateVertexShader(const IShaderDesc* desc) override;
	virtual IPixelShader* CreatePixelShader(const IShaderDesc* desc) override;
	virtual IComputeShader* CreateComputeShader(const IShaderDesc* desc) override;

	virtual IConstantBuffer* CreateConstantBuffer(const IConstantBufferDesc* desc) override;

public:
	TObjectHandle<INullRenderSystem>	mRenderSystem;
};

NS_END