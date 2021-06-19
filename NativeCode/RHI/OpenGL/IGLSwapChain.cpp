#include "IGLSwapChain.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

IGLSwapChain::IGLSwapChain()
{
#if defined(PLATFORM_WIN)
	//mDC = 0;
#elif defined(PLATFORM_DROID)
	mEglSurface = 0;
	mConfig = nullptr;
#elif defined(PLATFORM_IOS)

#endif
}

IGLSwapChain::~IGLSwapChain()
{
	Cleanup();
}

void IGLSwapChain::Cleanup()
{
#if defined(PLATFORM_WIN)
	/*if (mDC != nullptr)
	{
		ReleaseDC((HWND)mDesc.WindowHandle, mDC);
		mDC = nullptr;
	}*/
#elif defined(PLATFORM_DROID)
	auto rc = mRenderContext.GetPtr();
	auto mEglDisplay = rc->mEglDisplay;
	if (mEglSurface != 0)
	{
		eglDestroySurface(mEglDisplay, mEglSurface);
		mEglSurface = NULL;
	}
	mConfig = nullptr;
#endif
}

void IGLSwapChain::BindCurrent()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
#if defined(PLATFORM_WIN)
	//wglMakeCurrent(rc->mDC, rc->mContext);
	//wglMakeCurrent(mDC, (HGLRC)&rc->mContextAttributeList[0]);
	//GLCheck;
#else
	eglMakeCurrent(rc->mEglDisplay, mEglSurface, mEglSurface, rc->mEglContext);
	GLCheck;
#endif
}

void IGLSwapChain::Present(UINT SyncInterval, UINT Flags)
{
	AUTO_SAMP("Native.ISwapChain.Present");
	
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;

	//BindCurrent();

	/*auto rc = (IGLRenderContext*)mRenderContext.GetPtr();
	if (rc != nullptr)
	{
		GLSdk* refImmCmdList = rc->mImmContext[0]->mCmdList;
		if (refImmCmdList->GetCommandNumber() > 0)
		{
			refImmCmdList->Execute();
		}
	}

	rc->SwapImmCmdList();*/
#if defined(PLATFORM_WIN)
	SwapBuffers(rc->mDC);
	//SwapBuffers(mDC);
#else
	auto mEglDisplay = rc->mEglDisplay;
	eglSwapBuffers(mEglDisplay, mEglSurface);
#endif
}

void IGLSwapChain::OnLost()
{
#if defined(PLATFORM_WIN)
#else
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	auto mEglDisplay = rc->mEglDisplay;
	if (mEglSurface != nullptr)
	{
		if (mEglDisplay != nullptr)
		{
			eglMakeCurrent(mEglDisplay, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
			GLCheck;
			eglDestroySurface(mEglDisplay, mEglSurface);
			GLCheck;
		}
		mEglSurface = nullptr;
	}
#endif
}

vBOOL IGLSwapChain::OnRestore(const ISwapChainDesc* desc)
{
#if defined(PLATFORM_WIN)
#else
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return FALSE;
	mEglSurface = eglCreateWindowSurface(rc->mEglDisplay, mConfig, (EGLNativeWindowType)desc->WindowHandle, NULL);
	GLCheck;
	if (mEglSurface == EGL_NO_SURFACE)
	{
		return FALSE;
	}
	mDesc.WindowHandle = desc->WindowHandle;

	auto result = !eglMakeCurrent(rc->mEglDisplay, mEglSurface, mEglSurface, rc->mEglContext);
	GLCheck;;
	if (result)
	{
		return FALSE;
	}
#endif
	return TRUE;
}

#if defined(PLATFORM_WIN)
std::string ShowLastErrorMessage(DWORD dwError)
{
	HLOCAL hlocal = NULL;   // Buffer that gets the error message string  

							// Get the error code's textual description  
	BOOL fOk = FormatMessage(
		FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER | \
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dwError,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), //MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US),        
		(PTSTR)&hlocal,
		0,
		NULL);

	if (!fOk)
	{
		// Is it a network-related error?  
		HMODULE hDll = LoadLibraryEx(TEXT("netmsg.dll"), NULL,
			DONT_RESOLVE_DLL_REFERENCES);

		if (hDll != NULL)
		{
			FormatMessage(
				FORMAT_MESSAGE_FROM_HMODULE | FORMAT_MESSAGE_FROM_SYSTEM | \
				FORMAT_MESSAGE_IGNORE_INSERTS,
				hDll,
				dwError,
				MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL), //MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US),  
				(PTSTR)&hlocal,
				0,
				NULL);

			FreeLibrary(hDll);
		}
	}

	std::string result;
	if (hlocal != NULL)
	{
		//::MessageBox(NULL, (LPCSTR)LocalLock(hlocal), TEXT(""), MB_OK);  
		std::string result = (LPCSTR)LocalLock(hlocal);
		LocalFree(hlocal);
	}
	else
	{
		std::string result = "No text found for this error number...";
		//::MessageBox(NULL, TEXT("No text found for this error number..."), TEXT("Tip"), MB_OK);  
	}
	return result;
}
#endif

