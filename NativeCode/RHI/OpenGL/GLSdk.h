#pragma once
#include "../PreHead.h"
#include "../../Base/thread/vfxcritical.h"
#include "../../Base/thread/vfxthread.h"

#if defined(PLATFORM_WIN)
#include "glew/include/GL/glew.h"
#include "glew/include/GL/wglew.h"
#elif defined(PLATFORM_DROID)
#include "glew/include/GL/glew.h"
#include <EGL/egl.h>
#include <EGL/eglext.h>
//#include <GLES2/gl2.h>
//#include <GLES2/gl2ext.h>
//#include <GLES3/gl3.h>
//#include <GLES3/gl3ext.h>
#elif defined(PLATFORM_IOS)

#endif

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunknown-pragmas"
#endif

NS_BEGIN

void GLCheckError(const char* file, int line);

#if defined(NDEBUG) && defined(PLATFORM_DROID)
#define NO_GLERRORCHECK
#endif

#if defined(NO_GLERRORCHECK)
#define GLCheck_Impl(f,l) 
#else
#define GLCheck_Impl(f,l) GLCheckError(f,l);
#endif

#define GLCheck GLCheck_Impl(__FILE__,__LINE__);

class IGLGeometryMesh;
class IGLVertexBuffer;
class IGLIndexBuffer;
class IGLConstantBuffer;
class IGLShaderResourceView;

class GLSdk : public VIUnknown
{
public:
	struct GLBufferId
	{
		enum EBufferIdType
		{
			IdTypeUnknown,
			IdTypeBuffer,
			IdTypeTexture,
			IdTypeFrameBuffer,
			IdTypeVertexArrays,
			IdTypeRenderbuffers,
			IdTypeSampler,
			IdTypeProgram,
			IdTypeShader,
		};
		GLBufferId()
		{
			BufferId = -1;
			Type = IdTypeBuffer;
		}
		~GLBufferId();
		EBufferIdType	Type;
		GLuint			BufferId;
		inline bool IsValidBuffer() {
			return BufferId != -1;
		}

#if _DEBUG
		//debug data
		TObjectHandle<VIUnknown>	DebugInfo;
#endif
	};

