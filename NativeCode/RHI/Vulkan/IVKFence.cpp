#include "IVKFence.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKFence::IVKFence()
{
	mFence = nullptr;
}

IVKFence::~IVKFence()
{
	PostVkExecute([Fence = mFence](IVKRenderContext* rc)
		{
			if (Fence != nullptr)
			{
				vkDestroyFence(rc->mLogicalDevice, Fence, nullptr);
			}
		});
	mFence = nullptr;
}

bool IVKFence::Init(IVKRenderContext* rc)
{
	mRenderContext.FromObject(rc);
	VkFenceCreateInfo info{};
	info.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	info.flags = VK_FENCE_CREATE_SIGNALED_BIT;
	vkCreateFence(rc->mLogicalDevice, &info, rc->GetVkAllocCallBacks(), &mFence);
	return true;
}

void IVKFence::Wait()
{
	auto rc = mRenderContext.GetPtr();
	vkWaitForFences(rc->mLogicalDevice, 1, &mFence, VK_TRUE, UINT64_MAX);
}

void IVKFence::Reset()
{
	auto rc = mRenderContext.GetPtr();
	vkResetFences(rc->mLogicalDevice, 1, &mFence);
}

bool IVKFence::IsCompletion()
{
	auto rc = mRenderContext.GetPtr();
	return vkGetFenceStatus(rc->mLogicalDevice, mFence) == VK_SUCCESS;
	return true;
}

//////////////////////////////////////////////////////////////////////////

IVKSemaphore::IVKSemaphore()
{
	mSemaphore = nullptr;
}

IVKSemaphore::~IVKSemaphore()
{
	PostVkExecute([Semaphore = mSemaphore](IVKRenderContext* rc)
		{
			if (Semaphore != nullptr)
			{
				vkDestroySemaphore(rc->mLogicalDevice, Semaphore, nullptr);
			}
		});
	mSemaphore = nullptr;
}

void IVKSemaphore::Wait()
{
	/*auto rc = mRenderContext.GetPtr();
	VkSemaphoreWaitInfo info{};
	info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	info.flags = VK_SEMAPHORE_WAIT_ANY_BIT;
	info.semaphoreCount = 1;
	info.pSemaphores = &mSemaphore;
	uint64_t smpCount = 0;
	info.pValues = &smpCount;
	vkWaitSemaphores(rc->mLogicalDevice, &info, UINT64_MAX);*/
}

void IVKSemaphore::Signal(UINT64 count)
{
	/*auto rc = mRenderContext.GetPtr();

	VkSemaphoreSignalInfo info{};
	info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_SIGNAL_INFO;
	info.semaphore = mSemaphore;
	info.value = count;
	vkSignalSemaphore(rc->mLogicalDevice, &info);*/
}

UINT IVKSemaphore::GetSemaphoreCounter()
{
	return 0;
	/*auto rc = mRenderContext.GetPtr();
	UINT64 count = 0;
	vkGetSemaphoreCounterValue(rc->mLogicalDevice, mSemaphore, &count);
	return (UINT)count;*/
}

bool IVKSemaphore::Init(IVKRenderContext* rc)
{
	mRenderContext.FromObject(rc);
	VkSemaphoreCreateInfo info{};
	info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	info.flags = VK_FENCE_CREATE_SIGNALED_BIT;
	vkCreateSemaphore(rc->mLogicalDevice, &info, rc->GetVkAllocCallBacks(), &mSemaphore);
	return true;
}

NS_END
