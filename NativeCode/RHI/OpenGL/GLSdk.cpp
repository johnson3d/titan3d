#include "GLSdk.h"
#include "GLPreHead.h"
#include "IGLShaderProgram.h"
#include "IGLVertexShader.h"
#include "IGLPixelShader.h"
#include "../../Base/thread/vfxthread.h"
#include "../../Base/vfxSampCounter.h"

NS_BEGIN

bool GLSdk::IgnorGLCall = false;
AutoRef<GLSdk> GLSdk::ImmSDK;
AutoRef<GLSdk> GLSdk::DestroyCommands(new GLSdk());

GLSdk::BufferHolder::BufferHolder(const void* p, UINT len, bool copy)
{
	IsCopy = copy;
	Length = len;
	if (copy)
	{
		mPointer = new BYTE[len];
		if (p != nullptr)
			memcpy(mPointer, p, len);
		else
			memset(mPointer, 0, len);
	}
	else
	{
		if (p != nullptr)
		{
			mPointer = (BYTE*)p;
		}
		else
		{
			mPointer = new BYTE[len];
			memset(mPointer, 0, len);
			IsCopy = true;
		}
	}
}

void GLSdk::BufferHolder::Cleanup()
{
	if (IsCopy)
		Safe_DeleteArray(mPointer);
	else
		mPointer = nullptr;
	Length = 0;
	IsCopy = false;
}

