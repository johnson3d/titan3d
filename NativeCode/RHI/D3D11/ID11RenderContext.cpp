#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11RenderPipeline.h"
#include "ID11ConstantBuffer.h"
#include "ID11VertexBuffer.h"
#include "ID11IndexBuffer.h"
#include "ID11RenderTargetView.h"
#include "ID11DepthStencilView.h"
#include "ID11ShaderResourceView.h"
#include "ID11UnorderedAccessView.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11ComputeShader.h"
#include "ID11SwapChain.h"
#include "ID11RenderSystem.h"
#include "ID11Texture2D.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11InputLayout.h"
#include "ID11SamplerState.h"
#include "ID11RasterizerState.h"
#include "ID11DepthStencilState.h"
#include "ID11BlendState.h"
#include "ID11FrameBuffers.h"
#include "ID11ShaderProgram.h"
#include "ID11Fence.h"
#include "ID11Pass.h"

#define new VNEW

extern "C" bool ReflectShader(EngineNS::IShaderDesc* desc);

NS_BEGIN

ID11RenderContext* ID11RenderContext::DefaultRenderContext = nullptr;

ID11RenderContext::ID11RenderContext()
{
	mDevice = nullptr;
	mD3dDebug = nullptr;
	mFeatureLevel = D3D_FEATURE_LEVEL_11_1;
	mImmCmdList = nullptr;
	mHardwareContext = nullptr;
	mDefinedAnnotation = nullptr;
	//mDXGIDevice = nullptr;
	mSystem = nullptr;

	DefaultRenderContext = this;
}


ID11RenderContext::~ID11RenderContext()
{
	DefaultRenderContext = nullptr;

	Safe_Release(mDefinedAnnotation);
	Safe_Release(mHardwareContext);
	Safe_Release(mImmCmdList);

	if (mD3dDebug != nullptr)
	{
		HRESULT hr = hr = mD3dDebug->ReportLiveDeviceObjects(D3D11_RLDO_DETAIL);
		mD3dDebug->Release();
	}
	Safe_Release(mDevice);
	//Safe_Release(mDXGIDevice);
	Safe_Release(mSystem);
}

bool ID11RenderContext::Init(ID11RenderSystem* sys, const IRenderContextDesc* desc)
{
	if (sys)
		sys->AddRef();
	Safe_Release(mSystem);
	mSystem = sys;
	UINT createDeviceFlags = 0;
	if(desc->CreateDebugLayer!=FALSE)
		createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
	D3D_FEATURE_LEVEL featureLevels[] =
	{
		D3D_FEATURE_LEVEL_11_0,
		D3D_FEATURE_LEVEL_11_1,
		D3D_FEATURE_LEVEL_10_0,
	};
	UINT numFeatureLevels = ARRAYSIZE(featureLevels);

	auto hr = D3D11CreateDevice(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL/*(HMODULE)desc->AppHandle*/, createDeviceFlags,
		featureLevels, numFeatureLevels, D3D11_SDK_VERSION, &mDevice, &mFeatureLevel, &mHardwareContext);
	if (FAILED(hr))
		return false;

	ID3D11DeviceContext* pImmContext;
	hr = mDevice->CreateDeferredContext(0, &pImmContext);

	mImmCmdList = new ID11CommandList();
	mImmCmdList->InitD11Point(this, pImmContext);
	pImmContext->Release();

	
	//mDevice->QueryInterface(IID_IDXGIDevice, (void**)&mDXGIDevice);

	hr = mDevice->QueryInterface(__uuidof(ID3D11Debug), reinterpret_cast<void**>(&mD3dDebug));
	if (SUCCEEDED(hr))
	{
		
	}
	hr = mHardwareContext->QueryInterface(__uuidof(ID3DUserDefinedAnnotation), reinterpret_cast<void**>(&mDefinedAnnotation));
	if (SUCCEEDED(hr))
	{
		VFX_LTRACE(ELTT_Graphics, "ID3DUserDefinedAnnotation Query failed:%d\r\n", hr); 
	}
	return true;
}

void ID11RenderContext::FlushImmContext()
{
	mImmCmdList->EndCommand();
	mImmCmdList->Commit(this);
}

ISwapChain* ID11RenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new ID11SwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* ID11RenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new ID11CommandList();
	if (cmd_list->Init(this, desc) == false)
	{
		cmd_list->Release();
		return nullptr;
	}
	return cmd_list;
}

IDrawCall* ID11RenderContext::CreateDrawCall()
{
	auto pass = new ID11DrawCall();
	return pass;
}

IRenderPipeline* ID11RenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new ID11RenderPipeline();
	rpl->BindBlendState(desc->Blend);
	rpl->BindDepthStencilState(desc->DepthStencil);
	rpl->BindRasterizerState(desc->Rasterizer);
	rpl->BindGpuProgram(desc->GpuProgram);
	return rpl;
}

