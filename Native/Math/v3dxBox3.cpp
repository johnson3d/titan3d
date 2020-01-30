/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxbox3.cpp
	Created Time:		30:6:2002   16:36
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#include "v3dxBox3.h"
#include "v3dxIntersectFunction.h"

#define new VNEW

using namespace TPL_HELP;

v3dxVector3 v3dxBox3::GetCorner (int corner) const
{
	switch(corner)
	{
	case BOX3_CORNER_xyz:
		return v3dxVector3(minbox.x,minbox.y,minbox.z);
		break;
	case BOX3_CORNER_xyZ:
		return v3dxVector3(minbox.x,minbox.y,maxbox.z);
		break;
	case BOX3_CORNER_xYz:
		return v3dxVector3(minbox.x,maxbox.y,minbox.z);
		break;
	case BOX3_CORNER_xYZ:
		return v3dxVector3(minbox.x,maxbox.y,maxbox.z);
		break;
	case BOX3_CORNER_Xyz:
		return v3dxVector3(maxbox.x,minbox.y,minbox.z);
		break;
	case BOX3_CORNER_XyZ:
		return v3dxVector3(maxbox.x,minbox.y,maxbox.z);
		break;
	case BOX3_CORNER_XYz:
		return v3dxVector3(maxbox.x,maxbox.y,minbox.z);
		break;
	case BOX3_CORNER_XYZ:
		return v3dxVector3(maxbox.x,maxbox.y,maxbox.z);
		break;
	}
	return v3dxVector3(0.0f,0.0f,0.0f);
}

bool v3dxBox3::AdjacentX (const v3dxBox3& other) const
{
	float tmp1=std::abs(other.MinX()-MaxX());
	float tmp2= std::abs(other.MaxX()-MinX());
	if((tmp1<SMALL_EPSILON)||(tmp2<SMALL_EPSILON))
	{
		if (MaxY () < other.MinY () || MinY () > other.MaxY ()) 
			return false;
		if (MaxZ () < other.MinZ () || MinZ () > other.MaxZ ()) 
			return false;
		return true;
	}
	return false;
}

bool v3dxBox3::AdjacentY (const v3dxBox3& other) const
{
	if (std::abs(other.MinY () - MaxY ()) < SMALL_EPSILON ||
		std::abs(other.MaxY () - MinY ()) < SMALL_EPSILON)
	{
		if (MaxX () < other.MinX () || MinX () > other.MaxX ()) 
			return false;
		if (MaxZ () < other.MinZ () || MinZ () > other.MaxZ ()) 
			return false;
		return true;
	}
	 return false;
}

bool v3dxBox3::AdjacentZ (const v3dxBox3& other) const
{
	if (std::abs(other.MinZ () - MaxZ ()) < SMALL_EPSILON ||
		std::abs(other.MaxZ () - MinZ ()) < SMALL_EPSILON)
	{
		if (MaxX () < other.MinX () || MinX () > other.MaxX ()) 
			return false;
		if (MaxY () < other.MinY () || MinY () > other.MaxY ()) 
			return false;
		return true;
	}
	return false;
}

int v3dxBox3::Adjacent (const v3dxBox3& other) const
{
	if (AdjacentX (other))
	{
		if (other.MaxX () > MaxX ()) 
			return BOX3_SIDE_X;
		else return BOX3_SIDE_x;
	}
	if (AdjacentY (other))
	{
		if (other.MaxY () > MaxY ()) 
			return BOX3_SIDE_Y;
		else return BOX3_SIDE_y;
	}
	if (AdjacentZ (other))
	{
		if (other.MaxZ () > MaxZ ()) 
			return BOX3_SIDE_Z;
		else return BOX3_SIDE_z;
	}
	return -1;
}

bool v3dxBox3::Between (const v3dxBox3& box1, const v3dxBox3& box2) const
{
	// 首先周游测试，坐标是否在里面
	if (((maxbox.x >= box1.minbox.x && minbox.x <= box2.maxbox.x) ||
       (maxbox.x >= box2.minbox.x && minbox.x <= box1.maxbox.x)) &&
      ((maxbox.y >= box1.minbox.y && minbox.y <= box2.maxbox.y) ||
       (maxbox.y >= box2.minbox.y && minbox.y <= box1.maxbox.y)) &&
      ((maxbox.z >= box1.minbox.z && minbox.z <= box2.maxbox.z) ||
       (maxbox.z >= box2.minbox.z && minbox.z <= box1.maxbox.z)))
	{
		return true;
	}
	return false;
}

void v3dxBox3::ManhattanDistance (const v3dxBox3& other, v3dxVector3& dist) const
{
	if (other.MinX () >= MaxX ()) dist.x = other.MinX () - MaxX ();
	else if (MinX () >= other.MaxX ()) dist.x = MinX () - other.MaxX ();
	else dist.x = 0;
	if (other.MinY () >= MaxY ()) dist.y = other.MinY () - MaxY ();
	else if (MinY () >= other.MaxY ()) dist.y = MinY () - other.MaxY ();
	else dist.y = 0;
	if (other.MinZ () >= MaxZ ()) dist.z = other.MinZ () - MaxZ ();
	else if (MinZ () >= other.MaxZ ()) dist.z = MinZ () - other.MaxZ ();
	else dist.z = 0;
}


float v3dxBox3::SquaredOriginDist() const
{
	float res=0;
	if (minbox.x > 0) 
		res += minbox.x*minbox.x;
	else if (maxbox.x < 0) 
		res += maxbox.x*maxbox.x;
	if (minbox.y > 0) 
		res += minbox.y*minbox.y;
	else if (maxbox.y < 0) res += maxbox.y*maxbox.y;
	if (minbox.z > 0) 
		res += minbox.z*minbox.z;
	else if (maxbox.z < 0) 
		res += maxbox.z*maxbox.z;;
	return res;
}

bool v3dxBox3::intersect(const v3dxSphere &sphere) const
{
	return ::intersect( *this, sphere );
}