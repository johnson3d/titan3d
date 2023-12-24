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

			//显式欧拉积分
			Forward(step);

			//处理约束
			SolveJoints(step);
			CollectRBContacts();
			SolveRBContacts(step);

			//更新最终状态
			UpdatePosition(step);
			UpdateVelocity(step);
		}
	}
	void NxScene::Forward(const NxReal& step)
	{
		for (auto& j : mActors)
		{
			j->Forward(step);
		}
	}
	void NxScene::SolveJoints(const NxReal& step)
	{
		for (auto& j : mJoints)
		{
			j->SolveConstraint(this, step);
		}
	}
	void NxScene::UpdatePosition(const NxReal& step)
	{
		for (auto& j : mActors)
		{
			j->UpdateStep(step);
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
			const auto& lhAABB = lh->mWorldAABB;
			for (size_t j = i + 1; j < mActors.size(); j++)
			{
				const auto& rh = As<NxRigidBody>(mActors[j]);
				if (rh == nullptr)
					continue;
				const auto& rhAABB = rh->mWorldAABB;
				if (NxAABB::Contains(lhAABB, rhAABB) == NxMath::EContainmentType::Disjoint)
				{
					continue;
				}
				for (const auto& i : lh->mShapes)
				{
					for (const auto& j : rh->mShapes)
					{
						NxReal distance;
						NxVector3 dir;
						if (NxShape::Contact(i, j, distance, dir))
						{
							auto contact = MakeShared(new NxContactConstraint());
							contact->mShapePair = std::make_pair(i, j);
							contact->mContactDirection = dir;
							contact->BuildConstraint();
							mRbContacts.push_back(contact);
						}
					}
				}
			}
		}
	}
	void NxScene::SolveRBContacts(const NxReal& step)
	{
		for (const auto& i : mRbContacts)
		{
			i->ResetRagrange();
			i->SolveConstraint(this, step);
		}
	}
	void NxScene::UpdateVelocity(const NxReal& step)
	{
		for (const auto& i : mRbContacts)
		{
			auto force = i->GetRagrange() * NxReal::F_2_0() / (step);
			auto acc = force * i->GetRigidBody1()->mDesc.InvMass;

			auto v = i->mContactDirection * acc;
			i->GetRigidBody1()->mVelocity += v;

			acc = force * i->GetRigidBody2()->mDesc.InvMass;
			v = i->mContactDirection * acc;
			i->GetRigidBody2()->mVelocity -= v;
		}
	}
}

NS_END