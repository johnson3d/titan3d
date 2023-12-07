#include "NxScene.h"
#include "NxActor.h"
#include "NxJoint.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxScene);
	//https://zhuanlan.zhihu.com/p/542193907
	//https://www.mustenaka.cn/index.php/2023/09/06/pbd-method-learn-01/
	//https://zhuanlan.zhihu.com/p/542772924?utm_id=0
	//https://github.com/Jondolf/bevy_xpbd
	//https://github.com/InteractiveComputerGraphics/PositionBasedDynamics

	bool NxScene::Init(const NxSceneDesc& desc)
	{
		mDesc = desc;
		return true;
	}
	bool NxScene::AddActor(NxActor* actor)
	{
		if (actor->GetScene() == this)
			return true;
		actor->DetachScene();
		
		std::lock_guard lk(mLocker);
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
				actor->mScene.FromObject(nullptr);
				return true;
			}
		}
		return false;
	}
	void NxScene::Simulate(const NxReal& elapsedTime)
	{
		std::lock_guard lk(mLocker);

		auto TimeStep = mDesc.TimeStep;
		auto numOfStep = (int)(elapsedTime / TimeStep);
		auto step = TimeStep;
		auto residue = NxReal::Mod(elapsedTime, TimeStep);
		
		std::vector<NxActorPair> pairs;
		CollectCollisionPairs(pairs);

		for (int i = 0; i < numOfStep; i++)
		{
			step = (i == numOfStep - 1) ? residue : TimeStep;
			if (step == NxReal::Zero())
				continue;

			for (auto& i : mActors)
			{
				i->TryStep(step);
				/*if (i->GetRtti() == GetClassObject<NxRigidBody>())
				{
					auto rbody = ((NxRigidBody*)i);
				}*/
			}

			for (auto& i : mConstraints)
			{
				i->SolveConstraint(this, step);
			}

			ProcessDistancePairs(pairs, step);

			for (auto& i : mActors)
			{
				i->FixStep(step);
			}
		}
	}
	void NxScene::CollectCollisionPairs(std::vector<NxActorPair>& pairs)
	{
		for (size_t i = 0; i < mActors.size(); i++)
		{
			const auto& lh = mActors[i];
			auto lhAABB = NxAABB::Transform(lh->mAABB, *lh->GetTransform());
			for (size_t j = i + 1; j < mActors.size(); j++)
			{
				const auto& rh = mActors[j];
				auto rhAABB = NxAABB::Transform(rh->mAABB, *rh->GetTransform());				
				if (NxAABB::Contains(lhAABB, rhAABB) == NxMath::EContainmentType::Disjoint)
				{
					continue;
				}
				pairs.push_back(std::make_pair(lh, rh));
			}
		}
	}
	void NxScene::ProcessDistancePairs(const std::vector<NxActorPair>& pairs, const NxReal& step)
	{
		NxDistanceJoint djt;
		for (const auto& i : pairs)
		{
			if (i.first->GetRtti() == GetClassObject<NxRigidBody>() &&
				i.second->GetRtti() == GetClassObject<NxRigidBody>())
			{
				djt.mActorPair = i;
				djt.mLimitMax = NxReal::Maximum();
				djt.mLimitMin = NxDistanceJoint::CalcLimitMin((NxRigidBody*)i.first.GetPtr(), (NxRigidBody*)i.second.GetPtr());
				djt.SolveConstraint(this, step);
			}
		}
	}
}

NS_END