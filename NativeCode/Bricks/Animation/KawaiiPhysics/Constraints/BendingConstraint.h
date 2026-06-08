#pragma once
#include "../KawaiiTypes.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// XPBD centroid-based bending constraint for 1D chain/rope structures
	// (ref: Kelager 2010).
	// Bending constraint for the triplet (P1, Pivot, P2):
	//   bendVector = Pivot - centroid(P1, P2, Pivot)
	//   C = |bendVector| - RestLength
	struct FBendingConstraint
	{
		int32_t IndexP1 = -1;
		int32_t IndexPivot = -1;
		int32_t IndexP2 = -1;
		float RestLength = 0.0f;
		float Compliance = 0.0f;
		float MaxBending = 0.0f;
		float Lambda = 0.0f;
	};

} // namespace KawaiiPhysics

NS_END
