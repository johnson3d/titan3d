#pragma once

#include <vector>
#include <stdlib.h>
#include "../../Math/v3dxBox3.h"

class SurfaceAreaHeuristic
{
public:
	static std::vector<std::vector<uint32_t>> generateBatches(const std::vector<v3dxBox3>& aabbs, UINT targetSize, UINT splitGranularity);
};