bool IGLSwapChain::Init(IGLRenderContext* rc, const ISwapChainDesc* desc)
{
	mDesc = *desc;
	mRenderContext.FromObject(rc);

#if defined(PLATFORM_WIN)
//	mDC = ::GetDC((HWND)desc->WindowHandle);
//	PIXELFORMATDESCRIPTOR pfd;
//	//memset(&pfd, 0, sizeof(pfd));
//	pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
//	pfd.nVersion = 1;
//	pfd.dwFlags = 32804;
//	pfd.iPixelType = 0;
//	pfd.cColorBits = 32;
//	pfd.cRedBits = 8;
//	pfd.cRedShift = 16;
//	pfd.cGreenBits = 8;
//	pfd.cGreenShift = 8;
//	pfd.cBlueBits = 8;
//	pfd.cBlueShift = 0;
//	pfd.cAlphaBits = 8;
//	pfd.cAlphaShift = 24;
//	pfd.cAccumBits = 64;
//	pfd.cAccumRedBits = 16;
//	pfd.cAccumGreenBits = 16;
//	pfd.cAccumBlueBits = 16;
//	pfd.cAccumAlphaBits = 16;
//	pfd.cDepthBits = 24;
//	pfd.cStencilBits = 8;
//	pfd.cAuxBuffers = 4;
//	pfd.iLayerType = 0;
//	pfd.bReserved = 0;
//	pfd.dwLayerMask = 0;
//	pfd.dwDamageMask = 0;
//
//	//const int iPixelFormatAttributeList[] =
//	//{
//	//	WGL_DRAW_TO_WINDOW_ARB, GL_TRUE,
//	//	WGL_SUPPORT_OPENGL_ARB, GL_TRUE,
//	//	WGL_ACCELERATION_ARB, WGL_FULL_ACCELERATION_ARB,
//	//	WGL_DOUBLE_BUFFER_ARB, GL_TRUE,
//	//	WGL_PIXEL_TYPE_ARB, WGL_TYPE_RGBA_ARB,
//	//	WGL_COLOR_BITS_ARB, 32,
//	//	WGL_DEPTH_BITS_ARB, 24,
//	//	WGL_STENCIL_BITS_ARB, 8,
//	//	WGL_SWAP_METHOD_ARB, WGL_SWAP_EXCHANGE_ARB,
//	//	WGL_SAMPLES_ARB, 4,
//	//	0
//	//};
//
//	int iPixelFormat;
//	iPixelFormat = ChoosePixelFormat(mDC, &pfd);
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
#elif defined(PLATFORM_DROID)
	//auto mEglDisplay = rc->mEglDisplay;
	EGLConfigParms param;
	switch (desc->Format)
	{
		case PXF_R8G8B8A8_UINT:
		{
			param.redSize = 8;
			param.greenSize = 8;
			param.blueSize = 8;
			param.alphaSize = 8;
		}
		break;
		case PXF_B5G6R5_UNORM:
		{
			param.redSize = 5;
			param.greenSize = 6;
			param.blueSize = 5;
			param.alphaSize = 0;
		}
		break;
		case PXF_B4G4R4A4_UNORM:
		{
			param.redSize = 4;
			param.greenSize = 4;
			param.blueSize = 4;
			param.alphaSize = 4;
		}
		break;
		default:
			break;
	}
	VFX_LTRACE(ELTT_Graphics, "Swapchain MachConfig\r\n");
	mConfig = rc->MatchConfig(param);
	ASSERT(mConfig != nullptr);
	// For Android, need to get the EGL_NATIVE_VISUAL_ID and set it using ANativeWindow_setBuffersGeometry
	{
		EGLint format = 0;
		if (eglGetConfigAttrib(rc->mEglDisplay, mConfig, EGL_NATIVE_VISUAL_ID, &format))
		{
			ANativeWindow_setBuffersGeometry((EGLNativeWindowType)desc->WindowHandle, 0, 0, format);
			VFX_LTRACE(ELTT_Graphics, "ANativeWindow_setBuffersGeometry: Format = %d\r\n", format);
		}
	}

	VFX_LTRACE(ELTT_Graphics, "Swapchain eglCreateWindowSurface\r\n");
	mEglSurface = eglCreateWindowSurface(rc->mEglDisplay, mConfig,(EGLNativeWindowType)desc->WindowHandle, NULL);
	if (mEglSurface == EGL_NO_SURFACE)
	{
		VFX_LTRACE(ELTT_Graphics, "eglCreateWindowSurface failed\r\n");
		return false;
	}
	VFX_LTRACE(ELTT_Graphics, "eglCreateWindowSurface successed\r\n");
	if (!eglMakeCurrent(rc->mEglDisplay, mEglSurface, mEglSurface, rc->mEglContext))
	{
		VFX_LTRACE(ELTT_Graphics, "eglMakeCurrent failed\r\n");
		return false;
	}
	VFX_LTRACE(ELTT_Graphics, "eglMakeCurrent successed\r\n");
#elif defined(PLATFORM_IOS)

#endif
	return true;
}

NS_END