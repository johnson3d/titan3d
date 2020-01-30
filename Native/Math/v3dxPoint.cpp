#include "v3dxPoint.h"
#include<algorithm>
#include <algorithm>
#undef max
#undef min

#define new VNEW

// Simple functions.
inline int v3dxPoint::getMax() const
{
	return std::max(x,y);
}

inline int v3dxPoint::getAbsMax() const
{
	return std::max(abs(x), abs(y));
}

inline int v3dxPoint::getMin() const
{
	return std::min(x,y);
}

//VBaseStringA v3dxPoint::toString() const {
//	VBaseStringA result;
//	StringUtil::sprintf(result, "%d %d", x, y);
//	return result;
//}
//
//bool v3dxPoint::fromString(const char *str) {
//	int v = sscanf(str, "%d %d", &x, &y);
//	AX_ASSERT(v == 2);
//	return v == 2;
//}
