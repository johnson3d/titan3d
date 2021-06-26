#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "IGLRenderPipeline.h"
#include "IGLConstantBuffer.h"
#include "IGLVertexBuffer.h"
#include "IGLIndexBuffer.h"
#include "IGLRenderTargetView.h"
#include "IGLDepthStencilView.h"
#include "IGLUnorderedAccessView.h"
#include "IGLVertexShader.h"
#include "IGLPixelShader.h"
#include "IGLComputeShader.h"
#include "IGLInputLayout.h"
#include "IGLSwapChain.h"
#include "IGLFrameBuffers.h"
#include "IGLTexture2D.h"
#include "IGLShaderResourceView.h"
#include "IGLShaderProgram.h"
#include "IGLSamplerState.h"
#include "IGLRasterizerState.h"
#include "IGLDepthStencilState.h"
#include "IGLBlendState.h"
#include "IGLPass.h"

#include "../../../3rd/native/Image.Shared/XImageDecoder.h"
#include "../../../3rd/native/Image.Shared/XImageBuffer.h"

#include "../../Base/io/vfxfile.h"


#define new VNEW

NS_BEGIN

#if defined(PLATFORM_WIN)
extern std::string ShowLastErrorMessage(DWORD dwError);
#elif defined(PLATFORM_DROID)
const  int EGLMinRedBits = 5;
const  int EGLMinGreenBits = 6;
const  int EGLMinBlueBits = 5;
const  int EGLMinAlphaBits = 0;
const  int EGLMinDepthBits = 16;
const  int EGLMinStencilBits = 0;
const  int EGLMinSampleBuffers = 0;
const  int EGLMinSampleSamples = 0;

const EGLint Attributes[] = {
	EGL_RED_SIZE,       EGLMinRedBits,
	EGL_GREEN_SIZE,     EGLMinGreenBits,
	EGL_BLUE_SIZE,      EGLMinBlueBits,
	EGL_ALPHA_SIZE,     EGLMinAlphaBits,
	EGL_DEPTH_SIZE,     EGLMinDepthBits,
	EGL_STENCIL_SIZE,   EGLMinStencilBits,
	EGL_SAMPLE_BUFFERS, EGLMinSampleBuffers,
	EGL_SAMPLES,        EGLMinSampleSamples,
	EGL_RENDERABLE_TYPE,  EGL_OPENGL_ES3_BIT_KHR,//最小支持ES3
	EGL_SURFACE_TYPE, EGL_WINDOW_BIT | EGL_PBUFFER_BIT,
	EGL_CONFIG_CAVEAT,  EGL_NONE,
	EGL_NONE
};


void _LogConfigInfo(EGLDisplay display, EGLConfig  config)
{
	EGLint ResultValue = 0;
	eglGetConfigAttrib(display, config, EGL_RED_SIZE, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config : EGL_RED_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_GREEN_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_BLUE_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config :EGL_ALPHA_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_DEPTH_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_DEPTH_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_STENCIL_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_STENCIL_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_SAMPLE_BUFFERS, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SAMPLE_BUFFERS :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_BIND_TO_TEXTURE_RGB, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_BIND_TO_TEXTURE_RGB :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_SAMPLES, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SAMPLES :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_COLOR_BUFFER_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_COLOR_BUFFER_TYPE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_CONFIG_CAVEAT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFIG_CAVEAT :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_CONFIG_ID, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFIG_ID :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_CONFORMANT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFORMANT :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_LEVEL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_LEVEL :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_LUMINANCE_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_LUMINANCE_SIZE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_WIDTH, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_WIDTH :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_HEIGHT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_HEIGHT :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_PIXELS, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_PIXELS :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_MAX_SWAP_INTERVAL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_SWAP_INTERVAL :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_MIN_SWAP_INTERVAL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MIN_SWAP_INTERVAL :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_NATIVE_RENDERABLE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_RENDERABLE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_VISUAL_TYPE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_VISUAL_ID :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_RENDERABLE_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_RENDERABLE_TYPE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_SURFACE_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SURFACE_TYPE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_TYPE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_RED_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_RED_VALUE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_GREEN_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_GREEN_VALUE :	%u", ResultValue);
	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_BLUE_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_BLUE_VALUE :	%u", ResultValue);
}
#endif

IGLRenderContext::IGLRenderContext()
{
	mShaderModel = 5;
#if defined(PLATFORM_WIN)
	mDC = nullptr;
	mContext = nullptr;
#elif defined(PLATFORM_DROID)
	mEglDisplay = nullptr;
	mEglContext = nullptr;
	mConfig = nullptr;
	mSurfaceForOldSchoolPhone = nullptr;
#endif
}

IGLRenderContext::~IGLRenderContext()
{
	Cleanup();
}

void IGLRenderContext::Cleanup()
{
	if (GLSdk::ImmSDK != nullptr)
	{
		GLSdk::ImmSDK->ClearCommandList();
		GLSdk::ImmSDK = nullptr;
	}
	
	if (GLSdk::DestroyCommands != nullptr)
	{
		GLSdk::DestroyCommands->Execute();
		GLSdk::DestroyCommands = nullptr;
	}

#if defined(PLATFORM_WIN)
	if (mContext != nullptr)
	{
		wglDeleteContext(mContext);
		mContext = nullptr;
		ReleaseDC((HWND)mDesc.AppHandle, mDC);
		mDC = nullptr;
	}
#elif defined(PLATFORM_DROID)
	if (mSurfaceForOldSchoolPhone != 0)
	{
		eglDestroySurface(mEglDisplay, mSurfaceForOldSchoolPhone);
		mSurfaceForOldSchoolPhone = NULL;
	}
	if (mEglContext != NULL)
	{
		if (mEglDisplay != NULL)
			eglDestroyContext(mEglDisplay, mEglContext);
		mEglContext = NULL;
	}
	if (mEglDisplay != NULL)
	{
		eglTerminate(mEglDisplay);
		mEglDisplay = NULL;
	}
	mConfig = nullptr;
#endif
}

ISwapChain* IGLRenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new IGLSwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* IGLRenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new IGLCommandList();
	if (cmd_list->Init(this, desc, nullptr) == false)
	{
		cmd_list->Release();
		return nullptr;
	}
	return cmd_list;
}

