#pragma once
#include "GfxParticle.h"

NS_BEGIN

class GfxParticleModifier;
class GfxParticleSubState;

/*
粒子发射器形体
1、发射器形体仅用于确定粒子诞生时的初始姿态
2、基类代表默认的点发射器形体
3、分为四方体、喇叭体(锥体)、球体/半球体、模型体四种
*/
class GfxParticleEmitterShape : public VIUnknown
{
public:
	RTTI_DEF(GfxParticleEmitterShape, 0x2259be565ba31f9a, true);
	GfxParticleEmitterShape();
	void SetEmitter(GfxParticleSubState *pEmitter);
	virtual ~GfxParticleEmitterShape();
	virtual void GenEmissionPose(GfxParticleState *pParticle);

	VDef_ReadWrite(vBOOL, IsEmitFromShell, m);
	VDef_ReadWrite(vBOOL, IsRandomDirection, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableX, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableY, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableZ, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvX, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvY, m);
	VDef_ReadWrite(vBOOL, RandomDirAvailableInvZ, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableX, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableY, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableZ, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvX, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvY, m);
	VDef_ReadWrite(vBOOL, RandomPosAvailableInvZ, m);
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle);
	virtual void GenEmissionDirection(GfxParticleState *pParticle);
	void CalcEmitterAxis(v3dxVector3& vX, v3dxVector3& vY, v3dxVector3& vZ);
	void ProcessOffsetPosition(v3dxVector3 &vec);
	float GetAvaiableRandomPosValue(int type);

public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew = NULL);

	vBOOL mIsEmitFromShell;	// 是否仅从形体表面发射
	
	vBOOL mIsRandomDirection;// 是否随机方向（否则按形体中心至该点的延伸方向发射）
	vBOOL mRandomDirAvailableX;		// 随机方向X轴可用
	vBOOL mRandomDirAvailableY;		// 随机方向Y轴可用
	vBOOL mRandomDirAvailableZ;		// 随机方向Z轴可用
	vBOOL mRandomDirAvailableInvX;	// 随机方向-X轴可用
	vBOOL mRandomDirAvailableInvY;	// 随机方向-Y轴可用
	vBOOL mRandomDirAvailableInvZ;	// 随机方向-Z轴可用
	
	vBOOL mRandomPosAvailableX;		// 随机位置X轴可用
	vBOOL mRandomPosAvailableY;		// 随机位置Y轴可用
	vBOOL mRandomPosAvailableZ;		// 随机位置Z轴可用
	vBOOL mRandomPosAvailableInvX;	// 随机位置-X轴可用
	vBOOL mRandomPosAvailableInvY;	// 随机位置-Y轴可用
	vBOOL mRandomPosAvailableInvZ;	// 随机位置-Z轴可用
protected:
	TObjectHandle<GfxParticleSubState>	mHost;
};

class GfxParticleEmitterShapeBox : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeBox, 0x0a842f1d5ba31fb5, true);
	GfxParticleEmitterShapeBox();
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;

	v3dxVector3 GetSize();
	void SetSize(const v3dxVector3& size);
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	VDef_ReadWrite(float, SizeX, m);
	VDef_ReadWrite(float, SizeY, m);
	VDef_ReadWrite(float, SizeZ, m);
public:
	float mSizeX;
	float mSizeY;
	float mSizeZ;
};

class GfxParticleEmitterShapeCone : public GfxParticleEmitterShape
{
public:
	enum enDirectionType
	{
		EDT_ConeDirUp,
		EDT_ConeDirDown,
		EDT_EmitterDir,
		EDT_NormalOutDir,
		EDT_NormalInDir,
		EDT_OutDir,
		EDT_InDir,
	};

public:
	RTTI_DEF(GfxParticleEmitterShapeCone, 0x9400c01b5ba31fc7, true);
	GfxParticleEmitterShapeCone();
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
private:
	float GetCrossSectionRadius(float sliderVertical);
public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	float mAngle;	// 外边缘倾斜角度
	float mRadius;	// 根截面半径
	float mLength;	// 喇叭长度
	enDirectionType mDirType;

	VDef_ReadWrite(float, Angle, m);
	VDef_ReadWrite(float, Radius, m);
	VDef_ReadWrite(float, Length, m);
	VDef_ReadWrite(enDirectionType, DirType, m);
};

class GfxParticleEmitterShapeSphere : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeSphere, 0xed1aefe65ba31fd4, true);
	GfxParticleEmitterShapeSphere();
	virtual void GenEmissionPose(GfxParticleState *pParticle) override;

protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
public:
	virtual GfxParticleEmitterShape *Clone(GfxParticleEmitterShape* pNew) override;

	float mRadius;	// 半径
	vBOOL mIsRadialOutDirection;
	vBOOL mIsRadialInDirection;
	vBOOL mIsHemiSphere;// 是否是半球体
	VDef_ReadWrite(float, Radius, m);
	VDef_ReadWrite(vBOOL, IsRadialOutDirection, m);
	VDef_ReadWrite(vBOOL, IsRadialInDirection, m);
	VDef_ReadWrite(vBOOL, IsHemiSphere, m);
};

class GfxParticleEmitterShapeMesh : public GfxParticleEmitterShape
{
public:
	RTTI_DEF(GfxParticleEmitterShapeMesh, 0x340af8aa5ba31fe2, true);
	GfxParticleEmitterShapeMesh();
	void SetPoints(v3dxVector3* points, int num);
protected:
	virtual void GenEmissionPosition(GfxParticleState *pParticle) override;
	virtual void GenEmissionDirection(GfxParticleState *pParticle) override;
public:
	std::vector<v3dxVector3>	mEmitterPoints;
};

NS_END