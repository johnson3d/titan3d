#pragma once
#include "../IShaderProgram.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLShaderProgram : public IShaderProgram
{
public:
	IGLShaderProgram();
	~IGLShaderProgram();

	virtual void Cleanup() override;

	virtual vBOOL LinkShaders(IRenderContext* rc) override;
	virtual void ApplyShaders(ICommandList* cmd) override;
public:
	std::shared_ptr<GLSdk::GLBufferId>	mProgram;
	/*std::vector<IConstantBufferDesc>	mCBuffers;
	std::vector<TextureBindInfo>		mTextures;
	std::vector<SamplerBindInfo>		mSamplers;*/

	vBOOL								mNoPSProfilering;
public:
	bool Init(IGLRenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END