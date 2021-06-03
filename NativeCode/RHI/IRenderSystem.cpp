#include "IRenderSystem.h"
#include "IRenderContext.h"
#include "ICommandList.h"
#include "IVertexBuffer.h"
#include "IIndexBuffer.h"
#include "IRenderTargetView.h"
#include "IDepthStencilView.h"
#include "IConstantBuffer.h"
#include "IDrawCall.h"
#include "ISwapChain.h"
#include "IShaderResourceView.h"
#include "IInputLayout.h"
#include "ISamplerState.h"

#include "../Base/thread/vfxthread.h"

#if defined(PLATFORM_WIN)
#include "VirtualDevice/INullRenderSystem.h"
#include "D3D11/ID11RenderSystem.h"
#include "OpenGL/IGLRenderSystem.h"
//#include "Vulkan/IVKRenderSystem.h"
#define USE_VirtualDevice
#define USE_D11
#define USE_GL
//#define USE_VK
#elif defined(PLATFORM_DROID)
#include "OpenGL/IGLRenderSystem.h"
//#include "Vulkan/IVKRenderSystem.h"
#define USE_GL
//#define USE_VK

#elif defined(PLATFORM_IOS)
#include "MacMetal/MTLRenderSystem.h"
#endif

#define new VNEW

NS_BEGIN

IRenderSystem* IRenderSystem::Instance = nullptr;

IRenderSystem::IRenderSystem()
{
	mRHIType = RHT_D3D11;
}


IRenderSystem::~IRenderSystem()
{
}

IRenderSystem* IRenderSystem::CreateRenderSystem(ERHIType type, const IRenderSystemDesc* pDesc)
{
	GraphicsThreadId = vfxThread::GetCurrentThreadId();
	switch (type)
	{
	case RHT_VirtualDevice:
	{
#if defined(USE_VirtualDevice)
		auto sys = new INullRenderSystem();
		if (sys->Init(pDesc) == false)
		{
			sys->Release();
			return nullptr;
		}
		sys->mRHIType = type;
		IRenderSystem::Instance = sys;
		return sys;
#else
		return nullptr;
#endif
	}
	break;
	case RHT_D3D11:
	{
#if defined(USE_D11)
		auto sys = new ID11RenderSystem();
		if (sys->Init(pDesc) == false)
		{
			sys->Release();
			return nullptr;
		}
		sys->mRHIType = type;
		IRenderSystem::Instance = sys;
		return sys;
#else
		return nullptr;
#endif
	}
	case RHT_VULKAN:
	{
#if defined(USE_VK)
		auto sys = new IVKRenderSystem();
		if (sys->Init(pDesc) == false)
		{
			sys->Release();
			return nullptr;
		}
		sys->mRHIType = type;
		IRenderSystem::Instance = sys;
		return sys;
#else
		return nullptr;
#endif
	}
	case RHT_OGL:
	{
#if defined(USE_GL)
		auto sys = new IGLRenderSystem();
		if (sys->Init(pDesc) == false)
		{
			sys->Release();
			return nullptr;
		}
		sys->mRHIType = type;
		IRenderSystem::Instance = sys;
		return sys;
#else
		return nullptr;
#endif
	}
	case RHIType_Metal:
	{
#ifdef PLATFORM_IOS
		MtlSystem* pMtlSystem = new MtlSystem();
		if (pMtlSystem->Init(pDesc) == false)
		{
			pMtlSystem->Release();
			return nullptr;
		}
		return pMtlSystem;
#else
		return nullptr;
#endif
	}
	}
	return nullptr;
}

NS_END

//using namespace EngineNS;
//
//extern "C"
//{
//	VFX_API IRenderContext* SDK_IRenderSystem_CreateContext(IRenderSystem* self, UINT Adapter, void* windows, vBOOL createDebugLayer)
//	{
//		RHI_ASSERT(self);
//		IRenderContextDesc desc;
//		desc.AdapterId = Adapter;
//		desc.AppHandle = windows;
//		desc.CreateDebugLayer = createDebugLayer;
//		self->GetContextDesc(Adapter, &desc);
//		return self->CreateContext(&desc);
//	}
//}