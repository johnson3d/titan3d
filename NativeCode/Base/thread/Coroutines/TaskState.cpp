#include "TaskState.h"
#include "TaskScheduler.h"

NS_BEGIN

int ITaskState::NumOfITaskState = 0;

ITaskState::ITaskState()
{
	mScheduler = TaskScheduler::GetCurrentInstance();
	PostTargetThread = -1;
	mSemaphore = nullptr;
	mPostCompleted = true;

	NumOfITaskState++;
}

ITaskState::~ITaskState()
{
	--NumOfITaskState;
}

NS_END