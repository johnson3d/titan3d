#pragma once

#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	class NxConstraint : public NxEntity
	{
	public:
		ENGINE_RTTI(NxConstraint);
		virtual const NxPQ* GetTransform() const override {
			return nullptr;
		}
		virtual NxPQ* GetTransform() override {
			return nullptr;
		}
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) = 0;
	};
}

NS_END