	struct BufferHolder
	{
		BYTE* mPointer;
		UINT Length;
		bool IsCopy;
		BufferHolder(const void* p, UINT len, bool copy);
		~BufferHolder()
		{
			Cleanup();
		}
		void Cleanup();
	};
	bool IsCopyData()
	{
		if (IsGLThread() && this == ImmSDK)
			return false;
		return true;
	}
	typedef void (FGLApiExecute)();
	std::queue<std::pair<const char*, std::function<FGLApiExecute>>>	mCommands;
	std::queue<std::pair<const char*, std::function<FGLApiExecute>>>	mAppendCommands;
	void PushCommand(const std::function<FGLApiExecute>& cmd, const char* debugInfo)
	{
		if (IsGLThread() && this==ImmSDK)
		{
			if (mAppendCommands.size() > 0 || mCommands.size() > 0)
			{
				Execute();
			}
			cmd();
		}
		else
		{
			auto elem = std::make_pair(debugInfo, cmd);
			ASSERT(elem.second != nullptr);
			mCommands.push(elem);
		}
	}
	VCritical mLocker;
	void PushCommandDirect(const std::function<FGLApiExecute>& cmd, const char* debugInfo)
	{
		VAutoLock(mLocker);
		mAppendCommands.push(std::make_pair(debugInfo, cmd));
	}

#define SDKGLCheck GLCheck 
#define DBGTrace _vfxTraceA
#define DEF_Debugger(fun) void DBG_##fun
	static bool IgnorGLCall;

#define CMD_PUSH PushCommand([=]()->void { \
		if(IgnorGLCall) return;

#define CMD_END(a)  SDKGLCheck; \
		GLApiCounter[ID_##a]++; \
		GLApiName[ID_##a] = #a;\
		if(mWriteCmds)\
		{\
			DBGTrace(#a);\
			DBGTrace("\n");\
		}\
	},#a);

#define CMD_END_NOCHECK(a) \
		glGetError(); \
		if(mWriteCmds)\
		{\
			DBGTrace(#a);\
			DBGTrace("\n");\
		}\
	},#a);

public:
	static AutoRef<GLSdk>	ImmSDK;
	static AutoRef<GLSdk>	DestroyCommands;
	static bool				CheckGLError;
	vBOOL		mImmMode;
	vBOOL		mWriteCmds;
	vBOOL		mIsTempCmdList;

	vBOOL		WaitOtherCmdListExecuted;
	GLSdk(vBOOL isTemp = FALSE);
	~GLSdk();
	void ClearCommandList();
	bool IsGLThread();
	size_t GetCommandNumber() const {
		return mCommands.size();
	}
	void Execute();

	enum EGLApiId
	{
		ID_glActiveTexture,
		ID_glAttachShader,
		ID_glBindAttribLocation,
		ID_glBindVertexBuffer,
		ID_glBindIndexBuffer,
		ID_glBindBuffer,
		ID_glBindFramebuffer,
		ID_glBindRenderbuffer,
		ID_glBindTexture2D,
		ID_glBindTexture,
		ID_glBlendColor,
		ID_glBlendEquation,
		ID_glBlendEquationSeparate,
		ID_glBlendFunc,
		ID_glBlendFuncSeparate,
		ID_glBufferData,
		ID_glBufferSubData,
		ID_glCheckFramebufferStatus,
		ID_glClear,
		ID_glClearColor,
		ID_glClearDepthf,
		ID_glClearStencil,
		ID_glColorMask,
		ID_glCompileShader,
		ID_glCompressedTexImage2D,
		ID_glCompressedTexSubImage2D,
		ID_glCopyTexImage2D,
		ID_glCopyTexSubImage2D,
		ID_glCreateProgram,
		ID_glCreateShader,
		ID_glCullFace,
		ID_glDeleteBuffers,
		ID_glDeleteFramebuffers,
		ID_glDeleteProgram,
		ID_glDeleteRenderbuffers,
		ID_glDeleteShader,
		ID_glDeleteTextures,
		ID_glDepthFunc,
		ID_glDepthMask,
		ID_glDepthRangef,
		ID_glDetachShader,
		ID_glDisable,
		ID_glDisableVertexAttribArray,
		ID_glDrawArrays,
		ID_glDrawElements,
		ID_glEnable,
		ID_glEnableVertexAttribArray,
		ID_glFinish,
		ID_glFlush,
		ID_glFramebufferRenderbuffer,
		ID_glFramebufferTexture2D,
		ID_glFrontFace,
		ID_glGenBuffers,
		ID_glGenerateMipmap,
		ID_glGenFramebuffers,
		ID_glGenRenderbuffers,
		ID_glGenTextures,
		ID_glGetActiveAttrib,
		ID_glGetActiveUniform,
		ID_glGetAttachedShaders,
		ID_glGetAttribLocation,
		ID_glGetBooleanv,
		ID_glGetBufferParameteriv,
		ID_glGetError,
		ID_glGetFloatv,
		ID_glGetFramebufferAttachmentParameteriv,
		ID_glGetIntegerv,
		ID_glGetProgramiv,
		ID_glGetProgramInfoLog,
		ID_glGetRenderbufferParameteriv,
		ID_glGetShaderiv,
		ID_glGetShaderInfoLog,
		ID_glGetShaderPrecisionFormat,
		ID_glGetShaderSource,
		ID_glGetString,
		ID_glGetTexParameterfv,
		ID_glGetTexParameteriv,
		ID_glGetUniformfv,
		ID_glGetUniformiv,
		ID_glGetUniformLocation,
		ID_glGetVertexAttribfv,
		ID_glGetVertexAttribiv,
		ID_glGetVertexAttribPointerv,
		ID_glHint,
		ID_glIsBuffer,
		ID_glIsEnabled,
		ID_glIsFramebuffer,
		ID_glIsProgram,
		ID_glIsRenderbuffer,
		ID_glIsShader,
		ID_glIsTexture,
		ID_glLineWidth,
		ID_glLinkProgram,
		ID_glPixelStorei,
		ID_glPolygonOffset,
		ID_glReadPixels,
		ID_glReleaseShaderCompiler,
		ID_glRenderbufferStorage,
		ID_glSampleCoverage,
		ID_glScissor,
		ID_glShaderBinary,
		ID_glShaderSource,
		ID_glStencilFunc,
		ID_glStencilFuncSeparate,
		ID_glStencilMask,
		ID_glStencilMaskSeparate,
		ID_glStencilOp,
		ID_glStencilOpSeparate,
		ID_glTexImage2D,
		ID_glTexParameterf,
		ID_glTexParameterfv,
		ID_glTexParameteri,
		ID_glTexParameteriv,
		ID_glTexSubImage2D,
		ID_glUniform1f,
		ID_glUniform1fv,
		ID_glUniform1i,
		ID_glUniform1iv,
		ID_glUniform2f,
		ID_glUniform2fv,
		ID_glUniform2i,
		ID_glUniform2iv,
		ID_glUniform3f,
		ID_glUniform3fv,
		ID_glUniform3i,
		ID_glUniform3iv,
		ID_glUniform4f,
		ID_glUniform4fv,
		ID_glUniform4i,
		ID_glUniform4iv,
		ID_glUniformMatrix2fv,
		ID_glUniformMatrix3fv,
		ID_glUniformMatrix4fv,
		ID_glUseProgram,
		ID_glValidateProgram,
		ID_glVertexAttrib1f,
		ID_glVertexAttrib1fv,
		ID_glVertexAttrib2f,
		ID_glVertexAttrib2fv,
		ID_glVertexAttrib3f,
		ID_glVertexAttrib3fv,
		ID_glVertexAttrib4f,
		ID_glVertexAttrib4fv,
		ID_glVertexAttribPointer,
		ID_glViewport,
		ID_glReadBuffer,
		ID_glDrawRangeElements,
		ID_glTexImage3D,
		ID_glTexSubImage3D,
		ID_glCopyTexSubImage3D,
		ID_glCompressedTexImage3D,
		ID_glCompressedTexSubImage3D,
		ID_glGenQueries,
		ID_glDeleteQueries,
		ID_glIsQuery,
		ID_glBeginQuery,
		ID_glEndQuery,
		ID_glGetQueryiv,
		ID_glGetQueryObjectuiv,
		ID_glUnmapBuffer,
		ID_glGetBufferPointerv,
		ID_glDrawBuffers,
		ID_glUniformMatrix2x3fv,
		ID_glUniformMatrix3x2fv,
		ID_glUniformMatrix2x4fv,
		ID_glUniformMatrix4x2fv,
		ID_glUniformMatrix3x4fv,
		ID_glUniformMatrix4x3fv,
		ID_glBlitFramebuffer,
		ID_glRenderbufferStorageMultisample,
		ID_glFramebufferTextureLayer,
		ID_glMapBufferRange,
		ID_glFlushMappedBufferRange,
		ID_glBindVertexArray,
		ID_glDeleteVertexArrays,
		ID_glGenVertexArrays,
		ID_glIsVertexArray,
		ID_glGetIntegeri_v,
		ID_glBeginTransformFeedback,
		ID_glEndTransformFeedback,
		ID_glBindBufferRange,
		ID_glBindBufferBase,
		ID_glTransformFeedbackVaryings,
		ID_glGetTransformFeedbackVarying,
		ID_glVertexAttribIPointer,
		ID_glGetVertexAttribIiv,
		ID_glGetVertexAttribIuiv,
		ID_glVertexAttribI4i,
		ID_glVertexAttribI4ui,
		ID_glVertexAttribI4iv,
		ID_glVertexAttribI4uiv,
		ID_glGetUniformuiv,
		ID_glGetFragDataLocation,
		ID_glUniform1ui,
		ID_glUniform2ui,
		ID_glUniform3ui,
		ID_glUniform4ui,
		ID_glUniform1uiv,
		ID_glUniform2uiv,
		ID_glUniform3uiv,
		ID_glUniform4uiv,
		ID_glClearBufferiv,
		ID_glClearBufferuiv,
		ID_glClearBufferfv,
		ID_glClearBufferfi,
		ID_glGetStringi,
		ID_glCopyBufferSubData,
		ID_glGetUniformIndices,
		ID_glGetActiveUniformsiv,
		ID_glGetUniformBlockIndex,
		ID_glGetActiveUniformBlockiv,
		ID_glGetActiveUniformBlockName,
		ID_glUniformBlockBinding,
		ID_glDrawArraysInstanced,
		ID_glDrawElementsInstanced,
		ID_glFenceSync,
		ID_glIsSync,
		ID_glDeleteSync,
		ID_glClientWaitSync,
		ID_glWaitSync,
		ID_glGetInteger64v,
		ID_glGetSynciv,
		ID_glGetInteger64i_v,
		ID_glGetBufferParameteri64v,
		ID_glGenSamplers,
		ID_glDeleteSamplers,
		ID_glIsSampler,
		ID_glBindSampler,
		ID_glSamplerParameteri,
		ID_glSamplerParameteriv,
		ID_glSamplerParameterf,
		ID_glSamplerParameterfv,
		ID_glGetSamplerParameteriv,
		ID_glGetSamplerParameterfv,
		ID_glVertexAttribDivisor,
		ID_glBindTransformFeedback,
		ID_glDeleteTransformFeedbacks,
		ID_glGenTransformFeedbacks,
		ID_glIsTransformFeedback,
		ID_glPauseTransformFeedback,
		ID_glResumeTransformFeedback,
		ID_glGetProgramBinary,
		ID_glProgramBinary,
		ID_glProgramParameteri,
		ID_glInvalidateFramebuffer,
		ID_glInvalidateSubFramebuffer,
		ID_glTexStorage2D,
		ID_glTexStorage3D,
		ID_glGetInternalformativ,

		//es3.1
		ID_glDispatchCompute,
		ID_glDispatchComputeIndirect,
		ID_glDrawArraysIndirect,
		ID_glDrawElementsIndirect,
		ID_glFramebufferParameteri,
		ID_glGetFramebufferParameteriv,
		ID_glGetProgramInterfaceiv,
		ID_glGetProgramResourceIndex,
		ID_glGetProgramResourceName,
		ID_glGetProgramResourceiv,
		ID_glGetProgramResourceLocation,
		ID_glUseProgramStages,
		ID_glActiveShaderProgram,
		ID_glCreateShaderProgramv,
		ID_glBindProgramPipeline,
		ID_glDeleteProgramPipelines,
		ID_glGenProgramPipelines,
		ID_glIsProgramPipeline,
		ID_glGetProgramPipelineiv,
		ID_glProgramUniform1i,
		ID_glProgramUniform2i,
		ID_glProgramUniform3i,
		ID_glProgramUniform4i,
		ID_glProgramUniform1ui,
		ID_glProgramUniform2ui,
		ID_glProgramUniform3ui,
		ID_glProgramUniform4ui,
		ID_glProgramUniform1f,
		ID_glProgramUniform2f,
		ID_glProgramUniform3f,
		ID_glProgramUniform4f,
		ID_glProgramUniform1iv,
		ID_glProgramUniform2iv,
		ID_glProgramUniform3iv,
		ID_glProgramUniform4iv,
		ID_glProgramUniform1uiv,
		ID_glProgramUniform2uiv,
		ID_glProgramUniform3uiv,
		ID_glProgramUniform4uiv,
		ID_glProgramUniform1fv,
		ID_glProgramUniform2fv,
		ID_glProgramUniform3fv,
		ID_glProgramUniform4fv,
		ID_glProgramUniformMatrix2fv,
		ID_glProgramUniformMatrix3fv,
		ID_glProgramUniformMatrix4fv,
		ID_glProgramUniformMatrix2x3fv,
		ID_glProgramUniformMatrix3x2fv,
		ID_glProgramUniformMatrix2x4fv,
		ID_glProgramUniformMatrix4x2fv,
		ID_glProgramUniformMatrix3x4fv,
		ID_glProgramUniformMatrix4x3fv,
		ID_glValidateProgramPipeline,
		ID_glGetProgramPipelineInfoLog,
		ID_glBindImageTexture,
		ID_glGetBooleani_v,
		ID_glMemoryBarrier,
		ID_glMemoryBarrierByRegion,
		ID_glTexStorage2DMultisample,
		ID_glGetMultisamplefv,
		ID_glSampleMaski,
		ID_glGetTexLevelParameteriv,
		ID_glGetTexLevelParameterfv,
		ID_glVertexAttribFormat,
		ID_glVertexAttribIFormat,
		ID_glVertexAttribBinding,
		ID_glVertexBindingDivisor,

		//es3.2
		ID_glBlendBarrier,
		ID_glCopyImageSubData,
		ID_glDebugMessageControl,
		ID_glDebugMessageInsert,
		ID_glDebugMessageCallback,
		ID_glGetDebugMessageLog,
		ID_glPushDebugGroup,
		ID_glPopDebugGroup,
		ID_glObjectLabel,
		ID_glGetObjectLabel,
		ID_glObjectPtrLabel,
		ID_glGetObjectPtrLabel,
		ID_glGetPointerv,
		ID_glEnablei,
		ID_glDisablei,
		ID_glBlendEquationi,
		ID_glBlendEquationSeparatei,
		ID_glBlendFunci,
		ID_glBlendFuncSeparatei,
		ID_glColorMaski,
		ID_glIsEnabledi,
		ID_glDrawElementsBaseVertex,
		ID_glDrawRangeElementsBaseVertex,
		ID_glDrawElementsInstancedBaseVertex,
		ID_glFramebufferTexture,
		ID_glPrimitiveBoundingBox,
		ID_glGetGraphicsResetStatus,
		ID_glReadnPixels,
		ID_glGetnUniformfv,
		ID_glGetnUniformiv,
		ID_glGetnUniformuiv,
		ID_glMinSampleShading,
		ID_glPatchParameteri,
		ID_glTexParameterIiv,
		ID_glTexParameterIuiv,
		ID_glGetTexParameterIiv,
		ID_glGetTexParameterIuiv,
		ID_glSamplerParameterIiv,
		ID_glSamplerParameterIuiv,
		ID_glGetSamplerParameterIiv,
		ID_glGetSamplerParameterIuiv,
		ID_glTexBuffer,
		ID_glTexBufferRange,
		ID_glTexStorage3DMultisample,

		//ID_glBindVertexArray,
		//ID_glBindVertexBuffer,
		//ID_glBindIndexBuffer,
		ID_glBindConstantBuffer,
		//ID_glBindTexture2D,
		ID_glMapBuffer,
		ID_glPolygonMode,
		ID_glFlushDelayExecuter,
		ID_Number,
	};

	int GLApiCounter[ID_Number];
	const char* GLApiName[ID_Number];

	inline void PolygonMode(GLenum face, GLenum mode)
	{
		CMD_PUSH
		{
			::glPolygonMode(face, mode);
		}
		CMD_END(glPolygonMode);
	}

#pragma region GLES3
	inline void ActiveTexture(GLenum texture){
		CMD_PUSH
		{
			::glActiveTexture(texture);
		}
		CMD_END(glActiveTexture);
	};
	inline void AttachShader(const std::shared_ptr<GLSdk::GLBufferId>& program, const std::shared_ptr<GLSdk::GLBufferId>& shader) {
		CMD_PUSH
		{
			::glAttachShader(program->BufferId, shader->BufferId);
		}
		CMD_END(glAttachShader);
	}
	inline void BindAttribLocation(const std::shared_ptr<GLBufferId>& program, GLuint index, const GLchar* name) {
		std::string saved = name;
		CMD_PUSH
		{
			::glBindAttribLocation(program->BufferId, index, saved.c_str());
		}
		CMD_END(glBindAttribLocation);
	}
	void BindVertexBuffer(IGLVertexBuffer* vb);
	void BindIndexBuffer(IGLIndexBuffer* ib);

	std::shared_ptr<GLBufferId> mCurBindBuffer;
	inline void BindBuffer(GLenum target, const std::shared_ptr<GLBufferId>& buffer) {
		CMD_PUSH
		{
			if (buffer != nullptr)
			{
				if (buffer->BufferId == 0)
				{
					WaitOtherCmdListExecuted = TRUE;
					return;
				}
				ASSERT(buffer->BufferId != -1);
				DBG_BindBuffer(target, buffer->BufferId);
				::glBindBuffer(target, buffer->BufferId);
				mCurBindBuffer = buffer;
			}
			else
			{
				DBG_BindBuffer(target, 0);
				::glBindBuffer(target, 0);

				mCurBindBuffer.reset();
			}
		}
		CMD_END(glBindBuffer);
	}
	std::shared_ptr<GLBufferId> mCurFrameBuffer;
	inline void BindFramebuffer(GLenum target, const std::shared_ptr<GLBufferId>& framebuffer) {
		CMD_PUSH
		{
			if (framebuffer != nullptr)
			{
				::glBindFramebuffer(target, framebuffer->BufferId);
				mCurFrameBuffer = framebuffer;
			}
			else
			{
				::glBindFramebuffer(target, 0);
				mCurFrameBuffer.reset();
			}
		}
		CMD_END(glBindFramebuffer);
	}
	std::shared_ptr<GLBufferId> mCurRenderBuffer;
	inline void BindRenderbuffer(GLenum target, const std::shared_ptr<GLBufferId>& renderbuffer) {
		CMD_PUSH
		{
			if (renderbuffer != nullptr)
			{
				ASSERT(renderbuffer->BufferId != 0);
				::glBindRenderbuffer(target, renderbuffer->BufferId);
				mCurRenderBuffer = renderbuffer;
			}
			else
			{
				::glBindRenderbuffer(target, 0);
				mCurRenderBuffer.reset();
			}
		}
		CMD_END(glBindRenderbuffer);
	}
	void BindTexture2D(IGLShaderResourceView* srv);

	std::shared_ptr<GLBufferId> mCurBindTexture;
	inline void BindTexture(GLenum target, const std::shared_ptr<GLBufferId>& texture) {
		CMD_PUSH
		{
			if (texture != nullptr)
			{
				if (texture->BufferId == 0)
				{
					WaitOtherCmdListExecuted = TRUE;
					return;
				}
				ASSERT(texture->BufferId != 0);
				DBG_BindTexture(target, texture->BufferId);
				::glBindTexture(target, texture->BufferId);
				mCurBindTexture = texture;
			}
			else
			{
				DBG_BindTexture(target, 0);
				::glBindTexture(target, 0);
				mCurBindTexture.reset();
			}
		}
		CMD_END(glBindTexture);
	}
	inline void BlendColor(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) {
		CMD_PUSH
		{
			::glBlendColor(red, green, blue, alpha);
		}
		CMD_END(glBlendColor);
	}
	inline void BlendEquation(GLenum mode) {
		CMD_PUSH
		{
			::glBlendEquation(mode);
		}
		CMD_END(glBlendEquation);
	}
	inline void BlendEquationSeparate(GLenum modeRGB, GLenum modeAlpha) {
		CMD_PUSH
			::glBlendEquationSeparate(modeRGB, modeAlpha);
		CMD_END(glBlendEquationSeparate);
	}
	inline void BlendFunc(GLenum sfactor, GLenum dfactor) {
		CMD_PUSH
			::glBlendFunc(sfactor, dfactor);
		CMD_END(glBlendFunc);
	}
	inline void BlendFuncSeparate(GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha) {
		CMD_PUSH
			::glBlendFuncSeparate(srcRGB, dstRGB, srcAlpha, dstAlpha);
		CMD_END(glBlendFuncSeparate);
	}
	void BufferData(GLenum target, GLsizeiptr size, const GLvoid* data, GLenum usage);
	void BufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid* data);
	inline static GLenum CheckFramebufferStatus(GLenum target) {
		return ::glCheckFramebufferStatus(target);
	}
	inline void Clear(GLbitfield mask) {
		CMD_PUSH
			::glClear(mask);
		CMD_END(glClear);
	}
	inline void ClearColor(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) {
		CMD_PUSH
			::glClearColor(red, green, blue, alpha);
		CMD_END(glClearColor);
	}
	inline void ClearDepthf(GLfloat depth) {
		CMD_PUSH
			::glClearDepthf(depth);
		CMD_END(glClearDepthf);
	}
	inline void ClearStencil(GLint s) {
		CMD_PUSH
			::glClearStencil(s);
		CMD_END(glClearStencil);
	}
	inline void ColorMask(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha) {
		CMD_PUSH
			::glColorMask(red, green, blue, alpha);
		CMD_END(glColorMask);
	}
	inline void CompileShader(std::shared_ptr<GLSdk::GLBufferId> shader) {
		CMD_PUSH
			::glCompileShader(shader->BufferId);
		CMD_END(glCompileShader);
	}
	void CompressedTexImage2D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid* data);
	void CompressedTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid* data);
	inline void CopyTexImage2D(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border) {
		CMD_PUSH
			::glCopyTexImage2D(target, level, internalformat, x, y, width, height, border);
		CMD_END(glCopyTexImage2D);
	}
	inline void CopyTexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glCopyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height);
		CMD_END(glCopyTexSubImage2D);
	}
	std::shared_ptr<GLSdk::GLBufferId> CreateProgram();
	std::shared_ptr<GLSdk::GLBufferId> CreateShader(GLenum type);
	inline void CullFace(GLenum mode) {
		CMD_PUSH
			::glCullFace(mode);
		CMD_END(glCullFace);
	}
	inline void DepthFunc(GLenum func) {
		CMD_PUSH
			::glDepthFunc(func);
		CMD_END(glDepthFunc);
	}
	inline void DepthMask(GLboolean flag) {
		CMD_PUSH
			::glDepthMask(flag);
		CMD_END(glDepthMask);
	}
	inline void DepthRangef(GLfloat n, GLfloat f) {
		CMD_PUSH
			::glDepthRangef(n, f);
		CMD_END(glDepthRangef);
	}
	inline void DetachShader(const std::shared_ptr<GLSdk::GLBufferId>& program, const std::shared_ptr<GLSdk::GLBufferId>& shader) {
		CMD_PUSH
			::glDetachShader(program->BufferId, shader->BufferId);
		CMD_END(glDetachShader);
	}
	inline void Disable(GLenum cap) {
		CMD_PUSH
			::glDisable(cap);
		CMD_END(glDisable);
	}
	inline void DisableVertexAttribArray(GLuint index) {
		CMD_PUSH
			::glDisableVertexAttribArray(index);
		CMD_END(glDisableVertexAttribArray);
	}
	void DrawArrays(GLenum mode, GLint first, GLsizei count);
	void DrawElements(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices);
	inline void Enable(GLenum cap) {
		CMD_PUSH
			::glEnable(cap);
		CMD_END(glEnable);
	}
	inline void EnableVertexAttribArray(GLuint index) {
		CMD_PUSH
			::glEnableVertexAttribArray(index);
		CMD_END(glEnableVertexAttribArray);
	}
	inline void Finish(void) {
		CMD_PUSH
			::glFinish();
		CMD_END(glFinish);
	}
	inline void Flush(void) {
		CMD_PUSH
			::glFlush();
		CMD_END(glFlush);
	}
	inline void FramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget, const std::shared_ptr<GLBufferId>& renderbuffer) {
		CMD_PUSH
		{
			if (renderbuffer != nullptr)
			{
				ASSERT(renderbuffer->BufferId != 0);
				::glFramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer->BufferId);
			}
			else
			{
				::glFramebufferRenderbuffer(target, attachment, renderbuffertarget, 0);
			}
		}
		CMD_END(glFramebufferRenderbuffer);
	}
	inline void FramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget, const std::shared_ptr<GLBufferId>& texture, GLint level) {
		CMD_PUSH
		{
			if (texture != nullptr)
			{
				ASSERT(texture->BufferId != 0);
				::glFramebufferTexture2D(target, attachment, textarget, texture->BufferId, level);
			}
			else
			{
				::glFramebufferTexture2D(target, attachment, textarget, 0, level);
			}
		}
		CMD_END(glFramebufferTexture2D);
	}
	inline void FrontFace(GLenum mode) {
		CMD_PUSH
			::glFrontFace(mode);
		CMD_END(glFrontFace);
	}
	std::shared_ptr<GLBufferId> GenBufferId();
	/*inline void GenBuffers(GLsizei n, GLuint* buffers) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenBuffers(n, buffers);
		CMD_END(glGenBuffers);
	}*/
	inline void GenerateMipmap(GLenum target) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenerateMipmap(target);
		CMD_END(glGenerateMipmap);
	}
	std::shared_ptr<GLBufferId> GenFramebuffers();
	std::shared_ptr<GLBufferId> GenRenderbuffers();
	/*inline void GenRenderbuffers(GLsizei n, GLuint* renderbuffers) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenRenderbuffers(n, renderbuffers);
		CMD_END(glGenRenderbuffers);
	}*/
	std::shared_ptr<GLBufferId> GenTextures();
	/*inline void GenTextures(GLsizei n, GLuint* textures) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenTextures(n, textures);
		CMD_END(glGenTextures);
	}*/
	inline void GetActiveAttrib(const std::shared_ptr<GLSdk::GLBufferId>& program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetActiveAttrib(program->BufferId, index, bufsize, length, size, type, name);
		CMD_END(glGetActiveAttrib);
	}
	inline void GetActiveUniform(const std::shared_ptr<GLSdk::GLBufferId>& program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetActiveUniform(program->BufferId, index, bufsize, length, size, type, name);
		CMD_END(glGetActiveUniform);
	}
	inline void GetAttachedShaders(const std::shared_ptr<GLSdk::GLBufferId>& program, GLsizei maxcount, GLsizei* count, GLuint* shaders) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetAttachedShaders(program->BufferId, maxcount, count, shaders);
		CMD_END(glGetAttachedShaders);
	}
	inline void GetAttribLocation(GLint* location, const std::shared_ptr<GLSdk::GLBufferId>& program, const GLchar* name) {
		ASSERT(mImmMode);
		CMD_PUSH
			*location = ::glGetAttribLocation(program->BufferId, name);
		CMD_END_NOCHECK(glGetAttribLocation);
	}
	inline void GetBooleanv(GLenum pname, GLboolean* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetBooleanv(pname, params);
		CMD_END(glGetBooleanv);
	}
	inline void GetBufferParameteriv(GLenum target, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetBufferParameteriv(target, pname, params);
		CMD_END(glGetBufferParameteriv);
	}
	inline static GLenum GetError(void) {
		return glGetError();
	}
	inline void GetFloatv(GLenum pname, GLfloat* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetFloatv(pname, params);
		CMD_END(glGetFloatv);
	}
	inline void GetFramebufferAttachmentParameteriv(GLenum target, GLenum attachment, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetFramebufferAttachmentParameteriv(target, attachment, pname, params);
		CMD_END(glGetFramebufferAttachmentParameteriv);
	}
	inline void GetIntegerv(GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetIntegerv(pname, params);
		CMD_END(glGetIntegerv);
	}
	std::shared_ptr<GLSdk::GLBufferId> GetIntegerv(GLenum pname);
	inline void GetProgramiv(const std::shared_ptr<GLSdk::GLBufferId>& program, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetProgramiv(program->BufferId, pname, params);
		CMD_END(glGetProgramiv);
	}
	inline void GetProgramInfoLog(const std::shared_ptr<GLSdk::GLBufferId>& program, GLsizei bufsize, GLsizei* length, GLchar* infolog) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetProgramInfoLog(program->BufferId, bufsize, length, infolog);
		CMD_END(glGetProgramInfoLog);
	}
	inline void GetRenderbufferParameteriv(GLenum target, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetRenderbufferParameteriv(target, pname, params);
		CMD_END(glGetRenderbufferParameteriv);
	}
	inline void GetShaderiv(const std::shared_ptr<GLSdk::GLBufferId>& shader, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetShaderiv(shader->BufferId, pname, params);
		CMD_END(glGetShaderiv);
	}
	inline void GetShaderInfoLog(const std::shared_ptr<GLSdk::GLBufferId>& shader, GLsizei bufsize, GLsizei* length, GLchar* infolog) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetShaderInfoLog(shader->BufferId, bufsize, length, infolog);
		CMD_END(glGetShaderInfoLog);
	}
	inline void GetShaderPrecisionFormat(GLenum shadertype, GLenum precisiontype, GLint* range, GLint* precision) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetShaderPrecisionFormat(shadertype, precisiontype, range, precision);
		CMD_END(glGetShaderPrecisionFormat);
	}
	inline void GetShaderSource(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetShaderSource(shader, bufsize, length, source);
		CMD_END(glGetShaderSource);
	}
	inline static const GLubyte* GetString(GLenum name) {
		return ::glGetString(name);
	}
	inline void GetTexParameterfv(GLenum target, GLenum pname, GLfloat* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetTexParameterfv(target, pname, params);
		CMD_END(glGetTexParameterfv);
	}
	inline void GetTexParameteriv(GLenum target, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetTexParameteriv(target, pname, params);
		CMD_END(glGetTexParameteriv);
	}
	inline void GetUniformfv(GLuint program, GLint location, GLfloat* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetUniformfv(program, location, params);
		CMD_END(glGetUniformfv);
	}
	inline void GetUniformiv(GLuint program, GLint location, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetUniformiv(program, location, params);
		CMD_END(glGetUniformiv);
	}
	inline void GetUniformLocation(GLint* location, const std::shared_ptr<GLSdk::GLBufferId>& program, const GLchar* name) {
		ASSERT(mImmMode);
		CMD_PUSH
			*location =::glGetUniformLocation(program->BufferId, name);
		CMD_END_NOCHECK(glGetUniformLocation);
	}
	inline void GetVertexAttribfv(GLuint index, GLenum pname, GLfloat* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetVertexAttribfv(index, pname, params);
		CMD_END(glGetVertexAttribfv);
	}
	inline void GetVertexAttribiv(GLuint index, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetVertexAttribiv(index, pname, params);
		CMD_END(glGetVertexAttribiv);
	}
	inline void GetVertexAttribPointerv(GLuint index, GLenum pname, GLvoid** pointer) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetVertexAttribPointerv(index, pname, pointer);
		CMD_END(glGetVertexAttribPointerv);
	}
	inline void Hint(GLenum target, GLenum mode) {
		CMD_PUSH
			::glHint(target, mode);
		CMD_END(glHint);
	}
	inline void IsBuffer(GLboolean* ret, GLuint buffer) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsBuffer(buffer);
		CMD_END(glIsBuffer);
	}
	inline void IsEnabled(GLboolean* ret, GLenum cap) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsEnabled(cap);
		CMD_END(glIsEnabled);
	}
	inline void IsFramebuffer(GLboolean *ret, GLuint framebuffer) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsFramebuffer(framebuffer);
		CMD_END(glIsFramebuffer);
	}
	inline void IsProgram(GLboolean *ret, GLuint program) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsProgram(program);
		CMD_END(glIsProgram);
	}
	inline void IsRenderbuffer(GLboolean *ret, GLuint renderbuffer) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsRenderbuffer(renderbuffer);
		CMD_END(glIsRenderbuffer);
	}
	inline void IsShader(GLboolean *ret, GLuint shader) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsShader(shader);
		CMD_END(glIsShader);
	}
	inline void IsTexture(GLboolean *ret, GLuint texture) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsTexture(texture);
		CMD_END(glIsTexture);
	}
	inline void LineWidth(GLfloat width) {
		CMD_PUSH
			::glLineWidth(width);
		CMD_END(glLineWidth);
	}
	void LinkProgram(const std::shared_ptr<GLSdk::GLBufferId>& program, void* arg);
	inline void PixelStorei(GLenum pname, GLint param) {
		CMD_PUSH
			::glPixelStorei(pname, param);
		CMD_END(glPixelStorei);
	}
	inline void PolygonOffset(GLfloat factor, GLfloat units) {
		CMD_PUSH
			::glPolygonOffset(factor, units);
		CMD_END(glPolygonOffset);
	}
	inline void ReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLvoid* pixels) {
		ASSERT(pixels == nullptr);
		CMD_PUSH
			::glReadPixels(x, y, width, height, format, type, pixels);
		CMD_END(glReadPixels);
	}
	inline void ReleaseShaderCompiler(void) {
		CMD_PUSH
			::glReleaseShaderCompiler();
		CMD_END(glReleaseShaderCompiler);
	}
	inline void RenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glRenderbufferStorage(target, internalformat, width, height);
		CMD_END(glRenderbufferStorage);
	}
	inline void SampleCoverage(GLfloat value, GLboolean invert) {
		CMD_PUSH
			::glSampleCoverage(value, invert);
		CMD_END(glSampleCoverage);
	}
	inline void Scissor(GLint x, GLint y, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glScissor(x, y, width, height);
		CMD_END(glScissor);
	}
	inline void ShaderBinary(GLsizei n, const GLuint* shaders, GLenum binaryformat, const GLvoid* binary, GLsizei length) {
		std::vector<BYTE> saved;
		saved.resize(length);
		memcpy(&saved[0], binary, length);
		CMD_PUSH
			::glShaderBinary(n, shaders, binaryformat, &saved[0], length);
		CMD_END(glShaderBinary);
	}
	inline void ShaderSource(const std::shared_ptr<GLSdk::GLBufferId>& shader, GLsizei count, const GLchar* const* str, const GLint* length) {
		std::string saved = *str;
		CMD_PUSH
		{
			auto code = saved.c_str();
			::glShaderSource(shader->BufferId, count, &code, length);
		}
		CMD_END(glShaderSource);
	}
	inline void StencilFunc(GLenum func, GLint ref, GLuint mask) {
		CMD_PUSH
			::glStencilFunc(func, ref, mask);
		CMD_END(glStencilFunc);
	}
	inline void StencilFuncSeparate(GLenum face, GLenum func, GLint ref, GLuint mask) {
		CMD_PUSH
			::glStencilFuncSeparate(face, func, ref, mask);
		CMD_END(glStencilFuncSeparate);
	}
	inline void StencilMask(GLuint mask) {
		CMD_PUSH
			::glStencilMask(mask);
		CMD_END(glStencilMask);
	}
	inline void StencilMaskSeparate(GLenum face, GLuint mask) {
		CMD_PUSH
			::glStencilMaskSeparate(face, mask);
		CMD_END(glStencilMaskSeparate);
	}
	inline void StencilOp(GLenum fail, GLenum zfail, GLenum zpass) {
		CMD_PUSH
			::glStencilOp(fail, zfail, zpass);
		CMD_END(glStencilOp);
	}
	inline void StencilOpSeparate(GLenum face, GLenum fail, GLenum zfail, GLenum zpass) {
		CMD_PUSH
			::glStencilOpSeparate(face, fail, zfail, zpass);
		CMD_END(glStencilOpSeparate);
	}
	void TexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid* pixels);
	inline void TexParameterf(GLenum target, GLenum pname, GLfloat param) {
		CMD_PUSH
			::glTexParameterf(target, pname, param);
		CMD_END(glTexParameterf);
	}
	inline void TexParameterfv(GLenum target, GLenum pname, const GLfloat* params) {
		GLfloat saved[4];
		memcpy(saved, params, sizeof(GLfloat) * 4);
		CMD_PUSH
			::glTexParameterfv(target, pname, saved);
		CMD_END(glTexParameterfv);
	}
	inline void TexParameteri(GLenum target, GLenum pname, GLint param) {
		CMD_PUSH
			::glTexParameteri(target, pname, param);
		CMD_END(glTexParameteri);
	}
	inline void TexParameteriv(GLenum target, GLenum pname, const GLint* params) {
		GLint saved[4];
		memcpy(saved, params, sizeof(GLint) * 4);
		CMD_PUSH
			::glTexParameteriv(target, pname, saved);
		CMD_END(glTexParameteriv);
	}
	inline void TexSubImage2D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const GLvoid* pixels) {
		CMD_PUSH
			::glTexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels);
		CMD_END(glTexSubImage2D);
	}
	inline void Uniform1f(GLint location, GLfloat x) {
		CMD_PUSH
			::glUniform1f(location, x);
		CMD_END(glUniform1f);
	}
	inline void Uniform1fv(GLint location, GLsizei count, const GLfloat* v) {
		GLfloat saved = *v;
		CMD_PUSH
			::glUniform1fv(location, count, &saved);
		CMD_END(glUniform1fv);
	}
	inline void Uniform1i(GLint location, GLint x) {
		CMD_PUSH
			::glUniform1i(location, x);
		CMD_END_NOCHECK(glUniform1i);
	}
	inline void Uniform1iv(GLint location, GLsizei count, const GLint* v) {
		GLint saved = *v;
		CMD_PUSH
			::glUniform1iv(location, count, &saved);
		CMD_END(glUniform1iv);
	}
	inline void Uniform2f(GLint location, GLfloat x, GLfloat y) {
		CMD_PUSH
			::glUniform2f(location, x, y);
		CMD_END(glUniform2f);
	}
	inline void Uniform2fv(GLint location, GLsizei count, const GLfloat* v) {
		GLfloat saved[2];
		saved[0] = v[0];
		saved[1] = v[1];
		CMD_PUSH
			::glUniform2fv(location, count, saved);
		CMD_END(glUniform2fv);
	}
	inline void Uniform2i(GLint location, GLint x, GLint y) {
		CMD_PUSH
			::glUniform2i(location, x, y);
		CMD_END(glUniform2i);
	}
	inline void Uniform2iv(GLint location, GLsizei count, const GLint* v) {
		GLint saved[2];
		saved[0] = v[0];
		saved[1] = v[1];
		CMD_PUSH
			::glUniform2iv(location, count, saved);
		CMD_END(glUniform2iv);
	}
	inline void Uniform3f(GLint location, GLfloat x, GLfloat y, GLfloat z) {
		CMD_PUSH
			::glUniform3f(location, x, y, z);
		CMD_END(glUniform3f);
	}
	inline void Uniform3fv(GLint location, GLsizei count, const GLfloat* v) {
		GLfloat saved[3];
		saved[0] = v[0];
		saved[1] = v[1];
		saved[2] = v[2];
		CMD_PUSH
			::glUniform3fv(location, count, saved);
		CMD_END(glUniform3fv);
	}
	inline void Uniform3i(GLint location, GLint x, GLint y, GLint z) {
		CMD_PUSH
			::glUniform3i(location, x, y, z);
		CMD_END(glUniform3i);
	}
	inline void Uniform3iv(GLint location, GLsizei count, const GLint* v) {
		GLint saved[3];
		saved[0] = v[0];
		saved[1] = v[1];
		saved[2] = v[2];
		CMD_PUSH
			::glUniform3iv(location, count, saved);
		CMD_END(glUniform3iv);
	}
	inline void Uniform4f(GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w) {
		CMD_PUSH
			::glUniform4f(location, x, y, z, w);
		CMD_END(glUniform4f);
	}
	inline void Uniform4fv(GLint location, GLsizei count, const GLfloat* v) {
		GLfloat saved[4];
		saved[0] = v[0];
		saved[1] = v[1];
		saved[2] = v[2];
		saved[3] = v[3];
		CMD_PUSH
			::glUniform4fv(location, count, saved);
		CMD_END(glUniform4fv);
	}
	inline void Uniform4i(GLint location, GLint x, GLint y, GLint z, GLint w) {
		CMD_PUSH
			::glUniform4i(location, x, y, z, w);
		CMD_END(glUniform4i);
	}
	inline void Uniform4iv(GLint location, GLsizei count, const GLint* v) {
		GLint saved[4];
		saved[0] = v[0];
		saved[1] = v[1];
		saved[2] = v[2];
		saved[3] = v[3];
		CMD_PUSH
			::glUniform4iv(location, count, saved);
		CMD_END(glUniform4iv);
	}
	inline void UniformMatrix2fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[4];
		memcpy(saved, value, sizeof(GLfloat) * 4);
		CMD_PUSH
			::glUniformMatrix2fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix2fv);
	}
	inline void UniformMatrix3fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[8];
		memcpy(saved, value, sizeof(GLfloat) * 8);
		CMD_PUSH
			::glUniformMatrix3fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix3fv);
	}
	inline void UniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[16];
		memcpy(saved, value, sizeof(GLfloat) * 16);
		CMD_PUSH
			::glUniformMatrix4fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix4fv);
	}
	std::shared_ptr<GLSdk::GLBufferId> mCurProgram;
	void UseProgram(const std::shared_ptr<GLSdk::GLBufferId>& program, void* arg);
	inline void ValidateProgram(GLuint program) {
		CMD_PUSH
			::glValidateProgram(program);
		CMD_END(glValidateProgram);
	}
	inline void VertexAttrib1f(GLuint indx, GLfloat x) {
		CMD_PUSH
			::glVertexAttrib1f(indx, x);
		CMD_END(glVertexAttrib1f);
	}
	inline void VertexAttrib1fv(GLuint indx, const GLfloat* values) {
		GLfloat saved = *values;
		CMD_PUSH
			::glVertexAttrib1fv(indx, &saved);
		CMD_END(glVertexAttrib1fv);
	}
	inline void VertexAttrib2f(GLuint indx, GLfloat x, GLfloat y) {
		CMD_PUSH
			::glVertexAttrib2f(indx, x, y);
		CMD_END(glVertexAttrib2f);
	}
	inline void VertexAttrib2fv(GLuint indx, const GLfloat* values) {
		GLfloat saved[2];
		saved[0] = values[0];
		saved[1] = values[1];
		CMD_PUSH
			::glVertexAttrib2fv(indx, saved);
		CMD_END(glVertexAttrib2fv);
	}
	inline void VertexAttrib3f(GLuint indx, GLfloat x, GLfloat y, GLfloat z) {
		CMD_PUSH
			::glVertexAttrib3f(indx, x, y, z);
		CMD_END(glVertexAttrib3f);
	}
	inline void VertexAttrib3fv(GLuint indx, const GLfloat* values) {
		GLfloat saved[3];
		saved[0] = values[0];
		saved[1] = values[1];
		saved[2] = values[2];
		CMD_PUSH
			::glVertexAttrib3fv(indx, saved);
		CMD_END(glVertexAttrib3fv);
	}
	inline void VertexAttrib4f(GLuint indx, GLfloat x, GLfloat y, GLfloat z, GLfloat w) {
		CMD_PUSH
			::glVertexAttrib4f(indx, x, y, z, w);
		CMD_END(glVertexAttrib4f);
	}
	inline void VertexAttrib4fv(GLuint indx, const GLfloat* values) {
		GLfloat saved[4];
		saved[0] = values[0];
		saved[1] = values[1];
		saved[2] = values[2];
		saved[3] = values[3];
		CMD_PUSH
			::glVertexAttrib4fv(indx, saved);
		CMD_END(glVertexAttrib4fv);
	}
	inline void VertexAttribPointer(GLuint indx, GLint size, GLenum type, GLboolean normalized, GLsizei stride, const GLvoid* ptr) {
		CMD_PUSH
			::glVertexAttribPointer(indx, size, type, normalized, stride, ptr);
		CMD_END(glVertexAttribPointer);
	}
	inline void Viewport(GLint x, GLint y, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glViewport(x, y, width, height);
		CMD_END(glViewport);
	}

	// OpenGL ES 3.0

	inline void ReadBuffer(GLenum mode) {
		CMD_PUSH
			::glReadBuffer(mode);
		CMD_END(glReadBuffer);
	}
	inline void DrawRangeElements(GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, const GLvoid* indices) {
		CMD_PUSH
			::glDrawRangeElements(mode, start, end, count, type, indices);
		CMD_END(glDrawRangeElements);
	}
	inline void TexImage3D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLenum format, GLenum type, const GLvoid* pixels) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glTexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixels);
		CMD_END(glTexImage3D);
	}
	inline void TexSubImage3D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, const GLvoid* pixels) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixels);
		CMD_END(glTexSubImage3D);
	}
	inline void CopyTexSubImage3D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glCopyTexSubImage3D(target, level, xoffset, yoffset, zoffset, x, y, width, height);
			CMD_END(glCopyTexSubImage3D);
	}
	inline void CompressedTexImage3D(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLsizei imageSize, const GLvoid* data) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glCompressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, data);
		CMD_END(glCompressedTexImage3D);
	}
	inline void CompressedTexSubImage3D(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize, const GLvoid* data) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glCompressedTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, data);
		CMD_END(glCompressedTexSubImage3D);
	}
	inline void GenQueries(GLsizei n, GLuint* ids) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenQueries(n, ids);
		CMD_END(glGenQueries);
	}
	inline void DeleteQueries(GLsizei n, const GLuint* ids) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glDeleteQueries(n, ids);
		CMD_END(glDeleteQueries);
	}
	inline void IsQuery(GLboolean* ret, GLuint id) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsQuery(id);
		CMD_END(glIsQuery);
	}
	inline void BeginQuery(GLenum target, GLuint id) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glBeginQuery(target, id);
		CMD_END(glBeginQuery);
	}
	inline void EndQuery(GLenum target) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glEndQuery(target);
		CMD_END(glEndQuery);
	}
	inline void GetQueryiv(GLenum target, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetQueryiv(target, pname, params);
		CMD_END(glGetQueryiv);
	}
	inline void GetQueryObjectuiv(GLuint id, GLenum pname, GLuint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetQueryObjectuiv(id, pname, params);
		CMD_END(glGetQueryObjectuiv);
	}
	inline void MapBuffer(void** ppData, GLenum target, GLenum access)
	{
		ASSERT(mImmMode);
		CMD_PUSH
		{
			*ppData = ::glMapBuffer(target, access);
		}	
		CMD_END(glMapBuffer);
	}
	inline void UnmapBuffer(GLboolean* ret, GLenum target) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glUnmapBuffer(target);
		CMD_END(glUnmapBuffer);
	}
	inline void GetBufferPointerv(GLenum target, GLenum pname, GLvoid** params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetBufferPointerv(target, pname, params);
		CMD_END(glGetBufferPointerv);
	}
	void DrawBuffers(GLsizei n, const GLenum* bufs, const std::function<FGLApiExecute>& onStateError);
	inline void UniformMatrix2x3fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[6];
		memcpy(saved, value, sizeof(GLfloat) * 6);
		CMD_PUSH
			::glUniformMatrix2x3fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix2x3fv);
	}
	inline void UniformMatrix3x2fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[6];
		memcpy(saved, value, sizeof(GLfloat) * 6);
		CMD_PUSH
			::glUniformMatrix3x2fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix3x2fv);
	}
	inline void UniformMatrix2x4fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[8];
		memcpy(saved, value, sizeof(GLfloat) * 8);
		CMD_PUSH
			::glUniformMatrix2x4fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix2x4fv);
	}
	inline void UniformMatrix4x2fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[8];
		memcpy(saved, value, sizeof(GLfloat) * 8);
		CMD_PUSH
			::glUniformMatrix4x2fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix4x2fv);
	}
	inline void UniformMatrix3x4fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[12];
		memcpy(saved, value, sizeof(GLfloat) * 12);
		CMD_PUSH
			::glUniformMatrix3x4fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix3x4fv);
	}
	inline void UniformMatrix4x3fv(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {
		GLfloat saved[12];
		memcpy(saved, value, sizeof(GLfloat) * 12);
		CMD_PUSH
			::glUniformMatrix4x3fv(location, count, transpose, saved);
		CMD_END(glUniformMatrix4x3fv);
	}
	inline void BlitFramebuffer(GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter) {
		CMD_PUSH
			::glBlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
		CMD_END(glBlitFramebuffer);
	}
	inline void RenderbufferStorageMultisample(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glRenderbufferStorageMultisample(target, samples, internalformat, width, height);
		CMD_END(glRenderbufferStorageMultisample);
	}
	inline void FramebufferTextureLayer(GLenum target, GLenum attachment, GLuint texture, GLint level, GLint layer) {
		CMD_PUSH
			::glFramebufferTextureLayer(target, attachment, texture, level, layer);
		CMD_END(glFramebufferTextureLayer);
	}
	inline void MapBufferRange(GLvoid** ppData, GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ppData =::glMapBufferRange(target, offset, length, access);
		CMD_END(glMapBufferRange);
	}
	inline void FlushMappedBufferRange(GLenum target, GLintptr offset, GLsizeiptr length) {
		CMD_PUSH
			::glFlushMappedBufferRange(target, offset, length);
		CMD_END(glFlushMappedBufferRange);
	}
	//void BindVertexArray(IGLGeometryMesh* mesh);
	inline void BindVertexArray(const std::shared_ptr<GLSdk::GLBufferId>& vao)
	{
		CMD_PUSH
			if (vao != nullptr)
			{
				::glBindVertexArray(vao->BufferId);
			}
			else
			{
				::glBindVertexArray(0);
			}
		CMD_END(glBindVertexArray)
	}
	std::shared_ptr<GLSdk::GLBufferId> GenVertexArrays();
	/*inline void GenVertexArrays(GLsizei n, GLuint* arrays) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenVertexArrays(n, arrays);
		CMD_END(glGenVertexArrays);
	}*/
	inline void IsVertexArray(GLboolean* ret, GLuint array) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsVertexArray(array);
		CMD_END(glIsVertexArray);
	}
	inline void GetIntegeri_v(GLenum target, GLuint index, GLint* data) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetIntegeri_v(target, index, data);
		CMD_END(glGetIntegeri_v);
	}
	inline void BeginTransformFeedback(GLenum primitiveMode) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glBeginTransformFeedback(primitiveMode);
		CMD_END(glBeginTransformFeedback);
	}
	inline void EndTransformFeedback(void) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glEndTransformFeedback();
		CMD_END(glEndTransformFeedback);
	}
	inline void BindBufferRange(GLenum target, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size) {
		CMD_PUSH
			::glBindBufferRange(target, index, buffer, offset, size);
		CMD_END(glBindBufferRange);
	}
	void BindConstantBuffer(UINT32 Index, IGLConstantBuffer* cb);
	inline void BindBufferBase(GLenum target, GLuint index, const std::shared_ptr<GLBufferId> buffer) {
		CMD_PUSH
			if (buffer != nullptr)
			{
				::glBindBufferBase(target, index, buffer->BufferId);
			}
			else
			{
				::glBindBufferBase(target, index, 0);
			}
		CMD_END(glBindBufferBase);
	}
	inline void TransformFeedbackVaryings(GLuint program, GLsizei count, const GLchar* const* varyings, GLenum bufferMode) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glTransformFeedbackVaryings(program, count, varyings, bufferMode);
		CMD_END(glTransformFeedbackVaryings);
	}
	inline void GetTransformFeedbackVarying(GLuint program, GLuint index, GLsizei bufSize, GLsizei* length, GLsizei* size, GLenum* type, GLchar* name) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetTransformFeedbackVarying(program, index, bufSize, length, size, type, name);
		CMD_END(glGetTransformFeedbackVarying);
	}
	inline void VertexAttribIPointer(GLuint index, GLint size, GLenum type, GLsizei stride, const GLvoid* pointer) {
		CMD_PUSH
			::glVertexAttribIPointer(index, size, type, stride, pointer);
		CMD_END(glVertexAttribIPointer);
	}
	inline void GetVertexAttribIiv(GLuint index, GLenum pname, GLint* params) {
		CMD_PUSH
			::glGetVertexAttribIiv(index, pname, params);
		CMD_END(glGetVertexAttribIiv);
	}
	inline void GetVertexAttribIuiv(GLuint index, GLenum pname, GLuint* params) {
		CMD_PUSH
			::glGetVertexAttribIuiv(index, pname, params);
		CMD_END(glGetVertexAttribIuiv);
	}
	inline void VertexAttribI4i(GLuint index, GLint x, GLint y, GLint z, GLint w) {
		CMD_PUSH
			::glVertexAttribI4i(index, x, y, z, w);
		CMD_END(glVertexAttribI4i);
	}
	inline void VertexAttribI4ui(GLuint index, GLuint x, GLuint y, GLuint z, GLuint w) {
		CMD_PUSH
			::glVertexAttribI4ui(index, x, y, z, w);
		CMD_END(glVertexAttribI4ui);
	}
	inline void VertexAttribI4iv(GLuint index, const GLint* v) {
		GLint saved[4];
		memcpy(saved, v, sizeof(GLint) * 4);
		CMD_PUSH
			::glVertexAttribI4iv(index, saved);
		CMD_END(glVertexAttribI4iv);
	}
	inline void VertexAttribI4uiv(GLuint index, const GLuint* v) {
		GLuint saved[4];
		memcpy(saved, v, sizeof(GLuint) * 4);
		CMD_PUSH
			::glVertexAttribI4uiv(index, saved);
		CMD_END(glVertexAttribI4uiv);
	}
	inline void GetUniformuiv(GLuint program, GLint location, GLuint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetUniformuiv(program, location, params);
		CMD_END(glGetUniformuiv);
	}
	inline void GetFragDataLocation(GLint* location,GLuint program, const GLchar *name) {
		ASSERT(mImmMode);
		CMD_PUSH
			*location = ::glGetFragDataLocation(program, name);
		CMD_END(glGetFragDataLocation);
	}
	inline void Uniform1ui(GLint location, GLuint v0){
		CMD_PUSH
			::glUniform1ui(location, v0);
		CMD_END(glUniform1ui);
	}
	inline void Uniform2ui(GLint location, GLuint v0, GLuint v1) {
		CMD_PUSH
			::glUniform2ui(location, v0, v1);
		CMD_END(glUniform2ui);
	}
	inline void Uniform3ui(GLint location, GLuint v0, GLuint v1, GLuint v2) {
		CMD_PUSH
			::glUniform3ui(location, v0, v1, v2);
		CMD_END(glUniform3ui);
	}
	inline void Uniform4ui(GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3) {
		CMD_PUSH
			::glUniform4ui(location, v0, v1, v2, v3);
		CMD_END(glUniform4ui);
	}
	inline void Uniform1uiv(GLint location, GLsizei count, const GLuint* value) {
		GLuint saved[1];
		saved[0] = value[0];
		CMD_PUSH
			::glUniform1uiv(location, count, saved);
		CMD_END(glUniform1uiv);
	}
	inline void Uniform2uiv(GLint location, GLsizei count, const GLuint* value) {
		GLuint saved[2];
		memcpy(saved, value, 2 * sizeof(GLuint));
		CMD_PUSH
			::glUniform2uiv(location, count, saved);
		CMD_END(glUniform2uiv);
	}
	inline void Uniform3uiv(GLint location, GLsizei count, const GLuint* value) {
		GLuint saved[3];
		memcpy(saved, value, 3 * sizeof(GLuint));
		CMD_PUSH
			::glUniform3uiv(location, count, saved);
		CMD_END(glUniform3uiv);
	}
	inline void Uniform4uiv(GLint location, GLsizei count, const GLuint* value) {
		GLuint saved[4];
		memcpy(saved, value, 4 * sizeof(GLuint));
		CMD_PUSH
			::glUniform4uiv(location, count, saved);
		CMD_END(glUniform4uiv);
	}
	inline void ClearBufferiv(GLenum buffer, GLint drawbuffer, const GLint* value) {
		GLint saved = *value;
		CMD_PUSH
			::glClearBufferiv(buffer, drawbuffer, &saved);
		CMD_END(glClearBufferiv);
	}
	inline void ClearBufferuiv(GLenum buffer, GLint drawbuffer, const GLuint* value) {
		GLuint saved = *value;
		CMD_PUSH
			::glClearBufferuiv(buffer, drawbuffer, &saved);
		CMD_END(glClearBufferuiv);
	}
	inline void ClearBufferfv(GLenum buffer, GLint drawbuffer, const GLfloat* value) {
		GLfloat saved[4];
		memcpy(saved, value, sizeof(GLfloat)*4);
		CMD_PUSH
			::glClearBufferfv(buffer, drawbuffer, saved);
		CMD_END(glClearBufferfv);
	}
	inline void ClearBufferfi(GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil) {
		CMD_PUSH
			::glClearBufferfi(buffer, drawbuffer, depth, stencil);
		CMD_END(glClearBufferfi);
	}
	inline const GLubyte* GetStringi(GLenum name, GLuint index) {
		return ::glGetStringi(name, index);
	}
	inline void CopyBufferSubData(GLenum readTarget, GLenum writeTarget, GLintptr readOffset, GLintptr writeOffset, GLsizeiptr size) {
		CMD_PUSH
			::glCopyBufferSubData(readTarget, writeTarget, readOffset, writeOffset, size);
		CMD_END(glCopyBufferSubData);
	}
	inline void GetUniformIndices(const std::shared_ptr<GLSdk::GLBufferId>& program, GLsizei uniformCount, const GLchar* const* uniformNames, GLuint* uniformIndices) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetUniformIndices(program->BufferId, uniformCount, uniformNames, uniformIndices);
		CMD_END(glGetUniformIndices);
	}
	inline void GetActiveUniformsiv(const std::shared_ptr<GLSdk::GLBufferId>& program, GLsizei uniformCount, const GLuint* uniformIndices, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetActiveUniformsiv(program->BufferId, uniformCount, uniformIndices, pname, params);
		CMD_END(glGetActiveUniformsiv);
	}
	inline void GetUniformBlockIndex(GLuint* index, const std::shared_ptr<GLSdk::GLBufferId>& program, const GLchar* uniformBlockName) {
		ASSERT(mImmMode);
		CMD_PUSH
			*index =::glGetUniformBlockIndex(program->BufferId, uniformBlockName);
		CMD_END(glGetUniformBlockIndex);
	}
	inline void GetActiveUniformBlockiv(const std::shared_ptr<GLSdk::GLBufferId>& program, GLuint uniformBlockIndex, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetActiveUniformBlockiv(program->BufferId, uniformBlockIndex, pname, params);
		CMD_END(glGetUniformBlockIndex);
	}
	inline void GetActiveUniformBlockName(const std::shared_ptr<GLSdk::GLBufferId>& program, GLuint uniformBlockIndex, GLsizei bufSize, GLsizei* length, GLchar* uniformBlockName) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetActiveUniformBlockName(program->BufferId, uniformBlockIndex, bufSize, length, uniformBlockName);
		CMD_END(glGetActiveUniformBlockName);
	}
	inline void UniformBlockBinding(const std::shared_ptr<GLSdk::GLBufferId>& program, GLuint uniformBlockIndex, GLuint uniformBlockBinding) {
		CMD_PUSH
			::glUniformBlockBinding(program->BufferId, uniformBlockIndex, uniformBlockBinding);
		CMD_END(glUniformBlockBinding);
	}
	void DrawArraysInstanced(GLenum mode, GLint first, GLsizei count, GLsizei instanceCount);
	void DrawElementsInstanced(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices, GLsizei instanceCount);
	inline void FenceSync(GLsync* sync, GLenum condition, GLbitfield flags) {
		ASSERT(mImmMode);
		CMD_PUSH
			*sync = ::glFenceSync(condition, flags);
		CMD_END(glFenceSync);
	}
	inline void IsSync(GLboolean* ret, GLsync sync) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsSync(sync);
		CMD_END(glIsSync);
	}
	inline void DeleteSync(GLsync sync) {
		CMD_PUSH
			::glDeleteSync(sync);
		CMD_END(glDeleteSync);
	}
	inline void ClientWaitSync(GLenum* ret, GLsync sync, GLbitfield flags, GLuint64 timeout) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glClientWaitSync(sync, flags, timeout);
		CMD_END(glClientWaitSync);
	}
	inline void WaitSync(GLsync sync, GLbitfield flags, GLuint64 timeout) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glWaitSync(sync, flags, timeout);
		CMD_END(glWaitSync);
	}
	inline void GetInteger64v(GLenum pname, GLint64* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetInteger64v(pname, params);
		CMD_END(glGetInteger64v);
	}
	inline void GetSynciv(GLsync sync, GLenum pname, GLsizei bufSize, GLsizei* length, GLint* values) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetSynciv(sync, pname, bufSize, length, values);
		CMD_END(glGetSynciv);
	}
	inline void GetInteger64i_v(GLenum target, GLuint index, GLint64* data) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetInteger64i_v(target, index, data);
		CMD_END(glGetInteger64i_v);
	}
	inline void GetBufferParameteri64v(GLenum target, GLenum pname, GLint64* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetBufferParameteri64v(target, pname, params);
		CMD_END(glGetBufferParameteri64v);
	}
	std::shared_ptr<GLSdk::GLBufferId> GenSamplers();
	/*inline void GenSamplers(GLsizei count, GLuint* samplers) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenSamplers(count, samplers);
		CMD_END(glGenSamplers);
	}*/
	inline void glIsSampler(GLboolean* ret, GLuint sampler) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsSampler(sampler);
		CMD_END(glIsSampler);
	}
	inline void BindSampler(GLuint unit, const std::shared_ptr<GLSdk::GLBufferId>& sampler) {
		CMD_PUSH
		{
			if(sampler!=nullptr)
				::glBindSampler(unit, sampler->BufferId);
			else
				::glBindSampler(unit, 0);
		}
		CMD_END(glBindSampler);
	}
	inline void SamplerParameteri(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, GLint param) {
		CMD_PUSH
			::glSamplerParameteri(sampler->BufferId, pname, param);
		CMD_END(glSamplerParameteri);
	}
	inline void SamplerParameteriv(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, const GLint* param) {
		GLint saved = *param;
		CMD_PUSH
			::glSamplerParameteriv(sampler->BufferId, pname, &saved);
		CMD_END(glSamplerParameteriv);
	}
	inline void SamplerParameterf(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, GLfloat param) {
		CMD_PUSH
			::glSamplerParameterf(sampler->BufferId, pname, param);
		CMD_END(glSamplerParameterf);
	}
	inline void SamplerParameterfv(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, const GLfloat* param) {
		GLfloat saved[1];
		saved[0] = param[0];
		CMD_PUSH
			::glSamplerParameterfv(sampler->BufferId, pname, saved);
		CMD_END(glSamplerParameterfv);
	}
	inline void GetSamplerParameteriv(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetSamplerParameteriv(sampler->BufferId, pname, params);
		CMD_END(glGetSamplerParameteriv);
	}
	inline void GetSamplerParameterfv(const std::shared_ptr<GLSdk::GLBufferId>& sampler, GLenum pname, GLfloat* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetSamplerParameterfv(sampler->BufferId, pname, params);
		CMD_END(glGetSamplerParameterfv);
	}
	inline void VertexAttribDivisor(GLuint index, GLuint divisor) {
		CMD_PUSH
			::glVertexAttribDivisor(index, divisor);
		CMD_END(glVertexAttribDivisor);
	}
	inline void BindTransformFeedback(GLenum target, GLuint id) {
		CMD_PUSH
			::glBindTransformFeedback(target, id);
		CMD_END(glBindTransformFeedback);
	}
	inline void DeleteTransformFeedbacks(GLsizei n, const GLuint* ids) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glDeleteTransformFeedbacks(n, ids);
		CMD_END(glDeleteTransformFeedbacks);
	}
	inline void GenTransformFeedbacks(GLsizei n, GLuint* ids) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGenTransformFeedbacks(n, ids);
		CMD_END(glGenTransformFeedbacks);
	}
	inline void IsTransformFeedback(GLboolean* ret, GLuint id) {
		ASSERT(mImmMode);
		CMD_PUSH
			*ret = ::glIsTransformFeedback(id);
		CMD_END(glIsTransformFeedback);
	}
	inline void PauseTransformFeedback(void) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glPauseTransformFeedback();
		CMD_END(glPauseTransformFeedback);
	}
	inline void ResumeTransformFeedback(void) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glResumeTransformFeedback();
		CMD_END(glResumeTransformFeedback);
	}
	inline void GetProgramBinary(GLuint program, GLsizei bufSize, GLsizei* length, GLenum* binaryFormat, GLvoid* binary) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetProgramBinary(program, bufSize, length, binaryFormat, binary);
		CMD_END(glGetProgramBinary);
	}
	inline void ProgramBinary(GLuint program, GLenum binaryFormat, const GLvoid* binary, GLsizei length) {
		std::vector<BYTE> saved;
		saved.resize(length);
		memcpy(&saved[0], binary, length);
		CMD_PUSH
			::glProgramBinary(program, binaryFormat, &saved[0], length);
		CMD_END(glProgramBinary);
	}
	inline void ProgramParameteri(GLuint program, GLenum pname, GLint value) {
		CMD_PUSH
			::glProgramParameteri(program, pname, value);
		CMD_END(glProgramParameteri);
	}
	inline void InvalidateFramebuffer(GLenum target, GLsizei numAttachments, const GLenum* attachments) {
		std::vector<GLenum> saved;
		saved.resize(numAttachments);
		memcpy(&saved[0], attachments, numAttachments);
		CMD_PUSH
			::glInvalidateFramebuffer(target, numAttachments, &saved[0]);
		CMD_END(glInvalidateFramebuffer);
	}
	inline void InvalidateSubFramebuffer(GLenum target, GLsizei numAttachments, const GLenum* attachments, GLint x, GLint y, GLsizei width, GLsizei height) {
		std::vector<GLenum> saved;
		saved.resize(numAttachments);
		memcpy(&saved[0], attachments, numAttachments);
		CMD_PUSH
			::glInvalidateSubFramebuffer(target, numAttachments, &saved[0], x, y, width, height);
		CMD_END(glInvalidateSubFramebuffer);
	}
	inline void TexStorage2D(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height) {
		CMD_PUSH
			::glTexStorage2D(target, levels, internalformat, width, height);
		CMD_END(glTexStorage2D);
	}
	inline void TexStorage3D(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth) {
		CMD_PUSH
			::glTexStorage3D(target, levels, internalformat, width, height, depth);
		CMD_END(glTexStorage3D);
	}
	inline void GetInternalformativ(GLenum target, GLenum internalformat, GLenum pname, GLsizei bufSize, GLint* params) {
		ASSERT(mImmMode);
		CMD_PUSH
			::glGetInternalformativ(target, internalformat, pname, bufSize, params);
		CMD_END(glGetInternalformativ);
	}
