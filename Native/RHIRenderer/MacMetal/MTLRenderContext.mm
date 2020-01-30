#include "MTLRenderContext.h"
#include "MTLCommandList.h"
#include "MTLPass.h"
#include "MTLGeometryMesh.h"
#include "MTLVertexBuffer.h"
#include "MTLIndexBuffer.h"
#include "MTLFrameBuffers.h"
#include "MTLRenderTargetView.h"
#include "MTLDepthStencilView.h"
#include "MTLShaderResourceView.h"
#include "MTLSwapChain.h"
#include "MTLShaderProgram.h"
#include "MTLConstantBuffer.h"
#include "MTLInputLayout.h"
#include "MTLVertexShader.h"
#include "MTLPixelShader.h"
#include "MTLRenderPipeline.h"
#include "MTLBlendState.h"
#include "MTLDepthStencilState.h"
#include "MTLRasterizerState.h"
#include "MTLSamplerState.h"


NS_BEGIN
    
MtlContext::MtlContext()
{
	m_pDevice = nil;
	m_pCmdQueue = nil;
	m_pOneByteBuffer = nil;
	m_pOnePixelTex2D = nil;
	m_refMtlDrawable = nil;
	m_refMtlLayer = nil;
}

MtlContext::~MtlContext()
{
}

ICommandList* MtlContext::CreateCommandList(const ICommandListDesc* desc)
{
	MtlCmdList* pCmdList = new MtlCmdList();
	if (pCmdList->Init(this) == false)
	{
		pCmdList->Release();
		return nullptr;
	}

	return pCmdList;
}

ISwapChain* MtlContext::CreateSwapChain(const ISwapChainDesc* pDesc)
{
	auto pSwapChain = new MtlSwapChain();
	if (pSwapChain->Init(this, pDesc)==false)
	{
		pSwapChain->Release();
		return nullptr;
	}
	return pSwapChain;
}

IPass* MtlContext::CreatePass()
{
	auto pMeshDrawPass = new MtlMeshDrawPass();
	return pMeshDrawPass;
}

IRenderPipeline* MtlContext::CreateRenderPipeline(const IRenderPipelineDesc* pDesc)
{
	auto pMtlPipelineState = new MtlPipelineState();
	if (pMtlPipelineState->Init(this, pDesc) == false)
	{
		pMtlPipelineState->Release();
		return nullptr;
	}
	return pMtlPipelineState;
}

IVertexBuffer* MtlContext::CreateVertexBuffer(const IVertexBufferDesc* pDesc)
{
	auto pVB = new MtlVertexBuffer();
	if (pVB->Init(this, pDesc) == false)
	{
		pVB->Release();
		return nullptr;
	}
	return pVB;
}

IIndexBuffer* MtlContext::CreateIndexBuffer(const IIndexBufferDesc* pDesc)
{
	auto pIB = new MtlIndexBuffer();
	if (pIB->Init(this, pDesc) == false)
	{
		pIB->Release();
		return nullptr;
	}
	return pIB;
}
IGeometryMesh* MtlContext::CreateGeometryMesh()
{
	return new MtlGeometryMesh();
}

IFrameBuffers* MtlContext::CreateFrameBuffers(const IFrameBuffersDesc* pDesc)
{
	auto pFrameBuffer = new MtlFrameBuffer();
	if(pFrameBuffer->Init(this, pDesc) == false)
	{
		pFrameBuffer->Release();
		return nullptr;
	}
	return pFrameBuffer;
}

IRenderTargetView* MtlContext::CreateRenderTargetView(const IRenderTargetViewDesc* pDesc)
{
	auto pRTV = new MtlRTV();
	if (pRTV->Init(this, pDesc) == false)
	{
		pRTV->Release();
		return nullptr;
	}
	return pRTV;
}

IDepthStencilView* MtlContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* pDesc)
{
	auto pDSV = new MtlDepthStencilView();
	if (pDSV->Init(this, pDesc) == false)
	{
		pDSV->Release();
		return nullptr;
	}
	return pDSV;
}

ITexture2D* MtlContext::CreateTexture2D(const ITexture2DDesc* pDesc)
{
	auto pTex2d = new MtlTexture2D();
	if (pTex2d->Init(this, pDesc) == false)
	{
		pTex2d->Release();
		return nullptr;
	}
	return pTex2d;
}

IShaderResourceView* MtlContext::CreateShaderResourceView(const IShaderResourceViewDesc* pDesc)
{
	auto pSRV = new MtlShaderResView();
	if (pSRV->Init(this, pDesc) == false)
	{
		pSRV->Release();
		return nullptr;
	}
	pSRV->GetResourceState()->SetStreamState(SS_Valid);
	return pSRV;
}

