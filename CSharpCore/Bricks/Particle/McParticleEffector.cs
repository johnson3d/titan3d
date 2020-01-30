using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Macross;
using EngineNS.Editor;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace EngineNS.Bricks.Particle
{
    [Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class McParticleEffector
    {
        public static Random SRandom = new Random();
        public ParticleData ParticleData = new ParticleData();
        //[Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/InitSystem")]
        //[Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task<bool> InitSystem(CGfxParticleSystem sys, EngineNS.GamePlay.Actor.GActor actor, GParticleComponent compoent, GParticleComponent.GParticleComponentInitializer initializer)
        {
            DoInitSystem(sys, compoent);
            //await sys.SetHostActorMesh(compoent, initializer, UseMeshRName);
            //sys.SetHostActorMaterial(compoent, initializer, UseMaterialRName);
            await sys.InitSubSystem(actor, compoent);
            sys.HostActorMesh = compoent;
            return true;
        }

        public virtual void DoInitSystem(CGfxParticleSystem sys, GParticleComponent compoent)
        {
            CreateParticleSystems(sys);

            compoent.ParticleSystemPropertyChanged(sys);
            InitSystem(sys);
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/InitSystem")]
        //[Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public void InitSystem([EngineNS.Editor.DisplayParamName("粒子系统对象（CGfxParticleSystem）")]CGfxParticleSystem sys)
        {
            //var rc = CEngine.Instance.RenderContext;
            //sys.MaxParticle = maxNum;
            //var shapes = CreateParticleShapes();


            //if (shapes.Count == 0)
            //    return;

            ////初始化粒子池
            //sys.InitParticlePool(rc, sys.MaxParticle, shapes);

            if (sys.MacrossNode != null)
            {
                ParticleData.ParticleSystem = sys;
                if (sys.UseMaterialRName != null && string.IsNullOrEmpty(sys.UseMaterialRName.Address) == false)
                {
                    sys.SetHostActorMaterial(sys.UseMaterialRName);
                }
                sys.MacrossNode.Init(ParticleData);
            }

        }

        public virtual void CreateParticleSystems(CGfxParticleSystem sys)
        {
            if (sys.SubParticleSystems != null)
                sys.SubParticleSystems = new List<CGfxParticleSystem>();

            //sys.SubParticleSystems.Add(CreateParticleSystem1());
        }

        public CGfxParticleSystem CreateParticleSystem1()
        {
            return new CGfxParticleSystem();
        }

        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/CreateParticleShape1")]
        //[Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("创建粒子发射器形态")]
        public virtual EmitShape.CGfxParticleEmitterShape CreateParticleShape1()
        {
            return new EmitShape.CGfxParticleEmitterShapeBox();
        }

        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), nameof(Tick));
        public static Profiler.TimeScope ScopeTickDeath = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickDeath");
        public static Profiler.TimeScope ScopeTickSubBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickSubBorn");
        public static Profiler.TimeScope ScopeFireBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickFireBorn");
        public static Profiler.TimeScope ScopeOnParticleBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickOnParticleBorn");
        public static Profiler.TimeScope ScopeOnParticleStateBorn = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickOnParticleStateBorn");
        public static Profiler.TimeScope ScopeTickSubDeath = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickSubDeath");
        public static Profiler.TimeScope ScopeTickState = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickState");
        public static Profiler.TimeScope ScopeTickCompose = Profiler.TimeScopeManager.GetTimeScope(typeof(McParticleEffector), "TickCompose");
        public void Tick(CCommandList cmd, CGfxParticleSystem sys, float elaspe)
        {
            ScopeTick.Begin();

            sys.Simulate(elaspe);//这里处理粒子死亡释放

            ScopeTickDeath.Begin();
            ScopeTickSubDeath.Begin();
            sys.DealDeathParticles();
            ScopeTickSubDeath.End();
            ScopeTickSubBorn.Begin();

            sys.TriggerParticle();
         
            ScopeTickSubBorn.End();
            ScopeTickDeath.End();

            //粒子系统的更新
            if (sys.MacrossNode != null)
            {
                ParticleData.ParticleSystem = sys;
                sys.MacrossNode.Update(elaspe, ParticleData);
            }
            ScopeTickState.Begin();
            sys.UpdateParticleSubState(elaspe);
            ScopeTickState.End();

            ScopeTickCompose.Begin();
            sys.DealParticleStateCompose(elaspe);
            ScopeTickCompose.End();

            sys.Flush2VB(cmd);

            ScopeTick.End();
        }
        
        //生成粒子时的初始数据 宏图中设置
        [DisplayParamName("生成粒子逻辑")]
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/DoParticleBorn")]
        //[Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void DoParticleBorn([DisplayParamName("粒子系统对象（CGfxParticleSystem）")]CGfxParticleSystem sys, [DisplayParamName("粒子对象（CGfxParticle）")]ref CGfxParticle p)
        {
            //p.mLife = 100.0f;
 
        }

        #region DoParticleSubStateBorn
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        //[Editor.MacrossPanelPathAttribute("粒子系统/粒子系统对象(CGfxParticleSystem)/DoParticleSubState")]
        //[EngineNS.Editor.DisplayParamName("粒子发射器状态对象初始华")]
        public virtual void DoParticleSubStateBorn([EngineNS.Editor.DisplayParamName("粒子发射系统对象")]CGfxParticleSystem sys, [EngineNS.Editor.DisplayParamName("粒子发射器状态对象")]CGfxParticleSubState substate, int index)
        {
            if (substate.MacrossNode != null)
            {
                ParticleData.ParticleSubState = substate;
                ParticleData.ParticleEmitterShape = substate.Shape;
                substate.MacrossNode.Init(ParticleData);
            }
        }
        #endregion

        #region DoParticleSubStateTick
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/DoParticleSubState")]
        [EngineNS.Editor.DisplayParamName("粒子发射器状态对更新逻辑")]
        public virtual void DoParticleSubStateTick([EngineNS.Editor.DisplayParamName("粒子发射器状态对象")]CGfxParticleSubState substate, int index)
        {
            DoParticleSubStateTick1(substate);
        }

        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/DoParticleSubStateTick1")]
        [EngineNS.Editor.DisplayParamName("粒子发射器状态对更新逻辑")]
        public virtual void DoParticleSubStateTick1([EngineNS.Editor.DisplayParamName("粒子发射器状态对象")]CGfxParticleSubState substate)
        {
        }
        
        #endregion

        #region ParticleStateBorn
        //生成ParticleState时的初始数据 宏图中设置
        public virtual unsafe void DoParticleStateBorn(CGfxParticleSystem sys, CGfxParticleState** pStates, CGfxParticleSubState substate, int stateIndex, int particlenum = 0)
        {
            int nowindex = sys.ParticleNumber - particlenum;
            for (int i = 0; i < particlenum; i++)
            {
                if (stateIndex == 0)
                {
                    substate.InitParticle(this, sys, pStates[i]->Host);
                }
                substate.MacrossParticleStateNodeInit(this, sys, pStates[i]->Host, pStates[i], nowindex + i);
                substate.EffectParticleStatePose(ref *pStates[i]);

            }
        }
      

        #endregion

        //宏图中处理逻辑 工具中提供固定形式
        public unsafe void OnParticleStateTick(CGfxParticleSystem sys, CGfxParticleState** pStates, int num, int stateIndex, CGfxParticleSubState state, float elaspe, int pindex)
        {
            for (int i = num - 1; i >= 0 ; i--)
            {
                if (pindex == 0)
                {
                    state.UpdateParticle(this, elaspe, sys, pStates[i]->Host);
                }

                state.MacrossParticleStateNodeUpdate(this, elaspe, sys, pStates[i]->Host, pStates[i], i);
            }
        }

        //计算最终粒子姿态
        public unsafe void OnParticleCompose(CGfxParticleSystem sys, CGfxParticle** ppParticles, int num, float elaspe)
        {
            DoParticleCollectCompose(sys, ppParticles, num, sys.SubStates.Length, elaspe);
        }

        bool usecomposenodes = false;
        Vector3 prepos;
        Vector3 gravity;
        Vector4 matrixtovec4;
        Matrix matrix;
        public virtual unsafe void DoParticleCollectCompose(CGfxParticleSystem sys, CGfxParticle** ppParticles, int particlenum, int substatenum, float elaspe)
        {
            bool usecompute = true;
            for (int i = 0; i < particlenum; i++)
            {
                if (sys.BillBoardType != CGfxParticleSystem.BILLBOARDTYPE.BILLBOARD_DISABLE || sys.TriggerNodes.Count > 0)
                {
                    prepos = ppParticles[i]->FinalPose.mPosition;
                }
               
                if (sys.ComposeNodes.Count > 0)
                {
                    usecomposenodes = false;
                    for (int ni = 0; ni < sys.ComposeNodes.Count; ni++)
                    {
                        if (sys.ComposeNodes[ni].CanUsed)
                        {
                            sys.ComposeNodes[ni].ComposeParticle(sys, ref *ppParticles[i], substatenum, elaspe, usecompute);
                            if (usecomposenodes == false)
                            {
                                (*ppParticles[i]).FinalPose = sys.ComposeNodes[ni].ResultPose;// TODO.
                            }
                            else
                            {
                                CGfxParticlePose.AddFunc(ref (*ppParticles[i]).FinalPose, ref (*ppParticles[i]).FinalPose, ref sys.ComposeNodes[ni].ResultPose);
                            }

                            usecomposenodes = true;
                        }
                    }
                }

                usecompute = false;

                if (usecomposenodes == false)
                {
                    if (sys.SubStates.Length > 0)
                    {
                        var ps = ppParticles[i]->GetState(0);
                        ppParticles[i]->FinalPose = ps->mPose;
                        CGfxParticlePose.Set(ref ppParticles[i]->FinalPose, ref ps->mPose);
                        var substate = sys.SubStates[0];
                        if (substate.TransformNode != null)
                        {
                            Quaternion.RotationYawPitchRoll(substate.TransformNode.YawPitchRoll.X, substate.TransformNode.YawPitchRoll.Y, substate.TransformNode.YawPitchRoll.Z, out substate.TransformNode.Rotation);
                            Matrix.Transformation(substate.TransformNode.Scale, substate.TransformNode.Rotation, substate.TransformNode.Translation, out matrix);
                            
                            Vector3.Transform(ref (ppParticles[i]->FinalPose.mPosition), ref matrix, out matrixtovec4);
                            ppParticles[i]->FinalPose.mPosition.X = matrixtovec4.X;
                            ppParticles[i]->FinalPose.mPosition.Y = matrixtovec4.Y;
                            ppParticles[i]->FinalPose.mPosition.Z = matrixtovec4.Z;

                            if (substate.TransformNode.IgnoreRotation == false)
                            {
                                if (substate.RotationNodes == null || substate.RotationNodes.Count == 0)
                                {
                                    ppParticles[i]->FinalPose.mRotation = substate.TransformNode.Rotation;
                                }
                                else
                                {
                                    ppParticles[i]->FinalPose.mRotation *= substate.TransformNode.Rotation;
                                }
                            }

                            if (substate.TransformNode.IgnoreScale == false)
                            {
                                if (substate.ParticleScaleNode != null)
                                {
                                    ppParticles[i]->FinalPose.mScale *= substate.TransformNode.Scale;
                                }
                                else
                                {
                                    ppParticles[i]->FinalPose.mScale = substate.TransformNode.Scale;
                                }
                               
                            }
                        }

                    }
                    
                }
                //TODO..
                if (sys.TrailNode != null)
                {
                    sys.TrailNode.CreateAndUpdateTrailData(sys, ref *ppParticles[i], elaspe);
                }

                if (sys.TextureCutNode != null)
                {
                    sys.TextureCutNode.Update(elaspe, ref *ppParticles[i]);
                    ppParticles[i]->FinalPose.SetUserParamsY(1, (byte)sys.TextureCutNode.CurrentIndex);
                }


                //处理处罚结点
                if(sys.TriggerNodes.Count > 0)
                {
                    for (int j = 0; j < sys.TriggerNodes.Count; j++)
                    {
                        sys.TriggerNodes[j].RefreshUpdateEvent(ref ppParticles[i]->FinalPose, ref prepos);
                        sys.TriggerNodes[j].TriggerBornEvent(ref ppParticles[i]->FinalPose);
                        sys.TriggerNodes[j].TriggerUpdateEvent(ref ppParticles[i]->FinalPose, ref prepos);
                    }
                }

                if (sys.EnableEventReceiver)
                {
                    var tag = ppParticles[i]->Tag as ParticleTag;
                    if (tag != null)
                    {
                        if (tag.TriggerEvent != null)
                        {
                            if (tag.TriggerEvent.TriggerData.InheritPosition)
                            {
                                ppParticles[i]->FinalPose.mPosition += tag.TriggerEvent.Pose.mPosition;
                            }

                            if (tag.TriggerEvent.TriggerData.InheritScale)
                            {
                                ppParticles[i]->FinalPose.mScale *= tag.TriggerEvent.Pose.mScale;
                            }

                            if (tag.TriggerEvent.TriggerData.InheritRotation)
                            {
                                ppParticles[i]->FinalPose.mRotation *= tag.TriggerEvent.Pose.mRotation;
                            }

                            if (tag.TriggerEvent.TriggerData.InheritVelocity)//Todo
                            {
                                ppParticles[i]->FinalPose.mPosition += tag.TriggerEvent.Pose.mVelocity * elaspe;
                            }
                        }
                    }
                }

                if (sys.PointGravityNode != null)
                {
                    gravity = sys.PointGravityNode._Data.Point - ppParticles[i]->FinalPose.mPosition;
                   // var dis = gravity.Length();
                    //if (dis > 0.00001f)
                    {
                        ppParticles[i]->FinalPose.mAcceleration.X = 0.5f * sys.PointGravityNode._Data.Accelerated * ppParticles[i]->mLifeTick * ppParticles[i]->mLifeTick;

                        //if (dis < ppParticles[i]->FinalPose.mAcceleration.X)
                        //{
                        //    ppParticles[i]->FinalPose.mAcceleration.X = dis;
                        //}
                        gravity += ppParticles[i]->FinalPose.mVelocity;
                        gravity.Normalize();

                        ppParticles[i]->FinalPose.mPosition += gravity * ppParticles[i]->FinalPose.mAcceleration.X;
                    }
                   
                }

                if (sys.BillBoardType != CGfxParticleSystem.BILLBOARDTYPE.BILLBOARD_DISABLE)
                {
                    ppParticles[i]->FacePose(sys, sys.BillBoardType, sys.CoordSpace, ref prepos);
                }

                ppParticles[i]->FinalPose.SetUserParamsX(1, 1);
            }
        }
        
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/DoParticleDead")]
        [Editor.DisplayParamName("粒子死亡逻辑")]
        //[Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void DoParticleDead([Editor.DisplayParamName("粒子系统对象(CGfxParticleSystem)")]CGfxParticleSystem sys, [Editor.DisplayParamName("粒子对象(CGfxParticle)")]ref CGfxParticle p)
        {
            p.mExtData = IntPtr.Zero;
            p.mFlags = 0;
        }

        #region Logic
        public delegate float OnGetFactorDelegate([Editor.DisplayParamName("粒子生命值(float)")]float life, [Editor.DisplayParamName("粒子当前生命值(float)")]float lifetick);//[Editor.DisplayParamName("粒子对象(CGfxParticle)")]ref CGfxParticle p
        //OnGetFactorDelegate OnGetFactor;
        [Editor.MacrossPanelPathAttribute("粒子系统/粒子效应器(McParticleEffector)/LerpPose")]
        [Editor.DisplayParamName("两个粒子姿态对象中进行插值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public unsafe void LerpPoseFromState([Editor.DisplayParamName("粒子对象(CGfxParticle)")]ref CGfxParticle p, [Editor.DisplayParamName("粒子对象中状态对象1(int)")]int stateindex1, [Editor.DisplayParamName("粒子对象中状态对象2(int)")]int stateindex2, [Editor.DisplayParamName("过度系数(OnGetFactorDelegate)")]OnGetFactorDelegate GetFactor)
        {
            var ps0 = p.GetState(stateindex1);
            var ps1 = p.GetState(stateindex2);
            float factor = GetFactor == null ? 0 : GetFactor(p.mLife, p.mLifeTick);
            factor = (float)Math.Max(0.0, Math.Min(1.0, factor));
            CGfxParticlePose.Lerp(ref p.FinalPose, ref ps0->mPose, ref ps1->mPose, factor);
        }

        #endregion

        #region Common
        [Editor.MacrossPanelPathAttribute("粒子系统/RandomF2")]
        [Editor.DisplayParamName("在两个浮点数中插值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float RandomF2([Editor.DisplayParamName("数据1(float)")]float f1, [Editor.DisplayParamName("数据2(float)")]float f2)
        {
            if (f1 >= f2)
            {
                return f1;
            }
            return McParticleEffector.SRandom.Next((int)(f1 * 1000f), (int)(f2 * 1000f)) * 0.001f;
        }

        public static int SRandomI2(int f1, int f2)
        {
            if (f1 >= f2)
            {
                return f1;
            }
            return McParticleEffector.SRandom.Next(f1, f2);
        }

        public static float SRandomF2(float f1, float f2)
        {
            if (f1 >= f2)
            {
                return f1;
            }
            return McParticleEffector.SRandom.Next((int)(f1 * 1000f), (int)(f2 * 1000f)) * 0.001f;
        }

        public static void SRandomV4(ref Vector4 v1, ref Vector4 v2, ref Vector4 v3)
        {
            if (v1.X >= v2.X)
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v2.X * 1000f), (int)(v1.X * 1000f)) * 0.001f;
            }
            else
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v1.X * 1000f), (int)(v2.X * 1000f)) * 0.001f;
            }

            if (v1.Y >= v2.Y)
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v2.Y * 1000f), (int)(v1.Y * 1000f)) * 0.001f;
            }
            else
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v1.Y * 1000f), (int)(v2.Y * 1000f)) * 0.001f;
            }

            if (v1.Z >= v2.Z)
            {
                v3.Z = McParticleEffector.SRandom.Next((int)(v2.Z * 1000f), (int)(v1.Z * 1000f)) * 0.001f;
            }
            else
            {
                v3.Z = McParticleEffector.SRandom.Next((int)(v1.Z * 1000f), (int)(v2.Z * 1000f)) * 0.001f;
            }

            if (v1.W >= v2.W)
            {
                v3.W = McParticleEffector.SRandom.Next((int)(v2.W * 1000f), (int)(v1.W * 1000f)) * 0.001f;
            }
            else
            {
                v3.W = McParticleEffector.SRandom.Next((int)(v1.W * 1000f), (int)(v2.W * 1000f)) * 0.001f;
            }

        }

        public static void SRandomV3(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
        {
            if (v1.X >= v2.X)
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v2.X * 1000f), (int)(v1.X * 1000f)) * 0.001f;
            }
            else
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v1.X * 1000f), (int)(v2.X * 1000f)) * 0.001f;
            }

            if (v1.Y >= v2.Y)
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v2.Y * 1000f), (int)(v1.Y * 1000f)) * 0.001f;
            }
            else
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v1.Y * 1000f), (int)(v2.Y * 1000f)) * 0.001f;
            }

            if (v1.Z >= v2.Z)
            {
                v3.Z = McParticleEffector.SRandom.Next((int)(v2.Z * 1000f), (int)(v1.Z * 1000f)) * 0.001f;
            }
            else
            {
                v3.Z = McParticleEffector.SRandom.Next((int)(v1.Z * 1000f), (int)(v2.Z * 1000f)) * 0.001f;
            }

        }

        public static void SRandomScale(ref Vector3 v1, ref Vector3 v2, ref Vector3 v3)
        {
            float factor = McParticleEffector.SRandom.Next(0, 1000) * 0.001f;
            Vector3.Lerp(ref v1, ref v2, factor, out v3);
        }

        public static void SRandomRotation(ref Quaternion v1, ref Quaternion v2, out Quaternion v3)
        {
            float factor = McParticleEffector.SRandom.Next(0, 1000) * 0.001f;
            EngineNS.Quaternion.Lerp(ref v1, ref v2, factor, out v3);
        }

        public static void SRandomV2(ref Vector2 v1, ref Vector2 v2, ref Vector2 v3)
        {
            if (v1.X >= v2.X)
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v2.X * 1000f), (int)(v1.X * 1000f)) * 0.001f;
            }
            else
            {
                v3.X = McParticleEffector.SRandom.Next((int)(v1.X * 1000f), (int)(v2.X * 1000f)) * 0.001f;
            }

            if (v1.Y >= v2.Y)
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v2.Y * 1000f), (int)(v1.Y * 1000f)) * 0.001f;
            }
            else
            {
                v3.Y = McParticleEffector.SRandom.Next((int)(v1.Y * 1000f), (int)(v2.Y * 1000f)) * 0.001f;
            }

        }
        

        #endregion
    }

    public class McParticleHelper : BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //var modifier = new CGfxParticleSystem();
            //modifier.InitParticlePool(CEngine.Instance.RenderContext, 32, 
            //    new List<EmitShape.CGfxParticleEmitterShape>() 
            //    {
            //        new EmitShape.CGfxParticleEmitterShape()
            //    });
           
            //var sys = new McParticleEffector();
            //for (int i = 0; i < 100; i++)
            //{
            //    sys.Tick(modifier, 0.01F);
            //}
            //await base.DoTest();
        }

        #region SDK
        public static Profiler.TimeScope ScopeAll = Profiler.TimeScopeManager.GetTimeScope("BenchMark.All");
        public static Profiler.TimeScope ScopeDelegate = Profiler.TimeScopeManager.GetTimeScope("BenchMark.Delegate");
        public static Profiler.TimeScope ScopeMethod = Profiler.TimeScopeManager.GetTimeScope("BenchMark.Method");
        public static Profiler.TimeScope ScopeCallCpp = Profiler.TimeScopeManager.GetTimeScope("BenchMark.CallCpp");
        public static Profiler.TimeScope ScopeCpp2Delegate = Profiler.TimeScopeManager.GetTimeScope("BenchMark.Cpp2Delegate");
        public static Profiler.TimeScope ScopeCppPure = Profiler.TimeScopeManager.GetTimeScope("BenchMark.CppPure");
        public static Profiler.TimeScope ScopeTimeScope = Profiler.TimeScopeManager.GetTimeScope("BenchMark.TimeScope");
        public static Profiler.TimeScope ScopeTimeScopeDummy = Profiler.TimeScopeManager.GetTimeScope("BenchMark.TimeScopeDummy");
        public static bool DoBenchMark = false;
        public static void Benchmark()
        {
            if (DoBenchMark == false)
                return;
            ScopeAll.Begin();
            const int repeat = 1;//1000
            const int count = 100000;

            FTestDelegate fun = (int num, int a, float b) =>
            {
                int r = 0;
                for (int i = 0; i < num; i++)
                {
                    r += a;
                    r += (int)b;
                }
                return r;
            };

            ScopeDelegate.Begin();
            for (int i = 0; i < count; i++)
            {
                fun(repeat, 1, 2.0f);
            }
            var t2 = Support.Time.HighPrecision_GetTickCount();
            ScopeDelegate.End();

            ScopeMethod.Begin();
            for (int i = 0; i < count; i++)
            {
                TestCall(repeat, 1, 2.0f);
            }
            t2 = Support.Time.HighPrecision_GetTickCount();
            ScopeMethod.End();

            ScopeCallCpp.Begin();
            for (int i = 0; i < count; i++)
            {
                SDK_TestCall(repeat, 1, 2.0f);
            }
            ScopeCallCpp.End();

            ScopeCpp2Delegate.Begin();
            SDK_TestCallDelegate(fun, count, repeat, 1, 2.0f);
            ScopeCpp2Delegate.End();

            ScopeCppPure.Begin();
            SDK_TestPureCpp(count, repeat, 1, 2.0f);
            ScopeCppPure.End();

            const int samp_count = 10000;
            ScopeTimeScope.Begin();
            ScopeTimeScopeDummy.Enable = true;
            for (int i = 0; i < samp_count; i++)
            {
                ScopeTimeScopeDummy.Begin();
                ScopeTimeScopeDummy.End();
            }
            ScopeTimeScope.End();

            ScopeAll.End();
        }