IVertexBuffer* ID11RenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto vb = new ID11VertexBuffer();
	if (vb->Init(this, desc) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IIndexBuffer* ID11RenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto ib = new ID11IndexBuffer();
	if (ib->Init(this, desc) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IIndexBuffer* ID11RenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto ib = new ID11IndexBuffer();
	if (ib->Init(this, desc, pBuffer) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IVertexBuffer* ID11RenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto vb = new ID11VertexBuffer();
	if (vb->Init(this, desc, pBuffer) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IGeometryMesh* ID11RenderContext::CreateGeometryMesh()
{
	return new IGeometryMesh();
}

IFrameBuffers* ID11RenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new ID11FrameBuffers();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}
IRenderTargetView* ID11RenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new ID11RenderTargetView();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}

IDepthStencilView* ID11RenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new ID11DepthStencilView();
	if (drt->Init(this, desc) == false)
	{
		drt->Release();
		return nullptr;
	}
	return drt;
}

ITexture2D* ID11RenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	auto texture = new ID11Texture2D();
	if (texture->Init(this, desc) == false)
	{
		texture->Release();
		return nullptr;
	}
	return texture;
}

IShaderResourceView* ID11RenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	auto view = new ID11ShaderResourceView();
	if (view->Init(this, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	view->GetResourceState()->SetStreamState(SS_Valid);
	return view;
}

IGpuBuffer* ID11RenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	ID3D11Buffer* pBuffer;
	if (pInitData)
	{
		D3D11_SUBRESOURCE_DATA InitData;
		InitData.pSysMem = pInitData;
		auto hr = mDevice->CreateBuffer((D3D11_BUFFER_DESC*)desc, &InitData, &pBuffer);
		if (hr != S_OK)
			return nullptr;
	}
	else
	{
		auto hr = mDevice->CreateBuffer((D3D11_BUFFER_DESC*)desc, NULL, &pBuffer);
		if (hr != S_OK)
			return nullptr;
	}

	auto result = new ID11GpuBuffer();
	result->mBuffer = pBuffer;
	result->mDesc = *((D3D11_BUFFER_DESC*)desc);

	result->GetResourceState()->SetStreamState(SS_Valid);
	result->GetResourceState()->SetResourceSize(desc->ByteWidth);
	return result;
}

IShaderResourceView* ID11RenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	auto view = new ID11ShaderResourceView();
	if (view->Init(this, (ID11GpuBuffer*)pBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	view->GetResourceState()->SetStreamState(SS_Valid);
	view->GetResourceState()->SetResourceSize(pBuffer->GetResourceState()->GetResourceSize());
	return view;
}

IUnorderedAccessView* ID11RenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	auto view = new ID11UnorderedAccessView();
	if (view->Init(this, ((ID11GpuBuffer*)pBuffer)->mBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	return view;
}

ISamplerState* ID11RenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	auto sampler = new ID11SamplerState();
	if (sampler->Init(this, desc) == false)
	{
		sampler->Release();
		return nullptr;
	}
	return sampler;
}
IRasterizerState* ID11RenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	auto state = new ID11RasterizerState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IDepthStencilState* ID11RenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	auto state = new ID11DepthStencilState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IBlendState* ID11RenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	auto state = new ID11BlendState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IShaderResourceView* ID11RenderContext::LoadShaderResourceView(const char* file)
{
	return nullptr;
}
//shader
IShaderProgram* ID11RenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new ID11ShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* ID11RenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new ID11VertexShader();
	if (vs->Init(this, desc) == false)
	{
		vs->Release();
		return nullptr;
	}
	if (desc->Reflector == nullptr)
	{
		ReflectShader((IShaderDesc*)desc);
	}
	return vs;
}

IPixelShader* ID11RenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new ID11PixelShader();
	if (ps->Init(this, desc) == false)
	{
		ps->Release();
		return nullptr;
	}
	if (desc->Reflector != nullptr)
	{
		ReflectShader((IShaderDesc*)desc);
	}
	return ps;
}

IComputeShader* ID11RenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	auto cs = new ID11ComputeShader();
	if (cs->Init(this, desc) == false)
	{
		cs->Release();
		return nullptr;
	}
	if (desc->Reflector == nullptr)
	{
		ReflectShader((IShaderDesc*)desc);
	}
	return cs;
}

IInputLayout* ID11RenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new ID11InputLayout();
	if (layout->Init(this, desc) == false)
	{
		layout->Release();
		return nullptr;
	}
	return layout;
}

IConstantBuffer* ID11RenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new ID11ConstantBuffer();
	if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

IFence* ID11RenderContext::CreateFence()
{
	auto result = new ID11Fence();
	if (result->Init(mDevice) == false)
	{
		result->Release();
		return nullptr;
	}
	return result;
}

ICommandList* ID11RenderContext::GetImmCommandList()
{
	return mImmCmdList;
}

NS_END