#pragma endregion GLES3

#pragma region GLES31
	inline void ES31_DispatchCompute(GLuint num_groups_x, GLuint num_groups_y, GLuint num_groups_z)
	{
		CMD_PUSH
		{
			::glDispatchCompute(num_groups_x, num_groups_y, num_groups_z);
		}
		CMD_END(glDispatchCompute);
	}
	inline void ES31_DispatchComputeIndirect(GLintptr indirect)
	{
		CMD_PUSH
		{
			::glDispatchComputeIndirect(indirect);
		}
		CMD_END(glDispatchComputeIndirect);
	}
	inline void ES31_DrawArraysIndirect(GLenum mode, const void *indirect)
	{
		CMD_PUSH
		{
			::glDrawArraysIndirect(mode, indirect);
		}
		CMD_END(glDrawArraysIndirect);
	}
	inline void ES31_DrawElementsIndirect(GLenum mode, GLenum type, const void *indirect)
	{
		CMD_PUSH
		{
			::glDrawElementsIndirect(mode, type, indirect);
		}
		CMD_END(glDrawElementsIndirect);
	}
	inline void ES31_FramebufferParameteri(GLenum target, GLenum pname, GLint param)
	{
		CMD_PUSH
		{
			::glFramebufferParameteri(target, pname, param);
		}
		CMD_END(glFramebufferParameteri);
	}
	inline void ES31_GetFramebufferParameteriv(GLenum target, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetFramebufferParameteriv(target, pname, params);
		}
		CMD_END(glGetFramebufferParameteriv);
	}
	inline void ES31_GetProgramInterfaceiv(const std::shared_ptr<GLSdk::GLBufferId>& program, GLenum programInterface, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetProgramInterfaceiv(program->BufferId, programInterface, pname, params);
		}
		CMD_END(glGetProgramInterfaceiv);
	}
	inline void ES31_GetProgramResourceIndex(GLuint* result, GLuint program, GLenum programInterface, const GLchar *name)
	{
		CMD_PUSH
		{
			*result = ::glGetProgramResourceIndex(program, programInterface, name);
		}
		CMD_END(glGetProgramResourceIndex);
	}
	inline void ES31_GetProgramResourceName(const std::shared_ptr<GLSdk::GLBufferId>& program, GLenum programInterface, GLuint index, GLsizei bufSize, GLsizei *length, GLchar *name)
	{
		CMD_PUSH
		{
			::glGetProgramResourceName(program->BufferId, programInterface, index, bufSize, length, name);
		}
		CMD_END(glGetProgramResourceIndex);
	}
	inline void ES31_GetProgramResourceiv(const std::shared_ptr<GLSdk::GLBufferId>& program, GLenum programInterface, GLuint index, GLsizei propCount, const GLenum *props, GLsizei bufSize, GLsizei *length, GLint *params)
	{
		CMD_PUSH
		{
			::glGetProgramResourceiv(program->BufferId, programInterface, index, propCount, props, bufSize, length, params);
		}
		CMD_END(glGetProgramResourceiv);
	}
	inline void ES31_GetProgramResourceLocation(GLint* result, GLuint program, GLenum programInterface, const GLchar *name)
	{
		CMD_PUSH
		{
			*result = ::glGetProgramResourceLocation(program, programInterface, name);
		}
		CMD_END(glGetProgramResourceLocation);
	}
	inline void ES31_UseProgramStages(GLuint pipeline, GLbitfield stages, GLuint program)
	{
		CMD_PUSH
		{
			::glUseProgramStages(pipeline, stages, program);
		}
		CMD_END(glUseProgramStages);
	}
	inline void ES31_ActiveShaderProgram(GLuint pipeline, GLuint program)
	{
		CMD_PUSH
		{
			::glActiveShaderProgram(pipeline, program);
		}
		CMD_END(glActiveShaderProgram);
	}
	inline void ES31_CreateShaderProgramv(GLuint* result, GLenum type, GLsizei count, const GLchar *const*strings)
	{
		CMD_PUSH
		{
			*result = ::glCreateShaderProgramv(type, count, strings);
		}
		CMD_END(glCreateShaderProgramv);
	}
	inline void ES31_BindProgramPipeline(GLuint pipeline)
	{
		CMD_PUSH
		{
			::glBindProgramPipeline(pipeline);
		}
		CMD_END(glBindProgramPipeline);
	}
	inline void ES31_DeleteProgramPipelines(GLsizei n, const GLuint *pipelines)
	{
		CMD_PUSH
		{
			::glDeleteProgramPipelines(n, pipelines);
		}
		CMD_END(glDeleteProgramPipelines);
	}
	inline void ES31_GenProgramPipelines(GLsizei n, GLuint *pipelines)
	{
		CMD_PUSH
		{
			::glGenProgramPipelines(n, pipelines);
		}
		CMD_END(glGenProgramPipelines);
	}
	inline void ES31_IsProgramPipeline(GLboolean* result, GLuint pipeline)
	{
		CMD_PUSH
		{
			*result = ::glIsProgramPipeline(pipeline);
		}
		CMD_END(glIsProgramPipeline);
	}
	inline void ES31_GetProgramPipelineiv(GLuint pipeline, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetProgramPipelineiv(pipeline, pname, params);
		}
		CMD_END(glGetProgramPipelineiv);
	}
	inline void ES31_ValidateProgramPipeline(GLuint pipeline)
	{
		CMD_PUSH
		{
			::glValidateProgramPipeline(pipeline);
		}
		CMD_END(glValidateProgramPipeline);
	}
	inline void ES31_GetProgramPipelineInfoLog(GLuint pipeline, GLsizei bufSize, GLsizei *length, GLchar *infoLog)
	{
		CMD_PUSH
		{
			::glGetProgramPipelineInfoLog(pipeline, bufSize, length, infoLog);
		}
		CMD_END(glGetProgramPipelineInfoLog);
	}
	inline void ES31_BindImageTexture(GLuint unit, const std::shared_ptr<GLSdk::GLBufferId>& texture, GLint level, GLboolean layered, GLint layer, GLenum access, GLenum format)
	{
		CMD_PUSH
		{
			::glBindImageTexture(unit, texture->BufferId, level, layered, layer, access, format);
		}
		CMD_END(glBindImageTexture);
	}
	inline void ES31_GetBooleani_v(GLenum target, GLuint index, GLboolean *data)
	{
		CMD_PUSH
		{
			::glGetBooleani_v(target, index, data);
		}
		CMD_END(glGetBooleani_v);
	}
	inline void ES31_MemoryBarrier(GLbitfield barriers)
	{
		CMD_PUSH
		{
			::glMemoryBarrier(barriers);
		}
		CMD_END(glMemoryBarrier);
	}
	inline void ES31_MemoryBarrierByRegion(GLbitfield barriers)
	{
		CMD_PUSH
		{
			::glMemoryBarrierByRegion(barriers);
		}
		CMD_END(glMemoryBarrierByRegion);
	}
	inline void ES31_TexStorage2DMultisample(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height, GLboolean fixedsamplelocations)
	{
		CMD_PUSH
		{
			::glTexStorage2DMultisample(target,samples, internalformat, width, height, fixedsamplelocations);
		}
		CMD_END(glTexStorage2DMultisample);
	}
	inline void ES31_GetMultisamplefv(GLenum pname, GLuint index, GLfloat *val)
	{
		CMD_PUSH
		{
			::glGetMultisamplefv(pname, index, val);
		}
		CMD_END(glGetMultisamplefv);
	}
	inline void ES31_SampleMaski(GLuint maskNumber, GLbitfield mask)
	{
		CMD_PUSH
		{
			::glSampleMaski(maskNumber, mask);
		}
		CMD_END(glSampleMaski);
	}
	inline void ES31_GetTexLevelParameteriv(GLenum target, GLint level, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetTexLevelParameteriv(target, level, pname, params);
		}
		CMD_END(glGetTexLevelParameteriv);
	}
	inline void ES31_GetTexLevelParameterfv(GLenum target, GLint level, GLenum pname, GLfloat *params)
	{
		CMD_PUSH
		{
			::glGetTexLevelParameterfv(target, level, pname, params);
		}
		CMD_END(glGetTexLevelParameterfv);
	}
	inline void ES31_BindVertexBuffer(GLuint bindingindex, const std::shared_ptr<GLBufferId>& buffer, GLintptr offset, GLsizei stride)
	{
		CMD_PUSH
		{
			::glBindVertexBuffer(bindingindex, buffer->BufferId, offset, stride);
		}
		CMD_END(glBindVertexBuffer);
	}
	inline void ES31_VertexAttribFormat(GLuint attribindex, GLint size, GLenum type, GLboolean normalized, GLuint relativeoffset)
	{
		CMD_PUSH
		{
			::glVertexAttribFormat(attribindex, size, type, normalized, relativeoffset);
		}
		CMD_END(glVertexAttribFormat);
	}
	inline void ES31_VertexAttribIFormat(GLuint attribindex, GLint size, GLenum type, GLuint relativeoffset)
	{
		CMD_PUSH
		{
			::glVertexAttribIFormat(attribindex, size, type, relativeoffset);
		}
		CMD_END(glVertexAttribIFormat);
	}
	inline void ES31_VertexAttribBinding(GLuint attribindex, GLuint bindingindex)
	{
		CMD_PUSH
		{
			::glVertexAttribBinding(attribindex, bindingindex);
		}
		CMD_END(glVertexAttribBinding);
	}
	inline void ES31_VertexBindingDivisor(GLuint bindingindex, GLuint divisor)
	{
		CMD_PUSH
		{
			::glVertexBindingDivisor(bindingindex, divisor);
		}
		CMD_END(glVertexBindingDivisor);
	}

	inline void ES31_ProgramUniform1i(GLuint program, GLint location, GLint v0)
	{
		CMD_PUSH
		{
			::glProgramUniform1i(program, location, v0);
		}
		CMD_END(glProgramUniform1i);
	}
	inline void ES31_ProgramUniform2i(GLuint program, GLint location, GLint v0, GLint v1)
	{
		CMD_PUSH
		{
			::glProgramUniform2i(program, location, v0, v1);
		}
		CMD_END(glProgramUniform2i);
	}
	inline void ES31_ProgramUniform3i(GLuint program, GLint location, GLint v0, GLint v1, GLint v2)
	{
		CMD_PUSH
		{
			::glProgramUniform3i(program, location, v0, v1, v2);
		}
		CMD_END(glProgramUniform3i);
	}
	inline void ES31_ProgramUniform4i(GLuint program, GLint location, GLint v0, GLint v1, GLint v2, GLint v3)
	{
		CMD_PUSH
		{
			::glProgramUniform4i(program, location, v0, v1, v2, v3);
		}
		CMD_END(glProgramUniform2i);
	}
	inline void ES31_ProgramUniform1ui(GLuint program, GLint location, GLuint v0)
	{
		CMD_PUSH
		{
			::glProgramUniform1ui(program, location, v0);
		}
		CMD_END(glProgramUniform1ui);
	}
	inline void ES31_ProgramUniform2ui(GLuint program, GLint location, GLuint v0, GLuint v1)
	{
		CMD_PUSH
		{
			::glProgramUniform2ui(program, location, v0, v1);
		}
		CMD_END(glProgramUniform2ui);
	}
	inline void ES31_ProgramUniform3ui(GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2)
	{
		CMD_PUSH
		{
			::glProgramUniform3ui(program, location, v0, v1, v2);
		}
		CMD_END(glProgramUniform3ui);
	}
	inline void ES31_ProgramUniform4ui(GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3)
	{
		CMD_PUSH
		{
			::glProgramUniform4ui(program, location, v0, v1, v2, v3);
		}
		CMD_END(glProgramUniform4ui);
	}
	inline void ES31_ProgramUniform1f(GLuint program, GLint location, GLfloat v0)
	{
		CMD_PUSH
		{
			::glProgramUniform1f(program, location, v0);
		}
		CMD_END(glProgramUniform1f);
	}
	inline void ES31_ProgramUniform2f(GLuint program, GLint location, GLfloat v0, GLfloat v1)
	{
		CMD_PUSH
		{
			::glProgramUniform2f(program, location, v0, v1);
		}
		CMD_END(glProgramUniform2f);
	}
	inline void ES31_ProgramUniform3f(GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2)
	{
		CMD_PUSH
		{
			::glProgramUniform3f(program, location, v0, v1, v2);
		}
		CMD_END(glProgramUniform3f);
	}
	inline void ES31_ProgramUniform4f(GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3)
	{
		CMD_PUSH
		{
			::glProgramUniform4f(program, location, v0, v1, v2, v3);
		}
		CMD_END(glProgramUniform4f);
	}
	inline void ES31_ProgramUniform1iv(GLuint program, GLint location, GLsizei count, const GLint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform1iv(program, location, count, value);
		}
		CMD_END(glProgramUniform1iv);
	}
	inline void ES31_ProgramUniform2iv(GLuint program, GLint location, GLsizei count, const GLint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform2iv(program, location, count, value);
		}
		CMD_END(glProgramUniform2iv);
	}
	inline void ES31_ProgramUniform3iv(GLuint program, GLint location, GLsizei count, const GLint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform3iv(program, location, count, value);
		}
		CMD_END(glProgramUniform3iv);
	}
	inline void ES31_ProgramUniform4iv(GLuint program, GLint location, GLsizei count, const GLint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform4iv(program, location, count, value);
		}
		CMD_END(glProgramUniform4iv);
	}
	inline void ES31_ProgramUniform1uiv(GLuint program, GLint location, GLsizei count, const GLuint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform1uiv(program, location, count, value);
		}
		CMD_END(glProgramUniform1uiv);
	}
	inline void ES31_ProgramUniform2uiv(GLuint program, GLint location, GLsizei count, const GLuint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform2uiv(program, location, count, value);
		}
		CMD_END(glProgramUniform2uiv);
	}
	inline void ES31_ProgramUniform3uiv(GLuint program, GLint location, GLsizei count, const GLuint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform3uiv(program, location, count, value);
		}
		CMD_END(glProgramUniform3uiv);
	}
	inline void ES31_ProgramUniform4uiv(GLuint program, GLint location, GLsizei count, const GLuint *value)
	{
		CMD_PUSH
		{
			::glProgramUniform4uiv(program, location, count, value);
		}
		CMD_END(glProgramUniform4uiv);
	}
	inline void ES31_ProgramUniform1fv(GLuint program, GLint location, GLsizei count, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniform1fv(program, location, count, value);
		}
		CMD_END(glProgramUniform1fv);
	}
	inline void ES31_ProgramUniform2fv(GLuint program, GLint location, GLsizei count, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniform2fv(program, location, count, value);
		}
		CMD_END(glProgramUniform2fv);
	}
	inline void ES31_ProgramUniform3fv(GLuint program, GLint location, GLsizei count, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniform3fv(program, location, count, value);
		}
		CMD_END(glProgramUniform3fv);
	}
	inline void ES31_rogramUniform4fv(GLuint program, GLint location, GLsizei count, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniform4fv(program, location, count, value);
		}
		CMD_END(glProgramUniform4fv);
	}
	inline void ES31_ProgramUniformMatrix2fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix2fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix2fv);
	}
	inline void ES31_ProgramUniformMatrix3fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix3fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix3fv);
	}
	inline void ES31_ProgramUniformMatrix4fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix4fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix4fv);
	}
	inline void ES31_ProgramUniformMatrix2x3fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix2x3fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix2x3fv);
	}
	inline void ES31_ProgramUniformMatrix3x2fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix3x2fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix3x2fv);
	}
	inline void ES31_ProgramUniformMatrix2x4fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix2x4fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix2x4fv);
	}
	inline void ES31_ProgramUniformMatrix4x2fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix4x2fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix4x2fv);
	}
	inline void ES31_ProgramUniformMatrix3x4fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix3x4fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix3x4fv);
	}
	inline void ES31_ProgramUniformMatrix4x3fv(GLuint program, GLint location, GLsizei count, GLboolean transpose, const GLfloat *value)
	{
		CMD_PUSH
		{
			::glProgramUniformMatrix4x3fv(program, location, count, transpose, value);
		}
		CMD_END(glProgramUniformMatrix4x3fv);
	}
