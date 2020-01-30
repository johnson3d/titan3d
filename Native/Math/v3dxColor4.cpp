#include "v3dxColor4.h"

#define new VNEW

const Rgba Rgba::Zero = Rgba(0, 0, 0, 0);
const Rgba Rgba::Black = Rgba(0, 0, 0, 255);
const Rgba Rgba::Red = Rgba(255, 0, 0, 255);
const Rgba Rgba::Green = Rgba(0, 255, 0, 255);
const Rgba Rgba::Blue = Rgba(0, 0, 255, 255);
const Rgba Rgba::Yellow = Rgba(255, 255, 0, 255);
const Rgba Rgba::Magenta = Rgba(255, 0, 255, 255);
const Rgba Rgba::Cyan = Rgba(0, 255, 255, 255);
const Rgba Rgba::White = Rgba(255, 255, 255, 255);
const Rgba Rgba::LtGrey = Rgba(191, 191, 191, 255);
const Rgba Rgba::MdGrey = Rgba(127, 127, 127, 255);
const Rgba Rgba::DkGrey = Rgba(63, 63, 63, 255);

const Rgba Rgba::ColorTable[]= {
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

