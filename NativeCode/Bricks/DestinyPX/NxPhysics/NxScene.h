#pragma once
#include "NxActor.h"
#include "NxJoint.h"

NS_BEGIN

namespace NxPhysics
{
	struct TR_CLASS(SV_LayoutStruct = 8)
		NxSceneDesc
	{
		NxReal TimeStep;
	};
	class TR_CLASS()
		NxScene : public NxEntity
	{
	public:
		NxSceneDesc mDesc;
		NxPQ Transform;
		std::vector<NxAutoRef<NxActor>> mActors;
		std::vector<NxAutoRef<NxJoint>> mJoints;
		std::mutex mLocker;
	protected:
		std::vector<NxAutoRef<NxContactConstraint>> mRbContacts;
	public:
		ENGINE_RTTI(NxScene);
		TR_FUNCTION(SV_NoBind)
		virtual const NxPQ* GetTransform() const
		{
			return &Transform;
		}
		virtual NxPQ* GetTransform()
		{
			return &Transform;
		}
		bool Init(const NxSceneDesc& desc);
		bool AddActor(NxActor* actor);
		bool RemoveActor(NxActor* actor);
		void Simulate(const NxReal& elapsedTime);
		void LockScene()
		{
			mLocker.lock();
		}
		UINT GetNumOfActors() const {
			return (UINT)mActors.size();
		}
		NxActor* GetActor(UINT index) const {
			return mActors[index];
		}
		NxActor** UnsafeGetActorList() const {
			return (NxActor**)mActors.data();
		}
		void UnlockScene()
		{
			mLocker.unlock();
		}
	private:
		void Forward(const NxReal& step);
		void SolveJoints(const NxReal& step);
		void CollectRBContacts();
		void SolveRBContacts(const NxReal& step);
		void UpdatePosition(const NxReal& step);
		void UpdateVelocity(const NxReal& step);
	};
}

NS_END