#pragma endregion


#pragma region GLES32
	inline void ES32_BlendBarrier(void)
	{
		CMD_PUSH
		{
			::glBlendBarrierKHR();//android: glBlendBarrier
		}
		CMD_END(glBlendBarrier);
	}
	inline void ES32_CopyImageSubData(GLuint srcName, GLenum srcTarget, GLint srcLevel, GLint srcX, GLint srcY, GLint srcZ, GLuint dstName, GLenum dstTarget, GLint dstLevel, GLint dstX, GLint dstY, GLint dstZ, GLsizei srcWidth, GLsizei srcHeight, GLsizei srcDepth)
	{
		CMD_PUSH
		{
			::glCopyImageSubData(srcName, srcTarget, srcLevel, srcX, srcY, srcZ, dstName, dstTarget, dstLevel, dstX, dstY, dstZ, srcWidth, srcHeight, srcDepth);
		}
		CMD_END(glCopyImageSubData);
	}
	inline void ES32_DebugMessageControl(GLenum source, GLenum type, GLenum severity, GLsizei count, const GLuint *ids, GLboolean enabled)
	{
		CMD_PUSH
		{
			::glDebugMessageControl(source, type, severity, count, ids, enabled);
		}
		CMD_END(glDebugMessageControl);
	}
	inline void ES32_DebugMessageInsert(GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, const GLchar *buf)
	{
		CMD_PUSH
		{
			::glDebugMessageInsert(source, type, id, severity, length, buf);
		}
		CMD_END(glDebugMessageInsert);
	}
	inline void ES32_DebugMessageCallback(GLDEBUGPROC callback, const void *userParam)
	{
		CMD_PUSH
		{
			::glDebugMessageCallback(callback, userParam);
		}
		CMD_END(glDebugMessageCallback);
	}
	inline void ES32_GetDebugMessageLog(GLuint* result, GLuint count, GLsizei bufSize, GLenum *sources, GLenum *types, GLuint *ids, GLenum *severities, GLsizei *lengths, GLchar *messageLog)
	{
		CMD_PUSH
		{
			*result = ::glGetDebugMessageLog(count, bufSize, sources, types, ids, severities, lengths, messageLog);
		}
		CMD_END(glGetDebugMessageLog);
	}
	inline void ES32_PushDebugGroup(GLenum source, GLuint id, GLsizei length, const GLchar *message)
	{
		CMD_PUSH
		{
			::glPushDebugGroup(source, id, length, message);
		}
		CMD_END(glPushDebugGroup);
	}
	inline void ES32_PopDebugGroup(void)
	{
		CMD_PUSH
		{
			::glPopDebugGroup();
		}
		CMD_END(glPopDebugGroup);
	}
	inline void ES32_ObjectLabel(GLenum identifier, GLuint name, GLsizei length, const GLchar *label)
	{
		CMD_PUSH
		{
			::glObjectLabel(identifier, name, length, label);
		}
		CMD_END(glObjectLabel);
	}
	inline void ES32_GetObjectLabel(GLenum identifier, GLuint name, GLsizei bufSize, GLsizei *length, GLchar *label)
	{
		CMD_PUSH
		{
			::glGetObjectLabel(identifier, name, bufSize, length, label);
		}
		CMD_END(glGetObjectLabel);
	}
	inline void ES32_ObjectPtrLabel(const void *ptr, GLsizei length, const GLchar *label)
	{
		CMD_PUSH
		{
			::glObjectPtrLabel(ptr, length, label);
		}
		CMD_END(glObjectPtrLabel);
	}
	inline void ES32_GetObjectPtrLabel(const void *ptr, GLsizei bufSize, GLsizei *length, GLchar *label)
	{
		CMD_PUSH
		{
			::glGetObjectPtrLabel(ptr, bufSize, length, label);
		}
		CMD_END(glGetObjectPtrLabel);
	}
	inline void ES32_GetPointerv(GLenum pname, void **params)
	{
		CMD_PUSH
		{
			::glGetPointerv(pname, params);
		}
		CMD_END(glGetPointerv);
	}
	inline void ES32_Enablei(GLenum target, GLuint index)
	{
		CMD_PUSH
		{
			::glEnablei(target, index);
		}
		CMD_END(glEnablei);
	}
	inline void ES32_Disablei(GLenum target, GLuint index)
	{
		CMD_PUSH
		{
			::glDisablei(target, index);
		}
		CMD_END(glDisablei);
	}
	inline void ES32_BlendEquationi(GLuint buf, GLenum mode)
	{
		CMD_PUSH
		{
			::glBlendEquationi(buf, mode);
		}
		CMD_END(glBlendEquationi);
	}
	inline void ES32_BlendEquationSeparatei(GLuint buf, GLenum modeRGB, GLenum modeAlpha)
	{
		CMD_PUSH
		{
			::glBlendEquationSeparatei(buf, modeRGB, modeAlpha);
		}
		CMD_END(glBlendEquationSeparatei);
	}
	inline void ES32_BlendFunci(GLuint buf, GLenum src, GLenum dst)
	{
		CMD_PUSH
		{
			::glBlendFunci(buf, src, dst);
		}
		CMD_END(glBlendFunci);
	}
	inline void ES32_BlendFuncSeparatei(GLuint buf, GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha)
	{
		CMD_PUSH
		{
			::glBlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);
		}
		CMD_END(glBlendFuncSeparatei);
	}
	inline void ES32_ColorMaski(GLuint index, GLboolean r, GLboolean g, GLboolean b, GLboolean a)
	{
		CMD_PUSH
		{
			::glColorMaski(index, r, g, b, a);
		}
		CMD_END(glColorMaski);
	}
	inline void ES32_IsEnabledi(GLboolean* result, GLenum target, GLuint index)
	{
		CMD_PUSH
		{
			*result = ::glIsEnabledi(target, index);
		}
		CMD_END(glIsEnabledi);
	}
	inline void ES32_DrawElementsBaseVertex(GLenum mode, GLsizei count, GLenum type, const void *indices, GLint basevertex)
	{
		CMD_PUSH
		{
			::glDrawElementsBaseVertex(mode, count, type, indices, basevertex);
		}
		CMD_END(glDrawElementsBaseVertex);
	}
	inline void ES32_DrawRangeElementsBaseVertex(GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, const void *indices, GLint basevertex)
	{
		CMD_PUSH
		{
			::glDrawRangeElementsBaseVertex(mode, start, end, count, type, indices, basevertex);
		}
		CMD_END(glDrawRangeElementsBaseVertex);
	}
	inline void ES32_DrawElementsInstancedBaseVertex(GLenum mode, GLsizei count, GLenum type, const void *indices, GLsizei instancecount, GLint basevertex)
	{
		CMD_PUSH
		{
			::glDrawElementsInstancedBaseVertex(mode, count, type, indices, instancecount, basevertex);
		}
		CMD_END(glDrawElementsInstancedBaseVertex);
	}
	inline void ES32_FramebufferTexture(GLenum target, GLenum attachment, GLuint texture, GLint level)
	{
		CMD_PUSH
		{
			::glFramebufferTexture(target, attachment, texture, level);
		}
		CMD_END(glFramebufferTexture);
	}
	inline void ES32_PrimitiveBoundingBox(GLfloat minX, GLfloat minY, GLfloat minZ, GLfloat minW, GLfloat maxX, GLfloat maxY, GLfloat maxZ, GLfloat maxW)
	{
		CMD_PUSH
		{
#if PLATFORM_DROID
			::glPrimitiveBoundingBoxARB(minX, minY, minZ, minW, maxX, maxY, maxZ, maxW);//android:glPrimitiveBoundingBox
#else
			::glPrimitiveBoundingBoxARB(minX, minY, minZ, minW, maxX, maxY, maxZ, maxW);//android:glPrimitiveBoundingBox
#endif
		}
		CMD_END(glPrimitiveBoundingBox);
	}
	inline void ES32_GetGraphicsResetStatus(GLenum* result)
	{
		CMD_PUSH
		{
			*result = ::glGetGraphicsResetStatus();
		}
		CMD_END(glGetGraphicsResetStatus);
	}
	inline void ES32_ReadnPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLsizei bufSize, void *data)
	{
		CMD_PUSH
		{
			::glReadnPixels(x, y, width, height, format, type, bufSize, data);
		}
		CMD_END(glReadnPixels);
	}
	inline void ES32_GetnUniformfv(GLuint program, GLint location, GLsizei bufSize, GLfloat *params)
	{
		CMD_PUSH
		{
			::glGetnUniformfv(program, location, bufSize, params);
		}
		CMD_END(glGetnUniformfv);
	}
	inline void ES32_GetnUniformiv(GLuint program, GLint location, GLsizei bufSize, GLint *params)
	{
		CMD_PUSH
		{
			::glGetnUniformiv(program, location, bufSize, params);
		}
		CMD_END(glGetnUniformiv);
	}
	inline void ES32_GetnUniformuiv(GLuint program, GLint location, GLsizei bufSize, GLuint *params)
	{
		CMD_PUSH
		{
			::glGetnUniformuiv(program, location, bufSize, params);
		}
		CMD_END(glGetnUniformuiv);
	}
	inline void ES32_MinSampleShading(GLfloat value)
	{
		CMD_PUSH
		{
			::glMinSampleShading(value);
		}
		CMD_END(glMinSampleShading);
	}
	inline void ES32_PatchParameteri(GLenum pname, GLint value)
	{
		CMD_PUSH
		{
			::glPatchParameteri(pname, value);
		}
		CMD_END(glPatchParameteri);
	}
	inline void ES32_TexParameterIiv(GLenum target, GLenum pname, const GLint *params)
	{
		CMD_PUSH
		{
			::glTexParameterIiv(target, pname, params);
		}
		CMD_END(glTexParameterIiv);
	}
	inline void ES32_TexParameterIuiv(GLenum target, GLenum pname, const GLuint *params)
	{
		CMD_PUSH
		{
			::glTexParameterIuiv(target, pname, params);
		}
		CMD_END(glTexParameterIuiv);
	}
	inline void ES32_GetTexParameterIiv(GLenum target, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetTexParameterIiv(target, pname, params);
		}
		CMD_END(glGetTexParameterIiv);
	}
	inline void ES32_GetTexParameterIuiv(GLenum target, GLenum pname, GLuint *params)
	{
		CMD_PUSH
		{
			::glGetTexParameterIuiv(target, pname, params);
		}
		CMD_END(glGetTexParameterIuiv);
	}
	inline void ES32_SamplerParameterIiv(GLuint sampler, GLenum pname, const GLint *param)
	{
		CMD_PUSH
		{
			::glSamplerParameterIiv(sampler, pname, param);
		}
		CMD_END(glSamplerParameterIiv);
	}
	inline void ES32_SamplerParameterIuiv(GLuint sampler, GLenum pname, const GLuint *param)
	{
		CMD_PUSH
		{
			::glSamplerParameterIuiv(sampler, pname, param);
		}
		CMD_END(glSamplerParameterIuiv);
	}
	inline void ES32_GetSamplerParameterIiv(GLuint sampler, GLenum pname, GLint *params)
	{
		CMD_PUSH
		{
			::glGetSamplerParameterIiv(sampler, pname, params);
		}
		CMD_END(glGetSamplerParameterIiv);
	}
	inline void ES32_GetSamplerParameterIuiv(GLuint sampler, GLenum pname, GLuint *params)
	{
		CMD_PUSH
		{
			::glGetSamplerParameterIuiv(sampler, pname, params);
		}
		CMD_END(glGetSamplerParameterIuiv);
	}
	inline void ES32_TexBuffer(GLenum target, GLenum internalformat, GLuint buffer)
	{
		CMD_PUSH
		{
			::glTexBuffer(target, internalformat, buffer);
		}
		CMD_END(glTexBuffer);
	}
	inline void ES32_TexBufferRange(GLenum target, GLenum internalformat, GLuint buffer, GLintptr offset, GLsizeiptr size)
	{
		CMD_PUSH
		{
			::glTexBufferRange(target, internalformat, buffer, offset, size);
		}
		CMD_END(glTexBufferRange);
	}
	inline void ES32_TexStorage3DMultisample(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLboolean fixedsamplelocations)
	{
		CMD_PUSH
		{
			::glTexStorage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations);
		}
		CMD_END(glTexStorage3DMultisample);
	}
