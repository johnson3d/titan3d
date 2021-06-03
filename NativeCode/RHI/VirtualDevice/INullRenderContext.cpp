#include "INullRenderSystem.h"
#include "INullRenderContext.h"
#include "INullCommandList.h"
#include "INullRenderPipeline.h"
#include "INullConstantBuffer.h"
#include "INullVertexBuffer.h"
#include "INullIndexBuffer.h"
#include "INullRenderTargetView.h"
#include "INullDepthStencilView.h"
#include "INullVertexShader.h"
#include "INullPixelShader.h"
#include "INullInputLayout.h"
#include "INullSwapChain.h"
#include "INullFrameBuffers.h"
#include "INullShaderProgram.h"
#include "INullPass.h"
#include "INullDepthStencilState.h"
#include "INullBlendState.h"
#include "INullRasterizerState.h"
#include "INullSamplerState.h"

#include <set>

#define new VNEW

NS_BEGIN

INullRenderContext::INullRenderContext()
{
	
}


INullRenderContext::~INullRenderContext()
{
	
}

ISwapChain* INullRenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new INullSwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* INullRenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new INullCommandList();
	return cmd_list;
}


IDrawCall* INullRenderContext::CreateDrawCall()
{
	auto pass = new INullDrawCall();
	return pass;
}


IRenderPipeline* INullRenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new INullRenderPipeline();
	return rpl;
}

IVertexBuffer* INullRenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto vb = new INullVertexBuffer();
	vb->Init(this, desc);
	return vb;
}

IIndexBuffer* INullRenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto ib = new INullIndexBuffer();
	ib->Init(this, desc);
	return ib;
}

IIndexBuffer* INullRenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	return nullptr;
}

IVertexBuffer* INullRenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	return nullptr;
}

IGeometryMesh* INullRenderContext::CreateGeometryMesh()
{
	return new IGeometryMesh();
}

IFrameBuffers* INullRenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new INullFrameBuffers();
	return rt;
}
IRenderTargetView* INullRenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new INullRenderTargetView();
	return rt;
}

IDepthStencilView* INullRenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new INullDepthStencilView();
	return drt;
}

ITexture2D* INullRenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	return nullptr;
}
IShaderResourceView* INullRenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	return nullptr;
}
IShaderResourceView* INullRenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	return nullptr;
}
IGpuBuffer* INullRenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	return nullptr;
}
IUnorderedAccessView* INullRenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	return nullptr;
}
IShaderResourceView* INullRenderContext::LoadShaderResourceView(const char* file)
{
	return nullptr;
}
ISamplerState* INullRenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	auto pResult = new INullSamplerState();
	return pResult;
}
IRasterizerState* INullRenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	auto pResult = new INullRasterizerState();
	pResult->mDesc = *desc;
	return pResult;
}
IDepthStencilState* INullRenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	auto pResult = new INullDepthStencilState();
	pResult->Init(desc);
	return pResult;
}
IBlendState* INullRenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	auto pResult = new INullBlendState();
	pResult->Init(desc);
	return pResult;
}
//shader
IShaderProgram* INullRenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new INullShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* INullRenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new INullVertexShader();
	return vs;
}

IPixelShader* INullRenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new INullPixelShader();
	return ps;
}

IComputeShader* INullRenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	return nullptr;
}

IInputLayout* INullRenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new INullInputLayout();
	return layout;
}

IConstantBuffer* INullRenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new INullConstantBuffer();
	/*if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}*/
	return cb;
}

NS_END