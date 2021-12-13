#pragma once
#include "../IFence.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKFence : public IFence
{
public:
	IVKFence();
	~IVKFence();
	virtual void Wait() override;
	virtual void Reset() override;
	virtual bool IsCompletion() override;

public:
	bool Init(IVKRenderContext* rc);
	VkFence			mFence;
	TObjectHandle<IVKRenderContext>		mRenderContext;
};

class IVKSemaphore : public ISemaphore
{
public:
	IVKSemaphore();
	~IVKSemaphore();
	virtual void Wait() override;
	virtual void Signal(UINT64 count) override;
	virtual UINT GetSemaphoreCounter() override;

public:
	bool Init(IVKRenderContext* rc);
	VkSemaphore			mSemaphore;
	TObjectHandle<IVKRenderContext>		mRenderContext;
};

NS_END