#if PWindow
        public const string ModuleNC = @"Core.Windows.dll";
#elif PAndroid
        public const string ModuleNC = @"libCore.Droid.so";
#else
        public const string ModuleNC = @"Internal";
#endif
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_TestCall(int num, int a, float b);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        extern static void SDK_TestCallDelegate(FTestDelegate func, int num, int repeat, int a, float b);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        extern static void SDK_TestPureCpp(int num, int repeat, int a, float b);
        delegate int FTestDelegate(int num, int a, float b);
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]//这个强制inline实际测试下来严重降低性能
        static int TestCall(int num, int a, float b)
        {
            int r = 0;
            for (int i = 0; i < num; i++)
            {
                r += a;
                r += (int)b;
            }
            return r;
        }
        #endregion
    }
}

namespace EngineNS.GamePlay
{
    public partial class McGameInstance
    {
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static async System.Threading.Tasks.Task<GamePlay.Actor.GActor> CreateParticleActor2(
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]RName gmsName,
            [Editor.Editor_RNameMacrossType(typeof(EngineNS.Bricks.Particle.McParticleEffector))]RName macross, 
            bool IdentityDrawTransform)
        {
            var rc = CEngine.Instance.RenderContext;
            // ParticleActor
            var actor = new GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;

            var particleComp = new Bricks.Particle.GParticleComponent();
            var particleInit = new Bricks.Particle.GParticleComponent.GParticleComponentInitializer();
            particleInit.MeshName = gmsName;
            particleInit.MacrossName = macross;
            await particleComp.SetInitializer(rc, actor, actor, particleInit);
            particleComp.IsIdentityDrawTransform = true;

            actor.AddComponent(particleComp);

            return actor;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static bool AddActor2Scene(GamePlay.GGameInstance game, GamePlay.Actor.GActor actor, Guid sceneId)
        {
            GamePlay.SceneGraph.GSceneGraph scene = null;
            if (sceneId == Guid.Empty)
                sceneId = game.World.DefaultScene.SceneId;

            scene = game.World.GetScene(sceneId);
            if (scene == null)
                return false;
            game.World.AddActor(actor);
            scene.AddActor(actor);
            return true;
        }
    }
}
