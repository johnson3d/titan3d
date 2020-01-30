using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMovementComponentInitializer), "移动组件", "Movement Component")]
    public class GDynamicMovementComponent : GMovementComponent
    {
        Vector3 mAccelerate = Vector3.Zero;
        float mFriction = 0.2f;
        Vector3 F = Vector3.Zero;
        public override void CalculateVelocity(GPlacementComponent placement, float dtSecond)
        {
            F = (mDesireVelcity - mVelocity).NormalizeValue * 10;
            var mFrictionVec3 = F * mFriction;
            if (mVelocity == Vector3.Zero || mVelocity.Length() < 0.1f)
            {
                mFrictionVec3 = Vector3.Zero;
                mVelocity = Vector3.Zero;
            }
            if (mDesireVelcity == Vector3.Zero)
            {
                F = Vector3.Zero;
                mFrictionVec3 = (mVelocity).NormalizeValue * 10 * mFriction;
            }
            mAccelerate = (F - mFrictionVec3) / Mass;
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phyCtrlCom == null)
            {
                placement.Location += mTempVelcity * dtSecond;
            }
            if (mAccelerate == Vector3.Zero)
                mTempVelcity = Vector3.Zero;
            else
                mTempVelcity = mTempVelcity + mAccelerate * dtSecond;
            mTempVelcity = Math.Min(mTempVelcity.Length(), mMaxVelocity) * mTempVelcity.NormalizeValue;
            if (mHasGravity /*&& mIsInAir*/ && phyCtrlCom != null)
                mGravityVelocity = mGravityVelocity + mGravity * dtSecond;
            mTempVelcity.Y += mGravityVelocity;

            delta = mTempVelcity * dtSecond;

            var oldLoc = placement.Location;
            placement?.TryMove(ref delta, 0.0001f, dtSecond);
            var realLoc = placement.Location;
            mVelocity = (realLoc - oldLoc) / dtSecond;
            if (mOrientThisRotation)
            {
                if (mOrientation != Vector3.Zero)
                {
                    var rot = Quaternion.GetQuaternion(Vector3.UnitZ, mOrientation);
                    if (placement.Rotation != rot)
                    {
                        placement.Rotation = rot;

                    }
                }
            }
            if (phyCtrlCom != null)
            {
                switch (phyCtrlCom.CollisionFlags)
                {
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_None:
                        {
                            mIsInAir = true;
                        }
                        break;
                    case Bricks.PhysicsCore.PhyControllerCollisionFlag.eCOLLISION_DOWN:
                        {
                            mIsInAir = false;
                            mGravityVelocity = 0;
                            mVelocity.Y = 0;
                        }
                        break;
                }
            }
        }
        Vector3 mPredictedAccelerate = Vector3.Zero;
        Vector3 PredictedF = Vector3.Zero;
        public override Vector3 CalculatePredictionFuturePosition(float dtSecond)
        {
            PredictedF = (mDesireVelcity - mFutureVelocity).NormalizeValue * 10;
            var mFrictionVec3 = PredictedF * mFriction;
            if (mFutureVelocity == Vector3.Zero || mFutureVelocity.Length() < 0.1f)
            {
                mFrictionVec3 = Vector3.Zero;
                mFutureVelocity = Vector3.Zero;
            }
            if (mDesireVelcity == Vector3.Zero)
            {
                PredictedF = Vector3.Zero;
                mFrictionVec3 = (mFutureVelocity).NormalizeValue * 10 * mFriction;
            }
            mPredictedAccelerate = (F - mFrictionVec3) / Mass;
            if (mPredictedAccelerate == Vector3.Zero)
                mFutureTempVelocity = Vector3.Zero;
            else
                mFutureTempVelocity = mFutureTempVelocity + mAccelerate * dtSecond;
            mFutureTempVelocity = Math.Min(mFutureTempVelocity.Length(), mMaxVelocity) * mTempVelcity.NormalizeValue;
            mFutureDelta = mFutureTempVelocity * dtSecond;

            var oldLoc = mFutureLocation;
            mFutureLocation += mFutureDelta;
            var realLoc = mFutureLocation;
            mFutureVelocity = (realLoc - oldLoc) / dtSecond;
            return realLoc + Vector3.UnitY * 0.02f;
        }
    }
}