IShaderResourceView* MtlContext::LoadShaderResourceView(const char* FilePathName)
{
	auto pSRV = new MtlShaderResView();
	if (pSRV->CreateSRVFromFile(this, FilePathName, false) == false)
	{
		pSRV->Release();
		return nullptr;
	}
	return pSRV;
}

ISamplerState* MtlContext::CreateSamplerState(const ISamplerStateDesc* pDesc)
{
	MtlSamplerState* pSamplerState = new MtlSamplerState();
	if (pSamplerState->Init(this, pDesc) == false)
	{
		pSamplerState->Release();
		return nullptr;
	}
	return pSamplerState;
}

IRasterizerState* MtlContext::CreateRasterizerState(const IRasterizerStateDesc* pDesc)
{
	auto pRasterState = new MtlRasterState();
	if (pRasterState->Init(this, pDesc) == false)
	{
		pRasterState->Release();
		return nullptr;
	}
	return pRasterState;
}

IDepthStencilState* MtlContext::CreateDepthStencilState(const IDepthStencilStateDesc* pDesc)
{
	auto pDepthStencilState = new MtlDepthStencilState();
	if (pDepthStencilState->Init(this, pDesc) == false)
	{
		pDepthStencilState->Release();
		return nullptr;
	}
	return pDepthStencilState;
}

IBlendState* MtlContext::CreateBlendState(const IBlendStateDesc* pDesc)
{
	auto pMtlBlendState = new MtlBlendState();
	if (pMtlBlendState->Init(this, pDesc) == false)
	{
		pMtlBlendState->Release();
		return nullptr;
	}
	return pMtlBlendState;
}

//shader
IShaderProgram* MtlContext::CreateShaderProgram(const IShaderProgramDesc* pDesc)
{
	MtlGpuProgram* pGpuProgram = new MtlGpuProgram();
	if (pGpuProgram->Init(this, pDesc) == false)
	{
		pGpuProgram->Release();
		pGpuProgram = nullptr;
	}
	return pGpuProgram;
}

IVertexShader* MtlContext::CreateVertexShader(const IShaderDesc* pDesc)
{
	MtlVertexShader* pVS = new MtlVertexShader();
	if (pVS->Init(this, pDesc) == false)
	{
		pVS->Release();
		return nullptr;
	}
	return pVS;
}

IPixelShader* MtlContext::CreatePixelShader(const IShaderDesc* pDesc)
{
	MtlPixelShader* pPS = new MtlPixelShader();
	if (pPS->Init(this, pDesc) == false)
	{
		pPS->Release();
		return nullptr;
	}
	return pPS;
}

IComputeShader* MtlContext::CreateComputeShader(const IShaderDesc* desc)
{
	return nullptr;
}

IInputLayout* MtlContext::CreateInputLayout(const IInputLayoutDesc* pDesc)
{
	MtlInputLayout* pInputLayout = new MtlInputLayout();
	if (pInputLayout->Init(this, pDesc) == false)
	{
		pInputLayout->Release();
		return nullptr;
	}
	return pInputLayout;
}

IConstantBuffer* MtlContext::CreateConstantBuffer(const IConstantBufferDesc* pDesc)
{
	MtlConstBuffer* pCB = new MtlConstBuffer();
	if (pCB->Init(this, pDesc) == false)
	{
		pCB->Release();
		return nullptr;
	}
	return pCB;
}

bool MtlContext::Init(const IRenderContextDesc* pDesc)
{
	m_pDevice = MTLCreateSystemDefaultDevice();
	m_pCmdQueue = [m_pDevice newCommandQueue];
	m_pOneByteBuffer = [m_pDevice newBufferWithLength : 1 options : MTLResourceCPUCacheModeDefaultCache];

	MTLTextureDescriptor* pMtlTexDesc = [MTLTextureDescriptor new];
	pMtlTexDesc.textureType = MTLTextureType2D;
	pMtlTexDesc.width = 1;
	pMtlTexDesc.height = 1;
	pMtlTexDesc.depth = 1;
	pMtlTexDesc.arrayLength = 1;
	pMtlTexDesc.sampleCount = 1;
	pMtlTexDesc.mipmapLevelCount = 1;
	pMtlTexDesc.pixelFormat = MTLPixelFormatBGRA8Unorm;
	pMtlTexDesc.usage = MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead;
	m_pOnePixelTex2D = [m_pDevice newTextureWithDescriptor : pMtlTexDesc];

	[pMtlTexDesc release];
	pMtlTexDesc = nil;
	return true;
}


NS_END