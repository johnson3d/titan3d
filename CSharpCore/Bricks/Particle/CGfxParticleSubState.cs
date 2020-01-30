using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.Particle
{
    public class CGfxParticleSubState : AuxCoreObject<CGfxParticleSubState.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CGfxParticleSubState(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }
        protected EmitShape.CGfxParticleEmitterShape mShape;
        public EmitShape.CGfxParticleEmitterShape Shape
        {
            get
            {
                return mShape;
            }
            set
            {
                mShape = value;
                SDK_GfxParticleSubState_SetShapeType(CoreObject, value.CoreObject);
            }
        }
        public static Profiler.TimeScope ScopeTickGetNewBorns = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleSubState), "TickGetNewBorns");
        public static Profiler.TimeScope ScopeTickDoParticleBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleSubState), "TickDoParticleBorn");

        public static Profiler.TimeScope ScopeTickGetNewBorns2 = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleSubState), "TickGetNewBorns2");
        public static Profiler.TimeScope ScopeTickDoParticleStateBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleSubState), "TickDoParticleStateBorn");
        internal unsafe void OnParticleBorn(McParticleEffector effector, CGfxParticleSystem sys)
        {
            CGfxParticleState** pStates;
            int num = 0;
            ScopeTickGetNewBorns.Begin();
            SDK_GfxParticleSubState_GetNewBorns(CoreObject, &pStates, &num);
            ScopeTickGetNewBorns.End();
            ScopeTickDoParticleBorn.Begin();
            for (int i = 0; i < num; i++)
            {
                var particle = pStates[i]->Host;
                if ((IntPtr)particle == IntPtr.Zero)
                {
                    effector.DoParticleBorn(sys, ref CGfxParticle.Empty);
                }
                else
                    effector.DoParticleBorn(sys, ref (*particle));
            }
            ScopeTickDoParticleBorn.End();
        }

        public unsafe void InitParticle(McParticleEffector effector, CGfxParticleSystem sys, CGfxParticle* p)
        {
            if (sys.TrailNode != null)
            {
                sys.TrailNode.CreateTrail(ref (*p));
            }

            if (sys.TextureCutNode != null)
            {
                sys.TextureCutNode.GetStartValue(ref (*p));
            }

            p->SetParticleLife(sys.MinLife, sys.MaxLife);
            if (sys.EnableEventReceiver)
            {
                sys.BindTriggerEvent(ref (*p));
            }

            //if (sys.TriggerNodes.Count > 0)
            //{
            //    for (int i = 0; i < sys.TriggerNodes.Count; i++)
            //    {
            //        sys.TriggerNodes[i].TriggerBornEvent(ref p->FinalPose);
            //    }
            //}
        }

        public unsafe void UpdateParticle(McParticleEffector effector, float e, CGfxParticleSystem sys, CGfxParticle* p)
        {
        }

        public unsafe void MacrossParticleStateNodeInit( McParticleEffector effector, CGfxParticleSystem sys, CGfxParticle *p, CGfxParticleState *pt, int pi)
        {
            //粒子状态信息的初始化 粒子下面可能有多个状态
            if (MacrossParticleStateNode != null)
            {
                effector.ParticleData.ParticleSystem = sys;
                effector.ParticleData.ParticleSubState = this;
                effector.ParticleData.ParticleEmitterShape = Shape;

                effector.ParticleData.Particle = p;
                effector.ParticleData.ParticleState = pt;

                MacrossParticleStateNode.Init(effector.ParticleData, ref (*p), ref (*pt));
            }

            //var lifetick = p->mLifeTick / p->mLife;
            if (ColorNode != null)
            {
                ColorNode.GetStartValue();
                pt->mStartPose.SetUserParams_Color4(0, ColorNode.StartValue);
            }

            if (ParticleScaleNode != null)
            {
                ParticleScaleNode.GetStartValue();
                pt->mStartPose.mScale = ParticleScaleNode.StartValue;
            }

            if (ParticleVelocityNodes != null && ParticleVelocityNodes.Count > 0)
            {
                for (int i = 0; i < ParticleVelocityNodes.Count; i++)
                {
                    var ParticleVelocityNode = ParticleVelocityNodes[i];
                    if (ParticleVelocityNode != null)
                    {
                        ParticleVelocityNode.GetStartValue();
                        if (ParticleVelocityNodes.Count == 1)
                        {
                            //ParticleVelocityNode.StartValue.Normalize();
                            if (i == 0)
                            {
                                pt->mStartPose.mVelocity = ParticleVelocityNode.StartValue;
                            }
                            else
                            {
                                pt->mStartPose.mVelocity *= ParticleVelocityNode.StartValue;
                            }
                        }
                        else
                        {
                            ParticleVelocityNode.AddStartValue(pi, ref ParticleVelocityNode.StartValue);
                            //ParticleVelocityNode.AddStartValue(pt, ref ParticleVelocityNode.StartValue);
                        }
                        
                    }
                }
            }

            if (RotationNodes != null && RotationNodes.Count > 0)
            {
                for (int i = 0; i < RotationNodes.Count; i++)
                {
                    var rotationNode = RotationNodes[i];
                    if (rotationNode != null)
                    {
                        rotationNode.GetStartValue2();
                        //ParticleVelocityNode.StartValue.Normalize();
                        if (i == 0)
                        {
                            pt->mStartPose.mRotation = rotationNode.StartValue;
                        }
                        else
                        {
                            pt->mStartPose.mRotation *= rotationNode.StartValue;
                        }

                    }
                }
            }

            if (AcceleratedNode != null)
            {
                AcceleratedNode.GetStartValue();
                pt->mStartPose.mAcceleration.X = AcceleratedNode.StartValue;
            }

            if (VelocityByTangentNode != null)
            {
                VelocityByTangentNode.GetStartValue();
                pt->mStartPose.mAxis = VelocityByTangentNode.StartValue;
                
                VelocityByTangentNode.Power.GetStartValue();
                pt->mStartPose.mAcceleration.Y = VelocityByTangentNode.Power.StartValue;
            }
        }

        public unsafe void MacrossParticleStateNodeUpdate(McParticleEffector effector, float e, CGfxParticleSystem sys, CGfxParticle* p, CGfxParticleState* pt, int pi)
        {

            var life =(*p).mLife;
            var tick = (*p).mLifeTick;

            if (tick /  life >= 1)
            {
                if (ParticleVelocityNodes != null && ParticleVelocityNodes.Count > 1)
                {
                    for (int i = 0; i < ParticleVelocityNodes.Count; i++)
                    {
                        var ParticleVelocityNode = ParticleVelocityNodes[i];
                        if (ParticleVelocityNode != null)
                        {
                            ParticleVelocityNode.StartValues.RemoveAt(pi);
                        }
                    }
                }
                return;
            }

            effector.ParticleData.ParticleSystem = sys;
            effector.ParticleData.ParticleSubState = this;
            effector.ParticleData.ParticleEmitterShape = Shape;

            effector.ParticleData.Particle = p;
            effector.ParticleData.ParticleState = pt;
            

            if (ParticleScaleNode != null)
            {
                ParticleScaleNode.SetParticleScale2((*p).mLife, (*p).mLifeTick, ref (*pt).mPose);
                ParticleScaleNode.SetParticleScale(life, tick, ref (*pt).mPose);

                pt->mPose.mScale = ParticleScaleNode.Scale;
                pt->mPose.mScale *= pt->mStartPose.mScale;
            }

            if (VelocityByCenterNode != null)
            {
                VelocityByCenterNode.SetPower(life, tick);
                VelocityByCenterNode.SetPower2((*p).mLife, (*p).mLifeTick);
            }

            if (VelocityByTangentNode != null)
            {
                VelocityByTangentNode.SetAix(life, tick, ref pt->mStartPose.mAxis, pt->mStartPose.mAcceleration.Y);
                VelocityByTangentNode.SetAix2((*p).mLife, (*p).mLifeTick, ref pt->mStartPose.mAxis);
            }

            if (ParticleVelocityNodes != null && ParticleVelocityNodes.Count > 0)
            {
                for (int i = 0; i < ParticleVelocityNodes.Count; i++)
                {
                    var ParticleVelocityNode = ParticleVelocityNodes[i];
                    if (ParticleVelocityNode != null)
                    {
                        ParticleVelocityNode.SetParticleVelocity2((*p).mLife, (*p).mLifeTick, ref (*pt).mPose);
                        ParticleVelocityNode.SetParticleVelocity(life, tick, ref (*pt).mPose);

                        if (ParticleVelocityNodes.Count == 1)
                        {
                            if (i == 0)
                            {
                                pt->mPose.mVelocity = ParticleVelocityNode.Velocity;
                            }
                            else
                            {
                                pt->mPose.mVelocity += ParticleVelocityNode.Velocity;
                            }

                            pt->mPose.mVelocity *= pt->mStartPose.mVelocity;
                        }
                        else
                        {
                            if (i == 0)
                            {
                                pt->mPose.mVelocity = ParticleVelocityNode.Velocity * ParticleVelocityNode.StartValues[pi];
                            }
                            else
                            {
                                pt->mPose.mVelocity += ParticleVelocityNode.Velocity * ParticleVelocityNode.StartValues[pi];
                            }
                        }


                        //ParticleScaleNode.SetParticleScale2((*p).mLife, (*p).mLifeTick, ref (*pt).mPose);
                    }
                }
            }

            if (RotationNodes != null && RotationNodes.Count > 0)
            {
                for (int i = 0; i < RotationNodes.Count; i++)
                {
                    var RotationNode = RotationNodes[i];
                    if (RotationNode != null)
                    {

                        RotationNode.SetYawPitchRoll(life, tick);
                        
                        {
                            RotationNode.Update(e, ref pt->mPose, ref pt->mStartPose);

                        }

                        //ParticleScaleNode.SetParticleScale2((*p).mLife, (*p).mLifeTick, ref (*pt).mPose);
                        if (sys.IsBillBoard)
                        {
                            float Yaw, Pitch, Roll;
                            pt->mPose.mRotation.GetYawPitchRoll(out Yaw, out Pitch, out Roll);
                            pt->mPose.mAngle = Roll;
                        }
                    }
                }
            }

            if (AcceleratedNode != null)
            {
                AcceleratedNode.SetAccelerated(life, tick);
                pt->mPose.mAcceleration.X = AcceleratedNode.Accelerated * pt->mStartPose.mAcceleration.X;

            }
            else
            {
                pt->mPose.mAcceleration.X = 1f;
            }

            if (VelocityByCenterNode != null && VelocityByTangentNode != null)
            {
                VelocityByCenterNode.GetValue(ref (*pt), sys, this, e);
                VelocityByTangentNode.GetValue(ref (*pt), sys, this, e);

                pt->AccelerationEffect(e, ref VelocityByTangentNode.Offset, ref VelocityByCenterNode.Direction);
            }
            else if (VelocityByCenterNode != null)
            {
                VelocityByCenterNode.GetValue(ref (*pt), sys, this, e);

                VelocityByCenterNode.OnlyUseThisNode(e, ref (*pt));
            }
            else if (VelocityByTangentNode != null)
            {
                VelocityByTangentNode.GetValue(ref (*pt), sys, this, e);

                VelocityByTangentNode.OnlyUseThisNode(e, ref (*pt));
            }
            else if (MacrossParticleStateNode != null)
            {
                //if (ParticleVelocityNodes != null && ParticleVelocityNodes.Count > 0)
                {
                    pt->AccelerationEffect(e);
                }
            }

            //粒子状态信息的更新 粒子下面可能有多个状态
            if (MacrossParticleStateNode != null)
            {
                MacrossParticleStateNode.Update(e, effector.ParticleData, ref (*p), ref (*pt));
            }

            if (ColorNode != null)
            {
                ColorNode.SetParticleColor(life, tick, ref (*pt).mPose, ref (*pt).mStartPose);
            }

        }

        public void EffectParticleStatePose(ref CGfxParticleState state)
        {
            Vector4 temppos;
            state.mStartPose.mPosition *= Scale;
            Quaternion rot = EmitRotation;
            Vector3.Transform(ref state.mStartPose.mPosition, ref rot, out temppos);
            state.mStartPose.mPosition.X += temppos.X;
            state.mStartPose.mPosition.Y += temppos.Y;
            state.mStartPose.mPosition.Z += temppos.Z;
            state.mStartPose.mPosition += Position;

            if (rot.IsValid)
            {
                state.mStartPose.mRotation *= rot;
            }

            state.mStartPose.mScale *= Scale;

            Vector3.Transform(ref state.mStartPose.mVelocity, ref rot, out temppos);
            state.mStartPose.mVelocity.X = temppos.X;
            state.mStartPose.mVelocity.Y = temppos.Y;
            state.mStartPose.mVelocity.Z = temppos.Z;
            state.mStartPose.mVelocity.Normalize();

            state.SyncStartToPose();
        }
        public unsafe void SyncStartToPose(CGfxParticleState** pStates, int num)
        {
            if (pStates == null || num == 0)
                return;

            for (int i = 0; i < num; i++)
            {
                pStates[i]->SyncStartToPose();
            }
        }

        internal unsafe void OnParticleStateBorn(McParticleEffector effector, CGfxParticleSystem sys, int stateIndex)
        {
            CGfxParticleState** pStates;
            int num = 0;
            ScopeTickGetNewBorns2.Begin();
            //这里处理每个substate发射出来的粒子，如果num的预期超过粒子数目最大值就有可能...
            SDK_GfxParticleSubState_GetNewBorns(CoreObject, &pStates, &num);
  
            ScopeTickGetNewBorns2.End();
            ScopeTickDoParticleStateBorn.Begin();

            effector.DoParticleStateBorn(sys, pStates, this, stateIndex, num);
            
            ScopeTickDoParticleStateBorn.End();
            SDK_GfxParticleSubState_PushNewBorns(CoreObject);
            if (sys.IsBind == true)
                return;

            if (sys.HostActorPlacement == null)
                return;

            var placement = sys.HostActorPlacement as GParticlePlacementComponent;
            if (placement == null)
                return;

            Vector3 pos;
            Vector3 scale;
            Quaternion rot;
            Vector4 temppos;

            bool IsIgnore = placement.IsIgnore;
            placement.IsIgnore = false;
            placement.WorldMatrix.Decompose(out scale, out rot, out pos);
            placement.IsIgnore = IsIgnore;

            for (int i = 0; i < num; i++)
            {
                //pStates[i]->SyncStartToPose();
                pStates[i]->mStartPose.mPosition *= scale;
                Vector3.Transform(ref pStates[i]->mStartPose.mPosition, ref rot, out temppos);
                pStates[i]->mStartPose.mPosition.X += temppos.X;
                pStates[i]->mStartPose.mPosition.Y += temppos.Y;
                pStates[i]->mStartPose.mPosition.Z += temppos.Z;
                pStates[i]->mStartPose.mPosition += pos;

                if (rot.IsValid)
                {
                    pStates[i]->mStartPose.mRotation *= rot;
                }

                pStates[i]->mStartPose.mScale *= scale;

                Vector3.Transform(ref pStates[i]->mStartPose.mVelocity, ref rot, out temppos);
                pStates[i]->mStartPose.mVelocity.X = temppos.X;
                pStates[i]->mStartPose.mVelocity.Y = temppos.Y;
                pStates[i]->mStartPose.mVelocity.Z = temppos.Z;
                pStates[i]->mStartPose.mVelocity.Normalize();

                //mPose
                pStates[i]->mPose.mPosition *= scale;
                Vector3.Transform(ref pStates[i]->mPose.mPosition, ref rot, out temppos);
                pStates[i]->mPose.mPosition.X += temppos.X;
                pStates[i]->mPose.mPosition.Y += temppos.Y;
                pStates[i]->mPose.mPosition.Z += temppos.Z;
                pStates[i]->mPose.mPosition += pos;

                if (rot.IsValid)
                {
                    pStates[i]->mPose.mRotation *= rot;
                }

                pStates[i]->mPose.mScale *= scale;

                Vector3.Transform(ref pStates[i]->mPose.mVelocity, ref rot, out temppos);
                pStates[i]->mPose.mVelocity.X = temppos.X;
                pStates[i]->mPose.mVelocity.Y = temppos.Y;
                pStates[i]->mPose.mVelocity.Z = temppos.Z;
                pStates[i]->mPose.mVelocity.Normalize();


            }

        }

        #region AllNodes
        public ParticleEmitShapeNode MacrossNode
        {
            get;
            set;
        }

        public ParticleStateNode MacrossParticleStateNode
        {
            get;
            set;
        }

        public ColorBaseNode ColorNode
        {
            get;
            set;
        }

        public ParticleScaleNode ParticleScaleNode
        {
            get;
            set;
        }

        //向心速度结点
        public ParticleVelocityByCenterNode VelocityByCenterNode
        {
            get;
            set;
        }

        //切线速度结点
        public ParticleVelocityByTangentNode VelocityByTangentNode
        {
            get;
            set;
        }

        public List<ParticleVelocityNode> ParticleVelocityNodes
        {
            get;
            set;
        }

        public TransformNode TransformNode
        {
            get;
            set;
        }

        public List<RotationNode> RotationNodes
        {
            get;
            set;
        }

        public RandomDirectionNode RandomDirectionNode
        {
            get;
            set;
        }

        public AcceleratedNode AcceleratedNode
        {
            get;
            set;
        }

        #endregion

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Position
        {
            get
            {
                return SDK_GfxParticleSubState_GetPosition(CoreObject);
            }
            set
            {
                SDK_GfxParticleSubState_SetPosition(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Direction
        {
            get
            {
                return SDK_GfxParticleSubState_GetDirection(CoreObject);
            }
            set
            {
                SDK_GfxParticleSubState_SetDirection(CoreObject, value);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Scale
        {
            get;
            set;
        } = new Vector3(1.0f, 1.0f, 1.0f);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Color4 Color
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float EmitVelocity
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion EmitRotation
        {
            get;
            set;
        }//From mDirection 

        [System.ComponentModel.Browsable(false)]
        public string Name
        {
            get;
            set;
        } = "ParticleState";

        #region MacrossCall
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子发射形状状态对象(CGfxParticleSubState)/InitData")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("初始化发射器状态数据")]
        public void InitData([EngineNS.Editor.DisplayParamName("位置 X（float）")]float posx = 0.0f,
           [EngineNS.Editor.DisplayParamName("位置 Y（float）")]float posy = 0.0f,
           [EngineNS.Editor.DisplayParamName("位置 Z（float）")]float posz = 0.0f,
           [EngineNS.Editor.DisplayParamName("缩放 X（float）")]float scalex = 1.0f,
           [EngineNS.Editor.DisplayParamName("缩放 Y（float）")]float scaley = 1.0f,
           [EngineNS.Editor.DisplayParamName("缩放 Z（float）")]float scalez = 1.0f,
           [EngineNS.Editor.DisplayParamName("旋转轴 X（float）")]float rotx = 0.0f,
           [EngineNS.Editor.DisplayParamName("旋转轴 Y（float）")]float roty = 0.0f,
           [EngineNS.Editor.DisplayParamName("旋转轴 Z（float）")]float rotz = 0.0f,
           [EngineNS.Editor.DisplayParamName("旋转角 W（float）")]float rotw = 0.0f)
        {
            var rc = CEngine.Instance.RenderContext;

            SetPosition(posx, posy, posz);
            SetScale(scalex, scaley, scalez);
            SetEmitRotation(rotx, roty, rotz, rotw);
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子发射形状状态对象(CGfxParticleSubState)/SetPosition")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("设置发射器状态的初始位置")]
        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子发射形状状态对象(CGfxParticleSubState)/SetScale")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("设置发射器状态的初始缩放")]
        public void SetScale(float x, float y, float z)
        {
            Scale = new Vector3(x, y, z);
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子发射形状状态对象(CGfxParticleSubState)/SetDirection")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("设置发射器状态的初始方向")]
        public void SetDirection(float x, float y, float z)
        {
            Direction = new Vector3(x, y, z);
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子发射形状状态对象(CGfxParticleSubState)/SetEmitRotation")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("设置发射器状态的旋转信息")]
        public void SetEmitRotation(float x, float y, float z, float w)
        {
            EmitRotation = new Quaternion(x, y, z, w);
        }
        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxParticleSubState_GetNewBorns(NativePointer self, CGfxParticleState*** ppState, int* num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSubState_PushNewBorns(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxParticleSubState_GetParticles(NativePointer self, CGfxParticleState*** ppState, int* num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static EmitShape.CGfxParticleEmitterShape.NativePointer SDK_GfxParticleSubState_GetShapeType(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSubState_SetShapeType(NativePointer self, EmitShape.CGfxParticleEmitterShape.NativePointer shape);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSubState_Simulate(NativePointer self, float elaspe);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxParticleSubState_GetPosition(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSubState_SetPosition(NativePointer self, Vector3 value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static Vector3 SDK_GfxParticleSubState_GetDirection(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSubState_SetDirection(NativePointer self, Vector3 value);
        #endregion
    }
}
