#include "v3dxColor4.h"

#define new VNEW

const FColor FColor::Zero = FColor(0, 0, 0, 0);
const FColor FColor::Black = FColor(0, 0, 0, 255);
const FColor FColor::Red = FColor(255, 0, 0, 255);
const FColor FColor::Green = FColor(0, 255, 0, 255);
const FColor FColor::Blue = FColor(0, 0, 255, 255);
const FColor FColor::Yellow = FColor(255, 255, 0, 255);
const FColor FColor::Magenta = FColor(255, 0, 255, 255);
const FColor FColor::Cyan = FColor(0, 255, 255, 255);
const FColor FColor::White = FColor(255, 255, 255, 255);
const FColor FColor::LtGrey = FColor(191, 191, 191, 255);
const FColor FColor::MdGrey = FColor(127, 127, 127, 255);
const FColor FColor::DkGrey = FColor(63, 63, 63, 255);

const FColor FColor::ColorTable[]= {
	Black ,	
	Red ,	
	Green ,	
	Yellow ,	
	Blue ,	
	Cyan ,	
	Magenta ,	
	White ,	
};

//VBaseStringA Rgba::toString() const
//{
//	VBaseStringA result;
//
//	StringUtil::sprintf(result, "%d %d %d %d", r,g,b,a);
//
//	return result;
//}
//
//void Rgba::fromString(const char *str)
//{
//	int _r, _g, _b, _a;
//	int v = sscanf(str, "%d %d %d %d", &_r, &_g, &_b, &_a);
//	AX_ASSERT(v = 4);
//	r = _r; g = _g; b = _b; a = _a;
//}

