#include "IGLRenderSystem.h"
#include "IGLRenderContext.h"
#include "../../Base/thread/vfxthread.h"

#define new VNEW

extern unsigned char glewExperimental;

NS_BEGIN

IGLRenderSystem::IGLRenderSystem()
{
	mDeviceNumber = 0;
}


IGLRenderSystem::~IGLRenderSystem()
{
	Cleanup();
}

void IGLRenderSystem::Cleanup()
{
}

#if defined(PLATFORM_WIN)
vBOOL IGLRenderSystem::GetContextDesc(UINT32 index, IRenderContextDesc* desc)
{
	DISPLAY_DEVICEW adapter;

	ZeroMemory(&adapter, sizeof(DISPLAY_DEVICEW));
	adapter.cb = sizeof(DISPLAY_DEVICEW);

	if (!EnumDisplayDevicesW(NULL, index, &adapter, 0))
		return false;

	if (!(adapter.StateFlags & DISPLAY_DEVICE_ACTIVE))
		return false;

	int found = 0;
	for (int displayIndex = 0; ; displayIndex++)
	{
		DISPLAY_DEVICEW display;
		HDC dc;

		ZeroMemory(&display, sizeof(DISPLAY_DEVICEW));
		display.cb = sizeof(DISPLAY_DEVICEW);

		if (!EnumDisplayDevicesW(adapter.DeviceName, displayIndex, &display, 0))
			break;

		extern void Unicode2Ansi(const wchar_t* src, std::string& tar);
		extern void Unicode2Ansi(const wchar_t* src, char* tar);
		Unicode2Ansi(display.DeviceString, desc->DeviceName);

		dc = CreateDCW(L"DISPLAY", adapter.DeviceName, NULL, NULL);
		//GetDeviceCaps(dc, HORZSIZE);
		//GetDeviceCaps(dc, VERTSIZE);
		DeleteDC(dc);

		found++;

		/*if (adapter.StateFlags & DISPLAY_DEVICE_MODESPRUNED)
			monitor->win32.modesPruned = GL_TRUE;

		wcscpy(monitor->win32.adapterName, adapter.DeviceName);
		wcscpy(monitor->win32.displayName, display.DeviceName);
		
		monitors = realloc(monitors, sizeof(_GLFWmonitor*) * found);
		monitors[found - 1] = monitor;

		if (adapter.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE &&
			displayIndex == 0)
		{
			_GLFW_SWAP_POINTERS(monitors[0], monitors[found - 1]);
		}
		*/
	}
	return TRUE;
}
#elif defined(PLATFORM_DROID)
vBOOL IGLRenderSystem::GetContextDesc(UINT32 index, IRenderContextDesc* desc)
{
	if (index == 0)
	{
		//*desc = mContextDesc;
		return TRUE;
	}
	return FALSE;
}
#elif defined(PLATFORM_IOS)

#endif
IRenderContext* IGLRenderSystem::CreateContext(const IRenderContextDesc* desc)
{
	auto rc = new IGLRenderContext();
	if (rc->Init(this, desc) == false)
	{
		rc->Release();
		return nullptr;
	}
	return rc;
}

