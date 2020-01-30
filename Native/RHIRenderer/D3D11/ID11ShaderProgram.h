#pragma once
#include "../IShaderProgram.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11ShaderProgram : public IShaderProgram
{
public:
	ID11ShaderProgram();
	~ID11ShaderProgram();

	virtual vBOOL LinkShaders(IRenderContext* rc) override;
	virtual void ApplyShaders(ICommandList* cmd) override;

	virtual UINT FindCBuffer(const char* name) override;
	virtual UINT GetCBufferNumber() override;
	virtual IConstantBufferDesc* GetCBuffer(UINT index) override;

	virtual UINT GetShaderResourceNumber() const override;
	virtual bool GetShaderResourceBindInfo(UINT Index, TextureBindInfo* info) const override;
	virtual UINT GetTextureBindSlotIndex(const char* name) override;

	virtual UINT GetSamplerNumber() const override;
	virtual bool GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const override;
	virtual UINT GetSamplerBindSlotIndex(const char* name) override;

public:
	std::vector<IConstantBufferDesc>	mCBuffers;
	std::vector<TextureBindInfo>		mTextures;
	std::vector<SamplerBindInfo>		mSamplers;
public:
	bool Init(ID11RenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END