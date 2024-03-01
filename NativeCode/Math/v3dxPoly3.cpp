/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxpoly3.cpp
	Created Time:		30:6:2002   16:34
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/
#include "v3dxPoly3.h"
#include "v3dxMath.h"

#define new VNEW

using namespace TPL_HELP;

#define qisqrt(x) (1.0/sqrt(x))
#define fqisqrt(x) (float)(1.0/sqrt(x))

v3dxPoly3::v3dxPoly3 (int start_size)
{
	max_vertices = start_size;
	vertices = (v3dxVector3*)realloc( NULL, sizeof(v3dxVector3)*max_vertices );
	makeEmpty ();
}

v3dxPoly3::v3dxPoly3 (const v3dxPoly3& copy)
{
	max_vertices = copy.max_vertices;
	vertices = (v3dxVector3*)realloc( NULL, sizeof(v3dxVector3)*max_vertices );
	num_vertices = copy.num_vertices;
	memcpy (vertices, copy.vertices, sizeof (v3dxVector3)*num_vertices);
}

v3dxPoly3 & v3dxPoly3::operator = (const v3dxPoly3& copy)
{
	max_vertices = copy.max_vertices;
	vertices = (v3dxVector3*)realloc( vertices, sizeof(v3dxVector3)*max_vertices );
	num_vertices = copy.num_vertices;
	memcpy(vertices, copy.vertices, sizeof (v3dxVector3)*num_vertices);

	return *this;
}

v3dxPoly3::~v3dxPoly3 ()
{
	free( vertices );
}

void v3dxPoly3::getInversePoly( v3dxPoly3* pPoly )
{
	if(pPoly == this)
	{
		for(int i=0,j=num_vertices-1; i<j; ++i,--j)
			std::swap(vertices[i],vertices[j]);
	}
	else
	{
		pPoly->max_vertices = max_vertices;
		pPoly->num_vertices = num_vertices;
		pPoly->vertices = (v3dxVector3*)realloc( pPoly->vertices, sizeof(v3dxVector3)*max_vertices );
		v3dxVector3* pVertices = vertices + num_vertices - 1;
		for( int i=0 ; i<num_vertices ; i++ )
		{
			pPoly->vertices[0] = *pVertices;
			pVertices--;
		}
	}
	pPoly->PlaneNormal = -PlaneNormal;
}

bool v3dxPoly3::in (const v3dxVector3& v) const
{
	if( num_vertices<3 )
		return false;
	int flag = 0;
	int i, i1;
	i1 = 0;//num_vertices-1;

	v3dxVector3 vSpire = vertices[0] + PlaneNormal*10.f;
	//v3dxVector3 vSpire = v3dxVector3::ZERO;;

	for (i = 1 ; i < num_vertices ; i++)
	{
		int iResult = v3dxWhichSide3D (&v, &vertices[i1], &vertices[i],&vSpire);
		if ( iResult > 0) 
		{
			return false;
		}
		else
		{
			flag++;
		}
		i1 = i;
	}
	int iResult = v3dxWhichSide3D (&v, &vertices[num_vertices-1], &vertices[0],&vSpire );
	if ( iResult > 0) 
	{
		return false;
	}
	else
	{
		flag++;
	}
	return true;
}

bool v3dxPoly3::in (v3dxVector3* poly, int num_poly, const v3dxVector3& v)
{
	int i, i1;
	i1 = num_poly-1;
	for (i = 0 ; i < num_poly ; i++)
	{
		if (v3dxWhichSide3D_v2 (&v, &poly[i1], &poly[i]) > 0) 
			return false;
		i1 = i;
	}
	return true;
}

void v3dxPoly3::makeRoom (int new_max)
{
	if (new_max <= max_vertices) 
		return;
	vertices = (v3dxVector3*)realloc( vertices, sizeof(v3dxVector3)*new_max );
	max_vertices = new_max;
}

int v3dxPoly3::addVertex (float x, float y, float z,vBOOL bUpdateNormal)
{
	if (num_vertices >= max_vertices)
		makeRoom (max_vertices+5);
	vertices[num_vertices].X = x;
	vertices[num_vertices].Y = y;
	vertices[num_vertices].Z = z;
	num_vertices++;
	if( bUpdateNormal )
		UpdatePlaneNormal();
	return num_vertices-1;
}

