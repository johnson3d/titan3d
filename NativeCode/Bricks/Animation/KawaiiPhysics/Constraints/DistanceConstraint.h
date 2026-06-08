#pragma once
#include "../KawaiiTypes.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// XPBD distance constraint (ref: Macklin et al. 2016 "XPBD").
	// Uses flat int32 indices to reference the particle array;
	// supports independent compliance for stretch and shrink directions.
	struct FDistanceConstraint
	{
		int32_t IndexA = -1;
		int32_t IndexB = -1;
		float RestLength = 0.0f;
		float StretchCompliance = 0.0f;
		float ShrinkCompliance = 0.0f;
		float Lambda = 0.0f;
		bool bCollision = false;
	};

} // namespace KawaiiPhysics

NS_END