#if defined(PLATFORM_WIN)
bool __InitContext(HWND hWnd, HDC& dc, HGLRC& hContext)
{
	dc = ::GetDC((HWND)hWnd);
	PIXELFORMATDESCRIPTOR pfd;
	pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
	pfd.nVersion = 1;
	pfd.dwFlags = 32804;
	pfd.iPixelType = 0;
	pfd.cColorBits = 32;
	pfd.cRedBits = 8;
	pfd.cRedShift = 16;
	pfd.cGreenBits = 8;
	pfd.cGreenShift = 8;
	pfd.cBlueBits = 8;
	pfd.cBlueShift = 0;
	pfd.cAlphaBits = 8;
	pfd.cAlphaShift = 24;
	pfd.cAccumBits = 64;
	pfd.cAccumRedBits = 16;
	pfd.cAccumGreenBits = 16;
	pfd.cAccumBlueBits = 16;
	pfd.cAccumAlphaBits = 16;
	pfd.cDepthBits = 24;
	pfd.cStencilBits = 8;
	pfd.cAuxBuffers = 4;
	pfd.iLayerType = 0;
	pfd.bReserved = 0;
	pfd.dwLayerMask = 0;
	pfd.dwDamageMask = 0;

	int pfid = ChoosePixelFormat(dc, &pfd);
	if (!DescribePixelFormat(dc,
		pfid,
		sizeof(PIXELFORMATDESCRIPTOR),
		&pfd))
	{
		return false;
	}
	if (!SetPixelFormat(dc, pfid, &pfd))
	{
		return false;
	}
	hContext = wglCreateContext(dc);
	//::wglMakeCurrent(dc, hContext);
	//GLCheck;

	//const int iContextAttributeList[] =
	//{
	//	WGL_CONTEXT_MAJOR_VERSION_ARB, 3,                        
	//	WGL_CONTEXT_MINOR_VERSION_ARB, 3,                        
	//	WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
	//	0
	//};

	//auto wglCreateContextAttribsARB_ptr = (PFNWGLCREATECONTEXTATTRIBSARBPROC)wglGetProcAddress("wglCreateContextAttribsARB");
	//hContext = wglCreateContextAttribsARB_ptr(dc, NULL, iContextAttributeList);
	//if (hContext == 0)
	//{
	//	return false;
	//}

	return true;
}
#elif defined(PLATFORM_DROID)
const  int EGLMinRedBits = 5;
const  int EGLMinGreenBits = 6;
const  int EGLMinBlueBits = 5;
const  int EGLMinAlphaBits = 0;
const  int EGLMinDepthBits = 16;
const  int EGLMinStencilBits = 8; // This is required for UMG clipping
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
	EGL_RENDERABLE_TYPE,  EGL_OPENGL_ES2_BIT,
	EGL_SURFACE_TYPE, EGL_WINDOW_BIT | EGL_PBUFFER_BIT,
	EGL_CONFIG_CAVEAT,  EGL_NONE,
	EGL_NONE
};

EGLConfigParms::EGLConfigParms(const EGLConfigParms& Parms)
{
	validConfig = Parms.validConfig;
	redSize = Parms.redSize;
	greenSize = Parms.greenSize;
	blueSize = Parms.blueSize;
	alphaSize = Parms.alphaSize;
	depthSize = Parms.depthSize;
	stencilSize = Parms.stencilSize;
	sampleBuffers = Parms.sampleBuffers;
	sampleSamples = Parms.sampleSamples;
}

EGLConfigParms::EGLConfigParms() :
	validConfig(0)
	, redSize(8)
	, greenSize(8)
	, blueSize(8)
	, alphaSize(0)
	, depthSize(24)
	, stencilSize(0)
	, sampleBuffers(0)
	, sampleSamples(0)
{
	//// If not default, set the preference
	//int DepthBufferPreference = (int)FAndroidWindow::GetDepthBufferPreference();
	//if (DepthBufferPreference > 0)
	//	depthSize = DepthBufferPreference;
}


//void _LogConfigInfo(EGLDisplay display, EGLConfig  config)
//{
//	EGLint ResultValue = 0;
//	eglGetConfigAttrib(display, config, EGL_RED_SIZE, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config : EGL_RED_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_GREEN_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_GREEN_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_BLUE_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_BLUE_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_ALPHA_SIZE, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config :EGL_ALPHA_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_DEPTH_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_DEPTH_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_STENCIL_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_STENCIL_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_SAMPLE_BUFFERS, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SAMPLE_BUFFERS :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_BIND_TO_TEXTURE_RGB, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_BIND_TO_TEXTURE_RGB :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_SAMPLES, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SAMPLES :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_COLOR_BUFFER_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_COLOR_BUFFER_TYPE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_CONFIG_CAVEAT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFIG_CAVEAT :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_CONFIG_ID, &ResultValue); VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFIG_ID :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_CONFORMANT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_CONFORMANT :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_LEVEL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_LEVEL :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_LUMINANCE_SIZE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_LUMINANCE_SIZE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_WIDTH, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_WIDTH :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_HEIGHT, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_HEIGHT :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_MAX_PBUFFER_PIXELS, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_PBUFFER_PIXELS :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_MAX_SWAP_INTERVAL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MAX_SWAP_INTERVAL :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_MIN_SWAP_INTERVAL, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_MIN_SWAP_INTERVAL :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_NATIVE_RENDERABLE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_RENDERABLE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_VISUAL_TYPE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_NATIVE_VISUAL_ID, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_NATIVE_VISUAL_ID :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_RENDERABLE_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_RENDERABLE_TYPE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_SURFACE_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_SURFACE_TYPE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_TYPE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_TYPE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_RED_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_RED_VALUE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_GREEN_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_GREEN_VALUE :	%u", ResultValue);
//	eglGetConfigAttrib(display, config, EGL_TRANSPARENT_BLUE_VALUE, &ResultValue);  VFX_LTRACE(ELTT_Graphics, "config :EGL_TRANSPARENT_BLUE_VALUE :	%u", ResultValue);
//}

