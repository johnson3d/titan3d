#include "GfxParticleEmitterShape.h"
#include "GfxParticleModifier.h"
#include "GfxParticleSubState.h"

#define  new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxParticleEmitterShape, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::GfxParticleEmitterShapeBox, EngineNS::GfxParticleEmitterShape);
RTTI_IMPL(EngineNS::GfxParticleEmitterShapeCone, EngineNS::GfxParticleEmitterShape);
RTTI_IMPL(EngineNS::GfxParticleEmitterShapeSphere, EngineNS::GfxParticleEmitterShape);
RTTI_IMPL(EngineNS::GfxParticleEmitterShapeMesh, EngineNS::GfxParticleEmitterShape);

//void GfxParticleEmitter::Fire(GfxParticleModifier* modifier, const v3dxMatrix4& matWorldTrans, int count, const v3dxVector3* directions)
//{
//	for (int i = 0; i < count; ++i)
//	{
//		v3dParticle* pParticle = NULL;
//		if (modifier != NULL)
//			pParticle = modifier->AllocParticle();
//		else
//			break;
//
//		if(!pParticle)
//			break;
//		pParticle->Reset();
//
//		for (auto ite = mFollowerDatas.begin(); ite != mFollowerDatas.end(); ite++)
//		{
//			//v3dParticleEmitter* pem = new v3dParticleEmitter(*((*ite).followerEmitter));
//			GfxParticleEmitter* pem = new GfxParticleEmitter();
//			(*ite)->followerEmitter->Clone(pem);
//			pem->Reset();
//			pem->SetEnabled(TRUE);
//			pem->mHostModifier = (*ite)->followerModifier;
//			pem->mHostModifier->AddRef();
//			pem->mFollowerParticle = pParticle;
//			mFollowerEmittersMap[pParticle].push_back(pem);
//		}
//
//		pParticle->mState = v3dParticle::State_Initial;
//		pParticle->mFrame = (int)mParticleFrame->getRandomValue();
//		pParticle->mLife = mParticleLife->getRandomValue();
//		pParticle->mLifeTick = 0;//t - prevTick;
//		pParticle->mOriRotation = Math::DegreesToRadians(mEmitRotationAngle->getRandomValue());//  / 180 * 3.141592654f;
//		pParticle->mPose.mColor = mEmitColor->getRandomValue();
//		if (!mFaceToDirection)
//		{
//			v3dxVector3 emitRoatation = mEmitRotationMin;
//			emitRoatation.x = (mEmitRotationMin.x == mEmitRotationMax.x) ? mEmitRotationMin.x : Math::RangeRandom(mEmitRotationMin.x, mEmitRotationMax.x);
//			emitRoatation.y = (mEmitRotationMin.y == mEmitRotationMax.y) ? mEmitRotationMin.y : Math::RangeRandom(mEmitRotationMin.y, mEmitRotationMax.y);
//			emitRoatation.z = (mEmitRotationMin.z == mEmitRotationMax.z) ? mEmitRotationMin.z : Math::RangeRandom(mEmitRotationMin.z, mEmitRotationMax.z);
//			if (emitRoatation.x == 0 && emitRoatation.y == 0 && emitRoatation.z == 0)
//				emitRoatation.y = 1;
//			else
//				emitRoatation.normalize();
//			v3dxQuaternion rotQuat = v3dxQuaternion::IDENTITY;
//			if (emitRoatation == -v3dxVector3::UNIT_Y)
//				rotQuat.FromAngleAxis(Math::V3_PI, v3dxVector3::UNIT_X);
//			else
//				rotQuat = v3dxVector3::UNIT_Y.getRotationTo(emitRoatation);
//			v3dxQuaternion quat2 = v3dxQuaternion(emitRoatation, pParticle->mOriRotation);
//			pParticle->mPose.mRotation = rotQuat * quat2;
//		}
//		if (mEmitScaleAll)
//		{
//			float scale = mEmitScaleX->getRandomValue();
//			pParticle->mPose.mScale = v3dxVector3(scale, scale, scale);
//		}
//		else
//			pParticle->mPose.mScale = v3dxVector3(mEmitScaleX->getRandomValue(), mEmitScaleY->getRandomValue(), mEmitScaleZ->getRandomValue());
//
//		mShape->GenEmissionPose(pParticle);
//		if(directions != NULL && directions[i] != v3dxVector3::ZERO)
//			pParticle->mPose.mVelocity = directions[i];
//
//		if (mFollowerParticle != NULL)
//		{
//			pParticle->mPose.mPosition += mFollowerParticle->mPose.mPosition;
//		}
//
//		if (mSpawnMatrix != v3dxMatrix4::IDENTITY)
//		{
//			v3dxVector3 spawnTrans;
//			mSpawnMatrix.ExtractionTrans(spawnTrans);
//			pParticle->mPose.mPosition += spawnTrans;
//		}
//
//		switch (mCoordinateSpace)
//		{
//		case CSPACE_WORLD:
//			{
//				v3dxVec3TransformCoord(&pParticle->mPose.mPosition, &pParticle->mPose.mPosition, &matWorldTrans);
//				v3dxVector3 tempScale;
//				matWorldTrans.ExtractionScale(tempScale);
//				tempScale = tempScale * pParticle->mPose.mScale;
//				pParticle->mPose.mVelocity = pParticle->mPose.mVelocity * tempScale;
//			}
//			break;
//		case CSPACE_WORLDWITHDIRECTION:
//			{
//				v3dxVec3TransformCoord(&pParticle->mPose.mPosition, &pParticle->mPose.mPosition, &matWorldTrans);
//				v3dxVector3 tempScale;
//				v3dxQuaternion tempQuat;
//				v3dxVector3 tempPos;
//				matWorldTrans.Decompose(tempScale, tempPos, tempQuat);
//				tempScale = tempScale * pParticle->mPose.mScale;
//				pParticle->mPose.mVelocity *= tempScale;
//				pParticle->mPose.mVelocity = tempQuat * pParticle->mPose.mVelocity;
//			}
//			break;
//		case CSPACE_LOCAL:
//			{
//				v3dxVector3 tempScale;
//				v3dxQuaternion tempQuat;
//				v3dxVector3 tempPos;
//				matWorldTrans.Decompose(tempScale, tempPos, tempQuat);
//				pParticle->mPose.mVelocity = tempQuat * pParticle->mPose.mVelocity;
//			}
//			break;
//		case CSPACE_LOCALWITHDIRECTION:
//			{
//			}
//			break;
//		default:
//			break;
//		}
//
//		pParticle->mPose.mVelocity *= mEmitVelocity->getRandomValue();
//		mParticles.push_back(pParticle);
//	}
//}

