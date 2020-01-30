#include "IntegerType.h"

v3dUInt32_4 v3dUInt32_4::GetVar(int x, int y, int z, int w)
{
	v3dUInt32_4 result;
	result.x = x;
	result.y = y;
	result.z = z;
	result.w = w;
	return result;
}
v3dUInt32_4 v3dUInt32_4::Zero = GetVar(0,0,0,0);