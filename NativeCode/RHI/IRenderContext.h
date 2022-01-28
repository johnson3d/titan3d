#pragma once
#include "PreHead.h"
#include "IRenderPipeline.h"
#include "IVertexShader.h"
#include "IPixelShader.h"

NS_BEGIN

struct IDrawCallDesc;
class IDrawCall;
class IComputeDrawcall;
class ICopyDrawcall;

struct ISwapChainDesc;
class ISwapChain;

struct ICommandListDesc;
class ICommandList;

struct IVertexBufferDesc;
class IVertexBuffer;

struct IIndexBufferDesc;
class IIndexBuffer;

class IGeometryMesh;

class IShaderDesc;
class IVertexShader;
class IPixelShader;
class IComputeShader;

struct IInputLayoutDesc;
class IInputLayout;

struct IRenderPipelineDesc;
class IRenderPipeline;

struct IRenderTargetViewDesc;
class IRenderTargetView;

struct IDepthStencilViewDesc;
class IDepthStencilView;

struct IShaderResourceViewDesc;
class IShaderResourceView;

struct IUnorderedAccessViewDesc;
class IUnorderedAccessView;

struct ITexture2DDesc;
class ITexture2D;

struct IFrameBuffersDesc;
class IFrameBuffers;

struct IRenderPassDesc;
class IRenderPass;

struct IConstantBufferDesc;
class IConstantBuffer;

struct ISamplerStateDesc;
class ISamplerState;

struct IRasterizerStateDesc;
class IRasterizerState;

struct IDepthStencilStateDesc;
class IDepthStencilState;

struct IBlendStateDesc;
class IBlendState;

struct IShaderProgramDesc;
class IShaderProgram;

struct IGpuBufferDesc;
class IGpuBuffer;

class IFence;
class ISemaphore;

