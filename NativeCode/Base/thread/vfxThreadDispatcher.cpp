#include "vfxThreadDispatcher.h"

#define new VNEW

NS_BEGIN

static FContextTickableManager gFContextTickableManager;
FContextTickableManager* FContextTickableManager::GetInstance()
{
	return &gFContextTickableManager;
}

void FContextTickableManager::ThreadTick()
{
	for (size_t i = 0; i < mTickables.size(); i++)
	{
		mTickables[i]();
	} 
}

FRunTasks VParallelTaskManager::mRunTaskFunction;

bool VParallelTaskManager::RunTasks(FTaskSession* session)
{
	session->Count = session->GetNumOfTasks();
	if (mRunTaskFunction)
	{
		mRunTaskFunction(session);
		return true;
	}
	return false;
}

void VParallelTaskManager::Wait(FTaskSession* session)
{
	volatile int Count = 0;
	while (session->Count > 0)
	{
		Count++;
	}
}

static void TestParallelTask()
{
	FTaskSession session;
	auto& tasks = session.GetTasks();

	struct FTestTask : public IParallelTask
	{
	protected:
		virtual void DoWork(FTaskSession* session) override
		{
			Index++;
		}
	public:
		int Index = 0;
	};
	FTestTask t1;
	t1.Index = 0;
	tasks.push_back(&t1);

	VParallelTaskManager::RunTasks(&session);
	VParallelTaskManager::Wait(&session);
}


NS_END