//////////////////////////////////////////////////////////////////////////
GfxParticleEmitterShape::GfxParticleEmitterShape()
{
	mIsEmitFromShell = FALSE;
	mIsRandomDirection = FALSE;
	mRandomDirAvailableX = TRUE;
	mRandomDirAvailableY = TRUE;
	mRandomDirAvailableZ = TRUE;
	mRandomDirAvailableInvX = TRUE;
	mRandomDirAvailableInvY = TRUE;
	mRandomDirAvailableInvZ = TRUE;
	mRandomPosAvailableX = TRUE;
	mRandomPosAvailableY = TRUE;
	mRandomPosAvailableZ = TRUE;
	mRandomPosAvailableInvX = TRUE;
	mRandomPosAvailableInvY = TRUE;
	mRandomPosAvailableInvZ = TRUE;

	//if(m_pEmitter != NULL)
	//	m_pEmitter->AddRef();
}

void GfxParticleEmitterShape::SetEmitter(GfxParticleSubState *pEmitter)
{
	if (pEmitter == nullptr)
		return;
	mHost.FromObject(pEmitter);
}

GfxParticleEmitterShape::~GfxParticleEmitterShape()
{
	//Safe_Release(m_pEmitter);
}
void GfxParticleEmitterShape::GenEmissionPose(GfxParticleState *pParticle)
{
	GenEmissionPosition(pParticle);
	GenEmissionDirection(pParticle);
}

