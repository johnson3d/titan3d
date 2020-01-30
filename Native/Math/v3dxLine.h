#ifndef __v3dxLine__H__
#define __v3dxLine__H__

class v3dxVector3;
class v3dxBox3;
class v3dxSphere;
class v3dxCylinder;

const float Eps = 1E-5f;
const float Inf = 1E10;

#pragma pack(push,4)

class v3dxLine3
{
public:
	v3dxLine3();
	v3dxLine3( const v3dxVector3 &point,  const v3dxVector3 &direct );

	void setPoint( float ix,float iy,float iz );
	void setPoint( const v3dxVector3 &point ); 
	v3dxVector3 *getPoint();

	void setDirect( float ix,float iy,float iz );
	void setDirect( const v3dxVector3 &direct ); 
	const v3dxVector3 &getDirection() const;
	const v3dxVector3 &getPosition() const;

	 bool intersect( const v3dxBox3 &box, float &enter, float &exit ) const;
	 bool intersect( const v3dxSphere &sphere, float &enter, float &exit ) const;
	 bool intersect( const v3dxCylinder &cyl, float &enter, float &exit ) const;

	inline float getDistSq(const v3dxVector3 &pos){// added by Jones
		v3dxVector3 v = pos - m_point;
		return (v - (m_direct * (v * m_direct))).getLengthSq();
	}
	inline void FromPoint2(const v3dxVector3 & p1,const v3dxVector3 & p2){
		m_point = p1;
		m_direct = p2 - p1;
	}
	inline void FromPointDir(const v3dxVector3 & p,const v3dxVector3 & d){
		m_point = p;
		m_direct = d;
	}

	v3dxVector3 m_point;
	v3dxVector3 m_direct;
};

inline v3dxLine3::v3dxLine3() : 
m_point( 0.f, 0.f, 0.f ),
m_direct( 0.f, 0.f, 0.f )
{
}

inline v3dxLine3::v3dxLine3( const v3dxVector3 &point,  const v3dxVector3 &direct ) :
m_point(point),
m_direct(direct)
{
}

inline void v3dxLine3::setPoint( float x,float y,float z )
{
	m_point.setValue( x, y, z );
}

inline void v3dxLine3::setPoint( const v3dxVector3 &point )
{
	m_point = point;
}

inline v3dxVector3 *v3dxLine3::getPoint()
{
	return &m_point;
}

inline void v3dxLine3::setDirect( float x,float y,float z )
{
	m_direct.setValue( x, y, z );
}

inline void v3dxLine3::setDirect( const v3dxVector3 &direct )
{
	m_direct = direct;
}

inline const v3dxVector3 &v3dxLine3::getDirection() const
{
	return m_direct;
}

inline const v3dxVector3& v3dxLine3::getPosition() const
{
	return m_point;
}

#pragma pack(pop)

#endif
