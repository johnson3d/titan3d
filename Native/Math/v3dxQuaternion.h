/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxquaternion.h
	Created Time:		2003-05-12   16:30
	Modify Time:
	Original Author:	johnson
	Abstract:

	Note:

*********************************************************************/

#ifndef __v3dxQuaternion__H__
#define __v3dxQuaternion__H__

#include "vfxGeomTypes.h"
#include "v3dxMath.h"

class v3dxVector3;
class v3dxMatrix3;
class v3dxMatrix4;

#pragma pack(push,4)

class v3dxQuaternion : public v3dVector4_t
{
public:
	//Construction =================================================
	v3dxQuaternion();
	v3dxQuaternion(float p_x, float p_y, float p_z, float p_w);
	/// construction by euler 
	v3dxQuaternion(float p_x, float p_y, float p_z);
	v3dxQuaternion(v3dxVector3 v3, float fRadianAngle);
	v3dxQuaternion(const v3dxQuaternion& rkQ);

	~v3dxQuaternion();

	inline bool IsIdentity() const {
		return *this == IDENTITY;
	}

	//Convert method ================================================
	inline void axisRadianAngleToQuat(const v3dxVector3& axis, float fRadianAngle)
	{
		v3dxQuaternionRotationAxis((v3dxQuaternion*)this, &axis, fRadianAngle);
	}
	inline void eulerRadianAnglesToQuat(float p_x, float p_y, float p_z)
	{
		v3dxQuaternionRotationYawPitchRoll(this, p_y, p_x, p_z);
	}
	void getAxisRadianAngle(v3dxVector3 &v3, float &fAngle) const;
	//inline void getEulerRadianAngles(float &p_x, float &p_y, float &p_z) const
	//{
	//	v3dxVector3 vAxis;float fAngle;
	//	getAxisRadianAngle( vAxis , fAngle );
	//	//vAxis...轴
	//}
	// void eularToMe(float fRadX, float fRadY, float fRadZ);
	void fromRotationMatrix(const v3dxMatrix3& kRot);
	void fromRotationMatrix(const v3dxMatrix4& kRot);

	void toRotationMatrix(v3dxMatrix3& Matrix3) const;
	void toRotationMatrix(v3dxMatrix4& Matrix4) const;

	void FromAngleAxis(const float& rfAngle, const v3dxVector3& rkAxis);
	void ToAngleAxis(float &rfAngle, v3dxVector3& rkAxis) const;

	// void fromAxes (const v3dxVector3* akAxis);
	//通过3个轴，创建四元数
	void fromAxes(const v3dxVector3& xAxis, const v3dxVector3& yAxis, const v3dxVector3& zAxis);
	// void toAxis(v3dxVector3 *pvAxis) const;
	//四元数得到三个坐标轴
	void toAxis(v3dxVector3 &vAxisX, v3dxVector3 &vAxisY, v3dxVector3 &vAxisZ) const;
	//分别得到3个轴
	v3dxVector3 xAxis(void);
	v3dxVector3 yAxis(void);
	v3dxVector3 zAxis(void);

	// spherical linear interpolation
	void slerp(const v3dxQuaternion &p_Q, float fSlerp);
	static void slerp(v3dxQuaternion& pOutQuat, float fT, const v3dxQuaternion& rkP,
		const v3dxQuaternion& rkQ, bool shortestPath = false);

	static v3dxQuaternion slerpExtraSpins(float fT,
		const v3dxQuaternion& rkP, const v3dxQuaternion& rkQ,
		int iExtraSpins);

	//Basic operation ===============================================
	/// the identity of quaternion had two mode, we usually use the {1.0f,0.0f,0.0f,0.0f}
	float dot(const v3dxQuaternion& rkQ) const;  // dot product
	void identity();
	float magnitude() const;
	bool isNormalize() const;
	float norm() const;
	void normalize();
	v3dxQuaternion exp() const;
	v3dxQuaternion log() const;
	/// inverse of the quaternion
	v3dxQuaternion inverse() const;
	// apply to unit-length quaternion
	v3dxQuaternion unitInverse() const;

	v3dxQuaternion* Multiply(const v3dxQuaternion* rhs);

	static float AngularDistance(const v3dxQuaternion& lhs, const v3dxQuaternion& rhs);

	void set(float p_w, float p_x, float p_y, float p_z);
	/// get quaternions by sub script
	float& operator [] (int i);
	float operator [](int i) const;

	v3dxQuaternion &operator =(const v3dxQuaternion &p_Quatern);

	//inline friend v3dxQuaternion operator + (const v3dxQuaternion & quat1,const v3dxQuaternion & quat2);
	//inline friend v3dxQuaternion operator - (const v3dxQuaternion& quat1,const v3dxQuaternion& quat2);
	friend v3dxQuaternion operator* (float fScalar, const v3dxQuaternion& rkQ);

	v3dxQuaternion operator- () const;
	v3dxQuaternion operator+ (const v3dxQuaternion& rkQ) const;
	v3dxQuaternion operator- (const v3dxQuaternion& rkQ) const;
	v3dxQuaternion operator* (const v3dxQuaternion &rkQ) const;
	v3dxVector3 operator* (const v3dxVector3 &v3) const;
	v3dxQuaternion operator* (float fScalar) const;
	v3dxQuaternion operator/ (float aScalar) const
	{
		return v3dxQuaternion(x / aScalar, y / aScalar, z / aScalar, w / aScalar);
	}
	//void rotate(const v3dxVector3 p_vIn[], v3dxVector3 p_vOut[], int p_nCount) const;
	//void operator *=(const v3dxQuaternion &Quaternion);
	//void operator *=(const v3dxVector3 &v3);
	bool operator== (const v3dxQuaternion &rhs) const;

	// setup for spherical quadratic interpolation
	static void intermediate(const v3dxQuaternion& rkQ0,
		const v3dxQuaternion& rkQ1, const v3dxQuaternion& rkQ2,
		v3dxQuaternion& rka, v3dxQuaternion& rkB);

#ifdef USE_DX
	// spherical quadratic interpolation
	inline static void squad(v3dxQuaternion& outQuat, float fT, const v3dxQuaternion& rkP,
		const v3dxQuaternion& rkA, const v3dxQuaternion& rkB,
		const v3dxQuaternion& rkQ)
	{
		D3DXQuaternionSquad((D3DXQUATERNION *)&outQuat,
			(CONST D3DXQUATERNION *) &rkP,
			(CONST D3DXQUATERNION *) &rkA,
			(CONST D3DXQUATERNION *) &rkB,
			(CONST D3DXQUATERNION *) &rkQ,
			fT);
	}
#endif

public:
	// cutoff for sine near zero
	static float ms_fEpsilon;

	static v3dxQuaternion IDENTITY;
	static v3dxQuaternion ZERO;
};

#include "v3dxQuaternion.inl.h"

#pragma pack(pop)

#endif//#ifndef __v3dxQuaternion__H__