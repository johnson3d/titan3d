#ifndef __v3dxCylinder__H__
#define __v3dxCylinder__H__

#include "v3dxVector3.h"
//class v3dxVector3;

class v3dxSphere;

#pragma pack(push,4)

class v3dxCylinder
{
public:
	v3dxCylinder( const v3dxVector3 &p, const v3dxVector3 &d, float r );
	v3dxCylinder( const v3dxCylinder &c );
	v3dxCylinder( );
	~v3dxCylinder();

	void   getAxis( v3dxVector3 &apos,  v3dxVector3 &adir ) const;
	float getRadius(void) const;

	virtual void   getCenter( v3dxVector3 &center ) const;
	virtual float getScalarVolume(void) const;
	virtual void   getBounds( v3dxVector3 &min, v3dxVector3 &max ) const;

	void setValue( const v3dxVector3 &p, const v3dxVector3 &d, float r );
	void setAxis( const v3dxVector3 &p, const v3dxVector3 &d );
	void setRadius( float r );

	 bool intersect(const v3dxCylinder &cylinder) const;
	 bool intersect( const v3dxSphere &sphere ) const;
	 bool intersect( const v3dxBox3 &box ) const;

private:
	v3dxVector3  m_axisPos;
	v3dxVector3  m_axisDir;
	float m_radius;

};


inline v3dxCylinder::v3dxCylinder( const v3dxVector3 &p, const v3dxVector3 &d, float r ) :
m_axisPos(p), 
m_axisDir(d), 
m_radius (r) 
{
}

inline v3dxCylinder::v3dxCylinder( const v3dxCylinder &c ) :
m_axisPos(c.m_axisPos), 
m_axisDir(c.m_axisDir), 
m_radius (c.m_radius )
{
}

inline v3dxCylinder::v3dxCylinder( )
{
}

inline v3dxCylinder::~v3dxCylinder()
{
}


inline void v3dxCylinder::getAxis( v3dxVector3 &apos,  v3dxVector3 &adir ) const
{
	adir = m_axisDir;
	apos = m_axisPos;
}


inline float v3dxCylinder::getRadius(void) const
{
	return m_radius;
}


inline void v3dxCylinder::getCenter(v3dxVector3 &center) const
{
	center = m_axisPos + m_axisDir * .5;
}


inline float v3dxCylinder::getScalarVolume(void) const
{
	//	return isEmpty() ? 0.0f : (m_radius * m_radius * Pi * m_axisDir.length());
	return true;
}

/*! gives the boundaries of the volume */

inline void v3dxCylinder::getBounds( v3dxVector3 &min, v3dxVector3 &max ) const
{
	// this is rather simpleminded, but good enough for now

	if(m_axisDir[0] < 0)
	{
		min[0] = m_axisPos[0] + m_axisDir[0] - m_radius;
		max[0] = m_axisPos[0] - m_axisDir[0] + m_radius;
	}
	else
	{
		min[0] = m_axisPos[0] - m_axisDir[0] - m_radius;
		max[0] = m_axisPos[0] + m_axisDir[0] + m_radius;
	}

	if(m_axisDir[1] < 0)
	{
		min[1] = m_axisPos[1];
		max[1] = m_axisDir[1];
	}
	else
	{
		min[1] = m_axisPos[1];
		max[1] = m_axisDir[1];
	}

	if(m_axisDir[2] < 0)
	{
		min[2] = m_axisPos[2] + m_axisDir[2] - m_radius;
		max[2] = m_axisPos[2] - m_axisDir[2] + m_radius;
	}
	else
	{
		min[2] = m_axisPos[2] - m_axisDir[2] - m_radius;
		max[2] = m_axisPos[2] + m_axisDir[2] + m_radius;
	}
}

inline void v3dxCylinder::setValue( const v3dxVector3 &p, const v3dxVector3 &d, float r )
{
	m_axisPos = p;
	m_axisDir = d;
	m_radius = r;
}


inline void v3dxCylinder::setAxis( const v3dxVector3 &p, const v3dxVector3 &d )
{
	m_axisPos = p;
	m_axisDir = d;
}


inline void v3dxCylinder::setRadius( float r )
{
	m_radius = r;
}

#pragma pack(pop)

#endif