GLSdk::GLBufferId::~GLBufferId()
{
	if (GLSdk::ImmSDK == nullptr)
	{
		return;
	}
	
	EBufferIdType waitDeleteType = Type;
	GLuint waitDeleteId = BufferId;
	BufferId = -1;

	//无论如何gl资源销毁都要延迟到Frame end的时候处理，避免GLContext里面引用无效资源
	GLSdk::DestroyCommands->PushCommandDirect([=]()->void
	{
		switch (waitDeleteType)
		{
		case EngineNS::GLSdk::GLBufferId::IdTypeBuffer:
			::glDeleteBuffers(1, &waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeTexture:
			::glDeleteTextures(1, &waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeFrameBuffer:
			::glDeleteFramebuffers(1, &waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeVertexArrays:
			::glDeleteVertexArrays(1, &waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeSampler:
			::glDeleteSamplers(1, &waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeProgram:
			::glDeleteProgram(waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeShader:
			::glDeleteShader(waitDeleteId);
			break;
		case EngineNS::GLSdk::GLBufferId::IdTypeRenderbuffers:
			::glDeleteRenderbuffers(1, &waitDeleteId);
			break;
		default:
			break;
		}
	}, "DeleteGLBufferId");
}

GLSdk::GLSdk(vBOOL isTemp)
{
	mImmMode = FALSE;
	mWriteCmds = FALSE;
	WaitOtherCmdListExecuted = FALSE;
	mIsTempCmdList = isTemp;
}

GLSdk::~GLSdk()
{	
	/*DelayExecuter::Instance.TryExe([this]()->void
	{
		Execute();
	});*/
	ClearCommandList();
}

void GLSdk::ClearCommandList()
{
	VAutoLock(mLocker);
	while (mCommands.size()>0)
	{
		mCommands.pop();
	}
}

bool GLSdk::IsGLThread()
{
	auto threadId = vfxThread::GetCurrentThreadId();
	return GraphicsThreadId == threadId;
}

void GLSdk::Execute()
{
	WaitOtherCmdListExecuted = FALSE;
	std::pair<const char*, std::function<FGLApiExecute>> curCmd;
	
	{//这个队列通常是ImmSDK创建缓冲，更新缓冲，尽可能先执行了
		VAutoLock(mLocker);
		while (mAppendCommands.size() > 0)
		{
			curCmd = mAppendCommands.front();
			mAppendCommands.pop();
			ASSERT(curCmd.second != nullptr);
			curCmd.second();

			if (WaitOtherCmdListExecuted == TRUE)
			{
				ASSERT(mImmMode);
				mAppendCommands.push(curCmd);
				return;
			}
		}
	}

	while (mCommands.size()>0)
	{
		curCmd = mCommands.front();
		mCommands.pop();
		ASSERT(curCmd.second != nullptr);
		
		curCmd.second();
		if (WaitOtherCmdListExecuted == TRUE)
		{
			ASSERT(mImmMode);
			mCommands.push(curCmd);
			return;
		}
	}
	
	memset(GLApiCounter, 0, sizeof(GLApiCounter));
}

void GLSdk::DrawArrays(GLenum mode, GLint first, GLsizei count) 
{
	CMD_PUSH
		AUTO_SAMP("Native.GLSdk.Draw");
		::glDrawArrays(mode, first, count);
	CMD_END(glDrawArrays);
}

void GLSdk::DrawArraysInstanced(GLenum mode, GLint first, GLsizei count, GLsizei instanceCount) 
{
	CMD_PUSH
		AUTO_SAMP("Native.GLSdk.DrawInstanced");
		::glDrawArraysInstanced(mode, first, count, instanceCount);
	CMD_END(glDrawArraysInstanced);
}

void GLSdk::DrawElements(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices) 
{
	CMD_PUSH
		AUTO_SAMP("Native.GLSdk.Draw");
		::glDrawElements(mode, count, type, indices);
	CMD_END(glDrawElements);
}

void GLSdk::DrawElementsInstanced(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices, GLsizei instanceCount) {
	CMD_PUSH
		AUTO_SAMP("Native.GLSdk.DrawInstanced");
		::glDrawElementsInstanced(mode, count, type, indices, instanceCount);
	CMD_END(glDrawElementsInstanced);
}

void GLSdk::LinkProgram(const std::shared_ptr<GLSdk::GLBufferId>& program, void* arg)
{
	CMD_PUSH
	{
		::glLinkProgram(program->BufferId);
		auto pProg = (IGLShaderProgram*)arg;
		auto err = GLSdk::GetError();
		if (err != 0 && pProg != nullptr)
		{
			const char* vsCode;
			const char* psCode;
			vsCode = pProg->GetVertexShader()->GetDesc()->Es300Code.c_str();
			psCode = pProg->GetPixelShader()->GetDesc()->Es300Code.c_str();

			VFX_LTRACE(ELTT_Graphics, "LinkProgram Error:\nVSCode:%s\nPSCode:%s\n", vsCode, psCode);
			return;
		}
	}
	CMD_END(glLinkProgram);
}

void GLSdk::UseProgram(const std::shared_ptr<GLSdk::GLBufferId>& program, void* arg)
{
	CMD_PUSH
	{
		mCurProgram = program;
		if (program == nullptr)
			return;

		::glUseProgram(program->BufferId);
#if defined _DEBUG
		auto pProg = (IGLShaderProgram*)arg;
		auto err = GLSdk::GetError();
		if(err!=0 && pProg!=nullptr)
		{
			const char* vsCode;
			const char* psCode;
			vsCode = pProg->GetVertexShader()->GetDesc()->Es300Code.c_str();
			psCode = pProg->GetPixelShader()->GetDesc()->Es300Code.c_str();
			return;
		}
#endif
	}
	CMD_END(glUseProgram);
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenBufferId()
{
	GLBufferId* pResult = new GLBufferId();
	pResult->Type = GLBufferId::IdTypeBuffer;
	
	std::shared_ptr<GLBufferId> oResult(pResult);
	CMD_PUSH
		::glGenBuffers(1, &oResult->BufferId);
	CMD_END(glGenBuffers);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenSamplers()
{
	GLBufferId* pResult = new GLBufferId();
	pResult->Type = GLBufferId::IdTypeSampler;

	std::shared_ptr<GLBufferId> oResult(pResult);
	CMD_PUSH
		::glGenSamplers(1, &oResult->BufferId);
	CMD_END(glGenSamplers);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::CreateProgram()
{
	GLBufferId* pResult = new GLBufferId();
	pResult->Type = GLBufferId::IdTypeProgram;

	std::shared_ptr<GLBufferId> oResult(pResult);
	CMD_PUSH
	{
		oResult->BufferId = ::glCreateProgram();
	}
	CMD_END(glGenSamplers);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::CreateShader(GLenum type)
{
	GLBufferId* pResult = new GLBufferId();
	pResult->Type = GLBufferId::IdTypeShader;

	CMD_PUSH
		pResult->BufferId = ::glCreateShader(type);
	CMD_END(glCreateShader);

	return std::shared_ptr<GLBufferId>(pResult);
}

void GLSdk::BufferData(GLenum target, GLsizeiptr size, const GLvoid* data, GLenum usage) 
{
	std::shared_ptr<BufferHolder> pBufferHolder(new BufferHolder(data, (UINT)size, IsCopyData()));
	CMD_PUSH
	{
		::glBufferData(target, pBufferHolder->Length, pBufferHolder->mPointer, usage);
	}
	CMD_END(glBufferData);
}

void GLSdk::BufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid* data) 
{
	std::shared_ptr<BufferHolder> pBufferHolder(new BufferHolder(data, (UINT)size, IsCopyData()));
	CMD_PUSH
		::glBufferSubData(target, offset, pBufferHolder->Length, pBufferHolder->mPointer);
	CMD_END(glBufferSubData);
}

void GLSdk::TexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid* pixels) 
{
	std::shared_ptr<BufferHolder> pBufferHolder;
	if (pixels != nullptr)
	{
		UINT rowPitch = (GLInternalFormatSize(internalformat) * width + 3)/4 * 4;
		UINT size = rowPitch * height;
		pBufferHolder = std::make_shared<BufferHolder>(pixels, (UINT)size, IsCopyData());
	}
	CMD_PUSH
	{
		if(pixels==nullptr)
			::glTexImage2D(target, level, internalformat, width, height, border, format, type, pixels);
		else
		{
			::glTexImage2D(target, level, internalformat, width, height, border, format, type, pBufferHolder->mPointer);
			/*DWORD* pData = new DWORD[width*height];
			memset(pData, 0, sizeof(DWORD)*width*height);
			::glGetTexImage(target, level, GL_RGBA, GL_UNSIGNED_BYTE, pData);
			delete[] pData;*/
		}
	}
	CMD_END(glTexImage2D);
}

void GLSdk::CompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid* data) 
{
	std::shared_ptr<BufferHolder> pBufferHolder;
	if (data != nullptr)
	{
		pBufferHolder = std::make_shared<BufferHolder>(data, (UINT)imageSize, IsCopyData());
	}
	CMD_PUSH
	{
		if (data == nullptr)
			::glCompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, nullptr);
		else
		{
			::glCompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, pBufferHolder->mPointer);
			/*DWORD* pData = new DWORD[width*height];
			memset(pData, 0, sizeof(DWORD)*width*height);
			::glGetTexImage(target, level, GL_RGBA, GL_UNSIGNED_BYTE, pData);
			::glCompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, pBufferHolder->mPointer);
			memset(pData, 0, sizeof(DWORD)*width*height);
			::glGetTexImage(target, level, GL_RGBA, GL_UNSIGNED_BYTE, pData);
			delete[] pData;*/
		}
	}
	CMD_END(glCompressedTexImage2D);
}

void GLSdk::CompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid* data) 
{
	std::shared_ptr<BufferHolder> pBufferHolder;
	if (data != nullptr)
	{
		pBufferHolder = std::make_shared<BufferHolder>(data, (UINT)imageSize, IsCopyData());
	}
	CMD_PUSH
	{
		if (data == nullptr)
			::glCompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, nullptr);
		else
			::glCompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, pBufferHolder->mPointer);
	}
	CMD_END(glCompressedTexSubImage2D);
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenTextures()
{
	//ASSERT(mImmMode);
	GLBufferId* result = new GLBufferId();
	result->Type = GLBufferId::IdTypeTexture;

	std::shared_ptr<GLSdk::GLBufferId> oResult(result);
	CMD_PUSH
		::glGenTextures(1, &oResult->BufferId);
	CMD_END(glGenTextures);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenFramebuffers()
{
	GLBufferId* result = new GLBufferId();
	result->Type = GLBufferId::IdTypeFrameBuffer;

	std::shared_ptr<GLSdk::GLBufferId> oResult(result);
	CMD_PUSH
		::glGenFramebuffers(1, &oResult->BufferId);
	CMD_END(glGenFramebuffers);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenRenderbuffers()
{
	GLBufferId* result = new GLBufferId();
	result->Type = GLBufferId::IdTypeRenderbuffers;

	std::shared_ptr<GLSdk::GLBufferId> oResult(result);
	CMD_PUSH
		::glGenRenderbuffers(1, &oResult->BufferId);
	CMD_END(glGenRenderbuffers);

	return oResult;
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GenVertexArrays()
{
	//ASSERT(mImmMode);
	GLBufferId* result = new GLBufferId();
	result->Type = GLBufferId::IdTypeVertexArrays;

	std::shared_ptr<GLSdk::GLBufferId> oResult(result);
	CMD_PUSH
		::glGenVertexArrays(1, &oResult->BufferId);
	CMD_END(glGenVertexArrays);

	return oResult;
}

void GLSdk::DrawBuffers(GLsizei n, const GLenum* bufs, const std::function<FGLApiExecute>& onStateError)
{
	std::shared_ptr<BufferHolder> pBufferHolder(new BufferHolder(bufs, (UINT)n * sizeof(GLenum), IsCopyData()));
	CMD_PUSH
	{
		::glDrawBuffers(n, (const GLenum*)pBufferHolder->mPointer);
		auto hr = GLSdk::CheckFramebufferStatus(GL_FRAMEBUFFER);
		if (hr != GL_FRAMEBUFFER_COMPLETE)
		{
			switch (hr)
			{
				case GL_FRAMEBUFFER_UNDEFINED:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus FRAMEBUFFER_UNDEFINED\r\n");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus FRAMEBUFFER_INCOMPLETE_ATTACHMENT\r\n");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT\r\n");
					break;
				case GL_FRAMEBUFFER_UNSUPPORTED:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus FRAMEBUFFER_UNSUPPORTED\r\n");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus FRAMEBUFFER_INCOMPLETE_MULTISAMPLE\r\n");
					break;
				default:
					VFX_LTRACE(ELTT_Graphics, "CheckFramebufferStatus Unknown\r\n");
					break;
			}
			if (onStateError != nullptr)
			{
				onStateError();
			}
		}
	}
	CMD_END(glDrawBuffers);
}

std::shared_ptr<GLSdk::GLBufferId> GLSdk::GetIntegerv(GLenum pname)
{
	GLSdk::GLBufferId* pResult = new GLSdk::GLBufferId();
	pResult->Type = GLBufferId::IdTypeUnknown;

	CMD_PUSH
		::glGetIntegerv(pname, (GLint*)&pResult->BufferId);
	CMD_END(glGetIntegerv);

	return std::shared_ptr<GLBufferId>(pResult);
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API void SDK_GLSdk_IgnoreGLCall(vBOOL value)
	{
		GLSdk::IgnorGLCall = value ? true : false;
	}
}