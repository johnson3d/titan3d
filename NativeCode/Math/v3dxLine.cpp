#include "v3dxVector3.h"
#include "v3dxLine.h"
#include "v3dxBox3.h"
#include "v3dxSphere.h"
#include "v3dxCylinder.h"
#include "v3dxPlane3.h"

#define new VNEW

bool v3dxLine3::intersect( const v3dxBox3 &box, float &enter, float &exit ) const
{
	if( box.IsEmpty() )
		return false;

	v3dxVector3 low = box.Min();
	v3dxVector3 high = box.Max();

	float r;
	float te;
	float tl;

	float in  = 0.f;
	float out = Inf;
	
	float xDir = m_direct[0];
	float xPos = m_point[0];

	if(xDir > Eps || xDir < -Eps)
	{
		r = 1.f / xDir;

		if(xDir > 0.f)
		{
			te = (low [0] - xPos) * r;
			tl = (high[0] - xPos) * r;
		}
		else
		{
			te = (high[0] - xPos) * r;
			tl = (low [0] - xPos) * r;
		}

		// check for flat boxes, count them as intersected
		if(tl-te < Eps)        
			return true;

		//        if (te > 1)   return false;

		if(tl < out)   
			out = tl;

		if(te > in)    
			in  = te;
	}
	else if(xPos < low[0] || xPos > high[0])
	{
		return false;
	}


	float yDir = m_direct[1];
	float yPos = m_point[1];
	if(yDir > Eps || yDir < -Eps)
	{
		r = 1.f / yDir;

		if(yDir > 0.f)
		{
			te = (low [1] - yPos) * r;
			tl = (high[1] - yPos) * r;
		}
		else
		{
			te = (high[1] - yPos) * r;
			tl = (low [1] - yPos) * r;
		}

		// check for flat boxes, count them as intersected
		if(tl-te < Eps)        
			return true;

		//        if (te > 1)   return false;

		if(tl < out)   
			out = tl;

		if(te > in)    
			in  = te;
	}
	else if(yPos < low[1] || yPos > high[1])
	{
		return false;
	}


	float zDir = m_direct[2];
	float zPos = m_point[2];
	if(zDir > Eps || zDir < -Eps)
	{
		r = 1.f / zDir;

		if(zDir > 0.f)
		{
			te = (low [2] - zPos) * r;
			tl = (high[2] - zPos) * r;
		}
		else
		{
			te = (high[2] - zPos) * r;
			tl = (low [2] - zPos) * r;
		}

		// check for flat boxes, count them as intersected
		if(tl-te < Eps)        
			return true;

		//        if (te > 1)   return false;

		if(tl < out)   
			out = tl;

		if(te > in)    
			in  = te;
	}
	else if(zPos < low[2] || zPos > high[2])
	{
		return false;
	}

	enter = in;
	exit  = out;

	if(enter > exit)
		return false;

	return true;

}


bool v3dxLine3::intersect( const v3dxSphere &sphere, float &enter, float &exit ) const
{

	v3dxVector3 v;
	v3dxVector3 center = sphere.getCenter();

	float radius = sphere.getRadius();
	float h;
	float b;
	float d;
	float t1;
	float t2;

	v = center - m_point;

	h = (v.dotProduct(v))-radius;
	b = (v.dotProduct(m_point));

	if(h >= 0 && b <= 0)
		return false;

	d = b * b - h;

	if(d < 0)
		return false;

	d  = sqrtf(d);
	t1 = b - d;

	t2 = b + d;

	if( t1 < Eps )
	{
		if( t2 < Eps /*|| t2 > 1*/)
		{
			return false;
		}
	}

	enter = t1;
	exit  = t2;

	return true;
	
}

bool v3dxLine3::intersect( const v3dxCylinder &cyl, float &enter, float &exit ) const
{

	float radius = cyl.getRadius();

	v3dxVector3 adir;
	v3dxVector3 o_adir;
	v3dxVector3 apos;

	cyl.getAxis(apos, adir);

	o_adir = adir;
	adir = adir - apos;
	adir.normalize();

	bool isect;


	float ln;
	float dl;
	v3dxVector3  RC;
	v3dxVector3  n;
	v3dxVector3  D;

	RC = m_point - apos;

	// 与圆柱轴向量做叉乘
	v3dxVec3Cross( &n, &m_direct, &adir );
	ln =  n.getLength();
	
	// 如果与圆柱轴向量平行
	if(ln == 0)
	{
		D  = RC - (RC.dotProduct(adir)) * adir;
		dl = D.getLength();

		if(dl <= radius)   // line lies in cylinder
		{
			enter = 0;
			exit  = Inf;
		}
		else
		{
			return false;
		}
	}
	else
	{
		n.normalize();

		dl    = Math::Abs(RC.dotProduct(n));        //shortest distance
		isect = (dl <= radius);

		if(isect)
		{                 // if ray hits cylinder
			float t;
			float s;
			v3dxVector3  O;

			v3dxVec3Cross( &O, &RC, &adir );
			t = - (O.dotProduct(n)) / ln;
			v3dxVec3Cross( &O, &n, &adir );

			O.normalize();

			s = Math::Abs(
				(Math::Sqrt((radius * radius) - (dl * dl))) / (m_direct.dotProduct(O)));

			exit = t + s;

			if(exit < 0)
				return false;

			enter = t - s;

			if(enter < 0)
				enter = 0;
		}
		else
		{
			return false;
		}
	}

	float t;

	v3dxPlane3 bottom(-adir, apos);

	if(bottom.intersect(*this, t))
	{
		if(bottom.isInHalfSpace(m_point))
		{
			if(t > enter) 
				enter = t;
		}
		else
		{
			if(t < exit) 
				exit = t;
		}
	}

	v3dxPlane3 top(adir, apos + o_adir);

	if(top.intersect(*this, t))
	{
		if(top.isInHalfSpace(m_point))
		{
			if(t > enter)
				enter = t;
		}
		else
		{
			if(t < exit)
				exit = t;
		}
	}

	return (enter < exit);
}