#pragma endregion


#pragma region Debugger
/*
	DEF_Debugger(ActiveTexture)(GLenum texture) {}
	DEF_Debugger(AttachShader)(GLuint program, GLuint shader) {}
	DEF_Debugger(BindAttribLocation)(GLuint program, GLuint index, const GLchar* name) {}
	DEF_Debugger(BindBuffer)(GLenum target, GLuint buffer) {}
	DEF_Debugger(BindFramebuffer)(GLenum target, GLuint framebuffer) {}
	DEF_Debugger(BindRenderbuffer)(GLenum target, GLuint renderbuffer) {}
	DEF_Debugger(BindTexture)(GLenum target, GLuint texture) {}
	DEF_Debugger(BlendColor)(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) {}
	DEF_Debugger(BlendEquation)(GLenum mode) {}
	DEF_Debugger(BlendEquationSeparate)(GLenum modeRGB, GLenum modeAlpha) {}
	DEF_Debugger(BlendFunc)(GLenum sfactor, GLenum dfactor) {}
	DEF_Debugger(BlendFuncSeparate)(GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha) {}
	DEF_Debugger(BufferData)(GLenum target, GLsizeiptr size, const GLvoid* data, GLenum usage) {}
	DEF_Debugger(BufferSubData)(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid* data) {}
	DEF_Debugger(CheckFramebufferStatus)(GLenum target) {}
	DEF_Debugger(Clear)(GLbitfield mask) {}
	DEF_Debugger(ClearColor)(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha) {}
	DEF_Debugger(ClearDepthf)(GLfloat depth) {}
	DEF_Debugger(ClearStencil)(GLint s) {}
	DEF_Debugger(ColorMask)(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha) {}
	DEF_Debugger(CompileShader)(GLuint shader) {}
	DEF_Debugger(CompressedTexImage2D)(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid* data) {}
	DEF_Debugger(CompressedTexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid* data) {}
	DEF_Debugger(CopyTexImage2D)(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border) {}
	DEF_Debugger(CopyTexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height) {}
	DEF_Debugger(CreateProgram)(void) {}
	DEF_Debugger(CreateShader)(GLenum type) {}
	DEF_Debugger(CullFace)(GLenum mode) {}
	DEF_Debugger(DeleteBuffers)(GLsizei n, const GLuint* buffers) {}
	DEF_Debugger(DeleteFramebuffers)(GLsizei n, const GLuint* framebuffers) {}
	DEF_Debugger(DeleteProgram)(GLuint program) {}
	DEF_Debugger(DeleteRenderbuffers)(GLsizei n, const GLuint* renderbuffers) {}
	DEF_Debugger(DeleteShader)(GLuint shader) {}
	DEF_Debugger(DeleteTextures)(GLsizei n, const GLuint* textures) {}
	DEF_Debugger(DepthFunc)(GLenum func) {}
	DEF_Debugger(DepthMask)(GLboolean flag) {}
	DEF_Debugger(DepthRangef)(GLfloat n, GLfloat f) {}
	DEF_Debugger(DetachShader)(GLuint program, GLuint shader) {}
	DEF_Debugger(Disable)(GLenum cap) {}
	DEF_Debugger(DisableVertexAttribArray)(GLuint index) {}
	DEF_Debugger(DrawArrays)(GLenum mode, GLint first, GLsizei count) {}
	DEF_Debugger(DrawElements)(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices) {}
	DEF_Debugger(Enable)(GLenum cap) {}
	DEF_Debugger(EnableVertexAttribArray)(GLuint index) {}
	DEF_Debugger(Finish)(void) {}
	DEF_Debugger(Flush)(void) {}
	DEF_Debugger(FramebufferRenderbuffer)(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer) {}
	DEF_Debugger(FramebufferTexture2D)(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level) {}
	DEF_Debugger(FrontFace)(GLenum mode) {}
	DEF_Debugger(GenBuffers)(GLsizei n, GLuint* buffers) {}
	DEF_Debugger(GenerateMipmap)(GLenum target) {}
	DEF_Debugger(GenFramebuffers)(GLsizei n, GLuint* framebuffers) {}
	DEF_Debugger(GenRenderbuffers)(GLsizei n, GLuint* renderbuffers) {}
	DEF_Debugger(GenTextures)(GLsizei n, GLuint* textures) {}
	DEF_Debugger(GetActiveAttrib)(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name) {}
	DEF_Debugger(GetActiveUniform)(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name) {}
	DEF_Debugger(GetAttachedShaders)(GLuint program, GLsizei maxcount, GLsizei* count, GLuint* shaders) {}
	DEF_Debugger(GetAttribLocation)(GLuint program, const GLchar* name) {}
	DEF_Debugger(GetBooleanv)(GLenum pname, GLboolean* params) {}
	DEF_Debugger(GetBufferParameteriv)(GLenum target, GLenum pname, GLint* params) {}
	DEF_Debugger(GetError)(void) {}
	DEF_Debugger(GetFloatv)(GLenum pname, GLfloat* params) {}
	DEF_Debugger(GetFramebufferAttachmentParameteriv)(GLenum target, GLenum attachment, GLenum pname, GLint* params) {}
	DEF_Debugger(GetIntegerv)(GLenum pname, GLint* params) {}
	DEF_Debugger(GetProgramiv)(GLuint program, GLenum pname, GLint* params) {}
	DEF_Debugger(GetProgramInfoLog)(GLuint program, GLsizei bufsize, GLsizei* length, GLchar* infolog) {}
	DEF_Debugger(GetRenderbufferParameteriv)(GLenum target, GLenum pname, GLint* params) {}
	DEF_Debugger(GetShaderiv)(GLuint shader, GLenum pname, GLint* params) {}
	DEF_Debugger(GetShaderInfoLog)(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* infolog) {}
	DEF_Debugger(GetShaderPrecisionFormat)(GLenum shadertype, GLenum precisiontype, GLint* range, GLint* precision) {}
	DEF_Debugger(GetShaderSource)(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source) {}
	DEF_Debugger(GetString)(GLenum name) {}
	DEF_Debugger(GetTexParameterfv)(GLenum target, GLenum pname, GLfloat* params) {}
	DEF_Debugger(GetTexParameteriv)(GLenum target, GLenum pname, GLint* params) {}
	DEF_Debugger(GetUniformfv)(GLuint program, GLint location, GLfloat* params) {}
	DEF_Debugger(GetUniformiv)(GLuint program, GLint location, GLint* params) {}
	DEF_Debugger(GetUniformLocation)(GLuint program, const GLchar* name) {}
	DEF_Debugger(GetVertexAttribfv)(GLuint index, GLenum pname, GLfloat* params) {}
	DEF_Debugger(GetVertexAttribiv)(GLuint index, GLenum pname, GLint* params) {}
	DEF_Debugger(GetVertexAttribPointerv)(GLuint index, GLenum pname, GLvoid** pointer) {}
	DEF_Debugger(Hint)(GLenum target, GLenum mode) {}
	DEF_Debugger(IsBuffer)(GLuint buffer) {}
	DEF_Debugger(IsEnabled)(GLenum cap) {}
	DEF_Debugger(IsFramebuffer)(GLuint framebuffer) {}
	DEF_Debugger(IsProgram)(GLuint program) {}
	DEF_Debugger(IsRenderbuffer)(GLuint renderbuffer) {}
	DEF_Debugger(IsShader)(GLuint shader) {}
	DEF_Debugger(IsTexture)(GLuint texture) {}
	DEF_Debugger(LineWidth)(GLfloat width) {}
	DEF_Debugger(LinkProgram)(GLuint program) {}
	DEF_Debugger(PixelStorei)(GLenum pname, GLint param) {}
	DEF_Debugger(PolygonOffset)(GLfloat factor, GLfloat units) {}
	DEF_Debugger(ReadPixels)(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLvoid* pixels) {}
	DEF_Debugger(ReleaseShaderCompiler)(void) {}
	DEF_Debugger(RenderbufferStorage)(GLenum target, GLenum internalformat, GLsizei width, GLsizei height) {}
	DEF_Debugger(SampleCoverage)(GLfloat value, GLboolean invert) {}
	DEF_Debugger(Scissor)(GLint x, GLint y, GLsizei width, GLsizei height) {}
	DEF_Debugger(ShaderBinary)(GLsizei n, const GLuint* shaders, GLenum binaryformat, const GLvoid* binary, GLsizei length) {}
	DEF_Debugger(ShaderSource)(GLuint shader, GLsizei count, const GLchar* const* string, const GLint* length) {}
	DEF_Debugger(StencilFunc)(GLenum func, GLint ref, GLuint mask) {}
	DEF_Debugger(StencilFuncSeparate)(GLenum face, GLenum func, GLint ref, GLuint mask) {}
	DEF_Debugger(StencilMask)(GLuint mask) {}
	DEF_Debugger(StencilMaskSeparate)(GLenum face, GLuint mask) {}
	DEF_Debugger(StencilOp)(GLenum fail, GLenum zfail, GLenum zpass) {}
	DEF_Debugger(StencilOpSeparate)(GLenum face, GLenum fail, GLenum zfail, GLenum zpass) {}
	DEF_Debugger(TexImage2D)(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid* pixels) {}
	DEF_Debugger(TexParameterf)(GLenum target, GLenum pname, GLfloat param) {}
	DEF_Debugger(TexParameterfv)(GLenum target, GLenum pname, const GLfloat* params) {}
	DEF_Debugger(TexParameteri)(GLenum target, GLenum pname, GLint param) {}
	DEF_Debugger(TexParameteriv)(GLenum target, GLenum pname, const GLint* params) {}
	DEF_Debugger(TexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const GLvoid* pixels) {}
	DEF_Debugger(Uniform1f)(GLint location, GLfloat x) {}
	DEF_Debugger(Uniform1fv)(GLint location, GLsizei count, const GLfloat* v) {}
	DEF_Debugger(Uniform1i)(GLint location, GLint x) {}
	DEF_Debugger(Uniform1iv)(GLint location, GLsizei count, const GLint* v) {}
	DEF_Debugger(Uniform2f)(GLint location, GLfloat x, GLfloat y) {}
	DEF_Debugger(Uniform2fv)(GLint location, GLsizei count, const GLfloat* v) {}
	DEF_Debugger(Uniform2i)(GLint location, GLint x, GLint y) {}
	DEF_Debugger(Uniform2iv)(GLint location, GLsizei count, const GLint* v) {}
	DEF_Debugger(Uniform3f)(GLint location, GLfloat x, GLfloat y, GLfloat z) {}
	DEF_Debugger(Uniform3fv)(GLint location, GLsizei count, const GLfloat* v) {}
	DEF_Debugger(Uniform3i)(GLint location, GLint x, GLint y, GLint z) {}
	DEF_Debugger(Uniform3iv)(GLint location, GLsizei count, const GLint* v) {}
	DEF_Debugger(Uniform4f)(GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w) {}
	DEF_Debugger(Uniform4fv)(GLint location, GLsizei count, const GLfloat* v) {}
	DEF_Debugger(Uniform4i)(GLint location, GLint x, GLint y, GLint z, GLint w) {}
	DEF_Debugger(Uniform4iv)(GLint location, GLsizei count, const GLint* v) {}
	DEF_Debugger(UniformMatrix2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UseProgram)(GLuint program) {}
	DEF_Debugger(ValidateProgram)(GLuint program) {}
	DEF_Debugger(VertexAttrib1f)(GLuint indx, GLfloat x) {}
	DEF_Debugger(VertexAttrib1fv)(GLuint indx, const GLfloat* values) {}
	DEF_Debugger(VertexAttrib2f)(GLuint indx, GLfloat x, GLfloat y) {}
	DEF_Debugger(VertexAttrib2fv)(GLuint indx, const GLfloat* values) {}
	DEF_Debugger(VertexAttrib3f)(GLuint indx, GLfloat x, GLfloat y, GLfloat z) {}
	DEF_Debugger(VertexAttrib3fv)(GLuint indx, const GLfloat* values) {}
	DEF_Debugger(VertexAttrib4f)(GLuint indx, GLfloat x, GLfloat y, GLfloat z, GLfloat w) {}
	DEF_Debugger(VertexAttrib4fv)(GLuint indx, const GLfloat* values) {}
	DEF_Debugger(VertexAttribPointer)(GLuint indx, GLint size, GLenum type, GLboolean normalized, GLsizei stride, const GLvoid* ptr) {}
	DEF_Debugger(Viewport)(GLint x, GLint y, GLsizei width, GLsizei height) {}

	DEF_Debugger(ReadBuffer)(GLenum mode) {}
	DEF_Debugger(DrawRangeElements)(GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, const GLvoid* indices) {}
	DEF_Debugger(TexImage3D)(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLenum format, GLenum type, const GLvoid* pixels) {}
	DEF_Debugger(TexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, const GLvoid* pixels) {}
	DEF_Debugger(CopyTexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height) {}
	DEF_Debugger(CompressedTexImage3D)(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLsizei imageSize, const GLvoid* data) {}
	DEF_Debugger(CompressedTexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize, const GLvoid* data) {}
	DEF_Debugger(GenQueries)(GLsizei n, GLuint* ids) {}
	DEF_Debugger(DeleteQueries)(GLsizei n, const GLuint* ids) {}
	DEF_Debugger(IsQuery)(GLuint id) {}
	DEF_Debugger(BeginQuery)(GLenum target, GLuint id) {}
	DEF_Debugger(EndQuery)(GLenum target) {}
	DEF_Debugger(GetQueryiv)(GLenum target, GLenum pname, GLint* params) {}
	DEF_Debugger(GetQueryObjectuiv)(GLuint id, GLenum pname, GLuint* params) {}
	DEF_Debugger(UnmapBuffer)(GLenum target) {}
	DEF_Debugger(GetBufferPointerv)(GLenum target, GLenum pname, GLvoid** params) {}
	DEF_Debugger(DrawBuffers)(GLsizei n, const GLenum* bufs) {}
	DEF_Debugger(UniformMatrix2x3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix3x2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix2x4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix4x2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix3x4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(UniformMatrix4x3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value) {}
	DEF_Debugger(BlitFramebuffer)(GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter) {}
	DEF_Debugger(RenderbufferStorageMultisample)(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height) {}
	DEF_Debugger(FramebufferTextureLayer)(GLenum target, GLenum attachment, GLuint texture, GLint level, GLint layer) {}
	DEF_Debugger(MapBufferRange)(GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access) {}
	DEF_Debugger(FlushMappedBufferRange)(GLenum target, GLintptr offset, GLsizeiptr length) {}
	DEF_Debugger(BindVertexArray)(GLuint array) {}
	DEF_Debugger(DeleteVertexArrays)(GLsizei n, const GLuint* arrays) {}
	DEF_Debugger(GenVertexArrays)(GLsizei n, GLuint* arrays) {}
	DEF_Debugger(IsVertexArray)(GLuint array) {}
	DEF_Debugger(GetIntegeri_v)(GLenum target, GLuint index, GLint* data) {}
	DEF_Debugger(BeginTransformFeedback)(GLenum primitiveMode) {}
	DEF_Debugger(EndTransformFeedback)(void) {}
	DEF_Debugger(BindBufferRange)(GLenum target, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size) {}
	DEF_Debugger(BindBufferBase)(GLenum target, GLuint index, GLuint buffer) {}
	DEF_Debugger(TransformFeedbackVaryings)(GLuint program, GLsizei count, const GLchar* const* varyings, GLenum bufferMode) {}
	DEF_Debugger(GetTransformFeedbackVarying)(GLuint program, GLuint index, GLsizei bufSize, GLsizei* length, GLsizei* size, GLenum* type, GLchar* name) {}
	DEF_Debugger(VertexAttribIPointer)(GLuint index, GLint size, GLenum type, GLsizei stride, const GLvoid* pointer) {}
	DEF_Debugger(GetVertexAttribIiv)(GLuint index, GLenum pname, GLint* params) {}
	DEF_Debugger(GetVertexAttribIuiv)(GLuint index, GLenum pname, GLuint* params) {}
	DEF_Debugger(VertexAttribI4i)(GLuint index, GLint x, GLint y, GLint z, GLint w) {}
	DEF_Debugger(VertexAttribI4ui)(GLuint index, GLuint x, GLuint y, GLuint z, GLuint w) {}
	DEF_Debugger(VertexAttribI4iv)(GLuint index, const GLint* v) {}
	DEF_Debugger(VertexAttribI4uiv)(GLuint index, const GLuint* v) {}
	DEF_Debugger(GetUniformuiv)(GLuint program, GLint location, GLuint* params) {}
	DEF_Debugger(GetFragDataLocation)(GLuint program, const GLchar *name) {}
	DEF_Debugger(Uniform1ui)(GLint location, GLuint v0) {}
	DEF_Debugger(Uniform2ui)(GLint location, GLuint v0, GLuint v1) {}
	DEF_Debugger(Uniform3ui)(GLint location, GLuint v0, GLuint v1, GLuint v2) {}
	DEF_Debugger(Uniform4ui)(GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3) {}
	DEF_Debugger(Uniform1uiv)(GLint location, GLsizei count, const GLuint* value) {}
	DEF_Debugger(Uniform2uiv)(GLint location, GLsizei count, const GLuint* value) {}
	DEF_Debugger(Uniform3uiv)(GLint location, GLsizei count, const GLuint* value) {}
	DEF_Debugger(Uniform4uiv)(GLint location, GLsizei count, const GLuint* value) {}
	DEF_Debugger(ClearBufferiv)(GLenum buffer, GLint drawbuffer, const GLint* value) {}
	DEF_Debugger(ClearBufferuiv)(GLenum buffer, GLint drawbuffer, const GLuint* value) {}
	DEF_Debugger(ClearBufferfv)(GLenum buffer, GLint drawbuffer, const GLfloat* value) {}
	DEF_Debugger(ClearBufferfi)(GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil) {}
	DEF_Debugger(GetStringi)(GLenum name, GLuint index) {}
	DEF_Debugger(CopyBufferSubData)(GLenum readTarget, GLenum writeTarget, GLintptr readOffset, GLintptr writeOffset, GLsizeiptr size) {}
	DEF_Debugger(GetUniformIndices)(GLuint program, GLsizei uniformCount, const GLchar* const* uniformNames, GLuint* uniformIndices) {}
	DEF_Debugger(GetActiveUniformsiv)(GLuint program, GLsizei uniformCount, const GLuint* uniformIndices, GLenum pname, GLint* params) {}
	DEF_Debugger(GetUniformBlockIndex)(GLuint program, const GLchar* uniformBlockName) {}
	DEF_Debugger(GetActiveUniformBlockiv)(GLuint program, GLuint uniformBlockIndex, GLenum pname, GLint* params) {}
	DEF_Debugger(GetActiveUniformBlockName)(GLuint program, GLuint uniformBlockIndex, GLsizei bufSize, GLsizei* length, GLchar* uniformBlockName) {}
	DEF_Debugger(UniformBlockBinding)(GLuint program, GLuint uniformBlockIndex, GLuint uniformBlockBinding) {}
	DEF_Debugger(DrawArraysInstanced)(GLenum mode, GLint first, GLsizei count, GLsizei instanceCount) {}
	DEF_Debugger(DrawElementsInstanced)(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices, GLsizei instanceCount) {}
	DEF_Debugger(FenceSync)(GLenum condition, GLbitfield flags) {}
	DEF_Debugger(IsSync)(GLsync sync) {}
	DEF_Debugger(DeleteSync)(GLsync sync) {}
	DEF_Debugger(ClientWaitSync)(GLsync sync, GLbitfield flags, GLuint64 timeout) {}
	DEF_Debugger(WaitSync)(GLsync sync, GLbitfield flags, GLuint64 timeout) {}
	DEF_Debugger(GetInteger64v)(GLenum pname, GLint64* params) {}
	DEF_Debugger(GetSynciv)(GLsync sync, GLenum pname, GLsizei bufSize, GLsizei* length, GLint* values) {}
	DEF_Debugger(GetInteger64i_v)(GLenum target, GLuint index, GLint64* data) {}
	DEF_Debugger(GetBufferParameteri64v)(GLenum target, GLenum pname, GLint64* params) {}
	DEF_Debugger(GenSamplers)(GLsizei count, GLuint* samplers) {}
	DEF_Debugger(DeleteSamplers)(GLsizei count, const GLuint* samplers) {}
	DEF_Debugger(IsSampler)(GLuint sampler) {}
	DEF_Debugger(BindSampler)(GLuint unit, GLuint sampler) {}
	DEF_Debugger(SamplerParameteri)(GLuint sampler, GLenum pname, GLint param) {}
	DEF_Debugger(SamplerParameteriv)(GLuint sampler, GLenum pname, const GLint* param) {}
	DEF_Debugger(SamplerParameterf)(GLuint sampler, GLenum pname, GLfloat param) {}
	DEF_Debugger(SamplerParameterfv)(GLuint sampler, GLenum pname, const GLfloat* param) {}
	DEF_Debugger(GetSamplerParameteriv)(GLuint sampler, GLenum pname, GLint* params) {}
	DEF_Debugger(GetSamplerParameterfv)(GLuint sampler, GLenum pname, GLfloat* params) {}
	DEF_Debugger(VertexAttribDivisor)(GLuint index, GLuint divisor) {}
	DEF_Debugger(BindTransformFeedback)(GLenum target, GLuint id) {}
	DEF_Debugger(DeleteTransformFeedbacks)(GLsizei n, const GLuint* ids) {}
	DEF_Debugger(GenTransformFeedbacks)(GLsizei n, GLuint* ids) {}
	DEF_Debugger(IsTransformFeedback)(GLuint id) {}
	DEF_Debugger(PauseTransformFeedback)(void) {}
	DEF_Debugger(ResumeTransformFeedback)(void) {}
	DEF_Debugger(GetProgramBinary)(GLuint program, GLsizei bufSize, GLsizei* length, GLenum* binaryFormat, GLvoid* binary) {}
	DEF_Debugger(ProgramBinary)(GLuint program, GLenum binaryFormat, const GLvoid* binary, GLsizei length) {}
	DEF_Debugger(ProgramParameteri)(GLuint program, GLenum pname, GLint value) {}
	DEF_Debugger(InvalidateFramebuffer)(GLenum target, GLsizei numAttachments, const GLenum* attachments) {}
	DEF_Debugger(InvalidateSubFramebuffer)(GLenum target, GLsizei numAttachments, const GLenum* attachments, GLint x, GLint y, GLsizei width, GLsizei height) {}
	DEF_Debugger(TexStorage2D)(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height) {}
	DEF_Debugger(TexStorage3D)(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth) {}
	DEF_Debugger(GetInternalformativ)(GLenum target, GLenum internalformat, GLenum pname, GLsizei bufSize, GLint* params) {}
	*/
	DEF_Debugger(ActiveTexture)(GLenum texture){}
	DEF_Debugger(AttachShader)(GLuint program, GLuint shader) {}
	DEF_Debugger(BindAttribLocation)(GLuint program, GLuint index, const GLchar* name) {}
	DEF_Debugger(BindBuffer)(GLenum target, GLuint buffer)
	{
		if (mWriteCmds == FALSE)
			return;

		DBGTrace("==>BindBuffer(%d,%d)\n", target, buffer);
	}
	DEF_Debugger(BindFramebuffer)(GLenum target, GLuint framebuffer){}
	DEF_Debugger(BindRenderbuffer)(GLenum target, GLuint renderbuffer){}
	DEF_Debugger(BindTexture)(GLenum target, GLuint texture)
	{
		if (mWriteCmds == FALSE)
			return;

		DBGTrace("==>BindTexture(%d,%d)\n", target, texture);
	}
	DEF_Debugger(BlendColor)(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha){}
	DEF_Debugger(BlendEquation)(GLenum mode){}
	DEF_Debugger(BlendEquationSeparate)(GLenum modeRGB, GLenum modeAlpha){}
	DEF_Debugger(BlendFunc)(GLenum sfactor, GLenum dfactor){}
	DEF_Debugger(BlendFuncSeparate)(GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha, GLenum dstAlpha){}
	DEF_Debugger(BufferData)(GLenum target, GLsizeiptr size, const GLvoid* data, GLenum usage){}
	DEF_Debugger(BufferSubData)(GLenum target, GLintptr offset, GLsizeiptr size, const GLvoid* data){}
	DEF_Debugger(CheckFramebufferStatus)(GLenum target){}
	DEF_Debugger(Clear)(GLbitfield mask){}
	DEF_Debugger(ClearColor)(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha){}
	DEF_Debugger(ClearDepthf)(GLfloat depth){}
	DEF_Debugger(ClearStencil)(GLint s){}
	DEF_Debugger(ColorMask)(GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha){}
	DEF_Debugger(CompileShader)(GLuint shader){}
	DEF_Debugger(CompressedTexImage2D)(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLint border, GLsizei imageSize, const GLvoid* data){}
	DEF_Debugger(CompressedTexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLsizei imageSize, const GLvoid* data){}
	DEF_Debugger(CopyTexImage2D)(GLenum target, GLint level, GLenum internalformat, GLint x, GLint y, GLsizei width, GLsizei height, GLint border){}
	DEF_Debugger(CopyTexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint x, GLint y, GLsizei width, GLsizei height){}
	DEF_Debugger(CreateProgram)(void){}
	DEF_Debugger(CreateShader)(GLenum type){}
	DEF_Debugger(CullFace)(GLenum mode){}
	DEF_Debugger(DeleteBuffers)(GLsizei n, const GLuint* buffers){}
	DEF_Debugger(DeleteFramebuffers)(GLsizei n, const GLuint* framebuffers){}
	DEF_Debugger(DeleteProgram)(GLuint program){}
	DEF_Debugger(DeleteRenderbuffers)(GLsizei n, const GLuint* renderbuffers){}
	DEF_Debugger(DeleteShader)(GLuint shader){}
	DEF_Debugger(DeleteTextures)(GLsizei n, const GLuint* textures){}
	DEF_Debugger(DepthFunc)(GLenum func){}
	DEF_Debugger(DepthMask)(GLboolean flag){}
	DEF_Debugger(DepthRangef)(GLfloat n, GLfloat f){}
	DEF_Debugger(DetachShader)(GLuint program, GLuint shader){}
	DEF_Debugger(Disable)(GLenum cap){}
	DEF_Debugger(DisableVertexAttribArray)(GLuint index){}
	DEF_Debugger(DrawArrays)(GLenum mode, GLint first, GLsizei count){}
	DEF_Debugger(DrawElements)(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices){}
	DEF_Debugger(Enable)(GLenum cap){}
	DEF_Debugger(EnableVertexAttribArray)(GLuint index){}
	DEF_Debugger(Finish)(void){}
	DEF_Debugger(Flush)(void){}
	DEF_Debugger(FramebufferRenderbuffer)(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer){}
	DEF_Debugger(FramebufferTexture2D)(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level){}
	DEF_Debugger(FrontFace)(GLenum mode){}
	DEF_Debugger(GenBuffers)(GLsizei n, GLuint* buffers){}
	DEF_Debugger(GenerateMipmap)(GLenum target){}
	DEF_Debugger(GenFramebuffers)(GLsizei n, GLuint* framebuffers){}
	DEF_Debugger(GenRenderbuffers)(GLsizei n, GLuint* renderbuffers){}
	DEF_Debugger(GenTextures)(GLsizei n, GLuint* textures){}
	DEF_Debugger(GetActiveAttrib)(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name){}
	DEF_Debugger(GetActiveUniform)(GLuint program, GLuint index, GLsizei bufsize, GLsizei* length, GLint* size, GLenum* type, GLchar* name){}
	DEF_Debugger(GetAttachedShaders)(GLuint program, GLsizei maxcount, GLsizei* count, GLuint* shaders){}
	DEF_Debugger(GetAttribLocation)(GLuint program, const GLchar* name){}
	DEF_Debugger(GetBooleanv)(GLenum pname, GLboolean* params){}
	DEF_Debugger(GetBufferParameteriv)(GLenum target, GLenum pname, GLint* params){}
	DEF_Debugger(GetError)(void){}
	DEF_Debugger(GetFloatv)(GLenum pname, GLfloat* params){}
	DEF_Debugger(GetFramebufferAttachmentParameteriv)(GLenum target, GLenum attachment, GLenum pname, GLint* params){}
	DEF_Debugger(GetIntegerv)(GLenum pname, GLint* params){}
	DEF_Debugger(GetProgramiv)(GLuint program, GLenum pname, GLint* params){}
	DEF_Debugger(GetProgramInfoLog)(GLuint program, GLsizei bufsize, GLsizei* length, GLchar* infolog){}
	DEF_Debugger(GetRenderbufferParameteriv)(GLenum target, GLenum pname, GLint* params){}
	DEF_Debugger(GetShaderiv)(GLuint shader, GLenum pname, GLint* params){}
	DEF_Debugger(GetShaderInfoLog)(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* infolog){}
	DEF_Debugger(GetShaderPrecisionFormat)(GLenum shadertype, GLenum precisiontype, GLint* range, GLint* precision){}
	DEF_Debugger(GetShaderSource)(GLuint shader, GLsizei bufsize, GLsizei* length, GLchar* source){}
	DEF_Debugger(GetString)(GLenum name){}
	DEF_Debugger(GetTexParameterfv)(GLenum target, GLenum pname, GLfloat* params){}
	DEF_Debugger(GetTexParameteriv)(GLenum target, GLenum pname, GLint* params){}
	DEF_Debugger(GetUniformfv)(GLuint program, GLint location, GLfloat* params){}
	DEF_Debugger(GetUniformiv)(GLuint program, GLint location, GLint* params){}
	DEF_Debugger(GetUniformLocation)(GLuint program, const GLchar* name){}
	DEF_Debugger(GetVertexAttribfv)(GLuint index, GLenum pname, GLfloat* params){}
	DEF_Debugger(GetVertexAttribiv)(GLuint index, GLenum pname, GLint* params){}
	DEF_Debugger(GetVertexAttribPointerv)(GLuint index, GLenum pname, GLvoid** pointer){}
	DEF_Debugger(Hint)(GLenum target, GLenum mode){}
	DEF_Debugger(IsBuffer)(GLuint buffer){}
	DEF_Debugger(IsEnabled)(GLenum cap){}
	DEF_Debugger(IsFramebuffer)(GLuint framebuffer){}
	DEF_Debugger(IsProgram)(GLuint program){}
	DEF_Debugger(IsRenderbuffer)(GLuint renderbuffer){}
	DEF_Debugger(IsShader)(GLuint shader){}
	DEF_Debugger(IsTexture)(GLuint texture){}
	DEF_Debugger(LineWidth)(GLfloat width){}
	DEF_Debugger(LinkProgram)(GLuint program){}
	DEF_Debugger(PixelStorei)(GLenum pname, GLint param){}
	DEF_Debugger(PolygonOffset)(GLfloat factor, GLfloat units){}
	DEF_Debugger(ReadPixels)(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, GLvoid* pixels){}
	DEF_Debugger(ReleaseShaderCompiler)(void){}
	DEF_Debugger(RenderbufferStorage)(GLenum target, GLenum internalformat, GLsizei width, GLsizei height){}
	DEF_Debugger(SampleCoverage)(GLfloat value, GLboolean invert){}
	DEF_Debugger(Scissor)(GLint x, GLint y, GLsizei width, GLsizei height){}
	DEF_Debugger(ShaderBinary)(GLsizei n, const GLuint* shaders, GLenum binaryformat, const GLvoid* binary, GLsizei length){}
	DEF_Debugger(ShaderSource)(GLuint shader, GLsizei count, const GLchar* const* string, const GLint* length){}
	DEF_Debugger(StencilFunc)(GLenum func, GLint ref, GLuint mask){}
	DEF_Debugger(StencilFuncSeparate)(GLenum face, GLenum func, GLint ref, GLuint mask){}
	DEF_Debugger(StencilMask)(GLuint mask){}
	DEF_Debugger(StencilMaskSeparate)(GLenum face, GLuint mask){}
	DEF_Debugger(StencilOp)(GLenum fail, GLenum zfail, GLenum zpass){}
	DEF_Debugger(StencilOpSeparate)(GLenum face, GLenum fail, GLenum zfail, GLenum zpass){}
	DEF_Debugger(TexImage2D)(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, const GLvoid* pixels){}
	DEF_Debugger(TexParameterf)(GLenum target, GLenum pname, GLfloat param){}
	DEF_Debugger(TexParameterfv)(GLenum target, GLenum pname, const GLfloat* params){}
	DEF_Debugger(TexParameteri)(GLenum target, GLenum pname, GLint param){}
	DEF_Debugger(TexParameteriv)(GLenum target, GLenum pname, const GLint* params){}
	DEF_Debugger(TexSubImage2D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLsizei width, GLsizei height, GLenum format, GLenum type, const GLvoid* pixels){}
	DEF_Debugger(Uniform1f)(GLint location, GLfloat x){}
	DEF_Debugger(Uniform1fv)(GLint location, GLsizei count, const GLfloat* v){}
	DEF_Debugger(Uniform1i)(GLint location, GLint x){}
	DEF_Debugger(Uniform1iv)(GLint location, GLsizei count, const GLint* v){}
	DEF_Debugger(Uniform2f)(GLint location, GLfloat x, GLfloat y){}
	DEF_Debugger(Uniform2fv)(GLint location, GLsizei count, const GLfloat* v){}
	DEF_Debugger(Uniform2i)(GLint location, GLint x, GLint y){}
	DEF_Debugger(Uniform2iv)(GLint location, GLsizei count, const GLint* v){}
	DEF_Debugger(Uniform3f)(GLint location, GLfloat x, GLfloat y, GLfloat z){}
	DEF_Debugger(Uniform3fv)(GLint location, GLsizei count, const GLfloat* v){}
	DEF_Debugger(Uniform3i)(GLint location, GLint x, GLint y, GLint z){}
	DEF_Debugger(Uniform3iv)(GLint location, GLsizei count, const GLint* v){}
	DEF_Debugger(Uniform4f)(GLint location, GLfloat x, GLfloat y, GLfloat z, GLfloat w){}
	DEF_Debugger(Uniform4fv)(GLint location, GLsizei count, const GLfloat* v){}
	DEF_Debugger(Uniform4i)(GLint location, GLint x, GLint y, GLint z, GLint w){}
	DEF_Debugger(Uniform4iv)(GLint location, GLsizei count, const GLint* v){}
	DEF_Debugger(UniformMatrix2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UseProgram)(GLuint program){}
	DEF_Debugger(ValidateProgram)(GLuint program){}
	DEF_Debugger(VertexAttrib1f)(GLuint indx, GLfloat x){}
	DEF_Debugger(VertexAttrib1fv)(GLuint indx, const GLfloat* values){}
	DEF_Debugger(VertexAttrib2f)(GLuint indx, GLfloat x, GLfloat y){}
	DEF_Debugger(VertexAttrib2fv)(GLuint indx, const GLfloat* values){}
	DEF_Debugger(VertexAttrib3f)(GLuint indx, GLfloat x, GLfloat y, GLfloat z){}
	DEF_Debugger(VertexAttrib3fv)(GLuint indx, const GLfloat* values){}
	DEF_Debugger(VertexAttrib4f)(GLuint indx, GLfloat x, GLfloat y, GLfloat z, GLfloat w){}
	DEF_Debugger(VertexAttrib4fv)(GLuint indx, const GLfloat* values){}
	DEF_Debugger(VertexAttribPointer)(GLuint indx, GLint size, GLenum type, GLboolean normalized, GLsizei stride, const GLvoid* ptr){}
	DEF_Debugger(Viewport)(GLint x, GLint y, GLsizei width, GLsizei height){}
	
	DEF_Debugger(ReadBuffer)(GLenum mode){}
	DEF_Debugger(DrawRangeElements)(GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type, const GLvoid* indices){}
	DEF_Debugger(TexImage3D)(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLenum format, GLenum type, const GLvoid* pixels){}
	DEF_Debugger(TexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLenum type, const GLvoid* pixels){}
	DEF_Debugger(CopyTexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLint x, GLint y, GLsizei width, GLsizei height){}
	DEF_Debugger(CompressedTexImage3D)(GLenum target, GLint level, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth, GLint border, GLsizei imageSize, const GLvoid* data){}
	DEF_Debugger(CompressedTexSubImage3D)(GLenum target, GLint level, GLint xoffset, GLint yoffset, GLint zoffset, GLsizei width, GLsizei height, GLsizei depth, GLenum format, GLsizei imageSize, const GLvoid* data){}
	DEF_Debugger(GenQueries)(GLsizei n, GLuint* ids){}
	DEF_Debugger(DeleteQueries)(GLsizei n, const GLuint* ids){}
	DEF_Debugger(IsQuery)(GLuint id){}
	DEF_Debugger(BeginQuery)(GLenum target, GLuint id){}
	DEF_Debugger(EndQuery)(GLenum target){}
	DEF_Debugger(GetQueryiv)(GLenum target, GLenum pname, GLint* params){}
	DEF_Debugger(GetQueryObjectuiv)(GLuint id, GLenum pname, GLuint* params){}
	DEF_Debugger(UnmapBuffer)(GLenum target){}
	DEF_Debugger(GetBufferPointerv)(GLenum target, GLenum pname, GLvoid** params){}
	DEF_Debugger(DrawBuffers)(GLsizei n, const GLenum* bufs){}
	DEF_Debugger(UniformMatrix2x3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix3x2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix2x4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix4x2fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix3x4fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(UniformMatrix4x3fv)(GLint location, GLsizei count, GLboolean transpose, const GLfloat* value){}
	DEF_Debugger(BlitFramebuffer)(GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter){}
	DEF_Debugger(RenderbufferStorageMultisample)(GLenum target, GLsizei samples, GLenum internalformat, GLsizei width, GLsizei height){}
	DEF_Debugger(FramebufferTextureLayer)(GLenum target, GLenum attachment, GLuint texture, GLint level, GLint layer){}
	DEF_Debugger(MapBufferRange)(GLenum target, GLintptr offset, GLsizeiptr length, GLbitfield access){}
	DEF_Debugger(FlushMappedBufferRange)(GLenum target, GLintptr offset, GLsizeiptr length){}
	DEF_Debugger(BindVertexArray)(GLuint array){}
	DEF_Debugger(DeleteVertexArrays)(GLsizei n, const GLuint* arrays){}
	DEF_Debugger(GenVertexArrays)(GLsizei n, GLuint* arrays){}
	DEF_Debugger(IsVertexArray)(GLuint array){}
	DEF_Debugger(GetIntegeri_v)(GLenum target, GLuint index, GLint* data){}
	DEF_Debugger(BeginTransformFeedback)(GLenum primitiveMode){}
	DEF_Debugger(EndTransformFeedback)(void){}
	DEF_Debugger(BindBufferRange)(GLenum target, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size){}
	DEF_Debugger(BindBufferBase)(GLenum target, GLuint index, GLuint buffer){}
	DEF_Debugger(TransformFeedbackVaryings)(GLuint program, GLsizei count, const GLchar* const* varyings, GLenum bufferMode){}
	DEF_Debugger(GetTransformFeedbackVarying)(GLuint program, GLuint index, GLsizei bufSize, GLsizei* length, GLsizei* size, GLenum* type, GLchar* name){}
	DEF_Debugger(VertexAttribIPointer)(GLuint index, GLint size, GLenum type, GLsizei stride, const GLvoid* pointer){}
	DEF_Debugger(GetVertexAttribIiv)(GLuint index, GLenum pname, GLint* params){}
	DEF_Debugger(GetVertexAttribIuiv)(GLuint index, GLenum pname, GLuint* params){}
	DEF_Debugger(VertexAttribI4i)(GLuint index, GLint x, GLint y, GLint z, GLint w){}
	DEF_Debugger(VertexAttribI4ui)(GLuint index, GLuint x, GLuint y, GLuint z, GLuint w){}
	DEF_Debugger(VertexAttribI4iv)(GLuint index, const GLint* v){}
	DEF_Debugger(VertexAttribI4uiv)(GLuint index, const GLuint* v){}
	DEF_Debugger(GetUniformuiv)(GLuint program, GLint location, GLuint* params){}
	DEF_Debugger(GetFragDataLocation)(GLuint program, const GLchar *name){}
	DEF_Debugger(Uniform1ui)(GLint location, GLuint v0){}
	DEF_Debugger(Uniform2ui)(GLint location, GLuint v0, GLuint v1){}
	DEF_Debugger(Uniform3ui)(GLint location, GLuint v0, GLuint v1, GLuint v2){}
	DEF_Debugger(Uniform4ui)(GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3){}
	DEF_Debugger(Uniform1uiv)(GLint location, GLsizei count, const GLuint* value){}
	DEF_Debugger(Uniform2uiv)(GLint location, GLsizei count, const GLuint* value){}
	DEF_Debugger(Uniform3uiv)(GLint location, GLsizei count, const GLuint* value){}
	DEF_Debugger(Uniform4uiv)(GLint location, GLsizei count, const GLuint* value){}
	DEF_Debugger(ClearBufferiv)(GLenum buffer, GLint drawbuffer, const GLint* value){}
	DEF_Debugger(ClearBufferuiv)(GLenum buffer, GLint drawbuffer, const GLuint* value){}
	DEF_Debugger(ClearBufferfv)(GLenum buffer, GLint drawbuffer, const GLfloat* value){}
	DEF_Debugger(ClearBufferfi)(GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil){}
	DEF_Debugger(GetStringi)(GLenum name, GLuint index){}
	DEF_Debugger(CopyBufferSubData)(GLenum readTarget, GLenum writeTarget, GLintptr readOffset, GLintptr writeOffset, GLsizeiptr size){}
	DEF_Debugger(GetUniformIndices)(GLuint program, GLsizei uniformCount, const GLchar* const* uniformNames, GLuint* uniformIndices){}
	DEF_Debugger(GetActiveUniformsiv)(GLuint program, GLsizei uniformCount, const GLuint* uniformIndices, GLenum pname, GLint* params){}
	DEF_Debugger(GetUniformBlockIndex)(GLuint program, const GLchar* uniformBlockName){}
	DEF_Debugger(GetActiveUniformBlockiv)(GLuint program, GLuint uniformBlockIndex, GLenum pname, GLint* params){}
	DEF_Debugger(GetActiveUniformBlockName)(GLuint program, GLuint uniformBlockIndex, GLsizei bufSize, GLsizei* length, GLchar* uniformBlockName){}
	DEF_Debugger(UniformBlockBinding)(GLuint program, GLuint uniformBlockIndex, GLuint uniformBlockBinding){}
	DEF_Debugger(DrawArraysInstanced)(GLenum mode, GLint first, GLsizei count, GLsizei instanceCount){}
	DEF_Debugger(DrawElementsInstanced)(GLenum mode, GLsizei count, GLenum type, const GLvoid* indices, GLsizei instanceCount){}
	DEF_Debugger(FenceSync)(GLenum condition, GLbitfield flags){}
	DEF_Debugger(IsSync)(GLsync sync){}
	DEF_Debugger(DeleteSync)(GLsync sync){}
	DEF_Debugger(ClientWaitSync)(GLsync sync, GLbitfield flags, GLuint64 timeout){}
	DEF_Debugger(WaitSync)(GLsync sync, GLbitfield flags, GLuint64 timeout){}
	DEF_Debugger(GetInteger64v)(GLenum pname, GLint64* params){}
	DEF_Debugger(GetSynciv)(GLsync sync, GLenum pname, GLsizei bufSize, GLsizei* length, GLint* values){}
	DEF_Debugger(GetInteger64i_v)(GLenum target, GLuint index, GLint64* data){}
	DEF_Debugger(GetBufferParameteri64v)(GLenum target, GLenum pname, GLint64* params){}
	DEF_Debugger(GenSamplers)(GLsizei count, GLuint* samplers){}
	DEF_Debugger(DeleteSamplers)(GLsizei count, const GLuint* samplers){}
	DEF_Debugger(IsSampler)(GLuint sampler){}
	DEF_Debugger(BindSampler)(GLuint unit, GLuint sampler){}
	DEF_Debugger(SamplerParameteri)(GLuint sampler, GLenum pname, GLint param){}
	DEF_Debugger(SamplerParameteriv)(GLuint sampler, GLenum pname, const GLint* param){}
	DEF_Debugger(SamplerParameterf)(GLuint sampler, GLenum pname, GLfloat param){}
	DEF_Debugger(SamplerParameterfv)(GLuint sampler, GLenum pname, const GLfloat* param){}
	DEF_Debugger(GetSamplerParameteriv)(GLuint sampler, GLenum pname, GLint* params){}
	DEF_Debugger(GetSamplerParameterfv)(GLuint sampler, GLenum pname, GLfloat* params){}
	DEF_Debugger(VertexAttribDivisor)(GLuint index, GLuint divisor){}
	DEF_Debugger(BindTransformFeedback)(GLenum target, GLuint id){}
	DEF_Debugger(DeleteTransformFeedbacks)(GLsizei n, const GLuint* ids){}
	DEF_Debugger(GenTransformFeedbacks)(GLsizei n, GLuint* ids){}
	DEF_Debugger(IsTransformFeedback)(GLuint id){}
	DEF_Debugger(PauseTransformFeedback)(void){}
	DEF_Debugger(ResumeTransformFeedback)(void){}
	DEF_Debugger(GetProgramBinary)(GLuint program, GLsizei bufSize, GLsizei* length, GLenum* binaryFormat, GLvoid* binary){}
	DEF_Debugger(ProgramBinary)(GLuint program, GLenum binaryFormat, const GLvoid* binary, GLsizei length){}
	DEF_Debugger(ProgramParameteri)(GLuint program, GLenum pname, GLint value){}
	DEF_Debugger(InvalidateFramebuffer)(GLenum target, GLsizei numAttachments, const GLenum* attachments){}
	DEF_Debugger(InvalidateSubFramebuffer)(GLenum target, GLsizei numAttachments, const GLenum* attachments, GLint x, GLint y, GLsizei width, GLsizei height){}
	DEF_Debugger(TexStorage2D)(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height){}
	DEF_Debugger(TexStorage3D)(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height, GLsizei depth){}
	DEF_Debugger(GetInternalformativ)(GLenum target, GLenum internalformat, GLenum pname, GLsizei bufSize, GLint* params){}
#pragma endregion Debugger
};

NS_END
