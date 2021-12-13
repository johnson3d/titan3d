#include "IVKPixelShader.h"
#include "IVKRenderContext.h"
#include "../ShaderReflector.h"

#define new VNEW

NS_BEGIN

IVKPixelShader::IVKPixelShader()
{
	mShader = nullptr;
}


IVKPixelShader::~IVKPixelShader()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mShader != nullptr)
	{
		vkDestroyShaderModule(rc->mLogicalDevice, mShader, nullptr);
		mShader = nullptr;
	}
}

bool IVKPixelShader::Init(IVKRenderContext* rc, const IShaderDesc* desc)
{
	mRenderContext.FromObject(rc);
	mDesc.StrongRef((IShaderDesc*)desc);

	VkShaderModuleCreateInfo createInfo{};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.codeSize = desc->SpirV.size() * sizeof(UINT);
	createInfo.pCode = reinterpret_cast<const uint32_t*>(&desc->SpirV[0]);

	if (vkCreateShaderModule(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mShader) != VK_SUCCESS) 
	{
		return false;
	}
	((IShaderDesc*)desc)->GetReflector()->ReflectSpirV(desc);
	return true;
}

NS_END