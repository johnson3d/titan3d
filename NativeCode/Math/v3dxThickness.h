#pragma once
#include "vfxGeomTypes.h"

class v3dxThickness : public v3dVector4_t
{
public:
	v3dxThickness()
	{

	}
	v3dxThickness(float v)
	{
		left = v;
		right = v;
		top = v;
		bottom = v;
	}
	v3dxThickness(float ileft, float iright, float itop, float ibottom)
	{
		left = ileft;
		right = iright;
		top = itop;
		bottom = ibottom;
	}
};