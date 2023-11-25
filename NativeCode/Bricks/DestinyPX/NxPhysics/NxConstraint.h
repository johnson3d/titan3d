#pragma once

#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	class NxConstraint : public NxEntity
	{
	public:
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) = 0;
	};
}

NS_END