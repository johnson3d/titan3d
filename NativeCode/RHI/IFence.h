#pragma once
#include "IRenderResource.h"

NS_BEGIN

class TR_CLASS()
	IFence : public IRenderResource
{
public:
	virtual void Wait() = 0;
	virtual void Reset() = 0;
	virtual bool IsCompletion() = 0;
};

class TR_CLASS()
	ISemaphore : public IRenderResource
{
public:
	virtual void Wait() = 0;
	virtual void Signal(UINT64 count) = 0;
	virtual UINT GetSemaphoreCounter() = 0;
};

NS_END