void GfxParticleEmitterShape::ProcessOffsetPosition(v3dxVector3 &vec)
{
	if (mRandomPosAvailableX && !mRandomPosAvailableInvX)
	{
		if (vec.x < 0)
			vec.x *= -1;
	}
	else if (!mRandomPosAvailableX && mRandomPosAvailableInvX)
	{
		if (vec.x > 0)
			vec.x *= -1;
	}
	else if (!mRandomPosAvailableX && !mRandomPosAvailableInvX)
	{
		vec.x = 0;
	}

	if (mRandomPosAvailableY && !mRandomPosAvailableInvY)
	{
		if (vec.y < 0)
			vec.y *= -1;
	}
	else if (!mRandomPosAvailableY && mRandomPosAvailableInvY)
	{
		if (vec.y > 0)
			vec.y *= -1;
	}
	else if (!mRandomPosAvailableY && !mRandomPosAvailableInvY)
	{
		vec.y = 0;
	}

	if (mRandomPosAvailableZ && !mRandomPosAvailableInvZ)
	{
		if (vec.z < 0)
			vec.z *= -1;
	}
	else if (!mRandomPosAvailableZ && mRandomPosAvailableInvZ)
	{
		if (vec.z > 0)
			vec.z *= -1;
	}
	else if (!mRandomPosAvailableZ && !mRandomPosAvailableInvZ)
	{
		vec.z = 0;
	}
}
float GfxParticleEmitterShape::GetAvaiableRandomPosValue(int type)
{
	float retValue = 0;

	switch (type)
	{
	case 0:		// X
	{
		if (mRandomPosAvailableX && mRandomPosAvailableInvX)
		{
			retValue = Math::SymmetricRandom();
		}
		else if (mRandomPosAvailableX && !mRandomPosAvailableInvX)
		{
			retValue = Math::SymmetricRandom() + 1;
		}
		else if (!mRandomPosAvailableX && mRandomPosAvailableInvX)
		{
			retValue = Math::SymmetricRandom() - 1;
		}
	}
	break;

	case 1:		// Y
	{
		if (mRandomPosAvailableY && mRandomPosAvailableInvY)
		{
			retValue = Math::SymmetricRandom();
		}
		else if (mRandomPosAvailableY && !mRandomPosAvailableInvY)
		{
			retValue = Math::SymmetricRandom() + 1;
		}
		else if (!mRandomPosAvailableY && mRandomPosAvailableInvY)
		{
			retValue = Math::SymmetricRandom() - 1;
		}
	}
	break;

	case 2:		// Z
	{
		if (mRandomPosAvailableZ && mRandomPosAvailableInvZ)
		{
			retValue = Math::SymmetricRandom();
		}
		else if (mRandomPosAvailableZ && !mRandomPosAvailableInvZ)
		{
			retValue = Math::SymmetricRandom() + 1;
		}
		else if (!mRandomPosAvailableZ && mRandomPosAvailableInvZ)
		{
			retValue = Math::SymmetricRandom() - 1;
		}
	}
	break;
	}
	return retValue;
}
void GfxParticleEmitterShape::CalcEmitterAxis(v3dxVector3& vX, v3dxVector3& vY, v3dxVector3& vZ)
{
	vX = v3dxVector3::UNIT_X;
	vZ = v3dxVector3::UNIT_Z;
	if (vY != v3dxVector3::UNIT_Y)
	{
		vX = vY.crossProduct(v3dxVector3::UNIT_Y);
		vZ = vX.crossProduct(vY);
	}
}
void GfxParticleEmitterShape::GenEmissionPosition(GfxParticleState *pParticle)
{
	pParticle->mPose.mPosition = v3dxVector3::ZERO;
}
void GfxParticleEmitterShape::GenEmissionDirection(GfxParticleState *pParticle)
{
	pParticle->mPose.mAcceleration = v3dxVector3::ZERO;
	if (mIsRandomDirection)
	{
		float x = 0, y = 0, z = 0;
		if (mRandomDirAvailableX && mRandomDirAvailableInvX)
			x = Math::SymmetricRandom();
		else if (mRandomDirAvailableX && !mRandomDirAvailableInvX)
			x = Math::SymmetricRandom() + 1;
		else if (!mRandomDirAvailableX && mRandomDirAvailableInvX)
			x = Math::SymmetricRandom() - 1;

		if (mRandomDirAvailableY && mRandomDirAvailableInvY)
			y = Math::SymmetricRandom();
		else if (mRandomDirAvailableY && !mRandomDirAvailableInvY)
			y = Math::SymmetricRandom() + 1;
		else if (!mRandomDirAvailableY && mRandomDirAvailableInvY)
			y = Math::SymmetricRandom() - 1;

		if (mRandomDirAvailableZ && mRandomDirAvailableInvZ)
			z = Math::SymmetricRandom();
		else if (mRandomDirAvailableZ && !mRandomDirAvailableInvZ)
			z = Math::SymmetricRandom() + 1;
		else if (mRandomDirAvailableZ && !mRandomDirAvailableInvZ)
			z = Math::SymmetricRandom() - 1;

		pParticle->mPose.mVelocity = v3dxVector3(x, y, z).getNormal();
	}
}

