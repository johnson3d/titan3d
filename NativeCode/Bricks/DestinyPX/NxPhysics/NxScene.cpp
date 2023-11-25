#include "NxScene.h"
#include "NxActor.h"
#include "NxConstraint.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxScene);
	//https://zhuanlan.zhihu.com/p/542193907
	//https://www.mustenaka.cn/index.php/2023/09/06/pbd-method-learn-01/
	//https://zhuanlan.zhihu.com/p/542772924?utm_id=0

	bool NxScene::AddActor(NxActor* actor)
	{
		std::lock_guard lk(mLocker);
		auto prevScene = actor->mScene.GetPtr();
		if (prevScene == this)
			return false;
		if (prevScene != nullptr)
		{
			prevScene->RemoveActor(actor);
		}
		actor->mScene.FromObject(this);
		mActors.push_back(actor);
		return true;
	}
	bool NxScene::RemoveActor(NxActor* actor)
	{
		std::lock_guard lk(mLocker);
		for (auto i = mActors.begin(); i != mActors.end(); i++)
		{
			if (*i == actor)
			{
				mActors.erase(i);
				return true;
			}
		}
		return false;
	}
	void NxScene::Simulate(const NxReal& elapsedTime)
	{
		std::lock_guard lk(mLocker);

		auto numOfStep = (int)(elapsedTime / TimeStep);
		auto step = TimeStep;
		auto residue = NxReal::Mod(elapsedTime, TimeStep);
		
		for (int i = 0; i < numOfStep; i++)
		{
			step = (i == numOfStep - 1) ? residue : TimeStep;

			for (auto& i : mActors)
			{
				i->TryStep(step);
				/*if (i->GetRtti() == GetClassObject<NxRigidBody>())
				{
					auto rbody = ((NxRigidBody*)i);
				}*/
			}

			/*for (auto& i : mActors)
			{
				i->SolveStep(step);
			}*/
			for (auto& i : mConstraints)
			{
				i->SolveConstraint(this, step);
			}

			for (auto& i : mActors)
			{
				i->FixStep(step);
			}
		}
	}
}

NS_END