IDrawCall* IGLRenderContext::CreateDrawCall()
{
	auto pass = new IGLDrawCall();
	IDrawCallDesc desc;
	if (pass->Init(this, &desc) == false)
	{
		pass->Release();
		return nullptr;
	}
	return pass;
}


IRenderPipeline* IGLRenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new IGLRenderPipeline();
	if (rpl->Init(this, desc) == false)
	{
		rpl->Release();
		return nullptr;
	}
	rpl->BindBlendState(desc->Blend);
	rpl->BindDepthStencilState(desc->DepthStencil);
	rpl->BindRasterizerState(desc->Rasterizer);
	rpl->BindGpuProgram(desc->GpuProgram);
	return rpl;
}

IVertexBuffer* IGLRenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto vb = new IGLVertexBuffer();
	if (vb->Init(this, desc) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IIndexBuffer* IGLRenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto ib = new IGLIndexBuffer();
	if (ib->Init(this, desc) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IIndexBuffer* IGLRenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto ib = new IGLIndexBuffer();
	if (ib->Init(this, desc, (IGLGpuBuffer*)pBuffer) == false)
	{
		ib->Release();
		return nullptr;
	}
	return ib;
}

IVertexBuffer* IGLRenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	auto vb = new IGLVertexBuffer();
	if (vb->Init(this, desc, (IGLGpuBuffer*)pBuffer) == false)
	{
		vb->Release();
		return nullptr;
	}
	return vb;
}

IGeometryMesh* IGLRenderContext::CreateGeometryMesh()
{
	return new IGLGeometryMesh();
}

IFrameBuffers* IGLRenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new IGLFrameBuffers();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}
IRenderTargetView* IGLRenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new IGLRenderTargetView();
	if (rt->Init(this, desc) == false)
	{
		rt->Release();
		return nullptr;
	}
	return rt;
}

IDepthStencilView* IGLRenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new IGLDepthStencilView();
	if (drt->Init(this, desc) == false)
	{
		drt->Release();
		return nullptr;
	}
	return drt;
}

ITexture2D* IGLRenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	auto texture = new IGLTexture2D();
	if (texture->Init(this, desc) == false)
	{
		texture->Release();
		return nullptr;
	}
	return texture;
}

IShaderResourceView* IGLRenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	auto texture = new IGLShaderResourceView();
	if (texture->Init(this, desc) == false)
	{
		texture->Release();
		return nullptr;
	}
	texture->GetResourceState()->SetStreamState(SS_Valid);
	return texture;
}

IGpuBuffer* IGLRenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	auto result = new IGLGpuBuffer();
	if (false == result->Init(this, desc, pInitData))
	{
		result->Release();
		return nullptr;
	}

	result->GetResourceState()->SetStreamState(SS_Valid);
	result->GetResourceState()->SetResourceSize(desc->ByteWidth);
	return result;
}

IShaderResourceView* IGLRenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	auto view = new IGLShaderResourceView();
	if (view->Init(this, (IGLGpuBuffer*)pBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	view->GetResourceState()->SetStreamState(SS_Valid);
	view->GetResourceState()->SetResourceSize(pBuffer->GetResourceState()->GetResourceSize());
	return view;
}

IUnorderedAccessView* IGLRenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	auto view = new IGLUnorderedAccessView();
	if (view->Init(this, (IGLGpuBuffer*)pBuffer, desc) == false)
	{
		view->Release();
		return nullptr;
	}
	return view;
}


IShaderResourceView* IGLRenderContext::LoadShaderResourceView(const char* file)
{
	return nullptr;
}

ISamplerState* IGLRenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	auto sampler = new IGLSamplerState();
	if (sampler->Init(this, desc) == false)
	{
		sampler->Release();
		return nullptr;
	}
	return sampler;
}
IRasterizerState* IGLRenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	auto state = new IGLRasterizerState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IDepthStencilState* IGLRenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	auto state = new IGLDepthStencilState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
IBlendState* IGLRenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	auto state = new IGLBlendState();
	if (state->Init(this, desc) == false)
	{
		state->Release();
		return nullptr;
	}
	return state;
}
//shader
IShaderProgram* IGLRenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new IGLShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* IGLRenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new IGLVertexShader();
	if (vs->Init(this, desc) == false)
	{
		vs->Release();
		return nullptr;
	}
	return vs;
}

