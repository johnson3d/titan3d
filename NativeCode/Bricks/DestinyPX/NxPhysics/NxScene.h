#pragma once
#include "NxPreHead.h"

namespace NxPhysics
{
	struct NxSceneDesc
	{

	};
	class NxScene : public NxBase
	{
	public:
		std::vector<NxAutoRef<NxActor>> mActors;
	public:
		void Simulate(const NxReal& elapsedTime);
	};
}


