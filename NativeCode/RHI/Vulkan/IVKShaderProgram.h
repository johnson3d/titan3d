#pragma once
#include "../IShaderProgram.h"

NS_BEGIN

class IVKRenderContext;
class IVKShaderProgram : public IShaderProgram
{
public:
	IVKShaderProgram();
	~IVKShaderProgram();

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
public:
	bool Init(IVKRenderContext* rc, const IShaderProgramDesc* desc);
};

NS_END