IPixelShader* IGLRenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new IGLPixelShader();
	if (ps->Init(this, desc) == false)
	{
		ps->Release();
		return nullptr;
	}
	return ps;
}

IComputeShader* IGLRenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	auto cs = new IGLComputeShader();
	if (cs->Init(this, desc) == false)
	{
		cs->Release();
		return nullptr;
	}
	return cs;
}

IInputLayout* IGLRenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new IGLInputLayout();
	if (layout->Init(this, desc) == false)
	{
		layout->Release();
		return nullptr;
	}
	return layout;
}

IConstantBuffer* IGLRenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new IGLConstantBuffer();
	if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

ICommandList* IGLRenderContext::GetImmCommandList()
{
	return mImmContext;
}

//void IGLRenderContext::mImmCmdList(UINT SyncInterval, UINT Flags)
//{
//	if(mImmCmdList->GetCommandNumber()>0)
//		mImmCmdList->Execute();
//	IRenderContext::Present(SyncInterval, Flags);
//}

#if defined(PLATFORM_DROID)
EGLint GetContextRenderableType(EGLDisplay eglDisplay)
{
	const char *extensions = eglQueryString(eglDisplay, EGL_EXTENSIONS);

	// check whether EGL_KHR_create_context is in the extension string
	if (extensions != NULL && strstr(extensions, "EGL_KHR_create_context"))
	{
		VFX_LTRACE(ELTT_Graphics, "ES3 Context = %s\r\n", extensions);
		// extension is supported
		return EGL_OPENGL_ES3_BIT_KHR;
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "ES2 Context = %s\r\n", extensions);
	}
	// extension is not supported
	return EGL_OPENGL_ES2_BIT;
}
#endif

void GLAPIENTRY EngineGLDebugProc(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar* message, const void* userParam)
{
	/*if (source == 33350 && type == 33360 && id == 131202)
		return;
	else if (source == 33350 && type == 33361 && id == 131185)
		return;
	else if (source == 33350 && type == 33356 && id == 1282)
		return;*/

	//IGLRenderContext* rc = (IGLRenderContext*)userParam;

	const char* _source;
	const char* _type;
	const char* _severity;

	switch (severity)
	{
	case GL_DEBUG_SEVERITY_HIGH:
		_severity = "HIGH";
		break;
	case GL_DEBUG_SEVERITY_MEDIUM:
		_severity = "MEDIUM";
		break;
	case GL_DEBUG_SEVERITY_LOW:
		_severity = "LOW";
		break;
	case GL_DEBUG_SEVERITY_NOTIFICATION:
		_severity = "NOTIFICATION";
		return;
	default:
		_severity = "UNKNOWN";
		break;
	}

	switch (source) 
	{
		case GL_DEBUG_SOURCE_API:
			_source = "API";
			break;
		case GL_DEBUG_SOURCE_WINDOW_SYSTEM:
			_source = "WINDOW SYSTEM";
			break;
		case GL_DEBUG_SOURCE_SHADER_COMPILER:
			_source = "SHADER COMPILER";
			break;
		case GL_DEBUG_SOURCE_THIRD_PARTY:
			_source = "THIRD PARTY";
			break;
		case GL_DEBUG_SOURCE_APPLICATION:
			_source = "APPLICATION";
			break;
		case GL_DEBUG_SOURCE_OTHER:
			_source = "UNKNOWN";
			break;
		default:
			_source = "UNKNOWN";
			break;
	}

	switch (type)
	{
		case GL_DEBUG_TYPE_ERROR:
			_type = "ERROR";
			break;
		case GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR:
			_type = "DEPRECATED BEHAVIOR";
			break;
		case GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR:
			_type = "UDEFINED BEHAVIOR";
			break;
		case GL_DEBUG_TYPE_PORTABILITY:
			_type = "PORTABILITY";
			break;
		case GL_DEBUG_TYPE_PERFORMANCE:
			_type = "PERFORMANCE";
			break;
		case GL_DEBUG_TYPE_OTHER:
			_type = "OTHER";
			break;
		case GL_DEBUG_TYPE_MARKER:
			_type = "MARKER";
			break;
		default:
			_type = "UNKNOWN";
			break;
	}

	VFX_LTRACE(ELTT_Graphics, "%d: %s of %s severity, raised from %s: %s\n",
		id, _type, _severity, _source, message);
}

