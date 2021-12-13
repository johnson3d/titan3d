/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxmath.h
	Created Time:		30:6:2002   16:32
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#ifndef __V3DXMATH__H__
#define __V3DXMATH__H__
#include "vfxGeomTypes.h"

#ifdef EPSILON
#undef EPSILON
#endif
#define EPSILON 0.001f		/*�ǳ�С��ֵ*/

#ifdef SMALL_EPSILON
#undef SMALL_EPSILON
#endif
#define SMALL_EPSILON 0.000001f		/*�ǳ�С��ֵ*/

#define V_PI  ((float)3.1415926535)
#define V_TWOPI ((float)6.283185307)
#define V_HALFPI ((float)1.570796326794895)

#define V_DEG_TO_RAD (V_PI/(float)180.0)
#define V_RAD_TO_DEG ((float)180.0/V_PI)
#define V_DegToRad(deg) (((float)deg)*V_DEG_TO_RAD)
#define V_RadToDeg(rad) (((float)rad)*V_RAD_TO_DEG)

#if defined(PLATFORM_WIN)
//#define USE_DXMATH
#endif

class  Math
{
public:
	/** The angular units used by the API.
	@remarks
	By default, vps uses degrees in all it's external APIs.
	*/
	enum AngleUnit
	{
		AU_DEGREE,
		AU_RADIAN
	};

protected:
	// angle units used by the api
	static AngleUnit msAngleUnit;

	/// Size of the trig tables as determined by constructor.
	static int mTrigTableSize;

	/// Radian -> index factor value ( mTrigTableSize / 2 * PI )
	static float mTrigTableFactor;
	static float* mSinTable;
	static float* mTanTable;

	/** Private function to build trig tables.
	*/
	void buildTrigTables();
	static float SinTable(float fValue);
	static float TanTable(float fValue);

public:
	/** Default constructor.
	@param
	trigTableSize Optional parameter to set the size of the
	tables used to implement Sin, Cos, Tan
	*/
	Math(unsigned int trigTableSize = 4096);

	/** Default destructor.
	*/
	~Math();


	static inline int IAbs(int iValue) { return (iValue >= 0 ? iValue : -iValue); }
	static inline int ICeil(float fValue) { return int(ceil(fValue)); }
	static inline int IFloor(float fValue) { return int(floor(fValue)); }
	static int ISign(int iValue);

	static inline float Abs(float fValue) { return float(fabs(fValue)); }
	static inline double D_Abs(double fValue) { return fabs(fValue); }
	static float ACos(float fValue);
	static float ASin(float fValue);
	static inline float ATan(float fValue) { return float(atan(fValue)); }
	static inline float ATan2(float fY, float fX) { return float(atan2(fY, fX)); }
	static inline float Ceil(float fValue) { return float(ceil(fValue)); }

	/** Cosine function.
	@param
	fValue Angle in radians
	@param
	useTables If true, uses lookup tables rather than
	calculation - faster but less accurate.
	*/
	static inline float Cos(float fValue, bool useTables = false) {
		return (!useTables) ? float(cos(fValue)) : SinTable(fValue + HALF_PI);
	}

	static inline float Exp(float fValue) { return float(exp(fValue)); }

	static inline float Floor(float fValue) { return float(floor(fValue)); }

	static inline float Log(float fValue) { return float(log(fValue)); }

	static inline float Pow(float fBase, float fExponent) { return float(pow(fBase, fExponent)); }

	static float Sign(float fValue);

	/** Sine function.
	@param
	fValue Angle in radians
	@param
	useTables If true, uses lookup tables rather than
	calculation - faster but less accurate.
	*/
	static inline float Sin(float fValue, bool useTables = false) {
		return (!useTables) ? float(sin(fValue)) : SinTable(fValue);
	}

	static float Sqr(float fValue) { return fValue*fValue; }

	static float Sqrt(float fValue) { return float(sqrt(fValue)); }
	static double D_Sqrt(float fValue) { return sqrt(fValue); }

	/** Inverse square root i.e. 1 / Sqrt(x), good for vector
	normalisation.
	*/
	static float InvSqrt(float fValue);

	static float UnitRandom();  // in [0,1]

	static float RangeRandom(float fLow, float fHigh);  // in [fLow,fHigh]

	static float SymmetricRandom();  // in [-1,1]

									 /** Tangent function.
									 @param
									 fValue Angle in radians
									 @param
									 useTables If true, uses lookup tables rather than
									 calculation - faster but less accurate.
									 */
	static inline float Tan(float fValue, bool useTables = false) {
		return (!useTables) ? float(tan(fValue)) : TanTable(fValue);
	}

	static inline float DegreesToRadians(float degrees) { return degrees * fDeg2Rad; }
	static inline float RadiansToDegrees(float radians) { return radians * fRad2Deg; }

	/** Sets the native angle units (radians or degrees) expected by and returned by the vps API
	@remarks
	By default, vps's main API uses degrees (this Math class uses radians because that is the underlying
	unit used by the library routines. This may be changed by the user of the engine so that every instance
	of degrees actually accepts radians instead.
	@par
	You can set this directly after creating a new Root, and also before/after resource creation,
	depending on whether you want the change to affect resource files.
	@par
	Warning: don't set this during the middle of an app run - some classes store degrees internally
	as degrees, and perform the conversion for internal usage. Changing the AngleUnits between set
	and get will result in screwed up values. This affects some file loading too - notably particle
	system angle attributes. These values must also be changed in the particle files to use radians.

	*/
	static void setAngleUnit(AngleUnit unit);
	/** Get the unit being used for angles. */
	static AngleUnit getAngleUnit(void);

	/** Convert from the units the engine is currently using to radians. */
	static float AngleUnitsToRadians(float units);
	/** Convert from radians to the units the engine is currently using . */
	static float RadiansToAngleUnits(float radians);

	/** Checks wether a given point is inside a triangle, in a
	2-dimensional (Cartesian) space.
	@remarks
	The vertices of the triangle must be given in either
	trigonometrical (anticlockwise) or inverse trigonometrical
	(clockwise) order.
	@param
	px The X-coordinate of the point.
	@param
	py The Y-coordinate of the point.
	@param
	ax The X-coordinate of the triangle's first vertex.
	@param
	ay The Y-coordinate of the triangle's first vertex.
	@param
	bx The X-coordinate of the triangle's second vertex.
	@param
	by The Y-coordinate of the triangle's second vertex.
	@param
	cx The X-coordinate of the triangle's third vertex.
	@param
	cy The Y-coordinate of the triangle's third vertex.
	@returns
	If the point resides in the triangle, <b>true</b> is
	returned.
	@par
	If the point is outside the triangle, <b>false</b> is
	returned.
	*/
	static bool pointInTri2D(float px, float pz, float ax, float az, float bx, float bz, float cx, float cz);

	/** Compare 2 reals, using tolerance for inaccuracies.
	*/
	static bool RealEqual(float a, float b,float tolerance = EPSILON);

	// map a value to range [0, width)
	static int mapToRange(int v, int l, int h);
	static int mapToBound(int n, int width);

	// clamp
	template<typename T>
	static T clamp(T v, T l, T h);
	static int clampSbyte(int n);
	static int clampByte(int n);
	static int clampUshort(int n);
	static float saturate(float f);


	static const USHORT USHORT_INFINITY;
	static const float POS_INFINITY;
	static const float NEG_INFINITY;
	static const float V3_PI;
	static const float TWO_PI;
	static const float HALF_PI;
	static const float fDeg2Rad;
	static const float fRad2Deg;
};

inline int Math::mapToRange(int v, int l, int h)
{
	v = v - l;
	int len = h - l;

	int mod = v % len;
	if (mod >= 0)
		return mod;
	return len + mod;
}

// map a value to range [0, width)
inline int Math::mapToBound(int n, int width)
{
	n = n % width;
	if (n < 0)
		n += width;

	return n;
}

template<typename T>
T Math::clamp(T v, T l, T h)
{
	return ((v) < (l) ? (l) : (v) > (h) ? (h) : (v));
}

inline int Math::clampByte(int n)
{
	return clamp<int>(n, 0, 255);
}

inline int Math::clampSbyte(int n)
{
	return clamp<int>(n, -128, 127);
}

inline int Math::clampUshort(int n)
{
	return clamp<int>(n, 0, 0xffff);
}

inline float Math::saturate(float f)
{
	return clamp(f, 0.0f, 1.0f);
}


class v3dxPlane3;
class v3dxPoly3;
class v3dxSegment3;
class v3dxLine3;
class v3dxQuaternion;

class v3dxVector3;
class v3dxBox3;
class v3dxPlane3;
class v3dxSegment3;
class v3dxPoly3;
class v3dxMatrix4;

