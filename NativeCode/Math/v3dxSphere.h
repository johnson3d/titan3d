#ifndef __v3dxSphere__H__
#define __v3dxSphere__H__

#include "v3dxVector3.h"

#pragma pack(push,4)

/** A sphere primitive, mostly used for bounds checking. 
球的图元，用于绑定检测。
*/
class v3dxSphere
{
protected:
	float mRadius;
	v3dxVector3 mCenter;
public:
	bool IsEmpty()
	{
		return mRadius<0.f ? true : false;
	}
	/** Standard constructor*/
	v3dxSphere() : mRadius(-FLT_MAX), mCenter(v3dxVector3::ZERO) {}

	/** Constructor allowing arbitrary spheres. 
	*/
	v3dxSphere(const v3dxVector3& center, float radius): mRadius(radius), mCenter(center) {}

	/** Returns the radius of the sphere. 
	返回球的半径
	*/
	float getRadius(void) const { 
		return mRadius; 
	}

	/** Sets the radius of the sphere. 
	设置球的半径
	*/
	void setRadius(float radius) { 
		mRadius = radius; 
	}

	/** Returns the center point of the sphere. 
	返回球的中点
	*/
	const v3dxVector3& getCenter(void) const { 
		return mCenter; 
	}

	/** Sets the center point of the sphere. 
	设置球的中点
	*/
	void setCenter(const v3dxVector3& center) { 
		mCenter = center; 
	}

	vBOOL IsIn( const v3dxVector3 &pos )
	{
		v3dxVector3 rvec(pos - mCenter);
		float rSqr = rvec.getLength();
		return rSqr <= mRadius;
	}
};

#pragma pack(pop)

#endif