#if defined(PLATFORM_WIN)
#elif defined(PLATFORM_DROID)
EGLConfig IGLRenderContext::MatchConfig(const EGLConfigParms& defaultParms)
{
	EGLConfig			mConfig = nullptr;
	int ResultValue = 0;
	bool haveConfig = false;
	int finalDepthSize;
	int finalNativeVisualID;
	INT64 score = LONG_MAX;
	int matchId = -1;
	for (size_t i = 0; i < mDeviceConfigs.size(); i++)
	{
		INT64 currScore = 0;
		// Optional, Tegra-specific non-linear depth buffer, which allows for much better
		// effective depth range in relatively limited bit-depths (e.g. 16-bit)
		int bNonLinearDepth = 0;
		if (eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_DEPTH_ENCODING_NV, &ResultValue))
		{
			bNonLinearDepth = (ResultValue == EGL_DEPTH_ENCODING_NONLINEAR_NV) ? 1 : 0;
		}
		else
		{
			//// explicitly consume the egl error if EGL_DEPTH_ENCODING_NV does not exist.
			//GetError();
		}
		int r, g, b, a, d, s, sb, sc;// , nvi;
		
		r = mConfigParams[i].redSize;
		g = mConfigParams[i].greenSize;
		b = mConfigParams[i].blueSize;
		a = mConfigParams[i].alphaSize;
		d = mConfigParams[i].depthSize;
		s = mConfigParams[i].stencilSize;
		sb = mConfigParams[i].sampleBuffers;
		sc = mConfigParams[i].sampleSamples;

		// Favor EGLConfigLists by RGB, then Depth, then Non-linear Depth, then Stencil, then Alpha
		currScore = 0;
		currScore |= ((INT64)vfxMIN(abs(sb - defaultParms.sampleBuffers), 15)) << 29;
		currScore |= ((INT64)vfxMIN(abs(sc - defaultParms.sampleSamples), 31)) << 24;
		currScore |= vfxMIN(
			abs(r - defaultParms.redSize) +
			abs(g - defaultParms.greenSize) +
			abs(b - defaultParms.blueSize), 127) << 17;
		currScore |= vfxMIN(abs(d - defaultParms.depthSize), 63) << 11;
		currScore |= vfxMIN(abs(1 - bNonLinearDepth), 1) << 10;
		currScore |= vfxMIN(abs(s - defaultParms.stencilSize), 31) << 6;
		currScore |= vfxMIN(abs(a - defaultParms.alphaSize), 31) << 0;

		//#if _DEBUG
		//_LogConfigInfo(mEglDisplay, EGLConfigList[i]);
		//#endif

		if (currScore < score || !haveConfig)
		{
			mConfig = mDeviceConfigs[i];
			finalDepthSize = d;		// store depth/stencil sizes
			haveConfig = true;
			score = currScore;
			eglGetConfigAttrib(mEglDisplay, mConfig, EGL_NATIVE_VISUAL_ID, &ResultValue); finalNativeVisualID = ResultValue;

			matchId = (int)i;
		}
	}
	if(haveConfig)
	{
		VFX_LTRACE(ELTT_Graphics, "GLES Match Config1: %d", matchId);
		VFX_LTRACE(ELTT_Graphics, "Match ConfigAttrib: { r=%d;g=%d;b=%d;a=%d;d=%d;s=%d;sb=%d;}\r\n", 
			mConfigParams[matchId].redSize, 
			mConfigParams[matchId].greenSize, 
			mConfigParams[matchId].blueSize, 
			mConfigParams[matchId].alphaSize, 
			mConfigParams[matchId].depthSize,
			mConfigParams[matchId].stencilSize, 
			mConfigParams[matchId].sampleBuffers);
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "GLES Match Config1: failed");
	}

	return mConfig;
}
#endif

bool IGLRenderContext::Init(IGLRenderSystem* sys, const IRenderContextDesc* desc)
{
	mDesc = *desc;
#if defined(PLATFORM_WIN)
	mGLExtensions = wglGetExtensionsStringEXT();
#elif defined(PLATFORM_DROID)
	mEglDisplay = eglGetDisplay(0);
	if (mEglDisplay == EGL_NO_DISPLAY)
	{
		return false;
	}
	
	if (!eglInitialize(mEglDisplay, &MajorVersion, &MinorVersion))
	{
		return false;
	}
	EGLint numConfigs = 0;

	EGLBoolean result = eglChooseConfig(mEglDisplay, Attributes, NULL, 0, &numConfigs);
	if (result)
	{
		mDeviceConfigs.resize(numConfigs);
		mConfigParams.resize(numConfigs);
		result = eglChooseConfig(mEglDisplay, Attributes, &mDeviceConfigs[0], numConfigs, &numConfigs);
	}
	if (!result)
	{
		return false;
	}

	int ResultValue = 0;
	for (size_t i = 0; i < mDeviceConfigs.size(); i++)
	{
		int r, g, b, a, d, s, sb, sc;// , nvi;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_RED_SIZE, &ResultValue); r = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_GREEN_SIZE, &ResultValue); g = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_BLUE_SIZE, &ResultValue); b = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_ALPHA_SIZE, &ResultValue); a = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_DEPTH_SIZE, &ResultValue); d = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_STENCIL_SIZE, &ResultValue); s = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_SAMPLE_BUFFERS, &ResultValue); sb = ResultValue;
		eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_SAMPLES, &ResultValue); sc = ResultValue;

		// Optional, Tegra-specific non-linear depth buffer, which allows for much better
		// effective depth range in relatively limited bit-depths (e.g. 16-bit)
		int bNonLinearDepth = 0;
		if (eglGetConfigAttrib(mEglDisplay, mDeviceConfigs[i], EGL_DEPTH_ENCODING_NV, &ResultValue))
		{
			bNonLinearDepth = (ResultValue == EGL_DEPTH_ENCODING_NONLINEAR_NV) ? 1 : 0;
		}
		else
		{
			//// explicitly consume the egl error if EGL_DEPTH_ENCODING_NV does not exist.
			//GetError();
		}

		mConfigParams[i].validConfig = 1;
		mConfigParams[i].redSize = r;
		mConfigParams[i].greenSize = g;
		mConfigParams[i].blueSize = b;
		mConfigParams[i].alphaSize = a;
		mConfigParams[i].depthSize = d;
		mConfigParams[i].stencilSize = s;
		mConfigParams[i].sampleBuffers = sb;
		mConfigParams[i].sampleSamples = sc;

		VFX_LTRACE(ELTT_Graphics, "mConfig eglGetConfigAttrib(%d/%d): { r=%d;g=%d;b=%d;a=%d;d=%d;s=%d;sb=%d;sc=%d;NonLinearDepth=%d }\r\n", i, numConfigs, r, g, b, a, d, s, sb, sc, bNonLinearDepth);
	}


