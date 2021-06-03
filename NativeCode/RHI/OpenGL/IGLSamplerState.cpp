#include "IGLSamplerState.h"
#include "IGLRenderContext.h"

#define new VNEW

NS_BEGIN

IGLSamplerState::IGLSamplerState()
{
	
}

IGLSamplerState::~IGLSamplerState()
{
	Cleanup();
}

void IGLSamplerState::Cleanup()
{
	mSampler.reset();
}

bool IGLSamplerState::Init(IGLRenderContext* rc, const ISamplerStateDesc* desc)
{
	mDesc = *desc;

	auto sdk = GLSdk::ImmSDK;

	mSampler = sdk->GenSamplers();
	GLenum minFilter, magFilter;
	FilterToGL(mDesc.Filter, minFilter, magFilter);
	GLenum wrapS = AddressModeToGL(mDesc.AddressU);
	GLenum wrapT = AddressModeToGL(mDesc.AddressV);
	GLenum wrapR = AddressModeToGL(mDesc.AddressW);

	sdk->SamplerParameteri(mSampler, GL_TEXTURE_MIN_FILTER, minFilter);
	sdk->SamplerParameteri(mSampler, GL_TEXTURE_MAG_FILTER, magFilter);
	sdk->SamplerParameteri(mSampler, GL_TEXTURE_WRAP_S, wrapS);
	sdk->SamplerParameteri(mSampler, GL_TEXTURE_WRAP_T, wrapT);
	sdk->SamplerParameteri(mSampler, GL_TEXTURE_WRAP_R, wrapR);
	if (mDesc.CmpMode == CMP_NEVER)
	{
		sdk->SamplerParameteri(mSampler, GL_TEXTURE_COMPARE_MODE, GL_NONE);
	}
	else
	{
		GLenum cmpMode = ComparisionModeToGL(mDesc.CmpMode);
		sdk->SamplerParameteri(mSampler, GL_TEXTURE_COMPARE_MODE, GL_COMPARE_REF_TO_TEXTURE);
		sdk->SamplerParameteri(mSampler, GL_TEXTURE_COMPARE_FUNC, cmpMode);
	}

	//GLfloat testBorderColor[] = { 0,1,0,0 };
	//sdk->SamplerParameterfv(mSampler, GL_TEXTURE_BORDER_COLOR, testBorderColor);
	sdk->SamplerParameterfv(mSampler, GL_TEXTURE_BORDER_COLOR, (GLfloat*)mDesc.BorderColor);
	sdk->SamplerParameterf(mSampler, GL_TEXTURE_MIN_LOD, mDesc.MinLOD);
	float maxLod = mDesc.MaxLOD;
	if (mDesc.MaxLOD > 1000)
		maxLod = 1000;
	sdk->SamplerParameterf(mSampler, GL_TEXTURE_MAX_LOD, maxLod);

	return true;
}

NS_END