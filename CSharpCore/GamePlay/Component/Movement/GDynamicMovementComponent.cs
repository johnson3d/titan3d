using EngineNS.GamePlay.Actor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Component
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class GDynamicMovementComponent : GMovementComponent
    {
        [Rtti.MetaClass]
        public class GDynamicMovementComponentInitializer : GMovementComponentInitializer
        {
            [Rtti.MetaData]
            public float Force { get; set; } = 10.0f;
        }
        public override Vector3 DesireDirection
        {
            get => mDesireDirection;
            set
            {
                mDesireDirection = value;
                mDesireForce = mDesireDirection.NormalizeValue * Force;
            }
        }
        protected Vector3 mDesireForce = Vector3.Zero;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 DesireForce
        {
            get => mDesireForce;
            private set => mDesireForce = value;
        }
        public bool HaveForce
        {
            get { return mDesireForce.LengthSquared() > 0 ? true : false; }
        }
        protected float mForce = 10.0f;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Force
        {
            get => mForce;
            set
            {
                mForce = value;
                var init = Initializer as GDynamicMovementComponentInitializer;
                init.Force = value;
            }

        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            var initializer = v as GDynamicMovementComponentInitializer;
            if (initializer != null)
                Force = initializer.Force;
            return true;
        }
        public override void HandleInputData()
        {
            mDesireForce = mControllerInput.NormalizeValue * Force;
        }
        protected Vector3 mAccelerate = Vector3.Zero;
        protected Vector3 F = Vector3.Zero;
        protected float mVelocityPrecision = 0.01f;
        protected Vector3 mFrictionDir = Vector3.Zero;
        public override Vector3 ProcessingDisplacement(GPlacementComponent placement, float dtSecond)
        {
            if (mVelocity == Vector3.Zero || mVelocity.Length() < mVelocityPrecision)
            {
                mVelocity = Vector3.Zero;
            }
            mTempVelcity = mVelocity;
            if (!HaveForce && mTempVelcity.Length() < mVelocityPrecision)
            {
                mTempVelcity = Vector3.Zero;
            }
            else
            {
                mFrictionDir = -mVelocity.NormalizeValue;
                if (HaveForce && mFrictionDir == Vector3.Zero)
                {
                    mFrictionDir = -mDesireForce.NormalizeValue;
                }
                var theForce = mDesireForce;
                var friction = mFrictionDir * mMass * mFriction;
                F = (theForce + friction);
                mAccelerate = F / Mass;
                var v = mTempVelcity + mAccelerate * dtSecond;
                if (!HaveForce && v.NormalizeValue == -mTempVelcity.NormalizeValue)
                {
                    mTempVelcity = Vector3.Zero;
                }
                else
                {
                    mTempVelcity = v;
                }
                mTempVelcity = Math.Min(mTempVelcity.Length(), mMaxVelocity) * mTempVelcity.NormalizeValue;
            }
            var phyCtrlCom = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (mHasGravity /*&& mIsInAir*/ && phyCtrlCom != null)
                mGravityVelocity = mGravityVelocity + Gravity * dtSecond;
            else
            {
                mGravityVelocity = 0;
            }
            mTempVelcity.Y += mGravityVelocity;

            delta = mTempVelcity * dtSecond;
            return delta;
        }
        protected Vector3 mPredictedAccelerate = Vector3.Zero;
        protected Vector3 PredictedF = Vector3.Zero;
        public override Vector3 CalculatePredictionFuturePosition(float dtSecond)
        {
            if (mFutureVelocity == Vector3.Zero || mFutureVelocity.Length() < mVelocityPrecision)
            {
                mFutureVelocity = Vector3.Zero;
            }
            mFutureTempVelocity = mFutureVelocity;
            var theForce = mDesireForce;
            var friction = (-mFutureTempVelocity.NormalizeValue) * mMass * mFriction;
            if (theForce == Vector3.Zero && mFutureTempVelocity.Length() < mVelocityPrecision)
            {
                mFutureTempVelocity = Vector3.Zero;
                friction = Vector3.Zero;
            }
            PredictedF = (theForce + friction);
            mPredictedAccelerate = PredictedF / Mass;
            mFutureTempVelocity = mFutureTempVelocity + mAccelerate * dtSecond;
            mFutureTempVelocity = Math.Min(mFutureTempVelocity.Length(), mMaxVelocity) * mFutureTempVelocity.NormalizeValue;
            mFutureDelta = mFutureTempVelocity * dtSecond;

            var oldLoc = mFutureLocation;
            mFutureLocation += mFutureDelta;
            var realLoc = mFutureLocation;
            mFutureVelocity = (realLoc - oldLoc) / dtSecond;
            return realLoc + Vector3.UnitY * 0.02f;
        }
    }

}
