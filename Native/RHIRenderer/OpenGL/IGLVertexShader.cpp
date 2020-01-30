#include "IGLVertexShader.h"
#include "IGLRenderContext.h"
#include "IGLShaderReflector.h"
#include "../../../Graphics/GfxEngine.h"

#define new VNEW

NS_BEGIN

IGLVertexShader::IGLVertexShader()
{
}


IGLVertexShader::~IGLVertexShader()
{
	Cleanup();
}

void IGLVertexShader::Cleanup()
{
	mShader.reset();
}

const char* GLES_Define = "#version 310 es\r\n"
"#define f16vec float\r\n"
"#define f16vec2 vec2\r\n"
"#define f16vec3 vec3\r\n"
"#define f16vec4 vec4\r\n";

bool IGLVertexShader::Init(IGLRenderContext* rc, const IShaderDesc* desc)
{
	mDesc.StrongRef((IShaderDesc*)desc);

	AutoRef<GLSdk> sdk = GLSdk::ImmSDK;

	mShader = sdk->CreateShader(GL_VERTEX_SHADER);

	auto code = desc->Es300Code.c_str();
	sdk->ShaderSource(mShader, 1, &code, NULL);
	sdk->CompileShader(mShader);
	
	sdk->PushCommand([=]()->void
	{
		GLint Result = GL_FALSE;
		int InfoLogLength;

		sdk->GetShaderiv(mShader, GL_COMPILE_STATUS, &Result);
		sdk->GetShaderiv(mShader, GL_INFO_LOG_LENGTH, &InfoLogLength);
		if (InfoLogLength > 0) {
			auto glslVersion = (char*)sdk->GetString(GL_SHADING_LANGUAGE_VERSION);
			std::vector<char> VertexShaderErrorMessage(InfoLogLength + 1);
			sdk->GetShaderInfoLog(mShader, InfoLogLength, NULL, &VertexShaderErrorMessage[0]);
			auto errorStr = &VertexShaderErrorMessage[0];
			 
			VFX_LTRACE(ELTT_Graphics, "\nVS[%s] error:%s\n", glslVersion, errorStr);
			VFX_LTRACE(ELTT_Graphics, "\nerror Code:\n%s\n", code);
		}

		IGLShaderReflector::Reflect(rc, mShader, this);
	}, "GLVertexShader::Init");

	return true;
}

NS_END