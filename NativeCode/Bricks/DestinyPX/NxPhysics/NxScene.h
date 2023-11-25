#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	struct NxSceneDesc
	{

	};
	class NxScene : public NxEntity
	{
	public:
		NxReal TimeStep;
		NxPQ Transform;
		std::vector<NxAutoRef<NxActor>> mActors;
		std::vector<NxAutoRef<NxConstraint>> mConstraints;
		std::mutex mLocker;
	public:
		ENGINE_RTTI(NxScene);
		virtual const NxPQ* GetTransform() const
		{
			return &Transform;
		}
		virtual NxPQ* GetTransform()
		{
			return &Transform;
		}

		bool AddActor(NxActor* actor);
		bool RemoveActor(NxActor* actor);
		void Simulate(const NxReal& elapsedTime);
	private:
		
	};
}

NS_END