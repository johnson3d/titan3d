#include "v3dxMath.h"

inline v3dxVector2::v3dxVector2()
{
	x=y=0.0f;
}

inline v3dxVector2::v3dxVector2(float ix,float iy)
{ 
	x=ix;y=iy; 
}

inline void v3dxVector2::setValue(float ix,float iy)
{ 
	x=ix;
	y=iy; 
}

inline float v3dxVector2::getLength() const{ 
	return sqrtf(x*x+y*y);  
}

inline float v3dxVector2::getLengthSq() const{ 
	return x*x + y*y; 
}

inline void v3dxVector2::normalize(){ 
	float fLen=getLength();
	if(fLen>0){
		x=x/fLen;
		y=y/fLen;
	} 
}


inline void v3dxVector2::rotate (float angle)
{
	float s = sinf(angle);
	float c = cosf(angle);
	float nx = x * c + y * s;
	y = -x * s + y * c;
	x = nx;
}