inline EGLint GetContextRenderableType(EGLDisplay eglDisplay)
{
	const char *extensions = eglQueryString(eglDisplay, EGL_EXTENSIONS);

	// check whether EGL_KHR_create_context is in the extension string
	if (extensions != NULL && strstr(extensions, "EGL_KHR_create_context"))
	{
		// extension is supported
		return EGL_OPENGL_ES3_BIT_KHR;
	}
	// extension is not supported
	return EGL_OPENGL_ES2_BIT;
}
bool __InitContext(void* hWnd, EGLDisplay& mEglDisplay, EGLSurface& mEglSurface, EGLContext &mEglContext)
{
	EGLConfig					mConfig = nullptr;
	mEglDisplay = eglGetDisplay(0);
	{
		EGLint format = 0;
		eglGetConfigAttrib(mEglDisplay, mConfig, EGL_NATIVE_VISUAL_ID, &format);
		ANativeWindow_setBuffersGeometry((EGLNativeWindowType)hWnd, 0, 0, format);
	}
	EGLint majorVersion;
	EGLint minorVersion;

	//const char *extensions = eglQueryString(mEglDisplay, EGL_EXTENSIONS);
	//const char *version = eglQueryString(mEglDisplay, EGL_VERSION);
	if (!eglInitialize(mEglDisplay, &majorVersion, &minorVersion))
	{
		return false;
	}
	EGLint numConfigs = 0;

	EGLConfig* EGLConfigList = NULL;
	EGLBoolean result = eglChooseConfig(mEglDisplay, Attributes, NULL, 0, &numConfigs);
	if (result)
	{
		int NumConfigs = numConfigs;
		EGLConfigList = new EGLConfig[NumConfigs];
		result = eglChooseConfig(mEglDisplay, Attributes, EGLConfigList, NumConfigs, &numConfigs);
	}
	if (!result)
	{
		return false;
	}

	EGLint finalDepthSize, finalNativeVisualID;
	EGLConfigParms defaultParms;
	int ResultValue = 0;
	bool haveConfig = false;
	INT64 score = LONG_MAX;
	for (uint32_t i = 0; i < numConfigs; i++)
	{
		INT64 currScore = 0;
		int r, g, b, a, d, s, sb, sc;// , nvi;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_RED_SIZE, &ResultValue); r = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_GREEN_SIZE, &ResultValue); g = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_BLUE_SIZE, &ResultValue); b = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_ALPHA_SIZE, &ResultValue); a = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_DEPTH_SIZE, &ResultValue); d = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_STENCIL_SIZE, &ResultValue); s = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_SAMPLE_BUFFERS, &ResultValue); sb = ResultValue;
		eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_SAMPLES, &ResultValue); sc = ResultValue;

		// Optional, Tegra-specific non-linear depth buffer, which allows for much better
		// effective depth range in relatively limited bit-depths (e.g. 16-bit)
		int bNonLinearDepth = 0;
		if (eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_DEPTH_ENCODING_NV, &ResultValue))
		{
			bNonLinearDepth = (ResultValue == EGL_DEPTH_ENCODING_NONLINEAR_NV) ? 1 : 0;
		}
		else
		{
			//// explicitly consume the egl error if EGL_DEPTH_ENCODING_NV does not exist.
			//GetError();
		}

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
			mConfig = EGLConfigList[i];
			finalDepthSize = d;		// store depth/stencil sizes
			haveConfig = true;
			score = currScore;
			eglGetConfigAttrib(mEglDisplay, EGLConfigList[i], EGL_NATIVE_VISUAL_ID, &ResultValue); finalNativeVisualID = ResultValue;
		}
	}
	ASSERT(haveConfig);
	delete[] EGLConfigList;



