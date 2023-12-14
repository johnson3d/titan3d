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
		
		for (int i = 0; i < numOfStep; i++)
		{
			step = (i == numOfStep - 1) ? residue : TimeStep;
			if (step == NxReal::Zero())
				continue;

			Forward(step);

			SolveJoints(step);

			CollectRBContacts();

			SolveRBContacts(step);

			SolvePosition(step);

			SolveVelocity(step);
		}
	}
	void NxScene::Forward(const NxReal& step)
	{
		for (auto& j : mActors)
		{
			j->TryStep(step);
		}
	}
	void NxScene::SolveJoints(const NxReal& step)
	{
		for (auto& j : mJoints)
		{
			j->SolveConstraint(this, step);
		}
	}
	void NxScene::SolvePosition(const NxReal& step)
	{
		for (auto& j : mActors)
		{
			j->FixStep(step);
		}
	}
	void NxScene::CollectRBContacts()
	{
		mRbContacts.clear();
		for (size_t i = 0; i < mActors.size(); i++)
		{
			const auto& lh = As<NxRigidBody>(mActors[i]);
			if (lh == nullptr)
				continue;
			auto lhAABB = NxAABB::Transform(lh->mAABB, *lh->GetTryTransform());
			for (size_t j = i + 1; j < mActors.size(); j++)
			{
				const auto& rh = As<NxRigidBody>(mActors[j]);
				if (rh == nullptr)
					continue;
				auto rhAABB = NxAABB::Transform(rh->mAABB, *rh->GetTryTransform());				
				if (NxAABB::Contains(lhAABB, rhAABB) == NxMath::EContainmentType::Disjoint)
				{
					continue;
				}
				auto contact = MakeShared(new NxContactConstraint());
				contact->mActorPair = std::make_pair(lh, rh);
				mRbContacts.push_back(contact);
			}
		}
	}
	void NxScene::SolveRBContacts(const NxReal& step)
	{
		for (const auto& i : mRbContacts)
		{
			if (i->mActorPair.first->GetRtti() == GetClassObject<NxRigidBody>() &&
				i->mActorPair.second->GetRtti() == GetClassObject<NxRigidBody>())
			{
				auto actor0 = (NxRigidBody*)i->mActorPair.first.GetPtr();
				auto actor1 = (NxRigidBody*)i->mActorPair.second.GetPtr();
				i->ResetRagrange();
				i->mLimitMax = NxReal::Maximum();
				i->mLimitMin = NxContactConstraint::CalcLimitMin(actor0, actor1);
				i->mCompliance = actor0->mDesc.Compliance + actor1->mDesc.Compliance;
				i->SolveConstraint(this, step);
			}
		}
	}
	void NxScene::SolveVelocity(const NxReal& step)
	{
		for (const auto& i : mRbContacts)
		{
			auto force = i->GetRagrange() * NxReal::F_2_0() / (step);
			auto acc = force * i->mActorPair.first->mDesc.InvMass;

			auto v = i->GetGradient() * acc;
			i->mActorPair.first->mVelocity += v;

			acc = force * i->mActorPair.second->mDesc.InvMass;
			v = i->GetGradient() * acc;
			i->mActorPair.second->mVelocity -= v;
		}
	}
}

NS_END