void v3dxPoly3::splitWithPlane (v3dxPoly3& poly1, v3dxPoly3& poly2,
				  const v3dxPlane3& split_plane) const
{
	poly1.makeEmpty ();//等待接受分割后的前面部分
	poly2.makeEmpty ();//等待接受分割后的后面部分

	v3dxVector3 ptB;
	float sideA, sideB;
	v3dxVector3 ptA = vertices[num_vertices - 1];//取出多边形最后的顶点
	sideA = split_plane.classify (ptA);//顶点到这个分割平面的距离
	if (std::abs (sideA) < SMALL_EPSILON) //顶点在分割平面上
		sideA = 0;

	for (int i = -1 ; ++i < num_vertices ; )
	{
		ptB = vertices[i];
		sideB = split_plane.classify (ptB);
		if (std::abs (sideB) < SMALL_EPSILON) 
			sideB = 0.f;
		if (sideB < 0.f)
		{
			if (sideA > 0.f)//最后顶点在分割平面后方
			{
				//计算直线与分割平面的交点，这是一个简单的线----面交叉
				v3dxVector3 v = ptB; 
				v -= ptA;//得到一个向量，终点是当前计算点，起点是多边形最后点
				float sect = - split_plane.classify (ptA) / ( split_plane.getNormal ().dotProduct(v)  ) ;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly2.addVertex (ptB);
		}
		else if (sideB > 0.f)
		{
			if (sideA < 0.f)
			{
				//计算直线与分割平面的交点，这是一个简单的线----面交叉
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - split_plane.classify (ptA) / ( split_plane.getNormal ().dotProduct(v) );
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly1.addVertex (ptB);
		}
		else
		{
			poly1.addVertex (ptB);
			poly2.addVertex (ptB);
		}
		ptA = ptB;
		sideA = sideB;
	}
}

v3dxPoly3 v3dxPoly3::clipByPlanes(v3dxPlane3* Planes, int PlaneCount)
{
	v3dxPoly3 resultpoly = *this;
	v3dxPoly3 nomatter;
	for (int i = 0 ; i < PlaneCount ; ++i)
	{
		splitWithPlane(resultpoly, nomatter, Planes[i]);
	}
	return resultpoly;
}

void v3dxPoly3::cutToPlane (const v3dxPlane3& split_plane)
{
	v3dxPoly3 old (*this);
	makeEmpty ();

	v3dxVector3 ptB;
	float sideA, sideB;
	v3dxVector3 ptA = old.vertices[old.num_vertices - 1];
	sideA = split_plane.classify (ptA);
	if (std::abs(sideA) < SMALL_EPSILON)
		sideA = 0;

	for (int i = -1 ; ++i < old.num_vertices ; )
	{
		ptB = old.vertices[i];
		sideB = split_plane.classify (ptB);
		if (std::abs(sideB) < SMALL_EPSILON)
			sideB = 0;
		if (sideB > 0)
		{
			if (sideA < 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - split_plane.classify (ptA) / ( split_plane.getNormal ().dotProduct(v) ) ;
				v *= sect; v += ptA;
				addVertex (v);
			}
		}
		else if (sideB < 0)
		{
			if (sideA > 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - split_plane.classify (ptA) / ( split_plane.getNormal ().dotProduct(v) ) ;
				v *= sect; v += ptA;
				addVertex (v);
			}
			addVertex (ptB);
		}
		else
		{
			addVertex (ptB);
		}
		ptA = ptB;
		sideA = sideB;
	}
}

void v3dxPoly3::splitWithPlaneX (v3dxPoly3& poly1, v3dxPoly3& poly2,
				  float x) const
{
	poly1.makeEmpty ();
	poly2.makeEmpty ();

	v3dxVector3 ptB;
	float sideA, sideB;
	v3dxVector3 ptA = vertices[num_vertices - 1];
	sideA = ptA.X - x;
	if (std::abs(sideA) < SMALL_EPSILON)
		sideA = 0;

	for (int i = -1 ; ++i < num_vertices ; )
	{
		ptB = vertices[i];
		sideB = ptB.X - x;
		if (std::abs(sideB) < SMALL_EPSILON)
			sideB = 0;
		if (sideB > 0)
		{
			if (sideA < 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.X - x) / v.X;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly2.addVertex (ptB);
		}
		else if (sideB < 0)
		{
			if (sideA > 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.X - x) / v.X;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly1.addVertex (ptB);
		}
		else
		{
			poly1.addVertex (ptB);
			poly2.addVertex (ptB);
		}
		ptA = ptB;
		sideA = sideB;
	}
}

void v3dxPoly3::splitWithPlaneY (v3dxPoly3& poly1, v3dxPoly3& poly2,
				  float y) const
{
	poly1.makeEmpty ();
	poly2.makeEmpty ();

	v3dxVector3 ptB;
	float sideA, sideB;
	v3dxVector3 ptA = vertices[num_vertices - 1];
	sideA = ptA.Y - y;
	if (std::abs(sideA) < SMALL_EPSILON)
		sideA = 0;

	for (int i = -1 ; ++i < num_vertices ; )
	{
		ptB = vertices[i];
		sideB = ptB.Y - y;
		if (std::abs(sideB) < SMALL_EPSILON)
			sideB = 0;
		if (sideB > 0)
		{
			if (sideA < 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.Y - y) / v.Y;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly2.addVertex (ptB);
		}
		else if (sideB < 0)
		{
			if (sideA > 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.Y - y) / v.Y;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly1.addVertex (ptB);
		}
		else
		{
			poly1.addVertex (ptB);
			poly2.addVertex (ptB);
		}
		ptA = ptB;
		sideA = sideB;
	}
}

void v3dxPoly3::splitWithPlaneZ (v3dxPoly3& poly1, v3dxPoly3& poly2,
				  float z) const
{
	poly1.makeEmpty ();
	poly2.makeEmpty ();

	v3dxVector3 ptB;
	float sideA, sideB;
	v3dxVector3 ptA = vertices[num_vertices - 1];
	sideA = ptA.Z - z;
	if (std::abs(sideA) < SMALL_EPSILON)
		sideA = 0;

	for (int i = -1 ; ++i < num_vertices ; )
	{
		ptB = vertices[i];
		sideB = ptB.Z - z;
		if (std::abs(sideB) < SMALL_EPSILON)
			sideB = 0;
		if (sideB > 0)
		{
			if (sideA < 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.Z - z) / v.Z;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly2.addVertex (ptB);
		}
		else if (sideB < 0)
		{
			if (sideA > 0)
			{
				v3dxVector3 v = ptB; v -= ptA;
				float sect = - (ptA.Z - z) / v.Z;
				v *= sect; v += ptA;
				poly1.addVertex (v);
				poly2.addVertex (v);
			}
			poly1.addVertex (ptB);
		}
		else
		{
			poly1.addVertex (ptB);
			poly2.addVertex (ptB);
		}
		ptA = ptB;
		sideA = sideB;
	}
}

v3dxVector3 v3dxPoly3::computeNormal (v3dxVector3* vertices, int num)
{
	float ayz = 0;
	float azx = 0;
	float axy = 0;
	int i, i1;
	float x1, y1, z1, x, y, z;

	i1 = num-1;
	for (i = 0 ; i < num; i++)
	{
		x = vertices[i].X;
		y = vertices[i].Y;
		z = vertices[i].Z;
		x1 = vertices[i1].X;
		y1 = vertices[i1].Y;
		z1 = vertices[i1].Z;
		ayz += (z1+z) * (y-y1);//按照顺时针，逐个边的计算法向量的累加
		azx += (x1+x) * (z-z1);
		axy += (y1+y) * (x-x1);
		i1 = i;
	}

	float sqd = ayz*ayz + azx*azx + axy*axy;
	float invd;
	if (sqd < SMALL_EPSILON)
		invd = (float)1./SMALL_EPSILON;
	else
		invd = fqisqrt (sqd);

	v3dxVector3 vec = v3dxVector3 (ayz * invd, azx * invd, axy * invd);
	v3dxVec3Normalize(&vec,&vec);
	// @note noslopforever 2007-2-6 这样的结果是一个右手坐标系结果
	// fix到一个左手坐标系结果
	vec = -vec;
	return vec;
}

v3dxPlane3 v3dxPoly3::computePlane (v3dxVector3* vertices, int num_vertices)
{
	float D;
	v3dxVector3 pl = computeNormal (vertices, num_vertices);
	D = -pl.X*vertices[0].X - pl.Y*vertices[0].Y - pl.Z*vertices[0].Z;
	return v3dxPlane3 (pl, D);
}