GfxParticleEmitterShape *GfxParticleEmitterShape::Clone(GfxParticleEmitterShape* pNew)
{
	if (pNew == NULL)
	{
		pNew = new GfxParticleEmitterShape();
	}
	
	pNew->SetEmitter(mHost.GetPtr());
	pNew->mIsEmitFromShell = mIsEmitFromShell;
	pNew->mIsRandomDirection = mIsRandomDirection;
	pNew->mRandomDirAvailableX = mRandomDirAvailableX;
	pNew->mRandomDirAvailableY = mRandomDirAvailableY;
	pNew->mRandomDirAvailableZ = mRandomDirAvailableZ;
	pNew->mRandomDirAvailableInvX = mRandomDirAvailableInvX;
	pNew->mRandomDirAvailableInvY = mRandomDirAvailableInvY;
	pNew->mRandomDirAvailableInvZ = mRandomDirAvailableInvZ;
	pNew->mRandomPosAvailableX = mRandomPosAvailableX;
	pNew->mRandomPosAvailableY = mRandomPosAvailableY;
	pNew->mRandomPosAvailableZ = mRandomPosAvailableZ;
	pNew->mRandomPosAvailableInvX = mRandomPosAvailableInvX;
	pNew->mRandomPosAvailableInvY = mRandomPosAvailableInvY;
	pNew->mRandomPosAvailableInvZ = mRandomPosAvailableInvZ;

	return pNew;
}


//////////////////////////////////////////////////////////////////////////

GfxParticleEmitterShapeBox::GfxParticleEmitterShapeBox()
	: mSizeX(1)
	, mSizeY(1)
	, mSizeZ(1)
{
}
void GfxParticleEmitterShapeBox::GenEmissionPosition(GfxParticleState *pParticle)
{
	v3dxVector3 offset;
	offset.x = GetAvaiableRandomPosValue(0) * mSizeX * 0.5f;
	offset.y = GetAvaiableRandomPosValue(1) * mSizeY * 0.5f;
	offset.z = GetAvaiableRandomPosValue(2) * mSizeZ * 0.5f;
	if (mIsEmitFromShell)
	{
		float rang = Math::RangeRandom(0.0f, 3.0f);
		if (rang > 2.0f)
		{
			offset.x = offset.x > 0 ? mSizeX * 0.5f : mSizeX * -0.5f;
		}
		else if (rang > 1.0f)
		{
			offset.y = mSizeY > 0 ? mSizeY * 0.5f : mSizeY * -0.5f;
		}
		else
		{
			offset.z = mSizeZ > 0 ? mSizeZ * 0.5f : mSizeZ * -0.5f;
		}
	}


	pParticle->mPose.mPosition = offset;
}
v3dxVector3 GfxParticleEmitterShapeBox::GetSize()
{
	return v3dxVector3(mSizeX, mSizeY, mSizeZ);
}
void GfxParticleEmitterShapeBox::SetSize(const v3dxVector3& size)
{
	mSizeX = size.x;
	mSizeY = size.x;
	mSizeZ = size.x;
}
GfxParticleEmitterShape *GfxParticleEmitterShapeBox::Clone(GfxParticleEmitterShape* pNew)
{
	if (pNew == NULL)
	{
		pNew = new GfxParticleEmitterShapeBox();
	}
	GfxParticleEmitterShape::Clone(pNew);

	((GfxParticleEmitterShapeBox*)pNew)->mSizeX = mSizeX;
	((GfxParticleEmitterShapeBox*)pNew)->mSizeY = mSizeY;
	((GfxParticleEmitterShapeBox*)pNew)->mSizeZ = mSizeZ;

	return pNew;
}

//////////////////////////////////////////////////////////////////////////

