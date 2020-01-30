#include "GLPreHead.h"
#include "../../Core/thread/vfxthread.h"

#define new VNEW

#if PLATFORM_WIN
#pragma comment(lib, "OpenGL32.lib")
#pragma comment(lib, "glu32.lib") 
#else
#include "glu/glues_error.c"
#endif

NS_BEGIN

bool GLSdk::CheckGLError = true;

void GLCheckError(const char* file, int line)
{
	if (EngineIsCleared)
		return;

	if (GLSdk::CheckGLError == false)
	{
		GLSdk::GetError();
		return;
	}

#if defined(_DEBUG)
	auto threadId = vfxThread::GetCurrentThreadId();
	
	ASSERT(GraphicsThreadId == threadId);
#endif
	auto err = GLSdk::GetError();
	if (err != 0)
	{
#if PLATFORM_WIN
		auto errStr = gluErrorString(err);
#else
		auto errStr = Vfx_ErrorString(err);
#endif
		VFX_LTRACE(ELTT_Graphics, "(%s:%d)=>%d\n:", file, line, err, errStr);
	}
}



NS_END

using namespace EngineNS;

extern "C"
{
	
}