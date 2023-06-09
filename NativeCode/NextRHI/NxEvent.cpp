#include "NxEvent.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	IFence::IFence()
	{
	}
	UINT64 IFence::IncreaseExpect(ICmdQueue* queue, UINT64 num, EQueueType type) 
	{
		std::lock_guard<std::mutex> lck(mLocker);
		ExpectValue += num;
		auto completed = this->GetCompletedValue();
		ASSERT(completed <= ExpectValue);
		Signal(queue, ExpectValue, type);

		return ExpectValue;
	}
}

NS_END