GfxParticleEmitterShapeCone::GfxParticleEmitterShapeCone()
	: mAngle(30)
	, mRadius(1)
	, mLength(1)
	, mDirType(EDT_EmitterDir)
{
}
float GfxParticleEmitterShapeCone::GetCrossSectionRadius(float sliderVertical)
{
	if (sliderVertical == 0)
		return mRadius;
	else if (mAngle == 0)
		return mRadius;
	else if (mAngle < 0 && mAngle > -90)
	{
		return mRadius - sliderVertical * Math::Tan(Math::DegreesToRadians(-mAngle));
	}
	else if (mAngle > 0 && mAngle < 90)
		return  mRadius + sliderVertical * Math::Tan(Math::DegreesToRadians(mAngle));
	mAngle = 90;
	return mRadius + sliderVertical * (mLength > mRadius ? mLength : mRadius);
}
void GfxParticleEmitterShapeCone::GenEmissionPosition(GfxParticleState *pParticle)
{
	float fVerLen = Math::RangeRandom(0, mLength);
	float fRadius = GetCrossSectionRadius(fVerLen);
	if (!mIsEmitFromShell)
	{
		fRadius = Math::RangeRandom(-fRadius, fRadius);
	}
	v3dxVector3 offset = v3dxVector3(GetAvaiableRandomPosValue(0), 0, GetAvaiableRandomPosValue(2)).getNormal() * fRadius;
	offset.y = fVerLen;

	pParticle->mPose.mPosition = offset;
}
void GfxParticleEmitterShapeCone::GenEmissionDirection(GfxParticleState *pParticle)
{
	if (mIsRandomDirection)
		GfxParticleEmitterShape::GenEmissionDirection(pParticle);
	else
	{
		//v3dxVector3 vTo = v3dxVector3::UNIT_Y;
		auto ptr = mHost.GetPtr();
		switch (mDirType)
		{
		case GfxParticleEmitterShapeCone::EDT_ConeDirUp:
		{
			if (mAngle > 0)
			{
				float len = mRadius / Math::Tan(Math::DegreesToRadians(mAngle), true);
				auto emitterPos = ptr->mPosition;
				emitterPos.y -= len;
				v3dxVector3 vBase = pParticle->mPose.mPosition;
				pParticle->mPose.mVelocity = (vBase - emitterPos).getNormal();
			}
			else if (mAngle < 0)
			{
				float len = mRadius / Math::Tan(Math::DegreesToRadians(-mAngle), true);
				auto emitterPos = ptr->mPosition;
				emitterPos.y += len;
				v3dxVector3 vBase = pParticle->mPose.mPosition;
				pParticle->mPose.mVelocity = (emitterPos - vBase).getNormal();
			}
			else
			{
				pParticle->mPose.mVelocity = v3dxVector3::UNIT_Y;
			}
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_ConeDirDown:
		{
			v3dxVector3 emitterPos = ptr->mPosition;

			if (mAngle > 0)
			{
				float len = mRadius / Math::Tan(Math::DegreesToRadians(mAngle), true);
				emitterPos.y -= len;
				v3dxVector3 vBase = pParticle->mPose.mPosition;
				pParticle->mPose.mVelocity = (emitterPos - vBase).getNormal();
			}
			else if (mAngle < 0)
			{
				float len = mRadius / Math::Tan(Math::DegreesToRadians(-mAngle), true);
				emitterPos.y += len;
				v3dxVector3 vBase = pParticle->mPose.mPosition;
				pParticle->mPose.mVelocity = (vBase - emitterPos).getNormal();
			}
			else
			{
				pParticle->mPose.mVelocity = -v3dxVector3::UNIT_Y;
			}
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_EmitterDir:
		{
			pParticle->mPose.mVelocity = ptr->mDirection;
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_NormalOutDir:
		{
			v3dxVector3 emitterPos = ptr->mPosition;
			v3dxVector3 fromPos;
			if (mAngle > 0)
			{
				float epLen = mRadius / Math::Tan(Math::DegreesToRadians(mAngle), true);
				emitterPos.y -= epLen;
				float len = (pParticle->mPose.mPosition - emitterPos).getLength();
				fromPos = emitterPos;
				fromPos.y = emitterPos.y + len;
			}
			else if (mAngle < 0)
			{
				float epLen = mRadius / Math::Tan(Math::DegreesToRadians(-mAngle), true);
				emitterPos.y += epLen;
				float len = (pParticle->mPose.mPosition - emitterPos).getLength();
				fromPos = emitterPos;
				fromPos.y = emitterPos.y - len;
			}
			else
			{
				emitterPos.y = pParticle->mPose.mPosition.y;
				fromPos = emitterPos;
			}

			pParticle->mPose.mVelocity = (pParticle->mPose.mPosition - fromPos).getNormal();
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_NormalInDir:
		{
			v3dxVector3 emitterPos = ptr->mPosition, fromPos;
			if (mAngle > 0)
			{
				float epLen = mRadius / Math::Tan(Math::DegreesToRadians(mAngle), true);
				emitterPos.y -= epLen;
				float len = (pParticle->mPose.mPosition - emitterPos).getLength();
				fromPos = emitterPos;
				fromPos.y = emitterPos.y + len;
			}
			else if (mAngle < 0)
			{
				float epLen = mRadius / Math::Tan(Math::DegreesToRadians(-mAngle), true);
				emitterPos.y += epLen;
				float len = (pParticle->mPose.mPosition - emitterPos).getLength();
				fromPos = emitterPos;
				fromPos.y = emitterPos.y - len;
			}
			else
			{
				emitterPos.y = pParticle->mPose.mPosition.y;
				fromPos = emitterPos;
			}

			pParticle->mPose.mVelocity = (fromPos - pParticle->mPose.mPosition).getNormal();
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_OutDir:
		{
			v3dxVector3 emitterPos = ptr->mPosition;
			v3dxVector3 fromPos = v3dxVector3(emitterPos.x, pParticle->mPose.mPosition.y, emitterPos.z);
			pParticle->mPose.mVelocity = (pParticle->mPose.mPosition - fromPos).getNormal();
		}
		break;
		case GfxParticleEmitterShapeCone::EDT_InDir:
		{
			v3dxVector3 emitterPos = ptr->mPosition;
			v3dxVector3 fromPos = v3dxVector3(emitterPos.x, pParticle->mPose.mPosition.y, emitterPos.z);
			pParticle->mPose.mVelocity = (fromPos - pParticle->mPose.mPosition).getNormal();
		}
		break;
		}
	}
}
GfxParticleEmitterShape *GfxParticleEmitterShapeCone::Clone(GfxParticleEmitterShape* pNew)
{
	if (pNew == NULL)
	{
		pNew = new GfxParticleEmitterShapeCone();
	}
	GfxParticleEmitterShape::Clone(pNew);

	((GfxParticleEmitterShapeCone*)pNew)->mAngle = mAngle;
	((GfxParticleEmitterShapeCone*)pNew)->mRadius = mRadius;
	((GfxParticleEmitterShapeCone*)pNew)->mLength = mLength;
	((GfxParticleEmitterShapeCone*)pNew)->mDirType = mDirType;

	return pNew;
}

//////////////////////////////////////////////////////////////////////////

GfxParticleEmitterShapeSphere::GfxParticleEmitterShapeSphere()
	: mRadius(1)
	, mIsRadialOutDirection(FALSE)
	, mIsRadialInDirection(FALSE)
	, mIsHemiSphere(FALSE)
{
}
void GfxParticleEmitterShapeSphere::GenEmissionPose(GfxParticleState *pParticle)
{
	GenEmissionPosition(pParticle);
	GenEmissionDirection(pParticle);
}
void GfxParticleEmitterShapeSphere::GenEmissionDirection(GfxParticleState *pParticle)
{
	//float dz = mIsHemiSphere ? Math::UnitRandom() : Math::SymmetricRandom();
	v3dxVector3 emitterPos = mHost.GetPtr()->mPosition;

	if (mIsRandomDirection)
		GfxParticleEmitterShape::GenEmissionDirection(pParticle);
	else if (mIsRadialOutDirection)
	{
		pParticle->mPose.mVelocity = (pParticle->mPose.mPosition - emitterPos).getNormal();
	}
	else if (mIsRadialInDirection)
	{
		pParticle->mPose.mVelocity = (emitterPos - pParticle->mPose.mPosition).getNormal();
	}
	else
	{
		//pParticle->mPose.mVelocity = mHost.GetPtr()->mDirection;
	}
}
void GfxParticleEmitterShapeSphere::GenEmissionPosition(GfxParticleState *pParticle)
{
	float slider = mIsEmitFromShell ? 1 : Math::UnitRandom();
	v3dxVector3 offset = v3dxVector3(GetAvaiableRandomPosValue(0), GetAvaiableRandomPosValue(1), GetAvaiableRandomPosValue(2)).getNormal() * (mRadius * slider);

	//ProcessOffsetPosition(offset);

	pParticle->mPose.mPosition = offset;
	//if (mIsRandomDirection)
	//	pParticle->mPose.mVelocity = v3dxVector3(Math::SymmetricRandom(), Math::SymmetricRandom(), Math::SymmetricRandom()).getNormal();
	//else
	//	pParticle->mPose.mVelocity = m_pEmitter->mDirection;
}

GfxParticleEmitterShape *GfxParticleEmitterShapeSphere::Clone(GfxParticleEmitterShape* pNew)
{
	if (pNew == NULL)
	{
		pNew = new GfxParticleEmitterShapeSphere();
	}

	((GfxParticleEmitterShapeSphere*)pNew)->mRadius = mRadius;
	((GfxParticleEmitterShapeSphere*)pNew)->mIsRadialOutDirection = mIsRadialOutDirection;
	((GfxParticleEmitterShapeSphere*)pNew)->mIsRadialInDirection = mIsRadialInDirection;
	((GfxParticleEmitterShapeSphere*)pNew)->mIsHemiSphere = mIsHemiSphere;

	return pNew;
}

//////////////////////////////////////////////////////////////////////////
GfxParticleEmitterShapeMesh::GfxParticleEmitterShapeMesh()
{
}

void GfxParticleEmitterShapeMesh::GenEmissionPosition(GfxParticleState *pParticle)
{
	UINT size = (UINT)mEmitterPoints.size();
	if (size == 0)
		return;

	UINT idx = 0;
	if (size > RAND_MAX)
	{
		auto r1 = (USHORT)rand();
		auto r2 = (USHORT)rand();
		idx = r1 * RAND_MAX + r2;
	}
	else
	{
		idx = rand();
	}
	idx = idx % size;
	pParticle->mPose.mPosition = mEmitterPoints[idx];
}

void GfxParticleEmitterShapeMesh::GenEmissionDirection(GfxParticleState *pParticle)
{
	GfxParticleEmitterShape::GenEmissionDirection(pParticle);
}

void GfxParticleEmitterShapeMesh::SetPoints(v3dxVector3* points, int num)
{
	mEmitterPoints.resize(num);
	memcpy(&mEmitterPoints[0], points, sizeof(v3dxVector3)*num);
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetEmitter);

	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetIsEmitFromShell);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetIsRandomDirection);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableX);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableY);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableZ);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableInvX);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableInvY);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomDirAvailableInvZ);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableX);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableY);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableZ);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableInvX);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableInvY);
	Cpp2CS0(EngineNS, GfxParticleEmitterShape, GetRandomPosAvailableInvZ);

	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetIsEmitFromShell);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetIsRandomDirection);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableX);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableY);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableZ);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableInvX);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableInvY);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomDirAvailableInvZ);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableX);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableY);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableZ);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableInvX);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableInvY);
	Cpp2CS1(EngineNS, GfxParticleEmitterShape, SetRandomPosAvailableInvZ);

	Cpp2CS0(EngineNS, GfxParticleEmitterShapeBox, GetSizeX);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeBox, GetSizeY);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeBox, GetSizeZ);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeBox, SetSizeX);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeBox, SetSizeY);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeBox, SetSizeZ);

	Cpp2CS0(EngineNS, GfxParticleEmitterShapeCone, GetAngle);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeCone, GetRadius);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeCone, GetLength);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeCone, GetDirType);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeCone, SetAngle);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeCone, SetRadius);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeCone, SetLength);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeCone, SetDirType);

	Cpp2CS0(EngineNS, GfxParticleEmitterShapeSphere, GetRadius);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeSphere, GetIsRadialOutDirection);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeSphere, GetIsRadialInDirection);
	Cpp2CS0(EngineNS, GfxParticleEmitterShapeSphere, GetIsHemiSphere);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeSphere, SetRadius);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeSphere, SetIsRadialOutDirection);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeSphere, SetIsRadialInDirection);
	Cpp2CS1(EngineNS, GfxParticleEmitterShapeSphere, SetIsHemiSphere);

	Cpp2CS2(EngineNS, GfxParticleEmitterShapeMesh, SetPoints);
}