/* Old Code
	//EGLint attribList[] =
	//{
	//EGL_RED_SIZE,       5,
	//EGL_GREEN_SIZE,     6,
	//EGL_BLUE_SIZE,      5,
	//EGL_ALPHA_SIZE,     8,
	//EGL_DEPTH_SIZE,     8,
	//EGL_STENCIL_SIZE,   8,
	//EGL_SAMPLE_BUFFERS, 0,
	//// if EGL_KHR_create_context extension is supported, then we will use
	//// EGL_OPENGL_ES3_BIT_KHR instead of EGL_OPENGL_ES2_BIT in the attribute list
	//EGL_RENDERABLE_TYPE, GetContextRenderableType(mEglDisplay),
	//EGL_NONE
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
	mEglSurface = eglCreateWindowSurface(mEglDisplay, mConfig, (EGLNativeWindowType)hWnd, NULL);
	if (mEglSurface == EGL_NO_SURFACE)
	{
		return false;
	}
	EGLint contextAttribs[] = { EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE };
	mEglContext = eglCreateContext(mEglDisplay, mConfig,
		EGL_NO_CONTEXT, contextAttribs);

	if (mEglContext == EGL_NO_CONTEXT)
	{
		return false;
	}
	if (!eglMakeCurrent(mEglDisplay, mEglSurface, mEglSurface, mEglContext))
	{
		return false;
	}
	//eglSwapInterval(mEglDisplay, 0);
	return true;
}
#endif

bool IGLRenderSystem::Init(const IRenderSystemDesc* desc)
{
#if defined(PLATFORM_WIN)
	if(1)
	{
		HDC dc;
		__InitContext((HWND)desc->WindowHandle, dc, mContext);
		
		wglMakeCurrent(dc, mContext);
		GLCheck;

		glewInit();

		if (__wglewGetExtensionsStringEXT == nullptr)
		{
			VFX_LTRACE(ELTT_Graphics, "GLEW Init failed\r\n");
		}
		ASSERT(__wglewGetExtensionsStringEXT);
		
		wglMakeCurrent(nullptr, mContext);
		GLCheck;
		//wglDeleteContext(context);
		//context = nullptr;
		ReleaseDC((HWND)desc->WindowHandle, dc);
		dc = nullptr;
	}

	DWORD adapterIndex;
	for (adapterIndex = 0; ; adapterIndex++)
	{
		DISPLAY_DEVICEW adapter;

		ZeroMemory(&adapter, sizeof(DISPLAY_DEVICEW));
		adapter.cb = sizeof(DISPLAY_DEVICEW);

		if (!EnumDisplayDevicesW(NULL, adapterIndex, &adapter, 0))
			break;
	}

	mDeviceNumber = adapterIndex;
#elif defined(PLATFORM_DROID)
	glewExperimental = 1;
	if (1)
	{
		EGLDisplay mEglDisplay;
		EGLSurface mEglSurface;
		EGLContext mEglContext;
		auto ok = __InitContext((HWND)desc->WindowHandle, mEglDisplay, mEglSurface, mEglContext);
		if (ok == false)
			return false;

		auto inited = glewInit();
		if (inited != 0)
			return false;

		if (mEglSurface != 0)
		{
			eglDestroySurface(mEglDisplay, mEglSurface);
			mEglSurface = NULL;
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
	}

	mContextDesc.AdapterId = 0;
	strcpy(mContextDesc.DeviceName, (const char*)glGetString(GL_RENDERER));
	mDeviceNumber = 1;
#endif
	return true;
}

NS_END