inline void v3dxPrjLHtoRH(float *d, const float *s) // Exclusive function
{
	d[0] = -s[0];	d[1] = -s[1];	d[2] = -s[2];	d[3] = -s[3];
	d[4] = s[4];	d[5] = s[5];	d[6] = s[6];	d[7] = s[7];
	d[8] = -s[8];	d[9] = -s[9];	d[10] = -s[10];	d[11] = -s[11];
	d[12] = s[12];	d[13] = s[13];	d[14] = s[14];	d[15] = s[15];
}

inline void v3dxViewLHtoRH(float *d, const float *s)
{
	d[0] = -s[0];	d[1] = s[1];	d[2] = -s[2];	d[3] = s[3];
	d[4] = -s[4];	d[5] = s[5];	d[6] = -s[6];	d[7] = s[7];
	d[8] = -s[8];	d[9] = s[9];	d[10] = -s[10];	d[11] = s[11];
	d[12] = -s[12];	d[13] = s[13];	d[14] = -s[14];	d[15] = s[15];
}

inline v3dVector2_t* v3dxVec2Add(v3dVector2_t *pOut,const v3dVector2_t* p1,const v3dVector2_t* p2){
	pOut->x = p1->x + p2->x;
	pOut->y = p1->y + p2->y;
	return pOut;
}

inline v3dVector2_t* v3dxVec2Sub(v3dVector2_t *pOut,const v3dVector2_t* p1,const v3dVector2_t* p2){
	pOut->x = p1->x - p2->x;
	pOut->y = p1->y - p2->y;
	return pOut;
}

inline v3dVector2_t* v3dxVec2Mul(v3dVector2_t *pOut,float f,const v3dVector2_t* p){
	pOut->x = f * p->x;
	pOut->y = f * p->y;
	return pOut;
}

inline v3dVector2_t* v3dxVec2Normalize(v3dVector2_t *pOut,const v3dVector2_t* pvt){
	FLOAT fLen = sqrtf(pvt->x * pvt->x + pvt->y * pvt->y);
	pOut->x = pvt->x / fLen;
	pOut->y = pvt->y / fLen;
	return pOut;
}

inline FLOAT v3dxVec2Length(const v3dVector2_t* pvt){
	return sqrtf(pvt->x * pvt->x + pvt->y * pvt->y);
}

inline FLOAT v3dxVec2LengthSq(const v3dVector2_t* pvt){
	return (pvt->x * pvt->x + pvt->y * pvt->y);
}

/**
   
**/
inline float v3dxVec3Dot( const v3dVector3_t* v1,const v3dVector3_t* v2 ){ 
	return v1->x*v2->x+v1->y*v2->y+v1->z*v2->z; 
}
inline double v3dxDVec3Dot(const v3dDVector3_t* v1, const v3dDVector3_t* v2) {
	return v1->x * v2->x + v1->y * v2->y + v1->z * v2->z;
}
//#define v3dxVec3Dot(v1,v2)	((v1)->x*(v2)->x+(v1)->y*(v2)->y+(v1)->z*(v2)->z) 

inline v3dVector3_t* v3dxVec3Cross(v3dVector3_t *pOut,const v3dVector3_t* pvt1,const v3dVector3_t* pvt2){
	pOut->x=pvt1->y*pvt2->z-pvt1->z*pvt2->y;
	pOut->y=pvt1->z*pvt2->x-pvt1->x*pvt2->z;
	pOut->z=pvt1->x*pvt2->y-pvt1->y*pvt2->x;
	return pOut;
}

inline v3dDVector3_t* v3dxDVec3Cross(v3dDVector3_t* pOut, const v3dDVector3_t* pvt1, const v3dDVector3_t* pvt2) {
	pOut->x = pvt1->y * pvt2->z - pvt1->z * pvt2->y;
	pOut->y = pvt1->z * pvt2->x - pvt1->x * pvt2->z;
	pOut->z = pvt1->x * pvt2->y - pvt1->y * pvt2->x;
	return pOut;
}

inline v3dVector3_t* v3dxVec3Lerp(v3dVector3_t *pOut, const v3dVector3_t* pvt1, const v3dVector3_t* pvt2, float fSlerp) {

	pOut->x = pvt1->x * (1 - fSlerp) + pvt2->x * fSlerp;
	pOut->y = pvt1->y * (1 - fSlerp) + pvt2->y * fSlerp;
	pOut->z = pvt1->z * (1 - fSlerp) + pvt2->z * fSlerp;
	return pOut;
}
//#define v3dxVec3Cross(pOut,pvt1,pvt2)	{\
//	(pOut)->x=(pvt1)->y*(pvt2)->z-(pvt1)->z*(pvt2)->y;\
//	(pOut)->y=(pvt1)->z*(pvt2)->x-(pvt1)->x*(pvt2)->z;\
//	(pOut)->z=(pvt1)->x*(pvt2)->y-(pvt1)->y*(pvt2)->x;}\
//	(pOut);

inline v3dVector3_t* v3dxVec3Add(v3dVector3_t *pOut,const v3dVector3_t* p1,const v3dVector3_t* p2){
	pOut->x = p1->x + p2->x;
	pOut->y = p1->y + p2->y;
	pOut->z = p1->z + p2->z;
	return pOut;
}

//#define v3dxVec3Add(pOut,p1,p2)		{\
//	(pOut)->x = (p1)->x + (p2)->x;\
//	(pOut)->y = (p1)->y + (p2)->y;\
//	(pOut)->z = (p1)->z + (p2)->z;}\
//	(pOut);

inline v3dVector3_t* v3dxVec3Sub(v3dVector3_t *pOut,const v3dVector3_t* p1,const v3dVector3_t* p2){
	pOut->x = p1->x - p2->x;
	pOut->y = p1->y - p2->y;
	pOut->z = p1->z - p2->z;
	return pOut;
}

//#define v3dxVec3Sub( pOut, p1, p2)	{\
//	(pOut)->x = (p1)->x - (p2)->x;\
//	(pOut)->y = (p1)->y - (p2)->y;\
//	(pOut)->z = (p1)->z - (p2)->z;}\
//	(pOut);

//inline v3dVector3_t* v3dxVec3Mul(v3dVector3_t *pOut,float f,const v3dVector3_t* p){
//	pOut->x = f * p->x;
//	pOut->y = f * p->y;
//	pOut->z = f * p->z;
//	return pOut;
//}

#define v3dxVec3Mul(pOut,f,p)		{\
	(pOut)->x = f * (p)->x;\
	(pOut)->y = f * (p)->y;\
	(pOut)->z = f * (p)->z;}\
	(pOut);

inline v3dVector3_t* v3dxVec3Normalize(v3dVector3_t *pOut,const v3dVector3_t* pvt){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dVector3_t*)D3DXVec3Normalize((D3DXVECTOR3*)pOut, (D3DXVECTOR3*)pvt);
#else
	FLOAT fLen = sqrtf(pvt->x * pvt->x + pvt->y * pvt->y + pvt->z * pvt->z);
	pOut->x = pvt->x / fLen;
	pOut->y = pvt->y / fLen;
	pOut->z = pvt->z / fLen;
	return pOut;
#endif
}

inline float v3dxVec4Dot(v3dVector4_t *v1,const v3dVector4_t* v2){
	return v1->x*v2->x+v1->y*v2->y+v1->z*v2->z+v1->w*v2->w; 
}

inline v3dVector4_t* v3dxVec4Normalize(v3dVector4_t *pOut,const v3dVector4_t* pvt){
	FLOAT fLen = sqrtf(pvt->x * pvt->x + pvt->y * pvt->y + pvt->z * pvt->z + pvt->w * pvt->w );
	pOut->x = pvt->x / fLen;
	pOut->y = pvt->y / fLen;
	pOut->z = pvt->z / fLen;
	pOut->w = pvt->w / fLen;
	return pOut;
}

inline FLOAT v3dxVec3Length(const v3dVector3_t* pvt){
	return sqrtf(pvt->x * pvt->x + pvt->y * pvt->y + pvt->z * pvt->z);
}

inline FLOAT v3dxVec3LengthSq(const v3dVector3_t* pvt){
	return (pvt->x * pvt->x + pvt->y * pvt->y + pvt->z * pvt->z);
}

inline v3dMatrix4_t* v3dxMatrix4Mul( v3dMatrix4_t* pOut , const v3dMatrix4_t* mat1 , const v3dMatrix4_t* mat2){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixMultiply( (D3DXMATRIX*)pOut , (const D3DXMATRIX*)mat1 , (const D3DXMATRIX*)mat2 );
#else
	for (int i = 0;i < 4;i++) {
		for (int j = 0;j < 4;j++) {
			pOut->m[i][j] = mat1->m[i][0] * mat2->m[0][j] +
				mat1->m[i][1] * mat2->m[1][j] +
				mat1->m[i][2] * mat2->m[2][j] +
				mat1->m[i][3] * mat2->m[3][j];
		}
	}
	return pOut;
#endif
}

