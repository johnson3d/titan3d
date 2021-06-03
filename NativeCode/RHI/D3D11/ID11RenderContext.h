#pragma once
#include "D11PreHead.h"
#include "../IRenderContext.h"

NS_BEGIN

class ID11RenderSystem;
class ID11CommandList;

class ID11RenderContext : public IRenderContext
{
public:
	ID11RenderContext();
	~ID11RenderContext();

	static ID11RenderContext* DefaultRenderContext;

	virtual ERHIType GetRHIType() override
	{
		return RHT_D3D11;
	}
	virtual int GetShaderModel() override
	{
		return 5;
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
	virtual IBlendState* CreateBlendState(const IBlendStateDesc* desc) override;
	virtual IDepthStencilState* CreateDepthStencilState(const IDepthStencilStateDesc* desc) override;
	//shader
	virtual IShaderProgram* CreateShaderProgram(const IShaderProgramDesc* desc) override;
	virtual IVertexShader* CreateVertexShader(const IShaderDesc* desc) override;
	virtual IPixelShader* CreatePixelShader(const IShaderDesc* desc) override;
	virtual IComputeShader* CreateComputeShader(const IShaderDesc* desc) override;

	virtual IConstantBuffer* CreateConstantBuffer(const IConstantBufferDesc* desc) override;

	virtual IFence* CreateFence() override;

	virtual ICommandList* GetImmCommandList() override;

	virtual void FlushImmContext() override;
public:
	ID3D11Device*					mDevice;
	ID3D11Debug*					mD3dDebug;
	
	D3D_FEATURE_LEVEL               mFeatureLevel;
	ID11CommandList*				mImmCmdList;

	VCritical						mHWContextLocker;
	ID3D11DeviceContext*			mHardwareContext;

	//IDXGIDevice*					mDXGIDevice;
	ID11RenderSystem*				mSystem;
public:
	bool Init(ID11RenderSystem* sys, const IRenderContextDesc* desc);
	//static bool ReflectShader(IShaderDesc* desc);
	//static bool HLSLcc(IShaderDesc* desc);
	HRESULT CheckContext(ID3D11DeviceContext* dc)
	{
		if (mD3dDebug == nullptr)
			return S_OK;
		return mD3dDebug->ValidateContext(dc);
	}
};

NS_END