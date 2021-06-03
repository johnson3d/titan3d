#include "IMdfQueue.h"

#define new VNEW

NS_BEGIN

void IMdfQueue::GetInputStreams(DWORD* pOutStreams)
{
	if (pOutStreams == nullptr)
		return;

	*pOutStreams = 0;
	for (auto i : mMdfQueue)
	{
		i->GetInputStreams(*pOutStreams);
	}
}

void IMdfQueue::GetProvideStreams(DWORD* pOutStreams)
{
	if (pOutStreams == nullptr)
		return;

	*pOutStreams = 0;
	for (auto i : mMdfQueue)
	{
		i->GetProvideStreams(*pOutStreams);
	}
}

IMdfQueue* IMdfQueue::CloneMdfQueue()
{
	return nullptr;
}

NS_END