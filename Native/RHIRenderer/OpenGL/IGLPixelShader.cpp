#include "IGLPixelShader.h"
#include "IGLRenderContext.h"
#include "IGLShaderReflector.h"
#include "../../../Graphics/GfxEngine.h"

#define new VNEW

NS_BEGIN

IGLPixelShader::IGLPixelShader()
{
}


IGLPixelShader::~IGLPixelShader()
{
	Cleanup();
}

void IGLPixelShader::Cleanup()
{
	mShader.reset();
}

bool DiscardGLExtension(const std::string& code, std::string& ms)
{
	bool replaced = false;
	ms = code;
	auto np = ms.find("#extension GL_EXT_shader_texture_lod : enable");
	if (np != std::string::npos)
	{
		ms = ms.insert(np, "//");
		replaced = true;
	}
	return replaced;
}

extern const char* GLES_Define;

bool IGLPixelShader::Init(IGLRenderContext* rc, const IShaderDesc* desc)
{
	mDesc.StrongRef((IShaderDesc*)desc);

	auto sdk = GLSdk::ImmSDK;
	
	mShader = sdk->CreateShader(GL_FRAGMENT_SHADER);
	auto code = desc->Es300Code.c_str();
#if PLATFORM_WIN
	std::string ms;
	if (DiscardGLExtension(desc->Es300Code, ms))
	{
		code = ms.c_str();
	}
#endif
	sdk->ShaderSource(mShader, 1, &code, NULL);
	sdk->CompileShader(mShader);

	sdk->PushCommand([=]()->void
	{
		GLint Result = GL_FALSE;
		int InfoLogLength;

		sdk->GetShaderiv(mShader, GL_COMPILE_STATUS, &Result);
		sdk->GetShaderiv(mShader, GL_INFO_LOG_LENGTH, &InfoLogLength);
		if (InfoLogLength > 0) {
			auto glslVersion =  (char*)sdk->GetString(GL_SHADING_LANGUAGE_VERSION);
			std::vector<char> VertexShaderErrorMessage(InfoLogLength + 1);
			sdk->GetShaderInfoLog(mShader, InfoLogLength, NULL, &VertexShaderErrorMessage[0]);
			char* errorStr = &VertexShaderErrorMessage[0];
			VFX_LTRACE(ELTT_Graphics, "\nPS[%s] error:%s\n", glslVersion, errorStr);
			VFX_LTRACE(ELTT_Graphics, "\nerror Code:\n%s\n", code);
		}

		IGLShaderReflector::Reflect(rc, mShader, this);
	}, "GLPixelShader::Init");

	return true;
}

NS_END