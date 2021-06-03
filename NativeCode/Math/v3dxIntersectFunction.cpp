#include "v3dxIntersectFunction.h"

#include "v3dxCylinder.h"
#include "v3dxVector3.h"
#include "v3dxSphere.h"
#include "v3dxBox3.h"

#define new VNEW

 bool intersect(const v3dxCylinder &cylinder1, const v3dxCylinder &cylinder2)
{
	v3dxVector3   adir1, adir2, n, p;
	v3dxVector3   apos1, apos2;
	double  d;
	bool    retCode = false;

	cylinder1.getAxis(apos1, adir1);
	cylinder2.getAxis(apos2, adir2);

	//get the shortest distance between the two axes of the cylinders

	v3dxVec3Cross(&n, &adir1, &adir2);
	n.normalize();

	p = apos1 - apos2;
	d = fabs(n.dotProduct(p));


	if (d <= cylinder1.getRadius() + cylinder2.getRadius())
	{
		// the distance is smaller than the sum of the 2 radiuses
		retCode = true;
	}

	return retCode;

}




bool intersect(const v3dxCylinder &cylinder, const v3dxSphere   &sphere)
{
	bool  retCode;
	v3dxVector3 apos;
	v3dxVector3 adir;

	cylinder.getAxis(apos, adir);


	float  d = 0.f, s1 = 0.f, s2 = 0.f;
	v3dxVector3   c;
	v3dxVector3   u, u1, u2;

	//get the distance between the upper and lower point of the cylinder
	// and the sphere center

	s1 = (apos - sphere.getCenter()).getLength();
	s2 = (apos + adir - sphere.getCenter()).getLength();

	//check the smallest distance and set the vector coordinate
	if (s1 <= s2)
	{
		d = s1;
		c = apos;
	}
	else
	{
		d = s2;
		c = apos + adir;
	}

	// decompose the vector in u1 and u2 which are parallel and 
	// perpendicular to the cylinder axis respectively

	u = ((d - sphere.getRadius()) / d) * (c - sphere.getCenter());

	u1 = (u[0] * adir[0] + u[1] * adir[1] + u[2] * adir[2]) /
		(adir.getLength() * adir.getLength()) * adir;
	u2 = u - u1;

	if (u2.getLength() <= 10e-6)
	{
		retCode = (d <= sphere.getRadius());
	}
	else
	{
		retCode = (u2.getLength() <= cylinder.getRadius());
	}


	return retCode;
}


bool intersect(const v3dxCylinder &cylinder, const v3dxBox3   &box)
{

	bool  retCode;
	v3dxVector3 apos;
	v3dxVector3 adir;

	cylinder.getAxis(apos, adir);

	float  s1 = 0, s2 = 0, s3 = 0, s4 = 0, d = 0, d1 = 0, d2 = 0;
	v3dxVector3   c, p, p1, p2;
	v3dxVector3   u, u1, u2;

	// find the distance between the min and the max of the box
	//with the lower point and the upper point of the cylinder respectively

	s1 = (apos - box.Min()).getLength();
	s2 = (apos - box.Max()).getLength();

	s3 = (apos + adir - box.Min()).getLength();
	s4 = (apos + adir - box.Max()).getLength();

	//Check the minimum of the above distances

	if (s1 <= s2)
	{
		d1 = s1;
		p1 = box.Min();
	}
	else
	{
		d1 = s2;
		p1 = box.Max();
	}

	if (s3 <= s4)
	{
		d2 = s3;
		p2 = box.Min();
	}
	else
	{
		d2 = s4;
		p2 = box.Max();
	}

	//set the value of the vector corresponding to the shortest distance
	if (d1 <= d2)
	{
		d = d1;
		c = apos;
		p = p1;
	}
	else
	{
		d = d2;
		c = apos + adir;
		p = p2;
	}

	// decompose the vector in u1 and u2 which are parallel and 
	// perpendicular to the cylinder axis respectively
	u = p - c;
	u1 = (u[0] * adir[0] + u[1] * adir[1] + u[2] * adir[2]) /
		(adir.getLength() * adir.getLength()) * adir;

	u2 = u - u1;

	if (u1.getLength() <= 10e-6)
	{
		retCode = true;
	}
	else if (u2.getLength() <= 10e-6)
	{
		retCode = (d <= 10e-6);
	}
	else
	{
		retCode = (u2.getLength() <= cylinder.getRadius());
	}

	return retCode;

}


bool intersect(const v3dxBox3 &box, const v3dxSphere &sphere)
{

	bool    retCode;
	float  s;
	float  d = 0.f;

	//find the square of the distance from the sphere to the box

	for (int i = 0; i < 3; i++)
	{
		if (sphere.getCenter()[i] < box.Min()[i])
		{
			s = sphere.getCenter()[i] - box.Min()[i];
			d += s * s;
		}
		else if (sphere.getCenter()[i] > box.Max()[i])
		{
			s = sphere.getCenter()[i] - box.Max()[i];
			d += s * s;
		}
	}

	retCode = (d <= (sphere.getRadius() * sphere.getRadius()));


	return retCode;

}

/********************************************************/
/* AABB-triangle overlap test code                      */
/* by Tomas Akenine-M?ller                              */
/* Function: int triBoxOverlap(float boxcenter[3],      */
/*          float boxhalfsize[3],float triverts[3][3]); */
/* History:                                             */
/*   2001-03-05: released the code in its first version */
/*   2001-06-18: changed the order of the tests, faster */
/*                                                      */
/* Acknowledgement: Many thanks to Pierre Terdiman for  */
/* suggestions and discussions on how to optimize code. */
/* Thanks to David Hunt for finding a ">="-bug!         */
/********************************************************/
#include <math.h>
#include <stdio.h>

#define X 0
#define Y 1
#define Z 2

#define CROSS(dest,v1,v2) \
	dest[0]=v1[1]*v2[2]-v1[2]*v2[1]; \
	dest[1]=v1[2]*v2[0]-v1[0]*v2[2]; \
	dest[2]=v1[0]*v2[1]-v1[1]*v2[0];

#define DOT(v1,v2) (v1[0]*v2[0]+v1[1]*v2[1]+v1[2]*v2[2])

#define SUB(dest,v1,v2) \
	dest[0]=v1[0]-v2[0]; \
	dest[1]=v1[1]-v2[1]; \
	dest[2]=v1[2]-v2[2];

#define FINDMINMAX(x0,x1,x2,min,max) \
	min = max = x0;   \
	if(x1<min) min=x1;\
	if(x1>max) max=x1;\
	if(x2<min) min=x2;\
	if(x2>max) max=x2;

int planeBoxOverlap(float normal[3], float d, const v3dxVector3& maxbox)
{
	int q;
	float vmin[3], vmax[3];
	for (q = X;q <= Z;q++)
	{
		if (normal[q] > 0.0f)
		{
			vmin[q] = -maxbox[q];
			vmax[q] = maxbox[q];
		}
		else
		{
			vmin[q] = maxbox[q];
			vmax[q] = -maxbox[q];
		}
	}
	if (DOT(normal, vmin) + d > 0.0f) return 0;
	if (DOT(normal, vmax) + d >= 0.0f) return 1;

	return 0;
}