struct TR_CLASS(SV_LayoutStruct = 8)
IRenderContextDesc
{
	IRenderContextDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		memset(DeviceName, 0, sizeof(DeviceName));
		AdapterId = 0;
		AppHandle = nullptr;
		DeviceId = 0;
		VendorId = 0;
		Revision = 0;
		VideoMemory = 0;
		SysMemory = 0;
		SharedMemory = 0;
		CreateDebugLayer = FALSE;
		//WindowHandle = nullptr;
		//Width = 0;
		//Height = 0;
	}
	int				AdapterId;
	UINT			DeviceId;
	UINT			VendorId;
	UINT			Revision;
	UINT64			VideoMemory;
	UINT64			SysMemory;
	UINT64			SharedMemory;
	void*			AppHandle;
	vBOOL			CreateDebugLayer;
	char			DeviceName[256];
	TR_FUNCTION()
	char* GetDeviceName() {
		return DeviceName;
	}
	//void*			WindowHandle;
	//UINT			Width;
	//UINT			Height;
	TR_DISCARD()
	std::string ToString()
	{
		return "DeviceId:";
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
IRenderContextCaps
{
	IRenderContextCaps()
	{
		SetDefault();
	}
	void SetDefault()
	{
		ShaderModel = 5;
		MaxShaderStorageBlockSize = 1024 * 1024 *4;
		MaxShaderStorageBufferBindings = 16;
		ShaderStorageBufferOffsetAlignment = 16;
		MaxVertexShaderStorageBlocks = 16;
		MaxPixelShaderStorageBlocks = 16;
		MaxComputeShaderStorageBlocks = 16;
		MaxVertexUniformBlocks = 16;
		MaxPixelUniformBlocks = 16;
		MaxUniformBufferBindings = 16;
		MaxUniformBlockSize = 65536;
		MaxTextureBufferSize = 65536;
		MaxVertexTextureImageUnits = 16;
		MaxCombinedTextureImageUnits = 16;
		SupportFloatRT = 1;
		SupportHalfRT = 1;
		SupportFloatTexture = 1;
		SupportHalfTexture = 1;
		SupportFloatTextureLinearFilter = 0;
		SupportHalfTextureLinearFilter = 1;

		MaxTexture2DArray = 1024;
	}
	int ShaderModel;
	int MaxShaderStorageBlockSize;

	int MaxShaderStorageBufferBindings;
	int ShaderStorageBufferOffsetAlignment;

	int MaxVertexShaderStorageBlocks;	
	int MaxPixelShaderStorageBlocks;

	int MaxComputeShaderStorageBlocks;
	int MaxVertexUniformBlocks;

	int MaxPixelUniformBlocks;
	int MaxUniformBufferBindings;

	int MaxUniformBlockSize;
	int MaxTextureBufferSize;

	int MaxVertexTextureImageUnits;	
	int MaxCombinedTextureImageUnits;

	int SupportFloatRT;
	int SupportHalfRT;

	int SupportFloatTexture;
	int SupportHalfTexture;

	int SupportFloatTextureLinearFilter;
	int SupportHalfTextureLinearFilter;

	int MaxTexture2DArray;
};

class TR_CLASS()
	IRenderContext : public VIUnknown
{
public:
	IRenderContext();
	~IRenderContext();

	virtual ERHIType GetRHIType() = 0;
	virtual int GetShaderModel() {
		return 5;
	}
	static void ChooseShaderModel(int sm)
	{
		mChooseShaderModel = sm;
	}

	void BeginFrame();
	void EndFrame();

	virtual ISwapChain* CreateSwapChain(const ISwapChainDesc* desc) = 0;
	virtual ICommandList* CreateCommandList(const ICommandListDesc* desc) = 0;
	
	virtual IDrawCall* CreateDrawCall() = 0;
	virtual IComputeDrawcall* CreateComputeDrawcall() = 0;
	virtual ICopyDrawcall* CreateCopyDrawcall() = 0;
	virtual IRenderPipeline* CreateRenderPipeline(const IRenderPipelineDesc* desc) = 0;

	virtual IGpuBuffer* CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData) = 0;

	virtual IVertexBuffer* CreateVertexBuffer(const IVertexBufferDesc* desc) = 0;
	virtual IIndexBuffer* CreateIndexBuffer(const IIndexBufferDesc* desc) = 0;
	virtual IInputLayout* CreateInputLayout(const IInputLayoutDesc* desc) = 0;
	virtual IGeometryMesh* CreateGeometryMesh() = 0;

	virtual IIndexBuffer* CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer) = 0;
	virtual IVertexBuffer* CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer) = 0;

	virtual IRenderPass* CreateRenderPass(const IRenderPassDesc* desc) = 0;
	virtual IFrameBuffers* CreateFrameBuffers(const IFrameBuffersDesc* desc) = 0;
	
	virtual IRenderTargetView* CreateRenderTargetView(const IRenderTargetViewDesc* desc) = 0;
	
	virtual IDepthStencilView* CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc) = 0;

	virtual ITexture2D* CreateTexture2D(const ITexture2DDesc* desc) = 0;
	
	virtual IShaderResourceView* CreateShaderResourceView(const IShaderResourceViewDesc* desc) = 0;	
	//virtual IShaderResourceView* CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc) = 0;

	virtual IUnorderedAccessView* CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc) = 0;

	virtual IShaderResourceView* LoadShaderResourceView(const char* file) = 0;
	virtual ISamplerState* CreateSamplerState(const ISamplerStateDesc* desc) = 0;
	virtual IRasterizerState* CreateRasterizerState(const IRasterizerStateDesc* desc) = 0;
	virtual IDepthStencilState* CreateDepthStencilState(const IDepthStencilStateDesc* desc) = 0;
	virtual IBlendState* CreateBlendState(const IBlendStateDesc* desc) = 0;
	//shader
	virtual IShaderProgram* CreateShaderProgram(const IShaderProgramDesc* desc) = 0;
	virtual IVertexShader* CreateVertexShader(const IShaderDesc* desc) = 0;
	virtual IPixelShader* CreatePixelShader(const IShaderDesc* desc) = 0;
	virtual IComputeShader* CreateComputeShader(const IShaderDesc* desc) = 0;
	
	virtual IConstantBuffer* CreateConstantBuffer(const IConstantBufferDesc* desc) = 0;

	IConstantBuffer* CreateConstantBuffer(IShaderProgram* program, const char* name);
	IConstantBuffer* CreateConstantBuffer(IShaderProgram* program, UINT index);
	IConstantBuffer* CreateConstantBuffer2(IShaderDesc* program, UINT index);

	virtual IFence* CreateFence() { return nullptr; }
	virtual ISemaphore* CreateGpuSemaphore() { return nullptr; }

	virtual ICommandList* GetImmCommandList()
	{
		return nullptr;
	}
	
	virtual void FlushImmContext()
	{

	}
	//void BindCurrentSwapChain(ISwapChain* swapChain);
	//virtual void Present(UINT SyncInterval, UINT Flags);
	void PushFrameResource(IRenderResource* res);
protected:
	void ProcessFrameResources();
public:
	IRenderPipeline * GetPipeline(const IRenderPipelineDesc* desc)
	{
		return nullptr;
	}
	IVertexShader * GetVertexShader(const IShaderDesc* desc)
	{
		return nullptr;
	}
	IPixelShader * GetPixelShader(const IShaderDesc* desc)
	{
		return nullptr;
	}
	void GetRenderContextCaps(IRenderContextCaps* pCaps)
	{
		*pCaps = mContextCaps;
	}
	void UnsafeSetRenderContextCaps(IRenderContextCaps* pCaps)
	{//用于在PC上模拟低配置mobile GPU
		mContextCaps = *pCaps;
	}
	UINT GetCurrentFrame()const{
		return mCurrentFrame;
	}
protected:
	UINT											mCurrentFrame;
	std::map<IRenderPipelineDesc, IRenderPipeline*>		Pipelines;
	std::map<IShaderDesc, IVertexShader*>			VertexShaders;
	std::map<IShaderDesc, IPixelShader*>			PixelShaders;

	ISwapChain*										m_pSwapChain;

	IRenderContextCaps								mContextCaps;

	VCritical						mFrameResLocker;
	std::queue<IRenderResource*>	mFrameResources;
	VCritical						mLocker;
public:
	static int				mChooseShaderModel;
};

NS_END