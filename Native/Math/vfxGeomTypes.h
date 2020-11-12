/********************************************************************
	created:	2002/12/26
	created:	26:12:2002   14:28
	filename: 	vfxGeometry\vfxGeomTypes.h
	file path:	vfxGeometry
	file base:	vfxGeomTypes
	file ext:	h
	author:		johnson
*********************************************************************/
#ifndef __vfxGeomTypes_h_26_12_2002_14_28_
#define __vfxGeomTypes_h_26_12_2002_14_28_

#pragma once
#include "../BaseHead.h"
#include "../Core/debug/vfxnew.h"

#pragma pack(push,1)

struct v3dVector2_t
{
	float				x;
	float				y;
};

struct v3dUV_t
{
	float				u;
	float				v;
};

struct v3dVector3_t
{
	float				x;
	float				y;
	float				z;
};

struct v3dVector4_t
{
	union
	{
		struct
		{
			float				x;
			float				y;
			float				z;
			float				w;
		};
		struct  
		{
			float				r;
			float				g;
			float				b;
			float				a;
		};
	};	
};

struct v3dMatrix4_t
{
	union 
	{
		struct
		{
			float				m11, m12, m13, m14;			
			float				m21, m22, m23, m24;
			float				m31, m32, m33, m34;
			float				m41, m42, m43, m44;
		};
		float m[4][4];
	};
};

struct v3dMatrix3_t
{
	union 
	{
		struct
		{
			float				m11, m12, m13;			
			float				m21, m22, m23;
			float				m31, m32, m33;
//			float				m41, m42, m43;
		};
		float m[3][3];
	};
};

struct v3dSubSet
{
	int nMaterialId;
	int nStartIndex;
	int nIndexCount;
};

/** vertex Infuence 
 *	record the weight for vertex with bone
 */
struct v3dInfluence_t
{
public:
	int boneId;
	float weight;
};

struct v3dPNVertex_t
{
public:
	v3dVector3_t vPosition;
	v3dVector3_t vNormal;
};

struct v3dDT1Vertex_t
{
public:
	unsigned int	dwColor;  
	v3dUV_t uv;
};

struct v3dVertex_t
{
public:
	v3dVector3_t vPosition;
	v3dVector3_t vNormal;

	std::vector<v3dInfluence_t> vectorInfluence;
};

/** face -- consise of three verteies
*
*/
struct v3dFace_t
{
public:
	// vertex id
	unsigned short vertexId[3];
	/// texture coodinate id
	//int tvId[3];
};

namespace TPL_HELP
{
	template<class U,class V> inline U vfxMIN(const U & lh, const V & rh)
	{ return ((lh < rh) ? lh : U(rh)); }
	template<class U,class V> inline U vfxMAX(const U & lh, const V & rh)
	{ return ((lh > rh) ? lh : U(rh)); }
	template<class T> inline T vfxMOD (const T & i, const T & j)
	{ return (i<0)?j-(-i%j):i%j; }
	template<class T> inline T vfxDIV (const T & i, const T & j)
	{ return (i<0)?-(-i/j)-1:i/j; }
	
	template<class T>
	struct _max
	{
		T _M_t;
		template<class U,class V>
			_max(const U & lh,const V & rh)
		{
			_M_t = lh > rh ? T(lh) : T(rh);
		}
		inline operator T () const{
			return _M_t;
		}
	};
	template<class T>
	struct _min
	{
		T _M_t;
		template<class U,class V>
			_min(const U & lh,const V & rh)
		{
			_M_t = lh < rh ? T(lh) : T(rh);
		}
		inline operator T () const{
			return _M_t;
		}
	};

	template<typename T> inline bool vfxGE(const T& v1 , const T& v2 , const T& Epsilon){
		return ( v1-v2 >= Epsilon);
	}
	template<typename T> inline bool vfxLE(const T& v1 , const T& v2 , const T& Epsilon){
		return ( v2-v1 >= Epsilon);
	}
}

#pragma pack(pop)

#endif//#ifndef __vfxGeomTypes_h_26_12_2002_14_28_
