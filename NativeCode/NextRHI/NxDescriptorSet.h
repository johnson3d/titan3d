#pragma once
#include "NxRHIDefine.h"

NS_BEGIN

namespace NxRHI
{
	class IFence;
	class ICmdQueue;
	class TR_CLASS()
		IDescriptorSet : public IGpuResource
	{
	};

	class TR_CLASS()
		IDescriptorPool : public IGpuResource
	{
	public:
		virtual void Reset() = 0;
		UINT64 mFrameFenceValue = 0;
	};

	class TR_CLASS()
		NxDesriptorPoolManager : public VIUnknown
	{
		std::stack<AutoRef<IDescriptorPool>> mFreeFramePools;
		std::vector<AutoRef<IDescriptorPool>> mUsingFramePools;
		AutoRef<IDescriptorPool> mCurrentFramePool;
		AutoRef<IFence> mFrameFence;
		AutoRef<IDescriptorPool> Pop();
		void TickRecycle();
	public:
		virtual void Initialize(IGpuDevice* device);
		virtual IDescriptorPool* CreateDescriptorPool() = 0;
		IDescriptorPool* GetCurrentFramePool() {
			return mCurrentFramePool;
		}
		void AllocFramePool();
		void FreePool(ICmdQueue* queue, IDescriptorPool* pool);
		void NullCurrentFramePool()
		{
			mCurrentFramePool = nullptr;
		}
	};
}

NS_END