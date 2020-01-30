using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public struct CGfxParticlePose
    {
        //直接使用的属性
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 mPosition;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 mScale;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion mRotation;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt32_4 mColor;//已经使用 x0,y0,z0,w0,x1,y1,z1

        //用来计算的中间属性
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 mVelocity;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 mAcceleration;// x 为加速度力度(pose), x为重力加速度(finapose), y 为切线运动力度（pose）
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 mAxis; // 切线运动使用
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float mAngle;

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置用户数据")]
        [Editor.DisplayParamName("粒子姿态用户数据设置")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParams(int colume, byte x, byte y, byte z, byte w)
        {
            switch(colume)
            {
                case 0:
                    mColor.x0 = x;
                    mColor.y0 = y;
                    mColor.z0 = z;
                    mColor.w0 = w;
                    break;
                case 1:
                    mColor.x1 = x;
                    mColor.y1 = y;
                    mColor.z1 = z;
                    mColor.w1 = w;
                    break;
                case 2:
                    mColor.x2 = x;
                    mColor.y2 = y;
                    mColor.z2 = z;
                    mColor.w2 = w;
                    break;
                case 3:
                    mColor.x3 = x;
                    mColor.y3 = y;
                    mColor.z3 = z;
                    mColor.w3 = w;
                    break;
            }
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置用户数据 X")]
        [Editor.DisplayParamName("粒子姿态用户数据设置 X")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParamsX(int colume, byte x)
        {
            switch (colume)
            {
                case 0:
                    mColor.x0 = x;
                    break;
                case 1:
                    mColor.x1 = x;
                    break;
                case 2:
                    mColor.x2 = x;
                    break;
                case 3:
                    mColor.x3 = x;
                    break;
            }
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置用户数据 Y")]
        [Editor.DisplayParamName("粒子姿态用户数据设置 Y")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParamsY(int colume, byte y)
        {
            switch (colume)
            {
                case 0:
                    mColor.y0 = y;
                    break;
                case 1:
                    mColor.y1 = y;
                    break;
                case 2:
                    mColor.y2 = y;
                    break;
                case 3:
                    mColor.y3 = y;
                    break;
            }
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/获取用户数据 Y")]
        [Editor.DisplayParamName("获取粒子姿态用户数据 Y")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public byte GetUserParamsY(int colume)
        {
            switch (colume)
            {
                case 0:
                    return mColor.y0;
                case 1:
                    return mColor.y1;
                case 2:
                    return mColor.y2;
                case 3:
                    return mColor.y3;
            }

            return 0;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置用户数据 Z")]
        [Editor.DisplayParamName("粒子姿态用户数据设置 Z")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParamsZ(int colume, byte z)
        {
            switch (colume)
            {
                case 0:
                    mColor.z0 = z;
                    break;
                case 1:
                    mColor.z1 = z;
                    break;
                case 2:
                    mColor.z2 = z;
                    break;
                case 3:
                    mColor.z3 = z;
                    break;
            }
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/获取用户数据 Z")]
        [Editor.DisplayParamName("获取粒子姿态用户数据 Z")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public byte GetUserParamsZ(int colume)
        {
            switch (colume)
            {
                case 0:
                    return mColor.z0;
                case 1:
                    return mColor.z1;
                case 2:
                    return mColor.z2;
                case 3:
                    return mColor.z3;
            }

            return 0;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置用户数据")]
        [Editor.DisplayParamName("粒子姿态用户数据设置")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParamsW(int colume, byte w)
        {
            switch (colume)
            {
                case 0:
                    mColor.w0 = w;
                    break;
                case 1:
                    mColor.w1 = w;
                    break;
                case 2:
                    mColor.w2 = w;
                    break;
                case 3:
                    mColor.w3 = w;
                    break;
            }
        }
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/设置颜色到用户数据")]
        [Editor.DisplayParamName("粒子姿态用户数据设置")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetUserParams_Color4(int colume, Color4 clr)
        {
            switch (colume)
            {
                case 0:
                    mColor.x0 = (byte)(clr.Red * 255.0f);
                    mColor.y0 = (byte)(clr.Green * 255.0f);
                    mColor.z0 = (byte)(clr.Blue * 255.0f);
                    mColor.w0 = (byte)(clr.Alpha * 255.0f);
                    break;
                case 1:
                    mColor.x1 = (byte)(clr.Red * 255.0f);
                    mColor.y1 = (byte)(clr.Green * 255.0f);
                    mColor.z1 = (byte)(clr.Blue * 255.0f);
                    mColor.w1 = (byte)(clr.Alpha * 255.0f);
                    break;
                case 2:
                    mColor.x2 = (byte)(clr.Red * 255.0f);
                    mColor.y2 = (byte)(clr.Green * 255.0f);
                    mColor.z2 = (byte)(clr.Blue * 255.0f);
                    mColor.w2 = (byte)(clr.Alpha * 255.0f);
                    break;
                case 3:
                    mColor.x3 = (byte)(clr.Red * 255.0f);
                    mColor.y3 = (byte)(clr.Green * 255.0f);
                    mColor.z3 = (byte)(clr.Blue * 255.0f);
                    mColor.w3 = (byte)(clr.Alpha * 255.0f);
                    break;
            }
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/获取用户数据")]
        [Editor.DisplayParamName("粒子姿态用户数据获取")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt8_4 GetUserParams(int colume)
        {
            UInt8_4 result = new UInt8_4();
            switch (colume)
            {
                case 0:
                    result.x = mColor.x0;
                    result.y = mColor.y0;
                    result.z = mColor.z0;
                    result.w = mColor.w0;
                    break;
                case 1:
                    result.x = mColor.x1;
                    result.y = mColor.y1;
                    result.z = mColor.z1;
                    result.w = mColor.w1;
                    break;
                case 2:
                    result.x = mColor.x2;
                    result.y = mColor.y2;
                    result.z = mColor.z2;
                    result.w = mColor.w2;
                    break;
                case 3:
                    result.x = mColor.x3;
                    result.y = mColor.y3;
                    result.z = mColor.z3;
                    result.w = mColor.w3;
                    break;
            }
            return result;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/Lerp")]
        [Editor.DisplayParamName("粒子姿态线性插值")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void Lerp(ref CGfxParticlePose r, ref CGfxParticlePose lh, ref CGfxParticlePose rh, float v)
        {
            //r = lh * (1-v) + rh * v;
            float fa = 1 - v;
            r.mPosition = lh.mPosition * fa + rh.mPosition * v;
            r.mVelocity = lh.mVelocity * fa + rh.mVelocity * v;
            //r.mAcceleration = lh.mAcceleration * fa + rh.mAcceleration * v;
            r.mColor = UInt32_4.Lerp(ref lh.mColor, ref rh.mColor, v);
            r.mScale = lh.mScale * fa + rh.mScale * v;
            r.mRotation = Quaternion.Lerp(lh.mRotation, rh.mRotation, v);
            r.mAngle = lh.mAngle * fa + rh.mAngle * v;
        }

        public static void Set(ref CGfxParticlePose l, ref CGfxParticlePose r)
        {
            //r = lh * (1-v) + rh * v;
            l.mPosition = r.mPosition;
            l.mVelocity = r.mVelocity;
            l.mColor = r.mColor;
            l.mScale = r.mScale;
            l.mRotation = r.mRotation;
            l.mAngle = r.mAngle;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/AddFunc")]
        [Editor.DisplayParamName("粒子姿态相加")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void AddFunc(ref CGfxParticlePose r, ref CGfxParticlePose lh, ref CGfxParticlePose rh)
        {
            //r = lh * (1-v) + rh * v;
            r.mPosition = lh.mPosition  + rh.mPosition;
            r.mVelocity = lh.mVelocity + rh.mVelocity;
            //r.mAcceleration = lh.mAcceleration + rh.mAcceleration;
            r.mColor = UInt32_4.Lerp(ref lh.mColor, ref rh.mColor, 0.5f);
            r.mScale = lh.mScale + rh.mScale;
            r.mRotation = Quaternion.Lerp(lh.mRotation, rh.mRotation, 0.5f);
            r.mAngle = lh.mAngle + rh.mAngle;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/SubFunc")]
        [Editor.DisplayParamName("粒子姿态相减")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void SubFunc(ref CGfxParticlePose r, ref CGfxParticlePose lh, ref CGfxParticlePose rh)
        {
            //r = lh * (1-v) + rh * v;
            r.mPosition = lh.mPosition - rh.mPosition;
            r.mVelocity = lh.mVelocity - rh.mVelocity;
            //r.mAcceleration = lh.mAcceleration - rh.mAcceleration;
            r.mColor = UInt32_4.Lerp(ref lh.mColor, ref rh.mColor, 0.5f);
            r.mScale = lh.mScale - rh.mScale;
            r.mRotation = Quaternion.Lerp(lh.mRotation, rh.mRotation, 0.5f);
            r.mAngle = lh.mAngle - rh.mAngle;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/LerpVector3")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("向量插值")]
        public static void LerpVector3(ref Vector3 r, ref Vector3 lh, ref Vector3 rh, float v)
        {
            float fa = 1 - v;
            r = lh * fa + rh * v;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/Face2Cameral")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static void Face2Cameral(ref CGfxParticlePose pose, Graphics.CGfxCamera camera)
        {
            
        }
    }
    public unsafe struct CGfxParticleState
    {
        public CGfxParticle* Host;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public IntPtr mExtData;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CGfxParticlePose mStartPose;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CGfxParticlePose mPose;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CGfxParticlePose mPrevPose;

        //TODO..
        public void SyncStartToPose()
        {
            mPose.mPosition += mStartPose.mPosition;
            mPose.mRotation *= mStartPose.mRotation;
            mPose.mScale *= mStartPose.mScale;
            mPose.mAxis = mStartPose.mAxis;
            //mPose.mColor = mStartPose.mColor;
            
            mPose.mVelocity += mStartPose.mVelocity;
            mPose.mVelocity.Normalize();

            mPose.mAcceleration += mStartPose.mAcceleration;
            mPose.mAngle += mStartPose.mAngle;
        }

        //public void SyncStartToPose()
        //{
        //    mPose = mStartPose;
        //}

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPosePostion(6参)")]
        [Editor.DisplayParamName("设置粒子姿态对象的位置信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetPosePostion(float x1, float x2, float y1, float y2, float z1, float z2)
        {
            x2 = Math.Max(x2, x1 + 1f);
            y2 = Math.Max(y2, y1 + 1f);
            z2 = Math.Max(z2, z1 + 1f);
            mPose.mPosition = new EngineNS.Vector3(
                McParticleEffector.SRandom.Next((int)(x1 * 1000f), (int)(x2 * 1000f)) * 0.001f,
                McParticleEffector.SRandom.Next((int)(y1 * 1000f), (int)(y2 * 1000f)) * 0.001f,
                McParticleEffector.SRandom.Next((int)(z1 * 1000f), (int)(z2 * 1000f)) * 0.001f
                );
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPosePostion(3参)")]
        [Editor.DisplayParamName("设置粒子姿态对象的位置信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetPosePostion(float x, float y, float z)
        {
            mPose.mPosition.X = x;
            mPose.mPosition.Y = y;
            mPose.mPosition.Z = z;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartPosePostion")]
        [Editor.DisplayParamName("设置粒子姿态对象的起始位置信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetStartPosePostion(float x1, float x2, float y1, float y2, float z1, float z2)
        {
            x2 = Math.Max(x2, x1 + 1f);
            y2 = Math.Max(y2, y1 + 1f);
            z2 = Math.Max(z2, z1 + 1f);
            mStartPose.mPosition = new EngineNS.Vector3(
                McParticleEffector.SRandom.Next((int)(x1 * 1000f), (int)(x2 * 1000f)) * 0.001f,
                McParticleEffector.SRandom.Next((int)(y1 * 1000f), (int)(y2 * 1000f)) * 0.001f,
                McParticleEffector.SRandom.Next((int)(z1 * 1000f), (int)(z2 * 1000f)) * 0.001f
                );
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPoseRandomScale")]
        [Editor.DisplayParamName("粒子姿态缩放信息取随机值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetPoseRandomScale([Editor.DisplayParamName("缩放系数（float）")]float scale)
        {
            mPose.mScale = EngineNS.MathHelper.RandomDirection() * scale;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartPoseRandomScale")]
        [Editor.DisplayParamName("粒子姿态起始缩放信息取随机值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetStartPoseRandomScale([Editor.DisplayParamName("缩放系数（float）")]float scale)
        {
            mStartPose.mScale = EngineNS.MathHelper.RandomDirection() * scale;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPoseScale")]
        [Editor.DisplayParamName("粒子姿态缩放信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetPoseScale([Editor.DisplayParamName("缩放系数（Vector3）")]Vector3 scale)
        {
            mPose.mScale = scale;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartPoseScale")]
        [Editor.DisplayParamName("粒子姿态起始缩放信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetStartPoseScale([Editor.DisplayParamName("缩放系数（Vector3）")]Vector3 scale)
        {
            mStartPose.mScale = scale;
        }
        
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartPoseRandomAxis")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子姿态起始坐标信息取随机值")]
        public void SetStartPoseRandomAxis()
        {
            mStartPose.mAxis = EngineNS.MathHelper.RandomDirection();
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPoseRotation")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("设置粒子姿态的旋转信息")]
        public void SetPoseRotation([Editor.DisplayParamName("选择轴（Vector3）")]Vector3 aix, float angle)
        {
            angle = MathHelper.Deg2Rad * angle;
            Quaternion q = Quaternion.RotationAxis(aix, angle);
            mPose.mAngle = angle;
            mPose.mRotation = q;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetPoseAngleVelocity(支持每帧调用)")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("设置粒子角速度")]
        public void SetPoseAngleVelocity([Editor.DisplayParamName("选择轴（Vector3）")]Vector3 aix, [Editor.DisplayParamName("角度（float）")]float angle, [Editor.DisplayParamName("帧间隔（float）")] float elaspe)
        {
            angle = MathHelper.Deg2Rad * angle;
            mPose.mAngle += angle * elaspe;
            Quaternion q = Quaternion.RotationAxis(aix, angle);
            mPose.mRotation *= q;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartPoseRotation（初始化）")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("设置粒子姿态的起始旋转信息")]
        public void SetStartPoseRotation([Editor.DisplayParamName("选择轴（Vector3）")]Vector3 aix, float angle)
        {
            angle = MathHelper.Deg2Rad * angle;
            Quaternion q = Quaternion.RotationAxis(aix, angle);
            mStartPose.mAngle = angle;
            mStartPose.mRotation = q;
        }

        //粒子推进效果 Todo..
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/AccelerationEffect")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子推进效果")]
        public void AccelerationEffect([Editor.DisplayParamName("帧间隔（float）")]float elaspe)
        {
            mPose.mPosition += mPose.mVelocity * mPose.mAcceleration.X * elaspe;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/AccelerationEffect（运动速度，切线速度，向心速度）")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子推进效果")]
        public void AccelerationEffect([Editor.DisplayParamName("帧间隔（float）")]float elaspe, [Editor.DisplayParamName("切线速度（Vector3）")]ref Vector3 tangent, [Editor.DisplayParamName("向心速度（Vector3）")]ref Vector3 center)
        {
            Vector3 Velocity = mPose.mVelocity;

            Velocity *= mPose.mAcceleration.X * elaspe;
            Velocity += tangent + center;

            mPose.mPosition += Velocity;//(mPose.mVelocity * elaspe) * speed;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/AccelerationEffect（运动速度，切线速度）")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子推进效果")]
        public void AccelerationEffectTangent([Editor.DisplayParamName("帧间隔（float）")]float elaspe, [Editor.DisplayParamName("切线速度（Vector3）")]ref Vector3 tangent, [Editor.DisplayParamName("速度系数（float）")]float speed = 1)
        {
            Vector3 Velocity = mPose.mVelocity;

            Velocity += tangent;

            mPose.mPosition += Velocity;//(mPose.mVelocity * elaspe) * speed;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/AccelerationEffect（运动速度，向心速度）")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子推进效果")]
        public void AccelerationEffectCenter([Editor.DisplayParamName("帧间隔（float）")]float elaspe, [Editor.DisplayParamName("向心速度（Vector3）")]ref Vector3 center, [Editor.DisplayParamName("速度系数（float）")]float speed = 1)
        {
            Vector3 Velocity = mPose.mVelocity;

            Velocity += center * speed;

            mPose.mPosition += Velocity;//(mPose.mVelocity * elaspe) * speed;
        }
        

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/GetVelocity")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("获取粒子的速度")]
        public Vector3 GetVelocity()
        {
            //mPose.mAcceleration = new Vector3(x, y, z);
            return mPose.mVelocity;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/GetStartVelocity")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("获取粒子的起始速度")]
        public Vector3 GetStartVelocity()
        {
            //mPose.mAcceleration = new Vector3(x, y, z);
            return mStartPose.mVelocity;
        }

        //设置粒子的速度..
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetVelocity")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("设置粒子的速度")]
        public void SetVelocity([Editor.DisplayParamName("速度(Vector3）")]Vector3 velocity)
        {
            mPose.mVelocity = velocity;
        }

        //设置粒子的起始速度..
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/SetStartVelocity")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("设置粒子的起始速度")]
        public void SetStartVelocity([Editor.DisplayParamName("速度(Vector3）")]Vector3 velocity)
        {
            mStartPose.mVelocity = velocity;
        }
        
        //根据坐标系的起始角度旋转
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/RotateStartByAxisAngle")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("根据坐标系的起始角度旋转")]
        public void RotateStartByAxisAngle()
        {
            mStartPose.mRotation = Quaternion.RotationAxis(mPose.mAxis, mPose.mAngle);
        }

        //获取绕发射中心的旋转方向
        public void GetRotationByCenter(CGfxParticleSystem sys, CGfxParticleSubState substate, ref Quaternion rotation, out Vector3 result)
        {
            result = new Vector3(sys.Matrix.M41 + substate.Position.X,
                                       sys.Matrix.M42 + substate.Position.Y,
                                       sys.Matrix.M43 + substate.Position.Z);

            result = rotation * (mPose.mPosition - result) + result - mPose.mPosition;

        }
        //获取向心运动方向
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/GetDirectionToCenter")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("获取向心运动方向")]
        public void GetDirectionToCenter([Editor.DisplayParamName("粒子系统对象(CGfxParticleSystem)")]CGfxParticleSystem sys, [Editor.DisplayParamName("粒子发射器状态对象(CGfxParticleSubState)")]CGfxParticleSubState substate, out Vector3 dir)
        {
            Vector3 basepos = new Vector3(sys.Matrix.M41 + substate.Position.X,
                                         sys.Matrix.M42 + substate.Position.Y,
                                         sys.Matrix.M43 + substate.Position.Z);

            dir = mPose.mPosition - basepos;
            dir.Normalize();
        }

        //获取绕发射中心的旋转方向
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/GetRotationByCenter Return")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("获取绕坐标轴的切线方向")]
        public Vector3 GetRotationByCenter([Editor.DisplayParamName("粒子系统对象(CGfxParticleSystem)")]CGfxParticleSystem sys, [Editor.DisplayParamName("粒子发射器状态对象(CGfxParticleSubState)")]CGfxParticleSubState substate, [Editor.DisplayParamName("转向轴(Vector3)")]Vector3 aix)
        {
            Vector3 basepos = new Vector3(sys.Matrix.M41 + substate.Position.X,
                                         sys.Matrix.M42 + substate.Position.Y,
                                         sys.Matrix.M43 + substate.Position.Z);

            Vector3 dir = mPose.mPosition - basepos;
            Vector3.Cross(ref dir, ref aix, out basepos);
            basepos.Normalize();
            return basepos;
        }
        //获取向心运动方向
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticleState(粒子状态)/GetDirectionToCenter Return")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("获取向心运动方向")]
        public Vector3 GetDirectionToCenter([Editor.DisplayParamName("粒子系统对象(CGfxParticleSystem)")]CGfxParticleSystem sys, [Editor.DisplayParamName("粒子发射器状态对象(CGfxParticleSubState)")]CGfxParticleSubState substate)
        {
            Vector3 basepos = new Vector3(sys.Matrix.M41 + substate.Position.X,
                                         sys.Matrix.M42 + substate.Position.Y,
                                         sys.Matrix.M43 + substate.Position.Z);

            Vector3 dir = mPose.mPosition - basepos;
            dir.Normalize();
            return dir;
        }
    }
    public unsafe struct CGfxParticle
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public readonly int mIndex;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("粒子的生命")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/Life")]
        public float mLife;            // 生存期
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("粒子的当前年龄")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/LifeTick")]
        public float mLifeTick;        // 当前年龄
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public UInt32 mFlags;       //附加1
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public IntPtr mExtData;		//附加2
        public IntPtr mExtData2;   //附加3
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("粒子的最终形态")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/FinalPose")]
        public CGfxParticlePose FinalPose;
        public readonly CGfxParticleState** StateArray;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/GetState")]
        [Editor.DisplayParamName("获取粒子状态对象")]
        public CGfxParticleState* GetState([Editor.DisplayParamName("对象数组索引(int)")]int index)
        {
            return StateArray[index];
        }
        public static CGfxParticle Empty = new CGfxParticle();
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public object Tag
        {
            set
            {
                if (mExtData2 != IntPtr.Zero)
                {
                    var old = System.Runtime.InteropServices.GCHandle.FromIntPtr(mExtData2);
                    old.Free();
                    mExtData2 = IntPtr.Zero;
                }
                if (value != null)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.Alloc(value, System.Runtime.InteropServices.GCHandleType.Normal);
                    mExtData2 = System.Runtime.InteropServices.GCHandle.ToIntPtr(handle);
                }
            }
            get
            {
                if (mExtData2 == IntPtr.Zero)
                    return null;
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(mExtData2);
                return handle.Target;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public object WeakReferenceTag
        {
            set
            {
                if (mExtData != IntPtr.Zero)
                {
                    var old = System.Runtime.InteropServices.GCHandle.FromIntPtr(mExtData);
                    old.Free();
                    mExtData = IntPtr.Zero;
                }
                if (value != null)
                {
                    var handle = System.Runtime.InteropServices.GCHandle.Alloc(value, System.Runtime.InteropServices.GCHandleType.WeakTrackResurrection);
                    mExtData = System.Runtime.InteropServices.GCHandle.ToIntPtr(handle);
                }
            }
            get
            {
                if (mExtData == IntPtr.Zero)
                    return null;
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(mExtData);
                return handle.Target;
            }
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticlePose(粒子姿态)/(粒子对象)CGfxParticle/FreeTag")]
        [Editor.DisplayParamName("释放扩展数据")]
        public void FreeTag()
        {
            if (mExtData != IntPtr.Zero)
            {
                var old = System.Runtime.InteropServices.GCHandle.FromIntPtr(mExtData);
                old.Free();
                mExtData = IntPtr.Zero;
            }
        }

        [EngineNS.Editor.DisplayParamName("设置粒子的生命")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/SetParticleLife(value1 -- vlaue2)")]
        //DoParticleBorn
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetParticleLife([Editor.DisplayParamName("最大生命值（float）")]float life1, [Editor.DisplayParamName("最小生命值（float）")]float life2)
        {
            if (life1 == life2)
            {
                mLife = life1;
            }
            else
            {
                mLife = McParticleEffector.SRandom.Next((int)(life1 * 1000.0f), (int)(Math.Max(life2, life1 + 0.001f) * 1000.0f)) / 1000.0f;
            }
        }

        [EngineNS.Editor.DisplayParamName("设置粒子的生命")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/SetParticleLife")]
        //DoParticleBorn
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetParticleLife([Editor.DisplayParamName("生命值（float）")]float life)
        {
            mLife = life;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/SetFinalPoseRotation")]
        [Editor.DisplayParamName("设置姿态（Pose）旋转信息")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public unsafe void SetFinalPoseRotation([Editor.DisplayParamName("粒子对象(CGfxParticle)")]ref CGfxParticle p, [Editor.DisplayParamName("旋转坐标轴(Quaternion)")]Quaternion q)
        {
            FinalPose.mRotation *= q;
        }


        [Editor.DisplayParamName("设置粒子的Flag")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/SetParticleFlags")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetParticleFlags([Editor.DisplayParamName("附件1（UInt32）")]UInt32 flag)
        {
            mFlags = flag;
        }

        [Editor.DisplayParamName("设置粒子的扩展数据ExtData")]
        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/SetParticlExtData")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetParticlExtData([Editor.DisplayParamName("附件2（UInt32）")]IntPtr extdata)
        {
            mExtData = extdata;
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/CGfxParticle(粒子)/FacePose")]
        [Editor.DisplayParamName("设置姿态（Pose）朝向摄像机")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public unsafe void FacePose([Editor.DisplayParamName("粒子系统对象(CGfxParticleSystem)")]CGfxParticleSystem sys, [Editor.DisplayParamName("Bilboard类型(BILLBOARD_TYPE)")]CGfxParticleSystem.BILLBOARDTYPE type, [Editor.DisplayParamName("坐标系统类型(CoordinateSpace)")]CGfxParticleSystem.CoordinateSpace coord, ref Vector3 prepos)
        {
            var rc = CEngine.Instance.RenderContext;
            if (sys.UseCamera != null)
            {
                sys.Face2(ref this, type, sys.UseCamera, coord, ref prepos);
                return;
            }
#if PWindow
            EngineNS.GamePlay.IModuleInstance ModuleInstance = (EngineNS.GamePlay.IModuleInstance)EngineNS.CEngine.Instance.GameEditorInstance;
            if (ModuleInstance != null)
            {
                sys.Face2(ref this, type, ModuleInstance.GetMainViewCamera(), coord, ref prepos);
            }
#endif
        }
    }
}
