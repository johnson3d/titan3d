#include "IGLComputeShader.h"
#include "IGLRenderContext.h"
#include "IGLShaderReflector.h"

#define new VNEW

NS_BEGIN

IGLComputeShader::IGLComputeShader()
{
	mShader = 0;
}


IGLComputeShader::~IGLComputeShader()
{
	Cleanup();
}

void IGLComputeShader::Cleanup()
{
	mShader.reset();
	mProgram.reset();
}

bool IGLComputeShader::Init(IGLRenderContext* rc, const IShaderDesc* desc)
{
	mDesc.StrongRef((IShaderDesc*)desc);
	auto sdk = GLSdk::ImmSDK;

	mShader = sdk->CreateShader(GL_COMPUTE_SHADER);

	auto code = desc->Es300Code.c_str();
	sdk->ShaderSource(mShader, 1, &code, NULL);
	sdk->CompileShader(mShader);

	AutoRef<IGLComputeShader> pThis;
	pThis.StrongRef(this);
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

			VFX_LTRACE(ELTT_Graphics, "\nCS[%s] error:%s\n", glslVersion, errorStr);
			VFX_LTRACE(ELTT_Graphics, "\nerror Code:\n%s\n", code);
		}

		pThis->mProgram = IGLShaderReflector::Reflect(rc, mShader, this);
	}, "GLComputeShader::Init");
	return true;
}

NS_END