inline v3dDMatrix4_t* v3dxDMatrix4Mul(v3dDMatrix4_t* pOut, const v3dDMatrix4_t* mat1, const v3dDMatrix4_t* mat2) {
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

inline v3dMatrix4_t* v3dxMatrix4Mul( v3dMatrix4_t* pOut , const v3dMatrix4_t* mat1 , float f){
	for( int i=0 ; i<4 ; i++ )
		for( int j=0 ; j<4 ; j++ )
			pOut->m[i][j] = mat1->m[i][j] * f;
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4Add( v3dMatrix4_t* pOut , const v3dMatrix4_t* mat1 , const v3dMatrix4_t* mat2){
	for( int i=0 ; i<4 ; i++ )
		for( int j=0 ; j<4 ; j++ )
			pOut->m[i][j] = mat1->m[i][j] + mat2->m[i][j];
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4Sub( v3dMatrix4_t* pOut , const v3dMatrix4_t* mat1 , const v3dMatrix4_t* mat2){
	for( int i=0 ; i<4 ; i++ )
		for( int j=0 ; j<4 ; j++ )
			pOut->m[i][j] = mat1->m[i][j] - mat2->m[i][j];
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4Identity( v3dMatrix4_t* pOut ){
	pOut->m11=1.0f; pOut->m12=0.0f; pOut->m13=0.0f; pOut->m14=0.0f;
	pOut->m21=0.0f; pOut->m22=1.0f; pOut->m23=0.0f; pOut->m24=0.0f;
	pOut->m31=0.0f; pOut->m32=0.0f; pOut->m33=1.0f; pOut->m34=0.0f;
	pOut->m41=0.0f; pOut->m42=0.0f; pOut->m43=0.0f; pOut->m44=1.0f;
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4Translation( v3dMatrix4_t* pOut ,float x,float y,float z){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixTranslation( (D3DXMATRIX*)pOut , x , y , z );
#else
	pOut->m11=1.0f; pOut->m12=0.0f; pOut->m13=0.0f; pOut->m14=0.0f;
	pOut->m21=0.0f; pOut->m22=1.0f; pOut->m23=0.0f; pOut->m24=0.0f;
	pOut->m31=0.0f; pOut->m32=0.0f; pOut->m33=1.0f; pOut->m34=0.0f;
	pOut->m41=x; pOut->m42=y; pOut->m43=z; pOut->m44=1.0f;
	return pOut;
#endif
}

inline v3dDMatrix4_t* v3dxDMatrix4Translation(v3dDMatrix4_t* pOut, double x, double y, double z) {
	pOut->m11 = 1.0f; pOut->m12 = 0.0f; pOut->m13 = 0.0f; pOut->m14 = 0.0f;
	pOut->m21 = 0.0f; pOut->m22 = 1.0f; pOut->m23 = 0.0f; pOut->m24 = 0.0f;
	pOut->m31 = 0.0f; pOut->m32 = 0.0f; pOut->m33 = 1.0f; pOut->m34 = 0.0f;
	pOut->m41 = x; pOut->m42 = y; pOut->m43 = z; pOut->m44 = 1.0f;
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4Scale( v3dMatrix4_t* pOut ,float x,float y,float z){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixScaling( (D3DXMATRIX*)pOut , x , y , z );
#else
	pOut->m11=x; pOut->m12=0.0f; pOut->m13=0.0f; pOut->m14=0.0f;
	pOut->m21=0.0f; pOut->m22=y; pOut->m23=0.0f; pOut->m24=0.0f;
	pOut->m31=0.0f; pOut->m32=0.0f; pOut->m33=z; pOut->m34=0.0f;
	pOut->m41=0.0f; pOut->m42=0.0f; pOut->m43=0.0f; pOut->m44=1.0f;
	return pOut;
#endif
}

inline v3dDMatrix4_t* v3dxDMatrix4Scale(v3dDMatrix4_t* pOut, double x, double y, double z) {
	pOut->m11 = x; pOut->m12 = 0.0f; pOut->m13 = 0.0f; pOut->m14 = 0.0f;
	pOut->m21 = 0.0f; pOut->m22 = y; pOut->m23 = 0.0f; pOut->m24 = 0.0f;
	pOut->m31 = 0.0f; pOut->m32 = 0.0f; pOut->m33 = z; pOut->m34 = 0.0f;
	pOut->m41 = 0.0f; pOut->m42 = 0.0f; pOut->m43 = 0.0f; pOut->m44 = 1.0f;
	return pOut;
}

inline v3dMatrix4_t* v3dxMatrix4RotationX( v3dMatrix4_t* pOut ,float angle ){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixRotationX( (D3DXMATRIX*)pOut , angle );
#else
	pOut->m11 = 1.0f; pOut->m12 = 0.0f;        pOut->m13 = 0.0f;         pOut->m14 = 0.0f;
	pOut->m21 = 0.0f; pOut->m22 = cosf(angle); pOut->m23 = sinf(angle); pOut->m24 = 0.0f;
	pOut->m31 = 0.0f; pOut->m32 = sinf(angle); pOut->m33 = -cosf(angle); pOut->m34 = 0.0f;
	pOut->m41 = 0.0f; pOut->m42 = 0.0f;        pOut->m43 = 0.0f;         pOut->m44 = 1.0f;
	return pOut;
#endif
}

inline v3dMatrix4_t* v3dxMatrix4RotationY( v3dMatrix4_t* pOut ,float angle ){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixRotationY( (D3DXMATRIX*)pOut , angle );
#else
	pOut->m11 = cosf(angle); pOut->m12 = 0.0f; pOut->m13 = -sinf(angle); pOut->m14 = 0.0f;
	pOut->m21 = 0.0f;        pOut->m22 = 1.0f; pOut->m23 = 0.0f;		   pOut->m24 = 0.0f;
	pOut->m31 = sinf(angle); pOut->m32 = 0.0f; pOut->m33 = cosf(angle);  pOut->m34 = 0.0f;
	pOut->m41 = 0.0f; pOut->m42 = 0.0f;        pOut->m43 = 0.0f;         pOut->m44 = 1.0f;
	return pOut;
#endif
}

inline v3dMatrix4_t* v3dxMatrix4RotationZ( v3dMatrix4_t* pOut ,float angle ){
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixRotationZ( (D3DXMATRIX*)pOut , angle );
#else
	pOut->m11 = cosf(angle); pOut->m12 =  sinf(angle); pOut->m13 = 0.0f; pOut->m14 = 0.0f;
	pOut->m21 = -sinf(angle); pOut->m22 =  cosf(angle); pOut->m23 = 0.0f; pOut->m24 = 0.0f;
	pOut->m31 = 0.0f;        pOut->m32 = 0.0f;         pOut->m33 = 1.0f; pOut->m34 = 0.0f;
	pOut->m41 = 0.0f;        pOut->m42 = 0.0f;         pOut->m43 = 0.0f; pOut->m44 = 1.0f;
	return pOut;
#endif
}

inline v3dMatrix4_t* v3dxMatrix4Transpose( v3dMatrix4_t* pOut , const v3dMatrix4_t* pMat )
{
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixTranspose( (D3DXMATRIX*)pOut , (const D3DXMATRIX*)pMat );
#else
	if(pOut!=pMat)
	{
		pOut->m11 = pMat->m11; pOut->m12 =  pMat->m21; pOut->m13 = pMat->m31; pOut->m14 = pMat->m41;
		pOut->m21 = pMat->m12; pOut->m22 =  pMat->m22; pOut->m23 = pMat->m32; pOut->m24 = pMat->m24;
		pOut->m31 = pMat->m13; pOut->m32 =  pMat->m23; pOut->m33 = pMat->m33; pOut->m34 = pMat->m43;
		pOut->m41 = pMat->m14; pOut->m42 =  pMat->m24; pOut->m43 = pMat->m34; pOut->m44 = pMat->m44;
	}
	else
	{
		std::swap(pOut->m12, pOut->m21);
		std::swap(pOut->m13, pOut->m31);
		std::swap(pOut->m14, pOut->m41);

		std::swap(pOut->m23, pOut->m32);
		std::swap(pOut->m24, pOut->m42);
		
		std::swap(pOut->m34, pOut->m43);
	}

	return pOut;
#endif
}

inline v3dVector4_t* v3dxVec4Transform(v3dVector4_t* pOut,const v3dVector4_t* pVec,
											const v3dMatrix4_t* pMatrix)
{
	if( pOut==pVec )
	{
		v3dVector4_t vCalc = *pVec;
		pOut->x = pMatrix->m11*vCalc.x+pMatrix->m21*vCalc.y+pMatrix->m31*vCalc.z+pMatrix->m41*vCalc.w;
		pOut->y = pMatrix->m12*vCalc.x+pMatrix->m22*vCalc.y+pMatrix->m32*vCalc.z+pMatrix->m42*vCalc.w;
		pOut->z = pMatrix->m13*vCalc.x+pMatrix->m23*vCalc.y+pMatrix->m33*vCalc.z+pMatrix->m43*vCalc.w;
		pOut->w = pMatrix->m14*vCalc.x+pMatrix->m24*vCalc.y+pMatrix->m34*vCalc.z+pMatrix->m44*vCalc.w;
	}
	else
	{
		pOut->x = pMatrix->m11*pVec->x+pMatrix->m21*pVec->y+pMatrix->m31*pVec->z+pMatrix->m41*pVec->w;
		pOut->y = pMatrix->m12*pVec->x+pMatrix->m22*pVec->y+pMatrix->m32*pVec->z+pMatrix->m42*pVec->w;
		pOut->z = pMatrix->m13*pVec->x+pMatrix->m23*pVec->y+pMatrix->m33*pVec->z+pMatrix->m43*pVec->w;
		pOut->w = pMatrix->m14*pVec->x+pMatrix->m24*pVec->y+pMatrix->m34*pVec->z+pMatrix->m44*pVec->w;
	}

	return pOut;
}

/**

**/
inline v3dVector4_t* v3dxVec3TransformFull(v3dVector4_t* pOut,const v3dVector3_t* pVec,
													  const v3dMatrix4_t* pMatrix){
	pOut->x = pMatrix->m11*pVec->x+pMatrix->m21*pVec->y+pMatrix->m31*pVec->z+pMatrix->m41;
	pOut->y = pMatrix->m12*pVec->x+pMatrix->m22*pVec->y+pMatrix->m32*pVec->z+pMatrix->m42;
	pOut->z = pMatrix->m13*pVec->x+pMatrix->m23*pVec->y+pMatrix->m33*pVec->z+pMatrix->m43;
	pOut->w = pMatrix->m14*pVec->x+pMatrix->m24*pVec->y+pMatrix->m34*pVec->z+pMatrix->m44;
	
	return pOut;
}

/**
	
**/
inline v3dVector3_t* v3dxVec3TransformCoord(v3dVector3_t* pOut,const v3dVector3_t* pVec,
													   const v3dMatrix4_t* pMatrix)
{
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dVector3_t*)D3DXVec3TransformCoord( (D3DXVECTOR3*)pOut , (const D3DXVECTOR3*)pVec , (const D3DXMATRIX*)pMatrix );
#else
	float w;
	if( pVec == pOut ){
		v3dVector3_t vCalc = *pVec;
		pOut->x = pMatrix->m11*vCalc.x+pMatrix->m21*vCalc.y+pMatrix->m31*vCalc.z+pMatrix->m41;
		pOut->y = pMatrix->m12*vCalc.x+pMatrix->m22*vCalc.y+pMatrix->m32*vCalc.z+pMatrix->m42;
		pOut->z = pMatrix->m13*vCalc.x+pMatrix->m23*vCalc.y+pMatrix->m33*vCalc.z+pMatrix->m43;
		w = pMatrix->m14*vCalc.x+pMatrix->m24*vCalc.y+pMatrix->m34*vCalc.z+pMatrix->m44;
		pOut->x /= w;
		pOut->y /= w;
		pOut->z /= w;
	}
	else{
		pOut->x = pMatrix->m11*pVec->x+pMatrix->m21*pVec->y+pMatrix->m31*pVec->z+pMatrix->m41;
		pOut->y = pMatrix->m12*pVec->x+pMatrix->m22*pVec->y+pMatrix->m32*pVec->z+pMatrix->m42;
		pOut->z = pMatrix->m13*pVec->x+pMatrix->m23*pVec->y+pMatrix->m33*pVec->z+pMatrix->m43;
		w = pMatrix->m14*pVec->x+pMatrix->m24*pVec->y+pMatrix->m34*pVec->z+pMatrix->m44;
		pOut->x /= w;
		pOut->y /= w;
		pOut->z /= w;
	}
	return pOut;
#endif
}

inline v3dVector3_t* v3dxVec3TransformCoord(v3dVector3_t* pOut, const v3dVector3_t* pVec, const v3dVector4_t* rotation)
{
	float x = rotation->x + rotation->x;
	float y = rotation->y + rotation->y;
	float z = rotation->z + rotation->z;
	float wx = rotation->w * x;
	float wy = rotation->w * y;
	float wz = rotation->w * z;
	float xx = rotation->x * x;
	float xy = rotation->x * y;
	float xz = rotation->x * z;
	float yy = rotation->y * y;
	float yz = rotation->y * z;
	float zz = rotation->z * z;

	pOut->x = ((pVec->x * ((1.0f - yy) - zz)) + (pVec->y * (xy - wz))) + (pVec->z * (xz + wy));
	pOut->y = ((pVec->x * (xy + wz)) + (pVec->y * ((1.0f - xx) - zz))) + (pVec->z * (yz - wx));
	pOut->z = ((pVec->x * (xz - wy)) + (pVec->y * (yz + wx))) + (pVec->z * ((1.0f - xx) - yy));

	return pOut;
}

inline v3dMatrix4_t* v3dxTransposeMatrix4( v3dMatrix4_t* pOut , const v3dMatrix4_t* pIn )
{
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dMatrix4_t*)D3DXMatrixTranspose( (D3DXMATRIX*)pOut , (const D3DXMATRIX*)pIn );
#else
	if( pOut!=pIn ){
		for( int i=0 ; i<4 ; i++ ){
			for( int j=0 ; j<4 ; j++ ){
				pOut->m[i][j] = pIn->m[j][i];
			}
		}
	}else{
		float fSwap;
		for( int i=0 ; i<4 ; i++ ){
			for( int j=0 ; j<i ; j++ ){
				if( i!=j ){
					fSwap = pOut->m[i][j];
					pOut->m[i][j] = pOut->m[j][i];
					pOut->m[j][i] = fSwap;
				}
			}
		}
	}
	return pOut;
#endif
}

inline float v3dxMatrix4Determinant(const v3dMatrix4_t* m)
{
	float det;
	det  = m->m11 * (  m->m22 * m->m33 * m->m44 
					- m->m24 * m->m33 * m->m42
					+ m->m32 * m->m43 * m->m24
					- m->m23 * m->m32 * m->m44
					+ m->m42 * m->m23 * m->m34
					- m->m22 * m->m34 * m->m43);
	det -= m->m12 * (  m->m21 * m->m33 * m->m44
					- m->m23 * m->m31 * m->m44
					+ m->m41 * m->m23 * m->m34 
					- m->m24 * m->m33 * m->m41
					+ m->m31 * m->m43 * m->m24
					- m->m21 * m->m34 * m->m43);
	det += m->m13 * (  m->m21 * m->m32 * m->m44
					- m->m24 * m->m32 * m->m41
					+ m->m31 * m->m42 * m->m24
					- m->m22 * m->m31 * m->m44
					+ m->m41 * m->m22 * m->m34
					- m->m21 * m->m34 * m->m42);
	det -= m->m14 * (  m->m21 * m->m32 * m->m43
					- m->m23 * m->m32 * m->m41
					+ m->m31 * m->m42 * m->m23
					- m->m22 * m->m31 * m->m43
					+ m->m41 * m->m22 * m->m33
					- m->m21 * m->m33 * m->m42);
	return det;
}



#define MAT(m,r,c) (m)[r][c]
#define RETURN_ZERO \
{ \
	for( int i = 0; i < 4; ++i )	\
	{	\
		for (int j = 0; j < 4; ++j)	\
		{	\
			out->m[i][j] = 0.0f;	\
		}	\
	}	\
	return out;	\
}
// 4x4 matrix inversion by Gaussian reduction with partial pivoting followed by back/substitution;
// with loops manually unrolled.
#define SWAP_ROWS(a, b) { float *_tmp = a; (a)=(b); (b)=_tmp; }
inline v3dMatrix4_t* v3dxMatrix4Inverse_CXX(v3dMatrix4_t* out,const v3dMatrix4_t* mat , float* pDet)
{
	float wtmp[4][8];
	float m0, m1, m2, m3, s;
	float *r0, *r1, *r2, *r3;

	r0 = wtmp[0], r1 = wtmp[1], r2 = wtmp[2], r3 = wtmp[3];

	r0[0] = MAT(mat->m, 0, 0), r0[1] = MAT(mat->m, 0, 1),
		r0[2] = MAT(mat->m, 0, 2), r0[3] = MAT(mat->m, 0, 3),
		r0[4] = 1.0, r0[5] = r0[6] = r0[7] = 0.0,

		r1[0] = MAT(mat->m, 1, 0), r1[1] = MAT(mat->m, 1, 1),
		r1[2] = MAT(mat->m, 1, 2), r1[3] = MAT(mat->m, 1, 3),
		r1[5] = 1.0, r1[4] = r1[6] = r1[7] = 0.0,

		r2[0] = MAT(mat->m, 2, 0), r2[1] = MAT(mat->m, 2, 1),
		r2[2] = MAT(mat->m, 2, 2), r2[3] = MAT(mat->m, 2, 3),
		r2[6] = 1.0, r2[4] = r2[5] = r2[7] = 0.0,

		r3[0] = MAT(mat->m, 3, 0), r3[1] = MAT(mat->m, 3, 1),
		r3[2] = MAT(mat->m, 3, 2), r3[3] = MAT(mat->m, 3, 3),
		r3[7] = 1.0, r3[4] = r3[5] = r3[6] = 0.0;

	/* choose pivot - or die */
	if (Math::Abs(r3[0]) > Math::Abs(r2[0])) SWAP_ROWS(r3, r2);
	if (Math::Abs(r2[0]) > Math::Abs(r1[0])) SWAP_ROWS(r2, r1);
	if (Math::Abs(r1[0]) > Math::Abs(r0[0])) SWAP_ROWS(r1, r0);
	if (0.0F == r0[0]) RETURN_ZERO

		/* eliminate first variable     */
		m1 = r1[0] / r0[0]; m2 = r2[0] / r0[0]; m3 = r3[0] / r0[0];
	s = r0[1]; r1[1] -= m1 * s; r2[1] -= m2 * s; r3[1] -= m3 * s;
	s = r0[2]; r1[2] -= m1 * s; r2[2] -= m2 * s; r3[2] -= m3 * s;
	s = r0[3]; r1[3] -= m1 * s; r2[3] -= m2 * s; r3[3] -= m3 * s;
	s = r0[4];
	if (s != 0.0F) { r1[4] -= m1 * s; r2[4] -= m2 * s; r3[4] -= m3 * s; }
	s = r0[5];
	if (s != 0.0F) { r1[5] -= m1 * s; r2[5] -= m2 * s; r3[5] -= m3 * s; }
	s = r0[6];
	if (s != 0.0F) { r1[6] -= m1 * s; r2[6] -= m2 * s; r3[6] -= m3 * s; }
	s = r0[7];
	if (s != 0.0F) { r1[7] -= m1 * s; r2[7] -= m2 * s; r3[7] -= m3 * s; }

	/* choose pivot - or die */
	if (Math::Abs(r3[1]) > Math::Abs(r2[1])) SWAP_ROWS(r3, r2);
	if (Math::Abs(r2[1]) > Math::Abs(r1[1])) SWAP_ROWS(r2, r1);
	if (0.0F == r1[1]) RETURN_ZERO;

	/* eliminate second variable */
	m2 = r2[1] / r1[1]; m3 = r3[1] / r1[1];
	r2[2] -= m2 * r1[2]; r3[2] -= m3 * r1[2];
	r2[3] -= m2 * r1[3]; r3[3] -= m3 * r1[3];
	s = r1[4]; if (0.0F != s) { r2[4] -= m2 * s; r3[4] -= m3 * s; }
	s = r1[5]; if (0.0F != s) { r2[5] -= m2 * s; r3[5] -= m3 * s; }
	s = r1[6]; if (0.0F != s) { r2[6] -= m2 * s; r3[6] -= m3 * s; }
	s = r1[7]; if (0.0F != s) { r2[7] -= m2 * s; r3[7] -= m3 * s; }

	/* choose pivot - or die */
	if (Math::Abs(r3[2]) > Math::Abs(r2[2])) SWAP_ROWS(r3, r2);
	if (0.0F == r2[2]) RETURN_ZERO;

	/* eliminate third variable */
	m3 = r3[2] / r2[2];
	r3[3] -= m3 * r2[3], r3[4] -= m3 * r2[4],
		r3[5] -= m3 * r2[5], r3[6] -= m3 * r2[6],
		r3[7] -= m3 * r2[7];

	/* last check */
	if (0.0F == r3[3]) RETURN_ZERO;

	s = 1.0F / r3[3];             /* now back substitute row 3 */
	r3[4] *= s; r3[5] *= s; r3[6] *= s; r3[7] *= s;

	m2 = r2[3];                 /* now back substitute row 2 */
	s = 1.0F / r2[2];
	r2[4] = s * (r2[4] - r3[4] * m2), r2[5] = s * (r2[5] - r3[5] * m2),
		r2[6] = s * (r2[6] - r3[6] * m2), r2[7] = s * (r2[7] - r3[7] * m2);
	m1 = r1[3];
	r1[4] -= r3[4] * m1, r1[5] -= r3[5] * m1,
		r1[6] -= r3[6] * m1, r1[7] -= r3[7] * m1;
	m0 = r0[3];
	r0[4] -= r3[4] * m0, r0[5] -= r3[5] * m0,
		r0[6] -= r3[6] * m0, r0[7] -= r3[7] * m0;

	m1 = r1[2];                 /* now back substitute row 1 */
	s = 1.0F / r1[1];
	r1[4] = s * (r1[4] - r2[4] * m1), r1[5] = s * (r1[5] - r2[5] * m1),
		r1[6] = s * (r1[6] - r2[6] * m1), r1[7] = s * (r1[7] - r2[7] * m1);
	m0 = r0[2];
	r0[4] -= r2[4] * m0, r0[5] -= r2[5] * m0,
		r0[6] -= r2[6] * m0, r0[7] -= r2[7] * m0;

	m0 = r0[1];                 /* now back substitute row 0 */
	s = 1.0F / r0[0];
	r0[4] = s * (r0[4] - r1[4] * m0), r0[5] = s * (r0[5] - r1[5] * m0),
		r0[6] = s * (r0[6] - r1[6] * m0), r0[7] = s * (r0[7] - r1[7] * m0);

	MAT(out->m, 0, 0) = r0[4]; MAT(out->m, 0, 1) = r0[5], MAT(out->m, 0, 2) = r0[6]; MAT(out->m, 0, 3) = r0[7],
		MAT(out->m, 1, 0) = r1[4]; MAT(out->m, 1, 1) = r1[5], MAT(out->m, 1, 2) = r1[6]; MAT(out->m, 1, 3) = r1[7],
		MAT(out->m, 2, 0) = r2[4]; MAT(out->m, 2, 1) = r2[5], MAT(out->m, 2, 2) = r2[6]; MAT(out->m, 2, 3) = r2[7],
		MAT(out->m, 3, 0) = r3[4]; MAT(out->m, 3, 1) = r3[5], MAT(out->m, 3, 2) = r3[6]; MAT(out->m, 3, 3) = r3[7];

	return out;
}

#define SWAP_ROWS_D(a, b) { double *_tmp = a; (a)=(b); (b)=_tmp; }
inline v3dDMatrix4_t* v3dxDMatrix4Inverse_CXX(v3dDMatrix4_t* out, const v3dDMatrix4_t* mat, double* pDet)
{
	double wtmp[4][8];
	double m0, m1, m2, m3, s;
	double* r0, * r1, * r2, * r3;

	r0 = wtmp[0], r1 = wtmp[1], r2 = wtmp[2], r3 = wtmp[3];

	r0[0] = MAT(mat->m, 0, 0), r0[1] = MAT(mat->m, 0, 1),
		r0[2] = MAT(mat->m, 0, 2), r0[3] = MAT(mat->m, 0, 3),
		r0[4] = 1.0, r0[5] = r0[6] = r0[7] = 0.0,

		r1[0] = MAT(mat->m, 1, 0), r1[1] = MAT(mat->m, 1, 1),
		r1[2] = MAT(mat->m, 1, 2), r1[3] = MAT(mat->m, 1, 3),
		r1[5] = 1.0, r1[4] = r1[6] = r1[7] = 0.0,

		r2[0] = MAT(mat->m, 2, 0), r2[1] = MAT(mat->m, 2, 1),
		r2[2] = MAT(mat->m, 2, 2), r2[3] = MAT(mat->m, 2, 3),
		r2[6] = 1.0, r2[4] = r2[5] = r2[7] = 0.0,

		r3[0] = MAT(mat->m, 3, 0), r3[1] = MAT(mat->m, 3, 1),
		r3[2] = MAT(mat->m, 3, 2), r3[3] = MAT(mat->m, 3, 3),
		r3[7] = 1.0, r3[4] = r3[5] = r3[6] = 0.0;

	/* choose pivot - or die */
	if (Math::D_Abs(r3[0]) > Math::D_Abs(r2[0])) SWAP_ROWS_D(r3, r2);
	if (Math::D_Abs(r2[0]) > Math::D_Abs(r1[0])) SWAP_ROWS_D(r2, r1);
	if (Math::D_Abs(r1[0]) > Math::D_Abs(r0[0])) SWAP_ROWS_D(r1, r0);
	if (0.0F == r0[0]) RETURN_ZERO

		/* eliminate first variable     */
		m1 = r1[0] / r0[0]; m2 = r2[0] / r0[0]; m3 = r3[0] / r0[0];
	s = r0[1]; r1[1] -= m1 * s; r2[1] -= m2 * s; r3[1] -= m3 * s;
	s = r0[2]; r1[2] -= m1 * s; r2[2] -= m2 * s; r3[2] -= m3 * s;
	s = r0[3]; r1[3] -= m1 * s; r2[3] -= m2 * s; r3[3] -= m3 * s;
	s = r0[4];
	if (s != 0.0F) { r1[4] -= m1 * s; r2[4] -= m2 * s; r3[4] -= m3 * s; }
	s = r0[5];
	if (s != 0.0F) { r1[5] -= m1 * s; r2[5] -= m2 * s; r3[5] -= m3 * s; }
	s = r0[6];
	if (s != 0.0F) { r1[6] -= m1 * s; r2[6] -= m2 * s; r3[6] -= m3 * s; }
	s = r0[7];
	if (s != 0.0F) { r1[7] -= m1 * s; r2[7] -= m2 * s; r3[7] -= m3 * s; }

	/* choose pivot - or die */
	if (Math::D_Abs(r3[1]) > Math::D_Abs(r2[1])) SWAP_ROWS_D(r3, r2);
	if (Math::D_Abs(r2[1]) > Math::D_Abs(r1[1])) SWAP_ROWS_D(r2, r1);
	if (0.0F == r1[1]) RETURN_ZERO;

	/* eliminate second variable */
	m2 = r2[1] / r1[1]; m3 = r3[1] / r1[1];
	r2[2] -= m2 * r1[2]; r3[2] -= m3 * r1[2];
	r2[3] -= m2 * r1[3]; r3[3] -= m3 * r1[3];
	s = r1[4]; if (0.0F != s) { r2[4] -= m2 * s; r3[4] -= m3 * s; }
	s = r1[5]; if (0.0F != s) { r2[5] -= m2 * s; r3[5] -= m3 * s; }
	s = r1[6]; if (0.0F != s) { r2[6] -= m2 * s; r3[6] -= m3 * s; }
	s = r1[7]; if (0.0F != s) { r2[7] -= m2 * s; r3[7] -= m3 * s; }

	/* choose pivot - or die */
	if (Math::D_Abs(r3[2]) > Math::D_Abs(r2[2])) SWAP_ROWS_D(r3, r2);
	if (0.0F == r2[2]) RETURN_ZERO;

	/* eliminate third variable */
	m3 = r3[2] / r2[2];
	r3[3] -= m3 * r2[3], r3[4] -= m3 * r2[4],
		r3[5] -= m3 * r2[5], r3[6] -= m3 * r2[6],
		r3[7] -= m3 * r2[7];

	/* last check */
	if (0.0F == r3[3]) RETURN_ZERO;

	s = 1.0F / r3[3];             /* now back substitute row 3 */
	r3[4] *= s; r3[5] *= s; r3[6] *= s; r3[7] *= s;

	m2 = r2[3];                 /* now back substitute row 2 */
	s = 1.0F / r2[2];
	r2[4] = s * (r2[4] - r3[4] * m2), r2[5] = s * (r2[5] - r3[5] * m2),
		r2[6] = s * (r2[6] - r3[6] * m2), r2[7] = s * (r2[7] - r3[7] * m2);
	m1 = r1[3];
	r1[4] -= r3[4] * m1, r1[5] -= r3[5] * m1,
		r1[6] -= r3[6] * m1, r1[7] -= r3[7] * m1;
	m0 = r0[3];
	r0[4] -= r3[4] * m0, r0[5] -= r3[5] * m0,
		r0[6] -= r3[6] * m0, r0[7] -= r3[7] * m0;

	m1 = r1[2];                 /* now back substitute row 1 */
	s = 1.0F / r1[1];
	r1[4] = s * (r1[4] - r2[4] * m1), r1[5] = s * (r1[5] - r2[5] * m1),
		r1[6] = s * (r1[6] - r2[6] * m1), r1[7] = s * (r1[7] - r2[7] * m1);
	m0 = r0[2];
	r0[4] -= r2[4] * m0, r0[5] -= r2[5] * m0,
		r0[6] -= r2[6] * m0, r0[7] -= r2[7] * m0;

	m0 = r0[1];                 /* now back substitute row 0 */
	s = 1.0F / r0[0];
	r0[4] = s * (r0[4] - r1[4] * m0), r0[5] = s * (r0[5] - r1[5] * m0),
		r0[6] = s * (r0[6] - r1[6] * m0), r0[7] = s * (r0[7] - r1[7] * m0);

	MAT(out->m, 0, 0) = r0[4]; MAT(out->m, 0, 1) = r0[5], MAT(out->m, 0, 2) = r0[6]; MAT(out->m, 0, 3) = r0[7],
		MAT(out->m, 1, 0) = r1[4]; MAT(out->m, 1, 1) = r1[5], MAT(out->m, 1, 2) = r1[6]; MAT(out->m, 1, 3) = r1[7],
		MAT(out->m, 2, 0) = r2[4]; MAT(out->m, 2, 1) = r2[5], MAT(out->m, 2, 2) = r2[6]; MAT(out->m, 2, 3) = r2[7],
		MAT(out->m, 3, 0) = r3[4]; MAT(out->m, 3, 1) = r3[5], MAT(out->m, 3, 2) = r3[6]; MAT(out->m, 3, 3) = r3[7];

	return out;
}

inline v3dVector3_t* v3dxVec3TransformNormal( v3dVector3_t* pOut,const v3dVector3_t* pVec,
											  const v3dMatrix4_t* pMatrix )
{
#if defined(USE_DXMATH) && defined(USE_DX)
	return (v3dVector3_t*)D3DXVec3TransformNormal( (D3DXVECTOR3  *)pOut, (CONST D3DXVECTOR3*)pVec , (CONST D3DXMATRIX *)pMatrix );
#else
	if( pVec == pOut ){
		v3dVector3_t vCalc = *pVec;
		pOut->x = pMatrix->m11*vCalc.x+pMatrix->m21*vCalc.y+pMatrix->m31*vCalc.z;
		pOut->y = pMatrix->m12*vCalc.x+pMatrix->m22*vCalc.y+pMatrix->m32*vCalc.z;
		pOut->z = pMatrix->m13*vCalc.x+pMatrix->m23*vCalc.y+pMatrix->m33*vCalc.z;
	}
	else{
		pOut->x = pMatrix->m11*pVec->x+pMatrix->m21*pVec->y+pMatrix->m31*pVec->z;
		pOut->y = pMatrix->m12*pVec->x+pMatrix->m22*pVec->y+pMatrix->m32*pVec->z;
		pOut->z = pMatrix->m13*pVec->x+pMatrix->m23*pVec->y+pMatrix->m33*pVec->z;
	}
	return pOut;
#endif
}

inline v3dMatrix4_t* v3dxMatrix4RotationAxis( v3dMatrix4_t* Out,const v3dVector3_t* vAxis,float theta )
{
   /* This function performs an axis/angle rotation. (x,y,z) is any 
      vector on the axis. For greater speed, always use a unit vector 
      (length = 1). In this version, we will assume an arbitrary 
      length and normalize. */
   // normalize
   float length = sqrtf(vAxis->x*vAxis->x + vAxis->y*vAxis->y + vAxis->z*vAxis->z);

   // too close to 0, can't make a normalized vector
   if (length < 0.000001f)
      return NULL;

   float x = vAxis->x / length;
   float y = vAxis->y / length;
   float z = vAxis->z / length;

   // do the trig
   float c = cosf(-theta);
   float s = sinf(-theta);
   float t = 1.0f - c;   

   // build the rotation matrix
   Out->m11 = t*x*x + c;
   Out->m12 = t*x*y - s*z;
   Out->m13 = t*x*z + s*y;
   Out->m14 = 0;

   Out->m21 = t*x*y + s*z;
   Out->m22 = t*y*y + c;
   Out->m23 = t*y*z - s*x;
   Out->m24 = 0;

   Out->m31 = t*x*z - s*y;
   Out->m32 = t*y*z + s*x;
   Out->m33 = t*z*z + c;
   Out->m34 = 0;

   Out->m41 = Out->m42 = Out->m43 = 0.0f;
   Out->m44 = 1.0f;

   return Out;
}

inline v3dMatrix4_t* v3dxMatrix4MakePose(v3dMatrix4_t* pMatrix,const v3dVector3_t* pvPos,
									 const v3dVector3_t* pvDir,
									 const v3dVector3_t* pvUp,
									 const v3dVector3_t* pvRight)
{
	pMatrix->m11 = pvRight->x;  pMatrix->m21 = pvRight->y;  pMatrix->m31 = pvRight->z;
	pMatrix->m12 = pvUp->x;     pMatrix->m22 = pvUp->y;     pMatrix->m32 = pvUp->z;
	pMatrix->m13 = pvDir->x;    pMatrix->m23 = pvDir->y;    pMatrix->m33 = pvDir->z;

	pMatrix->m41 = pvPos->x;
    pMatrix->m42 = pvPos->y;
    pMatrix->m43 = pvPos->z;

	pMatrix->m14 = 0.f;
	pMatrix->m24 = 0.f;
	pMatrix->m34 = 0.f;
	pMatrix->m44 = 1.f;
	return pMatrix;
}

inline v3dMatrix4_t* v3dxMatrix4View(v3dMatrix4_t* pMatrix,const v3dVector3_t* pvPos,
									 const v3dVector3_t* pvDir,
									 const v3dVector3_t* pvUp,
									 const v3dVector3_t* pvRight)
{
	pMatrix->m11 = pvRight->x;    pMatrix->m12 = pvUp->x;    pMatrix->m13 = pvDir->x;
    pMatrix->m21 = pvRight->y;    pMatrix->m22 = pvUp->y;    pMatrix->m23 = pvDir->y;
    pMatrix->m31 = pvRight->z;    pMatrix->m32 = pvUp->z;    pMatrix->m33 = pvDir->z;

	pMatrix->m41 = -v3dxVec3Dot( pvPos, pvRight );
    pMatrix->m42 = -v3dxVec3Dot( pvPos, pvUp );
    pMatrix->m43 = -v3dxVec3Dot( pvPos, pvDir );

	pMatrix->m14 = 0.f;
	pMatrix->m24 = 0.f;
	pMatrix->m34 = 0.f;
	pMatrix->m44 = 1.f;
	return pMatrix;
}

inline v3dMatrix4_t* v3dxMatrixLookDirLH( v3dMatrix4_t* pOut,const v3dVector3_t* pvPos,
		const v3dVector3_t* pvDir , const v3dVector3_t* pvUp ){
	v3dVector3_t vDir,vRight,vUp;
	v3dxVec3Normalize( &vDir , pvDir );
	
	v3dxVec3Cross( &vRight , pvUp , & vDir );
	v3dxVec3Normalize( &vRight , &vRight );
	v3dxVec3Cross( &vUp , &vDir , &vRight );

	return v3dxMatrix4View( pOut , pvPos , &vDir , &vUp , &vRight );
}

/**

*/
inline int v3dxWhichSide2D(const v3dVector2_t* v,
	const v3dVector2_t* s1, const v3dVector2_t* s2) {
	float k = (s1->y - v->y)*(s2->x - s1->x);
	float k1 = (s1->x - v->x)*(s2->y - s1->y);
	if (k < k1)
		return -1;
	else if (k > k1)
		return 1;
	else
		return 0;
}
/**

**/
inline void v3dxCalcNormal(v3dVector3_t* norm, const v3dVector3_t* v1,
	const v3dVector3_t* v2, const v3dVector3_t* v3, vBOOL bNormalize = FALSE) {
	v3dVector3_t vTemp1, vTemp2;
	v3dxVec3Sub(&vTemp1, v1, v3);
	v3dxVec3Sub(&vTemp2, v2, v3);
	v3dxVec3Cross(norm, &vTemp1, &vTemp2);
	if (bNormalize)
		v3dxVec3Normalize(norm, norm);
}

/**

*/
inline float v3dxCalSquareDistance2D(const v3dVector2_t* v1, const v3dVector2_t* v2) {
	return((v1->x - v2->x)*(v1->x - v2->x) + (v1->y - v2->y)*(v1->y - v2->y));
}
inline float v3dxCalSquareDistance(const v3dVector3_t* v1, const v3dVector3_t* v2)
{
	return((v1->x - v2->x)*(v1->x - v2->x) + (v1->y - v2->y)*(v1->y - v2->y) + (v1->z - v2->z)*(v1->z - v2->z));
}

extern "C"
{
	 VFX_API v3dMatrix4_t* v3dxMatrixMultiply(v3dMatrix4_t* pOut, const v3dMatrix4_t* mat1, const v3dMatrix4_t* mat2);
	 VFX_API v3dMatrix4_t* v3dxMatrix4Inverse(v3dMatrix4_t* out, const v3dMatrix4_t* mat, float* pDet);
	 VFX_API v3dMatrix4_t* v3dxMatrix4Transpose_extern(v3dMatrix4_t* pOut, const v3dMatrix4_t* pMat);
	 VFX_API v3dMatrix4_t* v3dxMatrix4PerspectiveEx(v3dMatrix4_t* pOut,
								const float near_plane,
								const float far_plane,
								const float fov_horiz,
								const float fov_vert);

	 VFX_API v3dMatrix4_t* v3dxMatrix4Perspective(v3dMatrix4_t* pOut,
								  float fovy,
								  float Aspect,
								  float zn,
								  float zf);

	 VFX_API v3dMatrix4_t* v3dxMatrix4Ortho(
								  v3dMatrix4_t* pOut,
								  float w,
								  float h,
								  float zn,
								  float zf);

	 VFX_API v3dMatrix4_t* v3dxMatrix4OrthoForDirLightShadow(v3dMatrix4_t* pOut, float w, float h, float zn, float zf, float TexelOffsetNdcX, float TexelOffsetNdcY);

	 VFX_API void v3dxMatrixOrthoOffCenterLH(v3dMatrix4_t* pOut, float MinX, float MaxX, float MinY, float MaxY,
		float ZNear, float ZFar);

	 VFX_API void v3dxReflect(v3dxMatrix4* m,const v3dxPlane3* plane);

	 VFX_API void v3dxShadow(v3dMatrix4_t* m,const v3dVector4_t* l,const v3dxPlane3* plane);

	 VFX_API int v3dxWhichSide3D (const v3dVector3_t* p,
									 const v3dVector3_t* v1, const v3dVector3_t* v2 , const v3dVector3_t* vSpire );
	 VFX_API int v3dxWhichSide3D_v2 (const v3dVector3_t* p,
							  const v3dVector3_t* v1, const v3dVector3_t* v2);

	 VFX_API float v3dxPointLineDistance(const v3dVector3_t* p,
							  const v3dVector3_t* l1, const v3dVector3_t* l2,v3dVector3_t* outClosePoint);

	 VFX_API float v3dxPointPlaneDistance(const v3dVector3_t* p, const v3dxPlane3* plane);

	 VFX_API float v3dxArea3 (const v3dVector3_t *a, const v3dVector3_t *b,
								 const v3dVector3_t *c);
	 VFX_API float v3dxCalAngleXZ(const v3dVector3_t* vect);
	 VFX_API float v3dxCalAngleYZ(const v3dVector3_t* vect);
	 VFX_API float v3dxCalAngleXY(const v3dVector3_t* vect);
	
	 VFX_API vBOOL v3dxLineIntersectPlane(
						 FLOAT *pfT
						 ,v3dxVector3 *pvPoint
						 ,const v3dxVector3 *pvFrom
						 ,const v3dxVector3 *pvLength
						 ,const v3dxVector3 *pvA
						 ,const v3dxVector3 *pvB
						 ,const v3dxVector3 *pvC
						 ,vBOOL bDoubleFace=FALSE);

	 VFX_API vBOOL v3dxLineIntersectTriangle(
							 float *pfT,
							 v3dxVector3 *pvPoint,
							 const v3dxVector3 *pvFrom,
							 const v3dxVector3 *pvLength,
							 const v3dxVector3 *pvA,
							 const v3dxVector3 *pvB,
							 const v3dxVector3 *pvC,
							 const v3dxBox3 *pLineBox = NULL,
							 const v3dxBox3 *pTriBox = NULL,
							 OUT v3dxVector3 *pvNormal = NULL ,
							 float fPrecision = 0.0f
							 );

	 VFX_API vBOOL v3dxLineIntersectTriangleEx(
							float *pfT,
							v3dxVector3 *pvPoint,
							v3dxVector3 *pvNormal,
							const v3dxVector3 *pvFrom,
							const v3dxVector3 *pvLength,
							const v3dxVector3 *pvA,
							const v3dxVector3 *pvB,
							const v3dxVector3 *pvC,
							const v3dxBox3 *pLineBox = NULL,
							const v3dxBox3 *pTriBox = NULL,
							float fPrecision = 0.0f
							);

	 VFX_API vBOOL v3dxLineIntersectBox3( float *pfT_n,
							 v3dxVector3 *pvPoint_n,
							 float *pfT_f,
							 v3dxVector3 *pvPoint_f,
							 const v3dxVector3 *pvFrom,
							 const v3dxVector3 *pvDir,
							 const v3dxBox3* pBox
							 );

	 VFX_API vBOOL v3dxLineIntersectBox3_v2( float *pfT_n,
										 v3dxVector3 *pvPoint_n,
										 float *pfT_f,
										 v3dxVector3 *pvPoint_f,
										 v3dxVector3 *pvNormal,
										 const v3dxVector3 *pvFrom,
										 const v3dxVector3 *pvDir,
										 const v3dxBox3* pBox
							);
	
	 VFX_API vBOOL v3dxSegIntersectTriangleD( const v3dxVector3& tr1,
  							const v3dxVector3& tr2, 
							const v3dxVector3& tr3,
							const v3dxSegment3& seg, 
							v3dxVector3& isect);


	 VFX_API vBOOL v3dxLineIntersectPlane_v1( const v3dxVector3& u,
								 const v3dxVector3& v,
								 float A, float B, float C, float D,
								 v3dxVector3& isect, float& dist);

	 VFX_API vBOOL v3dxLineIntersectPlane_v2( const v3dxVector3& u,
								 const v3dxVector3& v,
								 const v3dxPlane3& p, 
								 v3dxVector3& isect, 
								 float& dist);

	 VFX_API vBOOL v3dxPlaneIntersectPolygon( const v3dxPlane3& plane, v3dxPoly3* poly,
  							v3dxSegment3& seg);


	 VFX_API void	v3dxAABBVertexEdges(const v3dxVector3& aabb_min, const v3dxVector3& aabb_max, v3dxVector3 v[8], int e[12][2]);
	 VFX_API void	v3dxAABBInnerPlanes(const v3dxVector3& aabb_min, const v3dxVector3& aabb_max, v3dxPlane3 p[6]);

	 VFX_API void	v3dxZPolyCWInnerPlanes(v3dxVector3* pPoly, int Length, float MaxY, float MinY, std::vector<v3dxPlane3>& OutResult);


	 VFX_API vBOOL IntersectPlane3(v3dVector3_t & q,const v3dxPlane3 & p1,const v3dxPlane3 & p2,const v3dxPlane3 & p3);
	 VFX_API vBOOL IntersectPlane2(v3dxLine3 & l,const v3dxPlane3 & p1,const v3dxPlane3 & p2);

	 VFX_API void v3dxQuaternionToAxisAngle(const v3dxQuaternion *pQ, v3dxVector3 *pAxis, float *pAngle);
	 VFX_API v3dxQuaternion* v3dxQuaternionMultiply(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1, CONST v3dxQuaternion *pQ2);
	 VFX_API v3dxQuaternion* v3dxQuaternionSlerp(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1,CONST v3dxQuaternion *pQ2, float t);
	 VFX_API v3dxQuaternion* v3dxQuaternionRotationAxis(v3dxQuaternion *pOut, CONST v3dxVector3 *pV, float Angle);
	 VFX_API v3dxQuaternion* v3dxQuaternionRotationYawPitchRoll(v3dxQuaternion *pOut, float Yaw, float Pitch, float Roll);
	 VFX_API v3dxVector3*	v3dxYawPitchRollQuaternionRotation(v3dxQuaternion pIn, v3dxVector3* pOutOLAngle);
	 VFX_API v3dxMatrix4* v3dxMatrixRotationQuaternion(v3dxMatrix4 *pOut, CONST v3dxQuaternion *pQ);

	 VFX_API HRESULT v3dxMatrixDecompose(v3dxVector3 *pOutScale, v3dxQuaternion *pOutRotation, v3dxVector3 *pOutTranslation, const v3dxMatrix4 *pM);

	 VFX_API v3dxMatrix4* v3dxMatrixTransformationOrigin(v3dxMatrix4 *pOut, CONST v3dxVector3 *pScaling,
		CONST v3dxQuaternion *pRotation,
		CONST v3dxVector3 *pTranslation);
	 VFX_API v3dxMatrix4* v3dxMatrixTransformation(v3dxMatrix4 *pOut, CONST v3dxVector3 *pScalingCenter,
		CONST v3dxQuaternion *pScalingRotation, CONST v3dxVector3 *pScaling,
		CONST v3dxVector3 *pRotationCenter, CONST v3dxQuaternion *pRotation,
		CONST v3dxVector3 *pTranslation);

	 VFX_API v3dxMatrix4* v3dxMatrixReflect(v3dxMatrix4 *pOut, CONST v3dxPlane3 *pPlane);

	 VFX_API v3dVector4_t* v3dxVec3TransformArray(v3dVector4_t *pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n);
	 VFX_API v3dVector3_t* v3dxVec3TransformNormalArray(v3dVector3_t*pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n);

	 VFX_API v3dxMatrix4* v3dxMatrixAffineTransformation
		(v3dxMatrix4 *pOut, FLOAT Scaling, CONST v3dVector3_t *pRotationCenter,
			CONST v3dVector4_t *pRotation, CONST v3dVector3_t *pTranslation);

	 VFX_API v3dVector3_t* v3dxPlaneIntersectLine(v3dVector3_t *pOut, CONST v3dxPlane3 *pP, CONST v3dVector3_t *pV1,CONST v3dVector3_t *pV2);

	 VFX_API v3dxPlane3* v3dxPlaneScale(v3dxPlane3 *pOut, CONST v3dxPlane3 *pP, FLOAT s);
	// Barycentric interpolation.
	// Slerp(Slerp(Q1, Q2, f+g), Slerp(Q1, Q3, f+g), g/(f+g))
	 VFX_API v3dxQuaternion* v3dxQuaternionBaryCentric(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1,
			CONST v3dxQuaternion *pQ2, CONST v3dxQuaternion *pQ3,FLOAT f, FLOAT g);

	 VFX_API v3dxQuaternion* v3dxQuaternionExp(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ);
	
	 VFX_API v3dxQuaternion* v3dxQuaternionLn(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ);

	// Build a quaternion from a rotation matrix.
	 VFX_API v3dxQuaternion* v3dxQuaternionRotationMatrix(v3dxQuaternion *pOut, CONST v3dxMatrix4 *pM);

	// Spherical quadrangle interpolation.
	// Slerp(Slerp(Q1, C, t), Slerp(A, B, t), 2t(1-t))
	 VFX_API v3dxQuaternion* v3dxQuaternionSquad(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1,CONST v3dxQuaternion *pA, CONST v3dxQuaternion *pB,CONST v3dxQuaternion *pC, FLOAT t);

	 VFX_API v3dMatrix4_t* v3dxMatrixLookAtLH(v3dMatrix4_t* pOut, const v3dVector3_t* pvPos,const v3dVector3_t* pvAt, const v3dVector3_t* pvUp);

	 VFX_API v3dVector3_t* v3dxVec3TransformCoordArray(v3dVector3_t *pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n);
	 VFX_API HRESULT v3dxComputeBoundingSphere(
			CONST v3dVector3_t *pFirstPosition,  // pointer to first position
			DWORD NumVertices,
			DWORD dwStride,                     // count in bytes to subsequent position vectors
			v3dVector3_t *pCenter,
			FLOAT *pRadius);

	 VFX_API vBOOL v3dxIntersectTri(
			CONST v3dxVector3 *p0,           // Triangle vertex 0 position
			CONST v3dxVector3 *p1,           // Triangle vertex 1 position
			CONST v3dxVector3 *p2,           // Triangle vertex 2 position
			CONST v3dxVector3 *pRayPos,      // Ray origin
			CONST v3dxVector3 *pRayDir,      // Ray direction
			FLOAT *pU,                       // Barycentric Hit Coordinates
			FLOAT *pV,                       // Barycentric Hit Coordinates
			FLOAT *pDist);                   // Ray-Intersection Parameter Distance

	 VFX_API v3dxPlane3* WINAPI v3dxPlaneTransform(v3dxPlane3* pout,
		 const v3dxPlane3* pplane,
		 const v3dxMatrix4* pm);
}

#endif//endif __V3DXMATH__H__