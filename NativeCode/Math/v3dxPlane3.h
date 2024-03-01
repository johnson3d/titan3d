/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxplane3.h
	Created Time:		30:6:2002   16:31
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#ifndef __V3DXPLANE3__H__
#define __V3DXPLANE3__H__

#include "vfxGeomTypes.h"
#include "v3dxMath.h"
#include "v3dxVector3.h"

class v3dxLine3;

#pragma pack(push,4)
class v3dxPlane3
{
public:
	v3dxVector3 m_vNormal;
	float m_fDD;
	v3dxPlane3 () : m_vNormal(0,0,1), m_fDD(0) {}
	v3dxPlane3 (const v3dVector3_t & plane_norm, float d=0) : m_vNormal(plane_norm.X,plane_norm.Y,plane_norm.Z), m_fDD(d) {}
	v3dxPlane3 (float a, float b, float c, float d=0) : m_vNormal(a,b,c), m_fDD(d) {}
	v3dxPlane3 (const v3dVector3_t & v1, const v3dVector3_t & v2, const v3dVector3_t & v3)
	{
		v3dxCalcNormal(&m_vNormal, &v1, &v2, &v3,TRUE);
		m_fDD = - v3dxVec3Dot(&m_vNormal,&v1);
	}
	v3dxPlane3(const v3dVector3_t & normal, const v3dxVector3 &point)
	{
		m_vNormal = normal;
		m_vNormal.normalize();

		m_fDD = - m_vNormal.dotProduct(point);
	}

	inline v3dxVector3 & normal () { return m_vNormal; }
	inline const v3dxVector3& getNormal () const { return m_vNormal; }

	inline float A () const {
		return m_vNormal.X; 
	}
	inline float B () const { 
		return m_vNormal.Y; 
	}
	inline float C () const { 
		return m_vNormal.Z; 
	}
	inline float D () const { 
		return m_fDD; 
	}

	inline float& A () { 
		return m_vNormal.X; 
	}
	inline float& B () { 
		return m_vNormal.Y; 
	}
	inline float& C () {
		return m_vNormal.Z; 
	}
  	inline float& D () { 
		return m_fDD; 
	}

	inline void set (float a, float b, float c, float d){ 
		m_vNormal.X = a; m_vNormal.Y = b; m_vNormal.Z = c; m_fDD = d; 
	}

  	inline void set (const v3dVector3_t & normal, float d){ 
		m_vNormal = normal; m_fDD = d; 
	}
	inline void set (const v3dVector3_t* pvNorm,const v3dVector3_t* pvPt){
		v3dxVec3Normalize( &m_vNormal, pvNorm );
		m_fDD = -v3dxVec3Dot( pvPt , &m_vNormal );
	}

  	void set (const v3dVector3_t & v1, const v3dVector3_t & v2, const v3dVector3_t & v3)
	{
		v3dxCalcNormal(&m_vNormal, &v1, &v2, &v3,TRUE);
		m_fDD = - v3dxVec3Dot(&m_vNormal,&v1);
	}	
	inline float classify (const v3dVector3_t& pt) const { 
		return v3dxVec3Dot(&m_vNormal,&pt) + m_fDD; 
	}
	static float classify (float A, float B, float C, float D,
                         const v3dVector3_t & pt){ 
		return A*pt.X + B*pt.Y + C*pt.Z + D; 
	}

	inline int witchSide(const v3dVector3_t* pvPos) const{
		return v3dxVec3Dot( &m_vNormal , pvPos ) + m_fDD > EPSILON; 
	}

  	inline float distance (const v3dVector3_t & pt) const{
		return classify (pt); 
	}
 	void Invert() { 
		m_vNormal = -m_vNormal;  m_fDD = -m_fDD; 
	}
	void MirrorPlane(){
		m_vNormal = -m_vNormal;
	}
  	void normalize(){
    	float f = m_vNormal.getLength();
    	if (f) { m_vNormal /= f;  m_fDD /= f; }
  	}

	bool isInHalfSpace(const v3dVector3_t &point) const
	{
		float scalar = v3dxVec3Dot(&m_vNormal,&point) - m_fDD;

		return scalar <= -EPSILON ? true : false;
	}

	float calculateX( float y, float z ){
		return -(m_fDD + B()*y + C()*z) / A();
	}
	float calculateY( float x, float z ){
		return -(m_fDD + A()*x + C()*z) / B();
	}
	float calculateZ( float x, float y ){
		return -(m_fDD + A()*x + B()*y) / C();
	}

	 bool intersect(const v3dxLine3 &line, float &t) const;

	 bool intersectInfinite(const v3dxLine3 &line, float &t) const;

	 int intersectSegement(const v3dxVector3& start, const v3dxVector3& end, float &t) const;

	 inline static void Transform(v3dxPlane3& result, const v3dxPlane3& plane, const v3dxMatrix4& transformationInverse)
	 {
		 v3dxPlaneTransform(&result, &plane, &transformationInverse);
	 }
};
#pragma pack(pop)

class v3dxDPlane3
{
public:
	v3dDVector3_t m_vNormal;
	double m_fDD;
};

#endif