#ifndef	_SOFT_RASTER_FRASTER_H_
#define _SOFT_RASTER_FRASTER_H_

#include "../../Inc/BaseStructure/Box2.cginc"

struct FSRTriangle
{
	TtBox2i BindingBox;
	float RcpSlope;
};

#endif//_SOFT_RASTER_FRASTER_H_