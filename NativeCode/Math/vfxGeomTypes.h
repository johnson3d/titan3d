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
#include "../Base/BaseHead.h"
#include "../Base/TypeUtility.h"
#include "../Base/CoreSDK.h"
#include "../Base/debug/vfxnew.h"

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

class v3dDVector3_t
{
public:
	double x;
	double y;
	double z;
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

struct v3dDMatrix4_t
{
	union
	{
		struct
		{
			double				m11, m12, m13, m14;
			double				m21, m22, m23, m24;
			double				m31, m32, m33, m34;
			double				m41, m42, m43, m44;
		};
		double m[4][4];
	};
	inline void identity()
	{
		m12 = m13 = m14 =
			m21 = m23 = m24 =
			m31 = m32 = m34 =
			m41 = m42 = m43 = 0.0;
		m11 = m22 = m33 = m44 = 1.0;
	}
	inline void ExtractionTrans(v3dDVector3_t& vTransPos) const
	{
		vTransPos.x = m41;
		vTransPos.y = m42;
		vTransPos.z = m43;
	}
	inline void ExtractionScale(v3dVector3_t& vScale) const
	{
		//vScale.x = sqrt(m11*m11 + m21*m21 + m31*m31); // getRow1().getLength();
		//vScale.y = sqrt(m12*m12 + m22*m22 + m32*m32);
		//vScale.z = sqrt(m13*m13 + m23*m23 + m33*m33);
		vScale.x = (float)sqrt(m11 * m11 + m12 * m12 + m13 * m13); // getRow1().getLength();
		vScale.y = (float)sqrt(m21 * m21 + m22 * m22 + m23 * m23);
		vScale.z = (float)sqrt(m31 * m31 + m32 * m32 + m33 * m33);
	}
	inline v3dDMatrix4_t operator * (const v3dDMatrix4_t& Mat) const
	{
		v3dDMatrix4_t mat;
		DMatrix4Mul(&mat, this, &Mat);
		return mat;
	}

private:
	static inline v3dDMatrix4_t* DMatrix4Mul(v3dDMatrix4_t* pOut, const v3dDMatrix4_t* mat1, const v3dDMatrix4_t* mat2) {
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 4; j++) {
				pOut->m[i][j] = mat1->m[i][0] * mat2->m[0][j] +
					mat1->m[i][1] * mat2->m[1][j] +
					mat1->m[i][2] * mat2->m[2][j] +
					mat1->m[i][3] * mat2->m[3][j];
			}
		}
		return pOut;
	}
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
