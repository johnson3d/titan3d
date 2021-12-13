#pragma once
#include "../IShaderProgram.h"
#include "IVKRenderContext.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKShaderProgram : public IShaderProgram
{
public:
	IVKShaderProgram();
	~IVKShaderProgram();

	virtual vBOOL LinkShaders(IRenderContext* rc) override;
	virtual void ApplyShaders(ICommandList* cmd) override;

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;

	VkPipelineShaderStageCreateInfo		mVSCreateInfo;
	VkPipelineShaderStageCreateInfo		mPSCreateInfo;

	std::map<UINT, std::vector<VkDescriptorSetLayoutBinding>>	mLayoutBindings;
	std::vector<IVKDescriptorPool::FDescriptorAlloc>	mAllocInfo;
	std::vector<VkDescriptorSetLayout>	mDescriptorSetLayout;

	VkPipelineLayout					mPipelineLayout;
public:
	bool Init(IVKRenderContext* rc, const IShaderProgramDesc* desc);
	void PushBinding(UINT descriptorSet, const VkDescriptorSetLayoutBinding& binding);
};

NS_END