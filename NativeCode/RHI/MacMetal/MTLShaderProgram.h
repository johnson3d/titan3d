#pragma once
#include "../IShaderProgram.h"
#include "../ShaderReflector.h"
#include "../PreHead.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlGpuProgram: public IShaderProgram
{
public:
	MtlGpuProgram();
	~MtlGpuProgram();

	virtual vBOOL LinkShaders(IRenderContext* pCtx) override;
	virtual void ApplyShaders(ICommandList* pCmd) override;

	virtual UINT FindCBuffer(const char* name) override;
	virtual UINT GetCBufferNumber() override;
	virtual IConstantBufferDesc* GetCBuffer(UINT index) override;

	virtual UINT GetShaderResourceNumber() const override;
	virtual bool GetShaderResourceBindInfo(UINT Index, ShaderRViewBindInfo* info) const override;
	virtual UINT GetTextureBindSlotIndex(const char* name) override;

	virtual UINT GetSamplerNumber() const override;
	virtual bool GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const override;
	virtual UINT GetSamplerBindSlotIndex(const char* name) override;

public:
	std::vector<IConstantBufferDesc>	mConstBufferDescArray;
	std::vector<ShaderRViewBindInfo>			mTexBindInfoArray;
	std::vector<SamplerBindInfo>			mSampBindInfoArray;

public:
	bool Init(MtlContext* pCtx, const IShaderProgramDesc* pDesc);
};

NS_END