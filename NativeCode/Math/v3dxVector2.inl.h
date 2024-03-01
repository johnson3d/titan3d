#include "v3dxMath.h"
#include "v3dxVector3.h"

inline v3dxVector2::v3dxVector2()
{
	X=Y=0.0f;
}

inline v3dxVector2::v3dxVector2(const v3dxVector3& v)
{
	X = v.X; Y = v.Y;
}

inline v3dxVector2::v3dxVector2(float ix,float iy)
{ 
	X=ix;Y=iy; 
}

inline void v3dxVector2::setValue(float ix,float iy)
{ 
	X=ix;
	Y=iy; 
}

inline float v3dxVector2::getLength() const{ 
	return sqrtf(X*X+Y*Y);  
}

inline float v3dxVector2::getLengthSq() const{ 
	return X*X + Y*Y; 
}

inline void v3dxVector2::normalize(){ 
	float fLen=getLength();
	if(fLen>0){
		X=X/fLen;
		Y=Y/fLen;
	} 
}

inline v3dxVector2 v3dxVector2::GetNormalize() const {
	v3dxVector2 result;
	float fLen = getLength();
	if (fLen > 0) {
		result.X = X / fLen;
		result.Y = Y / fLen;
	}
	return result;
}


inline void v3dxVector2::rotate (float angle)
{
	float s = sinf(angle);
	float c = cosf(angle);
	float nx = X * c + Y * s;
	Y = -X * s + Y * c;
	X = nx;
}
