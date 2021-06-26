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

	virtual UINT FindCBuffer(const char* name) override;
	virtual UINT GetCBufferNumber() override;
	virtual IConstantBufferDesc* GetCBuffer(UINT index) override;

	virtual UINT GetShaderResourceNumber() const override;
	virtual bool GetShaderResourceBindInfo(UINT Index, TSBindInfo* info, int dataSize) const override;
	virtual UINT GetTextureBindSlotIndex(const char* name) override;

	virtual UINT GetSamplerNumber() const override;
	virtual bool GetSamplerBindInfo(UINT Index, TSBindInfo* info, int dataSize) const override;
	virtual UINT GetSamplerBindSlotIndex(const char* name) override;
	
public:
	std::shared_ptr<GLSdk::GLBufferId>	mProgram;

	AutoRef<ShaderReflector>			mProgramReflector;
	/*std::vector<IConstantBufferDesc>	mCBuffers;
	std::vector<TextureBindInfo>		mTextures;
	std::vector<SamplerBindInfo>		mSamplers;*/

	vBOOL								mNoPSProfilering;
public:
	bool Init(IGLRenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END