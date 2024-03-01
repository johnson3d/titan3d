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
#include "v3dxDBox3.h"
#include "v3dxIntersectFunction.h"

#define new VNEW



using namespace TPL_HELP;

v3dxDVector3 v3dxDBox3::GetCorner (int corner) const
{
	switch(corner)
	{
	case BOX3_CORNER_xyz:
		return v3dxDVector3(minbox.X,minbox.Y,minbox.Z);
		break;
	case BOX3_CORNER_xyZ:
		return v3dxDVector3(minbox.X,minbox.Y,maxbox.Z);
		break;
	case BOX3_CORNER_xYz:
		return v3dxDVector3(minbox.X,maxbox.Y,minbox.Z);
		break;
	case BOX3_CORNER_xYZ:
		return v3dxDVector3(minbox.X,maxbox.Y,maxbox.Z);
		break;
	case BOX3_CORNER_Xyz:
		return v3dxDVector3(maxbox.X,minbox.Y,minbox.Z);
		break;
	case BOX3_CORNER_XyZ:
		return v3dxDVector3(maxbox.X,minbox.Y,maxbox.Z);
		break;
	case BOX3_CORNER_XYz:
		return v3dxDVector3(maxbox.X,maxbox.Y,minbox.Z);
		break;
	case BOX3_CORNER_XYZ:
		return v3dxDVector3(maxbox.X,maxbox.Y,maxbox.Z);
		break;
	}
	return v3dxDVector3(0.0f,0.0f,0.0f);
}

bool v3dxDBox3::AdjacentX (const v3dxDBox3& other) const
{
	double tmp1=std::abs(other.MinX()-MaxX());
	double tmp2= std::abs(other.MaxX()-MinX());
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

bool v3dxDBox3::AdjacentY (const v3dxDBox3& other) const
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

bool v3dxDBox3::AdjacentZ (const v3dxDBox3& other) const
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

int v3dxDBox3::Adjacent (const v3dxDBox3& other) const
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

bool v3dxDBox3::Between (const v3dxDBox3& box1, const v3dxDBox3& box2) const
{
	if (((maxbox.X >= box1.minbox.X && minbox.X <= box2.maxbox.X) ||
       (maxbox.X >= box2.minbox.X && minbox.X <= box1.maxbox.X)) &&
      ((maxbox.Y >= box1.minbox.Y && minbox.Y <= box2.maxbox.Y) ||
       (maxbox.Y >= box2.minbox.Y && minbox.Y <= box1.maxbox.Y)) &&
      ((maxbox.Z >= box1.minbox.Z && minbox.Z <= box2.maxbox.Z) ||
       (maxbox.Z >= box2.minbox.Z && minbox.Z <= box1.maxbox.Z)))
	{
		return true;
	}
	return false;
}

void v3dxDBox3::ManhattanDistance (const v3dxDBox3& other, v3dxDVector3& dist) const
{
	if (other.MinX () >= MaxX ()) dist.X = other.MinX () - MaxX ();
	else if (MinX () >= other.MaxX ()) dist.X = MinX () - other.MaxX ();
	else dist.X = 0;
	if (other.MinY () >= MaxY ()) dist.Y = other.MinY () - MaxY ();
	else if (MinY () >= other.MaxY ()) dist.Y = MinY () - other.MaxY ();
	else dist.Y = 0;
	if (other.MinZ () >= MaxZ ()) dist.Z = other.MinZ () - MaxZ ();
	else if (MinZ () >= other.MaxZ ()) dist.Z = MinZ () - other.MaxZ ();
	else dist.Z = 0;
}


double v3dxDBox3::SquaredOriginDist() const
{
	double res=0;
	if (minbox.X > 0) 
		res += minbox.X*minbox.X;
	else if (maxbox.X < 0) 
		res += maxbox.X*maxbox.X;
	if (minbox.Y > 0) 
		res += minbox.Y*minbox.Y;
	else if (maxbox.Y < 0) res += maxbox.Y*maxbox.Y;
	if (minbox.Z > 0) 
		res += minbox.Z*minbox.Z;
	else if (maxbox.Z < 0) 
		res += maxbox.Z*maxbox.Z;;
	return res;
}

//bool v3dxDBox3::intersect(const v3dxSphere &sphere) const
//{
//	return ::intersect( *this, sphere );
//}