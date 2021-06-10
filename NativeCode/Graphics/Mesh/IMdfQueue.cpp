#include "IMdfQueue.h"

#define new VNEW

NS_BEGIN

void IMdfQueue::GetInputStreams(UINT* pOutStreams)
{
	if (pOutStreams == nullptr)
		return;

	*pOutStreams = 0;
	for (auto i : mMdfQueue)
	{
		i->GetInputStreams(pOutStreams);
	}
}

void IMdfQueue::GetProvideStreams(UINT* pOutStreams)
{
	if (pOutStreams == nullptr)
		return;

	*pOutStreams = 0;
	for (auto i : mMdfQueue)
	{
		i->GetProvideStreams(pOutStreams);
	}
}

IMdfQueue* IMdfQueue::CloneMdfQueue()
{
	return nullptr;
}

NS_END