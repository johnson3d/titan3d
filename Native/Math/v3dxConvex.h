#pragma once
#include "v3dxPlane3.h"

#pragma pack(push,4)

NS_BEGIN

class v3dxConvex
{
public:
	std::vector<v3dxPlane3>		Planes;

	vBOOL IsContain(const v3dxVector3* pos)
	{
		for (auto i : Planes)
		{
			if (i.witchSide(pos) > 0)
				return FALSE;
		}
		return TRUE;
	}
};

NS_END

#pragma pack(pop)
