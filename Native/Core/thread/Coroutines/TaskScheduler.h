#pragma once
#include "TaskState.h"
#include <queue>

NS_BEGIN

struct ITaskState;
class TaskScheduler
{
private:
	std::queue<ITaskState*>			mPostExec;
	std::queue<ITaskState*>			mContinueExec;
	int								TargetId;
public:
	static TaskScheduler* GetCurrentInstance();

	static TaskScheduler* GetSchedulerByTargetId(int target);

	std::string						ThreadName;
	
	void SetThreadInfo(const char* name, int id);

	void PushPost(ITaskState* state);
	void PushContinue(ITaskState* state);

	void DoPost();
	void DoContinue();
};

NS_END