/*  Old Code
	//EGLint attribList[] =
	//{
	//	EGL_RED_SIZE,       5,
	//	EGL_GREEN_SIZE,     6,
	//	EGL_BLUE_SIZE,      5,
	//	EGL_ALPHA_SIZE,     8,
	//	EGL_DEPTH_SIZE,     8,
	//	EGL_STENCIL_SIZE,   8,
	//	EGL_SAMPLE_BUFFERS, 0,
	//	// if EGL_KHR_create_context extension is supported, then we will use
	//	// EGL_OPENGL_ES3_BIT_KHR instead of EGL_OPENGL_ES2_BIT in the attribute list
	//	EGL_RENDERABLE_TYPE, GetContextRenderableType(mEglDisplay),
	//	EGL_NONE
	//};

	//// Choose config
	//if (!eglChooseConfig(mEglDisplay, attribList, &mConfig, 1, &numConfigs))
	//{
	//	return false;
	//}
*/
	if (numConfigs < 1)
	{
		return false;
	}
	//mGLExtensions = eglGetExtensionsString();
#elif defined(PLATFORM_IOS)

#endif

	auto GPUFamily = (const char*)GLSdk::GetString(GL_RENDERER);
	auto GLVersion = (const char*)GLSdk::GetString(GL_VERSION);
	auto pExt = (const char*)GLSdk::GetString(GL_EXTENSIONS);
	if (pExt != nullptr)
		mGLExtensions = pExt;

	glGetIntegerv(GL_MAJOR_VERSION, &MajorVersion);
	glGetIntegerv(GL_MINOR_VERSION, &MinorVersion);
	std::string strVersion = GLVersion;
	
	if (MajorVersion == 3 && MinorVersion == 0)
	{
		mShaderModel = 3;
	}
	else if (MajorVersion == 3 && MinorVersion == 1)
	{
		mShaderModel = 4;
	}
	else if (MajorVersion == 3 && MinorVersion == 2)
	{
		mShaderModel = 5;
	}
	mContextCaps.ShaderModel = mShaderModel;

	VFX_LTRACE(ELTT_Graphics, "GPU Max ShaderModel = %d\r\n", mShaderModel);

	VFX_LTRACE(ELTT_Graphics, "GL Renderer = %s\r\n", GPUFamily);
	VFX_LTRACE(ELTT_Graphics, "GL GLVersion = %s\r\n", GLVersion);
	VFX_LTRACE(ELTT_Graphics, "GL GLExtensions = %s\r\n", pExt);

	mVglExt.ARB_multisample = HasExtension("WGL_ARB_multisample");
	mVglExt.ARB_framebuffer_sRGB = HasExtension("WGL_ARB_framebuffer_sRGB");
	mVglExt.EXT_framebuffer_sRGB = HasExtension("WGL_EXT_framebuffer_sRGB");
	mVglExt.ARB_create_context = HasExtension("WGL_ARB_create_context");
	mVglExt.ARB_create_context_profile = HasExtension("WGL_ARB_create_context_profile");
	mVglExt.EXT_create_context_es2_profile = HasExtension("WGL_EXT_create_context_es2_profile");
	mVglExt.ARB_create_context_robustness = HasExtension("WGL_ARB_create_context_robustness");
	mVglExt.EXT_swap_control = HasExtension("WGL_EXT_swap_control");
	mVglExt.ARB_pixel_format = HasExtension("WGL_ARB_pixel_format");
	mVglExt.ARB_context_flush_control = HasExtension("WGL_ARB_context_flush_control");
	mVglExt.EXT_texture_compression_s3tc = HasExtension("GL_EXT_texture_compression_s3tc") || HasExtension("GL_OES_texture_compression_S3TC");
	mVglExt.IMG_texture_compression_pvrtc = HasExtension("GL_IMG_texture_compression_pvrtc");
	mVglExt.OVR_multiview = HasExtension("GL_OVR_multiview");
	mVglExt.OVR_multiview2 = HasExtension("GL_OVR_multiview2");
	mVglExt.OVR_multiview_multisampled_render_to_texture = HasExtension("GL_OVR_multiview_multisampled_render_to_texture");
	mVglExt.EXT_texture_buffer = HasExtension("GL_EXT_texture_buffer");

	if (HasExtension("GL_AMD_gpu_shader_half_float"))
	{
		VFX_LTRACE(ELTT_Graphics, "Half Shader:AMD half\r\n", pExt);
	}
	else if (HasExtension("GL_NV_gpu_shader5"))
	{
		VFX_LTRACE(ELTT_Graphics, "Half Shader:NV shader5 half\r\n", pExt);
	}
	else if (HasExtension("GL_AMD_gpu_shader_int16"))
	{
		VFX_LTRACE(ELTT_Graphics, "Half Shader: shader_int16\r\n", pExt);
	}
	else if (HasExtension("EXT_shader_16bit_storage"))
	{
		VFX_LTRACE(ELTT_Graphics, "Half Shader:16bit storage\r\n", pExt);
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "Half Shader:none\r\n", pExt);
	}

	{
		GLint size;
		glGetIntegerv(GL_MAX_SHADER_STORAGE_BLOCK_SIZE, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxShaderStorageBlockSize = %d\r\n", size);
		mContextCaps.MaxShaderStorageBlockSize = size;

		glGetIntegerv(GL_MAX_SHADER_STORAGE_BUFFER_BINDINGS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxShaderStorageBufferBindings= %d\r\n", size);
		mContextCaps.MaxShaderStorageBufferBindings = size;

		glGetIntegerv(GL_SHADER_STORAGE_BUFFER_OFFSET_ALIGNMENT, &size);
		VFX_LTRACE(ELTT_Graphics, "GL ShaderStorageBufferOffsetAlignment= %d\r\n", size);
		mContextCaps.ShaderStorageBufferOffsetAlignment = size;

		glGetIntegerv(GL_MAX_VERTEX_SHADER_STORAGE_BLOCKS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxVertexShaderStorageBlocks= %d\r\n", size); 
		mContextCaps.MaxVertexShaderStorageBlocks = size;

		glGetIntegerv(GL_MAX_FRAGMENT_SHADER_STORAGE_BLOCKS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxPixelShaderStorageBlocks= %d\r\n", size);
		mContextCaps.MaxPixelShaderStorageBlocks = size;

		glGetIntegerv(GL_MAX_COMPUTE_SHADER_STORAGE_BLOCKS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxComputeShaderStorageBlocks= %d\r\n", size);
		mContextCaps.MaxComputeShaderStorageBlocks = size;

		glGetIntegerv(GL_MAX_VERTEX_UNIFORM_BLOCKS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxVertexUniformBlocks= %d\r\n", size);
		mContextCaps.MaxVertexUniformBlocks = size;

		glGetIntegerv(GL_MAX_FRAGMENT_UNIFORM_BLOCKS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxPixelUniformBlocks= %d\r\n", size);
		mContextCaps.MaxPixelUniformBlocks = size;

		glGetIntegerv(GL_MAX_UNIFORM_BUFFER_BINDINGS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxUniformBufferBindings= %d\r\n", size);
		mContextCaps.MaxUniformBufferBindings = size;

		glGetIntegerv(GL_MAX_UNIFORM_BLOCK_SIZE, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxUniformBlockSize= %d\r\n", size);
		mContextCaps.MaxUniformBlockSize = size;

		glGetIntegerv(GL_MAX_TEXTURE_BUFFER_SIZE, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxTextureBufferSize= %d\r\n", size);
		mContextCaps.MaxTextureBufferSize = size;

		glGetIntegerv(GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxVertexTextureImageUnits= %d\r\n", size);

		glGetIntegerv(GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS, &size);
		VFX_LTRACE(ELTT_Graphics, "GL MaxCombinedTextureImageUnits= %d\r\n", size);

		mContextCaps.SupportFloatRT = (HasExtension("GL_EXT_color_buffer_float")|| HasExtension("GL_ARB_color_buffer_float")) ? 1 : 0;
		mContextCaps.SupportHalfRT = (HasExtension("GL_EXT_color_buffer_half_float") || HasExtension("GL_ARB_half_float_pixel")) ? 1:0;

		mContextCaps.SupportFloatTexture = (HasExtension("GL_OES_texture_float") || HasExtension("GL_ARB_texture_float")) ? 1 : 0;
		mContextCaps.SupportHalfTexture = (HasExtension("GL_OES_texture_half_float") || HasExtension("GL_ARB_half_float_pixel")) ? 1 : 0;

		VFX_LTRACE(ELTT_Graphics, "GL SupportFloatRT= %d\r\n", mContextCaps.SupportFloatRT);
		VFX_LTRACE(ELTT_Graphics, "GL SupportHalfRT= %d\r\n", mContextCaps.SupportHalfRT);
		VFX_LTRACE(ELTT_Graphics, "GL SupportFloatTexture= %d\r\n", mContextCaps.SupportFloatTexture);
		VFX_LTRACE(ELTT_Graphics, "GL SupportHalfTexture= %d\r\n", mContextCaps.SupportHalfTexture);
		
		//glGetFramebufferAttachmentParameteriv(GL_DRAW_FRAMEBUFFER, GL_BACK, GL_COLOR_ATTACHMENT0, GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING);
		/*glGetIntegerv(GL_ARB_half_float_pixel, &size);
		VFX_LTRACE(ELTT_Graphics, "GL ARBHalfFloatPixel= %d\r\n", size);
		glGetIntegerv(GL_ARB_color_buffer_float, &size);
		VFX_LTRACE(ELTT_Graphics, "GL ARBColorBufferFloat= %d\r\n", size);
		glGetIntegerv(GL_ARB_texture_float, &size);
		VFX_LTRACE(ELTT_Graphics, "GL ARBTextureFloat= %d\r\n", size);*/
	}

	Init2(desc->AppHandle);

	AutoRef<GLSdk> mImmCmdListPool;
	mImmCmdListPool = new GLSdk();
	mImmCmdListPool->mImmMode = TRUE;

	ICommandListDesc cmdDesc;
	mImmContext = new IGLCommandList();
	mImmContext->Init(this, &cmdDesc, mImmCmdListPool);

	GLSdk::ImmSDK.StrongRef(GetGLImmCommandList()->mCmdList);

	if (desc->CreateDebugLayer)
	{
		GetGLImmCommandList()->mCmdList->Enable(GL_DEBUG_OUTPUT);
		GetGLImmCommandList()->mCmdList->Enable(GL_DEBUG_OUTPUT_SYNCHRONOUS);

		//GL_DONT_CARE
		if (IRenderContext::mChooseShaderModel >= 5)
		{
			GetGLImmCommandList()->mCmdList->ES32_DebugMessageControl(GL_DONT_CARE, GL_DONT_CARE, GL_DEBUG_SEVERITY_HIGH, 0, nullptr, GL_TRUE);
			GetGLImmCommandList()->mCmdList->ES32_DebugMessageControl(GL_DONT_CARE, GL_DEBUG_TYPE_PERFORMANCE_ARB, GL_DONT_CARE, 0, nullptr, GL_FALSE);

			GetGLImmCommandList()->mCmdList->ES32_DebugMessageCallback(EngineGLDebugProc, this);
		}
	}

	return true;
}

void IGLRenderContext::FlushImmContext()
{
	mImmContext->Commit(this);

	if (GLSdk::DestroyCommands->GetCommandNumber() > 0)
	{
		GLSdk::DestroyCommands->Execute();
	}
}

#if defined(PLATFORM_DROID)
void IGLRenderContext::CreateSurfaceAndMakeCurrent(void* winHandle)
{
	auto pConfig = mConfig;
	// For Android, need to get the EGL_NATIVE_VISUAL_ID and set it using ANativeWindow_setBuffersGeometry
	{
		EGLint format = 0;
		if (eglGetConfigAttrib(mEglDisplay, pConfig, EGL_NATIVE_VISUAL_ID, &format))
		{
			ANativeWindow_setBuffersGeometry((EGLNativeWindowType)winHandle, 0, 0, format);
			VFX_LTRACE(ELTT_Graphics, "ANativeWindow_setBuffersGeometry: Format = %d\r\n", format);
		}
	}

	VFX_LTRACE(ELTT_Graphics, "Swapchain eglCreateWindowSurface\r\n");
	mSurfaceForOldSchoolPhone = eglCreateWindowSurface(mEglDisplay, pConfig, (EGLNativeWindowType)winHandle, NULL);
	if (mSurfaceForOldSchoolPhone == EGL_NO_SURFACE)
	{
		VFX_LTRACE(ELTT_Graphics, "eglCreateWindowSurface failed\r\n");
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "eglCreateWindowSurface successed\r\n");
	}
}
#endif

bool IGLRenderContext::Init2(void* winHandle)
{
#if defined(PLATFORM_WIN)
	mDC = ::GetDC((HWND)winHandle);
	//PIXELFORMATDESCRIPTOR pfd;
	////memset(&pfd, 0, sizeof(pfd));
	//pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
	//pfd.nVersion = 1;
	//pfd.dwFlags = 32804;
	//pfd.iPixelType = 0;
	//pfd.cColorBits = 32;
	//pfd.cRedBits = 8;
	//pfd.cRedShift = 16;
	//pfd.cGreenBits = 8;
	//pfd.cGreenShift = 8;
	//pfd.cBlueBits = 8;
	//pfd.cBlueShift = 0;
	//pfd.cAlphaBits = 8;
	//pfd.cAlphaShift = 24;
	//pfd.cAccumBits = 64;
	//pfd.cAccumRedBits = 16;
	//pfd.cAccumGreenBits = 16;
	//pfd.cAccumBlueBits = 16;
	//pfd.cAccumAlphaBits = 16;
	//pfd.cDepthBits = 24;
	//pfd.cStencilBits = 8;
	//pfd.cAuxBuffers = 4;
	//pfd.iLayerType = 0;
	//pfd.bReserved = 0;
	//pfd.dwLayerMask = 0;
	//pfd.dwDamageMask = 0;

	//const int iPixelFormatAttributeList[] =
	//{
	//	WGL_DRAW_TO_WINDOW_ARB, GL_TRUE,                        
	//	WGL_SUPPORT_OPENGL_ARB, GL_TRUE,                        
	//	WGL_ACCELERATION_ARB, WGL_FULL_ACCELERATION_ARB,        
	//	WGL_DOUBLE_BUFFER_ARB, GL_TRUE,                         
	//	WGL_PIXEL_TYPE_ARB, WGL_TYPE_RGBA_ARB,                  
	//	WGL_COLOR_BITS_ARB, 32,                                 
	//	WGL_DEPTH_BITS_ARB, 24,                                 
	//	WGL_STENCIL_BITS_ARB, 8,                                
	//	WGL_SWAP_METHOD_ARB, WGL_SWAP_EXCHANGE_ARB,             
	//	WGL_SAMPLES_ARB, 4,                                
	//	0
	//};
	
//	int iPixelFormat;
//	iPixelFormat = ChoosePixelFormat(mDC, &pfd);
//	//int iNumFormat;
//	//wglChoosePixelFormatARB(mDC, iPixelFormatAttributeList, NULL, 1, &iPixelFormat, (UINT *)&iNumFormat);
//	if (!DescribePixelFormat(mDC,
//		iPixelFormat,
//		sizeof(PIXELFORMATDESCRIPTOR),
//		&pfd))
//	{
//		return false;
//	}
//
//	vBOOL ok = SetPixelFormat(mDC, iPixelFormat, &pfd);
//	if (!ok)
//	{
//		auto error = ::GetLastError();
//		if (error != 0)
//		{
//			ShowLastErrorMessage(error);
//		}
//#if defined _DEBUG
//		return false;
//#endif
//	}
	
	mContextAttributeList.clear();
	mContextAttributeList.push_back(WGL_CONTEXT_MAJOR_VERSION_ARB);
	mContextAttributeList.push_back(MajorVersion);
	mContextAttributeList.push_back(WGL_CONTEXT_MINOR_VERSION_ARB);
	mContextAttributeList.push_back(MinorVersion);
	mContextAttributeList.push_back(WGL_CONTEXT_FLAGS_ARB);
	mContextAttributeList.push_back(WGL_CONTEXT_DEBUG_BIT_ARB | WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB);
	mContextAttributeList.push_back(0);
	/*const int iContextAttributeList[] =
	{
		WGL_CONTEXT_MAJOR_VERSION_ARB, MajorVersion,
		WGL_CONTEXT_MINOR_VERSION_ARB, MinorVersion,
		WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_DEBUG_BIT_ARB | WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
		0
	};*/
	mContext = wglCreateContextAttribsARB(mDC, NULL, &mContextAttributeList[0]);
	auto errorcode = GetLastError();
	if (mContext == 0)
	{
		return false;
	}
	::wglMakeCurrent(mDC, mContext);
#elif defined(PLATFORM_DROID)
	//找到了一个8，8，8，0, 24, 0的最佳匹配Config用来创建Context
	EGLConfigParms param;
	VFX_LTRACE(ELTT_Graphics, "Context MachConfig\r\n");
	mConfig = this->MatchConfig(param);
	ASSERT(mConfig != nullptr);
	// For Android, need to get the EGL_NATIVE_VISUAL_ID and set it using ANativeWindow_setBuffersGeometry
	{
		EGLint format = 0;
		if (eglGetConfigAttrib(mEglDisplay, mConfig, EGL_NATIVE_VISUAL_ID, &format))
		{
			ANativeWindow_setBuffersGeometry((EGLNativeWindowType)winHandle, 0, 0, format);
			VFX_LTRACE(ELTT_Graphics, "ANativeWindow_setBuffersGeometry: Format = %d\r\n", format);
		}
	}

	VFX_LTRACE(ELTT_Graphics, "Context eglCreateContext: EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE \r\n");
	EGLint contextAttribs[] = { EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE };
	mEglContext = eglCreateContext(mEglDisplay, mConfig,
		EGL_NO_CONTEXT, contextAttribs);
	if (mEglContext == EGL_NO_CONTEXT)
	{
		VFX_LTRACE(ELTT_Graphics, "Context eglCreateContext: failed \r\n");
		return false;
	}

	VFX_LTRACE(ELTT_Graphics, "Context eglCreateContext: successed \r\n");
	/*auto mEglSurface = eglCreateWindowSurface(mEglDisplay, mConfig, (EGLNativeWindowType)winHandle, NULL);
	if (mEglSurface == EGL_NO_SURFACE)
	{
		return false;
	}*/

	/*CreateSurfaceAndMakeCurrent(winHandle);
	if (!eglMakeCurrent(mEglDisplay, mSurfaceForOldSchoolPhone, mSurfaceForOldSchoolPhone, mEglContext))
	{
		VFX_LTRACE(ELTT_Graphics, "eglMakeCurrent failed\r\n");
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "eglMakeCurrent successed\r\n");
	}*/
	if (eglMakeCurrent(mEglDisplay, nullptr, nullptr, mEglContext) == false)
	{
		VFX_LTRACE(ELTT_Graphics, "Context eglMakeCurrent: failed, try to create surface and makecurrent\r\n");
	}
	else
	{
		VFX_LTRACE(ELTT_Graphics, "Context eglMakeCurrent: successed \r\n");
	}
	GLCheck;

#elif defined(PLATFORM_IOS)

#endif

	auto glslVersion = (char*)GLSdk::ImmSDK->GetString(GL_SHADING_LANGUAGE_VERSION);
	VFX_LTRACE(ELTT_Graphics, "Shader Language = %s\r\n", glslVersion);
	return true;
}

bool IGLRenderContext::HasExtension(const char* ext)
{
	auto pos = mGLExtensions.find(ext);
	if (pos == std::string::npos)
		return false;
	return true;
}

NS_END