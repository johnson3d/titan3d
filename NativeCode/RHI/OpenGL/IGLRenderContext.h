#pragma once
#include "../IRenderContext.h"
#include "GLPreHead.h"
#include "IGLRenderSystem.h"

NS_BEGIN

class IGLRenderSystem;
class IGLCommandList;

class IGLRenderContext : public IRenderContext
{
public:
	IGLRenderContext();
	~IGLRenderContext();
	virtual ERHIType GetRHIType() override
	{
		return RHT_OGL;
	}
	virtual int GetShaderModel() override
	{
		return mShaderModel;
	}

	virtual void Cleanup() override;

	virtual ISwapChain* CreateSwapChain(const ISwapChainDesc* desc) override;
	virtual ICommandList* CreateCommandList(const ICommandListDesc* desc) override;
	
	virtual IDrawCall* CreateDrawCall() override;

	virtual IRenderPipeline* CreateRenderPipeline(const IRenderPipelineDesc* desc) override;

	//������Ϣ
	virtual IVertexBuffer* CreateVertexBuffer(const IVertexBufferDesc* desc) override;
	virtual IIndexBuffer* CreateIndexBuffer(const IIndexBufferDesc* desc) override;
	virtual IInputLayout* CreateInputLayout(const IInputLayoutDesc* desc) override;
	virtual IGeometryMesh* CreateGeometryMesh() override;

	virtual IIndexBuffer* CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer) override;
	virtual IVertexBuffer* CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer) override;
	//��Դ��Ϣ
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

	virtual ICommandList* GetImmCommandList() override;
	virtual void FlushImmContext() override;
	//virtual void Present(UINT SyncInterval, UINT Flags) override;
public:
	int							mShaderModel;
	std::string					mGLExtensions;

	struct vglExtension
	{
		bool ARB_multisample;
		bool ARB_framebuffer_sRGB;
		bool EXT_framebuffer_sRGB;
		bool ARB_create_context;
		bool ARB_create_context_profile;
		bool EXT_create_context_es2_profile;
		bool ARB_create_context_robustness;
		bool EXT_swap_control;
		bool ARB_pixel_format;
		bool ARB_context_flush_control;
		bool EXT_texture_compression_s3tc;
		bool IMG_texture_compression_pvrtc;
		bool OVR_multiview;
		bool OVR_multiview2;
		bool OVR_multiview_multisampled_render_to_texture;
		bool EXT_texture_buffer;
	};
	vglExtension				mVglExt;
public:
	IRenderContextDesc			mDesc;
	
	AutoRef<IGLCommandList>		mImmContext;

	IGLCommandList* GetGLImmCommandList()
	{
		return mImmContext;
	}

#if defined(PLATFORM_WIN)
	HDC					mDC;
	HGLRC				mContext;
#elif defined(PLATFORM_DROID)
	EGLContext			mEglContext;
	EGLDisplay			mEglDisplay;
	EGLConfig			mConfig;
	std::vector<EGLConfig>		mDeviceConfigs;
	std::vector<EGLConfigParms>	mConfigParams;

	EGLConfig MatchConfig(const EGLConfigParms& defaultParms);
	
	EGLSurface			mSurfaceForOldSchoolPhone;
	void CreateSurfaceAndMakeCurrent(void* winHandle);
#elif defined(PLATFORM_IOS)

#endif
public:
	bool Init(IGLRenderSystem* sys, const IRenderContextDesc* desc);
	bool HasExtension(const char* ext);
private:
	bool Init2(void* winHandle);
};

NS_END