using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using EngineNS;
using EngineNS.Graphics.Mesh;
using System.Collections;
using System.ComponentModel;

namespace EngineNS.Bricks.Particle
{
    [Editor.MacrossPanelPath("粒子系统/创建粒子系统对象(Create CGfxParticleSystem)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    public class CGfxParticleSystem : AuxCoreObject<CGfxParticleSystem.NativePointer>
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
        public enum BILLBOARDTYPE
        {
            BILLBOARD_DISABLE = 0,
            BILLBOARD_FREE,
            BILLBOARD_LOCKY_EYE,
            BILLBOARD_LOCKY_PARALLEL,//平行摄像机
            BILLBOARD_LOCKVELOCITY,//平行运行速度
        }
        public enum CoordinateSpace
        {
            CSPACE_WORLD,
            CSPACE_LOCAL,
            CSPACE_LOCALWITHDIRECTION,
            CSPACE_WORLDWITHDIRECTION,
        }

        public CGfxParticleSystem()
        {
            mCoreObject = NewNativeObjectByNativeName<NativePointer>("GfxParticleSystem");

            Matrix = EngineNS.Matrix.Identity;

            IsBillBoard = false;

            IsBind = true;

            //TrailDataControlData = new TrailDataControl();
        }
        ~CGfxParticleSystem()
        {
            unsafe
            {
                CGfxParticle* pPartical;
                int num = 0;
                int stride = 0;
                SDK_GfxParticleSystem_GetParticlePool(CoreObject, &pPartical, &num, &stride);
                for (int i = 0; i < num; i++)
                {
                    byte* address = (byte*)pPartical + stride * i;
                    ((CGfxParticle*)address)->FreeTag();
                }
            }
        }

        public void ResetTime()
        {
            PrevFireTime = 0f;
            CurLiveTime = 0f;
            IsFirstFire = true;
            //SDK_GfxParticleSystem_ClearParticles(CoreObject);
            if (SubParticleSystems == null || SubParticleSystems.Count == 0)
                return;

            for (int i = 0; i < SubParticleSystems.Count; i++)
            {
                SubParticleSystems[i].ResetTime();
            }

        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public EngineNS.GamePlay.Actor.GActor HostActor
        {
            get;
            set;
        }

        public GamePlay.Component.GPlacementComponent HostActorPlacement
        {
            get
            {
                if (HostActorMesh != null)
                    return HostActorMesh.Placement;

                return HostActor?.Placement;
            }
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GamePlay.Component.GMeshComponent HostActorMesh
        {
            get;
            set;
        }
        [System.ComponentModel.Browsable(false)]
        public Macross.MacrossGetter<McParticleEffector> Effector
        {
            get;
            set;
        }

        public EngineNS.Matrix Matrix;

        CVertexBuffer mPosVB;
        public CVertexBuffer PosVB
        {
            get { return mPosVB; }
        }
        CVertexBuffer mScaleVB;
        public CVertexBuffer ScaleVB
        {
            get { return mScaleVB; }
        }
        CVertexBuffer mRotateVB;
        public CVertexBuffer RotateVB
        {
            get { return mRotateVB; }
        }
        CVertexBuffer mColorVB;
        public CVertexBuffer ColorVB
        {
            get { return mColorVB; }
        }
        public void BindParticleVB(CPass pass)
        {
            if (SubStates == null || SubStates.Length == 0)
                return;

            if (pass != null && pass.GeometryMesh != null)
            {//这里在粒子Mesh创建Pass的时候调用                
                if (pass.AttachVBs == null)
                {
                    if (HostActorMesh != null && HostActorMesh.SceneMesh != null)//MeshPrimitives
                    {
                        pass.AttachVBs = new CVertexArray();
                        if (TrailNode != null && IsTrail && pass.GeometryMesh != null)
                        {
                            pass.BindGeometry(TrailNode.GeomMesh, 0, 0);
                        }
                        else
                        {
                            pass.BindGeometry(HostActorMesh.SceneMesh.MeshPrimitives, 0, 0);
                        }
                            

                        pass.AttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstPos, mPosVB);
                        pass.AttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstScale, mScaleVB);
                        pass.AttachVBs.BindVertexBuffer(EVertexSteamType.VST_InstQuat, mRotateVB);
                        pass.AttachVBs.BindVertexBuffer(EVertexSteamType.VST_F4_1, mColorVB);

                        if (MaterialInstanceNode != null)
                        {
                            MaterialInstanceNode.SetMaterialInstanceValue(HostActorMesh.SceneMesh);
                        }
                        
                    }
                }
                if (pass.AttachVBs == null)
                    return;

                if (TrailNode != null && IsTrail && pass.GeometryMesh != null)
                {
                    if (TrailNode.TrailDataControlData != null && TrailNode.TrailDataControlData.IsVaild())
                    {
                        TrailNode.TrailDataControlData.BindBuffs(CEngine.Instance.RenderContext, TrailNode.GeomMesh);

                        pass.AttachVBs.NumInstances = 1;
                    }
                    else
                    {
                        pass.AttachVBs.NumInstances = 0;
                    }

                }
                else
                {
                    pass.AttachVBs.NumInstances = (uint)ParticleNumber;
                }
            }
        }

        public void Simulate(float elaspedTime)
        {
            lock (this)
            {
                SDK_GfxParticleSystem_Simulate(CoreObject, elaspedTime);
            }

            //if (SubParticleSystems != null)
            //{
            //    for (int i = 0; i < SubParticleSystems.Count; i++)
            //    {
            //        SubParticleSystems[i].Simulate(elaspedTime);
            //    }
            //}
            if (TriggerEvents.Count > 0)
            {
                for (int i = 0; i < TriggerEvents.Count; i++)
                {
                    TriggerEvents[i].Update(elaspedTime);
                }
            }
        }

        public unsafe void DealDeathParticles()
        {
            unsafe
            {
                CGfxParticle** ppParticles;
                int num;
                GetDeathParticles(&ppParticles, &num);
                //处理粒子死亡
                for (int i = 0; i < num; i++)
                {
                    if (TrailNode != null)
                    {
                        TrailNode.DealDeathParticle(ref *ppParticles[i]);
                    }

                    if (TriggerNodes.Count > 0)
                    {
                        for (int j = 0; j < TriggerNodes.Count; j++)
                        {
                            TriggerNodes[j].TriggerDeathEvent(ref ppParticles[i]->FinalPose);
                        }
                    }

                    Effector.Get(false).DoParticleDead(this, ref *ppParticles[i]);

                    ppParticles[i]->Tag = null;
                }
            }

            //if (SubParticleSystems != null)
            //{
            //    for (int i = 0; i < SubParticleSystems.Count; i++)
            //    {
            //        SubParticleSystems[i].DealDeathParticles();
            //    }
            //}
        }

        public unsafe void DealParticleStateCompose(float elaspe)
        {
            unsafe
            {
                CGfxParticle** ppParticles;
                int num;
                GetParticles(&ppParticles, &num);
                //宏图合成最终粒子姿态
                if (num > 0)
                {
                    Effector.Get(false).OnParticleCompose(this, ppParticles, num, elaspe);
                }
            }

            //if (SubParticleSystems != null)
            //{
            //    for (int i = 0; i < SubParticleSystems.Count; i++)
            //    {
            //        SubParticleSystems[i].DealDeathParticles();
            //    }
            //}
        }

        public void FireParticles()
        {
            int num = FireParticles(FireCountPerTime);
            if (num < FireCountPerTime)
            {
            }

            for (int i = 0; i < SubStates.Length; i++)
            {
                SubStates[i].OnParticleStateBorn(Effector.Get(false), this, i);
            }
        }
        public int FireParticles(int num)
        {
            return SDK_GfxParticleSystem_FireParticles(CoreObject, num);
        }
        public unsafe void GetParticles(CGfxParticle*** ppParticles, int* num)
        {
            SDK_GfxParticleSystem_GetParticles(CoreObject, ppParticles, num);
        }
        public unsafe void GetDeathParticles(CGfxParticle*** ppParticles, int* num)
        {
            SDK_GfxParticleSystem_GetDeathParticles(CoreObject, ppParticles, num);
        }
        public static Profiler.TimeScope ScopeFlush2VB = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxParticleSystem), nameof(Flush2VB));
        public void Flush2VB(CCommandList cmd)
        {
            ScopeFlush2VB.Begin();
            lock (this)
            {
                SDK_GfxParticleSystem_Flush2VB(CoreObject, cmd.CoreObject, vBOOL.FromBoolean(false));
            }
            ScopeFlush2VB.End();
        }

        public CGfxParticleSubState[] SubStates
        {
            get;
            private set;
        }
        public int ParticleNumber
        {
            get
            {
                return SDK_GfxParticleSystem_GetParticleNum(CoreObject);
            }
        }

        public bool CanTriggerParticle()
        {
            if (CurLiveTime > LiveTime)
                return false;

            if (CurLiveTime <= FireDelay)
                return false;

            if (IsFirstFire)
            {
                IsFirstFire = false;
                return true;
            }
            return CurLiveTime - PrevFireTime > FireInterval;
        }

        TriggerEvent TriggeringEvent;
        public void BindTriggerEvent(ref CGfxParticle particle)
        {
            if (TriggeringEvent == null)
                return;

            var tag = particle.Tag as ParticleTag;
            if (tag == null)
            {
                tag = new ParticleTag();
                particle.Tag = tag;
            }

            tag.TriggerEvent = TriggeringEvent;
        }

        public void TriggerParticle()
        {
            if (SubStates == null)
                return;
            if (EnableEventReceiver == false)
            {
                if (CanTriggerParticle() == false)
                    return;

                FireParticles();
            }


            //待触发特效
            if (TriggerEvents.Count == 0)
            {
                return;
            }

            for (int i = TriggerEvents.Count - 1; i >=0; i--)
            {
                var te = TriggerEvents[i];
                TriggeringEvent = te;


                if (te.CanTriggerParticle(this))
                {
                    te.ResetPreTriggerTick();
                    FireParticles();
                }

                TriggeringEvent = null;

                if (te.IsDeath(this))
                {
                    TriggerEvents.RemoveAt(i);
                }
            }

        }
 
        public void UpdateParticleSubState(float elaspe)
        {
            if (SubStates != null)
            {
                var effector = Effector.Get(false);
                for (int i = 0; i < SubStates.Length; i++)
                {
                    if (CurLiveTime <= FireDelay)
                        continue;

                    unsafe
                    {
                        CGfxParticleState** pStates;
                        int num = 0;
                        CGfxParticleSubState.SDK_GfxParticleSubState_GetParticles(SubStates[i].CoreObject, &pStates, &num);
                        if (num > 0)
                        {
                            //宏图处理粒子状态变更
                            effector.OnParticleStateTick(this, pStates, num, i, SubStates[i], elaspe, i);
                        }
                    }

                    //这里处理SubState里面的粒子如果死亡，移除状态
                    CGfxParticleSubState.SDK_GfxParticleSubState_Simulate(SubStates[i].CoreObject, elaspe);

                    if (TrailNode != null)
                    {
                        TrailNode.Clear();
                    }
                    
                    if (SubStates[i].MacrossNode != null)
                    {
                        effector.ParticleData.ParticleSystem = this;
                        effector.ParticleData.ParticleSubState = SubStates[i];
                        effector.ParticleData.ParticleEmitterShape = SubStates[i].Shape;
                        SubStates[i].MacrossNode.Update(elaspe, effector.ParticleData);
                    }

                    var TransformNode = SubStates[i].TransformNode;
                    if (TransformNode != null)
                    {
                        float tick = 0;
                        if (TransformNode.Loop)
                        {
                            tick = ((EngineNS.CEngine.Instance.EngineTimeSecond - TransformNode.StartTick) % TransformNode.Duration) / TransformNode.Duration;
                        }
                        else
                        {
                            tick = MathHelper.FClamp((EngineNS.CEngine.Instance.EngineTimeSecond - TransformNode.StartTick) / TransformNode.Duration, 0f, 1f);
                        }
                        SubStates[i].TransformNode.SetMatrix(tick);
                    }
                }
            }
        }

        #region All Nodes
        //宏图中设置 ParticleSystemNode
        public ParticleSystemNode MacrossNode;

        public List<EngineNS.Bricks.Particle.TriggerNode> TriggerNodes = new List<EngineNS.Bricks.Particle.TriggerNode>();


        public PointGravityNode PointGravityNode;

        TrailNode mTrailNode;
        public TrailNode TrailNode
        {
            get => mTrailNode;
            set
            {
                mTrailNode = value;
                IsTrail = value != null;
            }
        }

        public TextureCutNode TextureCutNode;

        public MaterialInstanceNode MaterialInstanceNode;

        public List<ParticleComposeNode> ComposeNodes = new List<ParticleComposeNode>();

        #endregion
        public Graphics.CGfxCamera UseCamera;
        
        [Category("资源")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Mesh)]
        [Editor.DisplayParamName("模型资源(RName)")]
        public RName UseMeshRName
        {
            get;
            set;
        }
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance)]
        [Editor.DisplayParamName("材质资源(RName)")]
        [Category("资源")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public RName UseMaterialRName
        {
            get;
            set;
        }

        [Browsable(false)]
        public string Name
        {
            get;
            set;
        } = "ParticleSystem";

        [Browsable(false)]
        public string EditorVisibleName
        {
            get;
            set;
        }

        #region Box
        public ParticleAABB AABB = new ParticleAABB();
        [Browsable(false)]
        public float L
        {
            get
            {
                return AABB.L;
            }
            set
            {
                AABB.L = value;
            }
        }

        [Browsable(false)]
        public float W
        {
            get
            {
                return AABB.W;
            }
            set
            {
                AABB.W = value;
            }
        }

        [Browsable(false)]
        public float H
        {
            get
            {
                return AABB.H;
            }
            set
            {
                AABB.H = value;
            }
        }

        [Browsable(false)]
        public float X
        {
            get
            {
                return AABB.X;
            }
            set
            {
                AABB.X = value;
            }
        }

        [Browsable(false)]
        public float Y
        {
            get
            {
                return AABB.Y;
            }
            set
            {
                AABB.Y = value;
            }
        }

        [Browsable(false)]
        public float Z
        {
            get
            {
                return AABB.Z;
            }
            set
            {
                AABB.Z = value;
            }
        }

        [Browsable(false)]
        public bool IsShowBox
        {
            get;
            set;
        } = false;


        #endregion
        [Category("BillBoard")]
        [Editor.DisplayParamName("粒子资源类型是否为BillBoard(bool)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsBillBoard
        {
            get;
            set;
        }

        [Category("BillBoard")]
        [Editor.DisplayParamName("Bilboard类型")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public BILLBOARDTYPE BillBoardType
        {
            get;
            set;
        } = BILLBOARDTYPE.BILLBOARD_DISABLE;

        [Category("BillBoard")]
        [Editor.DisplayParamName("Bilboard坐标系统类型")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public CoordinateSpace CoordSpace
        {
            get;
            set;
        } = CoordinateSpace.CSPACE_WORLD;

        [Category("粒子类型信息")]
        [Editor.DisplayParamName("发射出来的粒子是否绑定父组件(bool)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsBind
        {
            get;
            set;
        }

        [Category("粒子类型信息")]
        [Editor.DisplayParamName("发射出来的粒子是否为拖尾飘带效果(bool)")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsTrail
        {
            get
            {
                return SDK_GfxParticleSystem_GetIsTrail(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetIsTrail(CoreObject, vBOOL.FromBoolean(value));
            }
        }

        [Category("粒子发射信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("最大粒子数量(RName)")]
        public int MaxParticle
        {
            get;
            set;
        }

        [Category("粒子发射信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("粒子系统的生命值(float)")]
        public float LiveTime
        {
            get
            {
                return SDK_GfxParticleSystem_GetLiveTime(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetLiveTime(CoreObject, value);
            }
        }

        public float CurLiveTime
        {
            get
            {
                return SDK_GfxParticleSystem_GetCurLiveTime(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetCurLiveTime(CoreObject, value);
            }
        }

        public bool IsFirstFire
        {
            get;
            set;
        } = true;

        [Category("粒子发射信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("延迟多久播放粒子(float)")]
        public float FireDelay
        {
            get;
            set;
        } = 0.0f;

        public float PrevFireTime
        {
            get
            {
                return SDK_GfxParticleSystem_GetPrevFireTime(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetPrevFireTime(CoreObject, value);
            }
        }
        [Category("粒子生命值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("粒子最大生命值")]
        public float MaxLife
        {
            get;
            set;
        } = 1f;
        [Category("粒子生命值")]
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.DisplayParamName("粒子最小生命值")]
        public float MinLife
        {
            set;
            get;
        } = 0f;

        [Category("粒子发射信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("隔多久播放一次粒子(float)")]
        public float FireInterval
        {
            get
            {
                return SDK_GfxParticleSystem_GetFireInterval(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetFireInterval(CoreObject, value);
            }
        }

        [Category("粒子发射信息")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.DisplayParamName("一次播放粒子的数量(int)")]
        public int FireCountPerTime
        {
            get
            {
                return SDK_GfxParticleSystem_GetFireCountPerTime(CoreObject);
            }
            set
            {
                SDK_GfxParticleSystem_SetFireCountPerTime(CoreObject, value);
            }
        }

        [Category("EventReceiver")]
        [Editor.DisplayParamName("启用")]
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool EnableEventReceiver
        {
            get;
            set;
        } = false;

        public List<TriggerEvent> TriggerEvents = new List<TriggerEvent>();

        public void Face2(ref CGfxParticle p, BILLBOARDTYPE type, Graphics.CGfxCamera camera, CoordinateSpace coord, ref Vector3 prepos)
        {
            if (HostActor == null && HostActorMesh == null)
                return;
            unsafe
            {
                Matrix worldMatrix;
                if (HostActor != null)
                {
                    worldMatrix = HostActor.Placement.WorldMatrix;
                }
                else if (HostActorMesh != null)
                {
                    worldMatrix = HostActorMesh.Placement.WorldMatrix;
                }
                
                fixed (CGfxParticlePose* pose1 = &p.FinalPose)
                fixed (Vector3* pos = &prepos)
                {
                    SDK_GfxParticleSystem_Face2(CoreObject, pose1, type, camera.CoreObject, coord, &worldMatrix, vBOOL.FromBoolean(IsBind), vBOOL.FromBoolean(IsBillBoard), pos);
                }
            }
        }

        public CGfxParticleSystem FindSystemByEditorName(string name)
        {
            if (SubParticleSystems == null || SubParticleSystems.Count == 0)
                return null;

            for (int i = 0; i < SubParticleSystems.Count; i++)
            {
                if (SubParticleSystems[i].EditorVisibleName.Equals(name))
                {
                    return SubParticleSystems[i];
                }
            }
            return null;
        }

        public void InitTriggerNodes()
        {
            if (SubParticleSystems != null && SubParticleSystems.Count == 0)
                return;

            for (int i = 0; i < SubParticleSystems.Count; i++)
            {
                InitSunSystemTriggerNodes(SubParticleSystems[i].TriggerNodes);
            }

            SubParticleSystems.Sort((a, b) =>
            {
                if (a.EnableEventReceiver)
                    return -1;
                return 0;
            });
        }

        public void InitSunSystemTriggerNodes(List<EngineNS.Bricks.Particle.TriggerNode> triggernodes)
        {
            if (triggernodes.Count == 0)
                return;

            for (int i = 0; i < triggernodes.Count; i++)
            {
                var triggernode = triggernodes[i];
                //if (triggernode._Data.TriggerNames.Count > 0)
                //{
                //    for (int j = 0; j > triggernode._Data.TriggerNames.Count; j++)
                //    {
                //        var name = triggernode._Data.TriggerNames[j];
                //        var subsys = FindSystemByEditorName(name);
                //        if(subsys != null)
                //        {
                //            triggernode.ParticleSystems.Add(subsys);
                //        }
                //    }
                //}
                var name = triggernode._Data.TriggerReceiver;
                if (string.IsNullOrEmpty(name) == false)
                {
                    var subsys = FindSystemByEditorName(name);
                    if (subsys != null)
                    {
                        triggernode.ParticleSystems.Add(subsys);
                    }
                }

            }
        }

        public List<CGfxParticleSystem> SubParticleSystems;

        public async System.Threading.Tasks.Task InitSubSystem(EngineNS.GamePlay.Actor.GActor actor, GParticleComponent compoent)
        {
            if (SubParticleSystems == null)
                return;

            for (int i = 0; i < SubParticleSystems.Count; i++)
            {
                if (SubParticleSystems[i].Effector == null)
                {
                    SubParticleSystems[i].Effector = Effector;
                }
                GParticleSubSystemComponent subsyscomponent = new GParticleSubSystemComponent();
                await subsyscomponent.InitParticleSubSystemComponent(actor, compoent, SubParticleSystems[i]);
                
                SubParticleSystems[i].InitParticlePool(CEngine.Instance.RenderContext);

                subsyscomponent.AddHelpMeshComponentHelp(EngineNS.CEngine.Instance.RenderContext, subsyscomponent.Host, SubParticleSystems[i]);
            }
            
            InitTriggerNodes();
        }

        public void InitSystemData(RName MaterialRName, RName MeshRName, int maxNum = 15, float FireInterval = 0.5f, int FireCountPerTime = 3, float LiveTime = 10.0f, float Firedelay = 0.0f)
        {
            UseMaterialRName = MaterialRName;
            UseMeshRName = MeshRName;

            //初始化粒子池
            this.FireInterval = FireInterval;
            this.FireCountPerTime = FireCountPerTime;
            this.LiveTime = LiveTime;

            this.FireDelay = Firedelay;
        }

        public void SetHostActorMaterial([EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance)][Editor.DisplayParamName("材质资源(RName)")]RName rname)
        {
             UseMaterialRName = rname;
        }
      
        #region Temp Nodes
        public List<EmitShape.CGfxParticleEmitterShape> TempSubStates = new List<EmitShape.CGfxParticleEmitterShape>();
        public List<EngineNS.Bricks.Particle.ParticleEmitShapeNode> TempParticleSubStateNodes = new List<EngineNS.Bricks.Particle.ParticleEmitShapeNode>();
        //public List<EngineNS.Bricks.Particle.TriggerNode> TempTriggerNodes = new List<EngineNS.Bricks.Particle.TriggerNode>();
        public Dictionary<int, EngineNS.Bricks.Particle.ParticleStateNode> TempParticleStateNodes = new Dictionary<int, ParticleStateNode>();

        public Dictionary<int, EngineNS.Bricks.Particle.ColorBaseNode> TempParticleColorNodes = new Dictionary<int, EngineNS.Bricks.Particle.ColorBaseNode>();
        public Dictionary<int, EngineNS.Bricks.Particle.ParticleScaleNode> TempParticleScaleNodes = new Dictionary<int, EngineNS.Bricks.Particle.ParticleScaleNode>();
        public Dictionary<int, EngineNS.Bricks.Particle.ParticleVelocityByCenterNode> TempVelocityByCenterNodes = new Dictionary<int, EngineNS.Bricks.Particle.ParticleVelocityByCenterNode>();
        public Dictionary<int, EngineNS.Bricks.Particle.ParticleVelocityByTangentNode> TempVelocityByTangentNodes = new Dictionary<int, EngineNS.Bricks.Particle.ParticleVelocityByTangentNode>();
        public Dictionary<int, List<EngineNS.Bricks.Particle.ParticleVelocityNode>> TempParticleVelocityNodes = new Dictionary<int, List<EngineNS.Bricks.Particle.ParticleVelocityNode>>();
        public Dictionary<int, List<EngineNS.Bricks.Particle.RotationNode>> TempParticleRotationNodes = new Dictionary<int, List<EngineNS.Bricks.Particle.RotationNode>>();

        public Dictionary<int, EngineNS.Bricks.Particle.TransformNode> TempParticleTransformNodes = new Dictionary<int, EngineNS.Bricks.Particle.TransformNode>();

        public Dictionary<int, RandomDirectionNode> TempRandomDirectionNodes = new Dictionary<int, RandomDirectionNode>();

        public Dictionary<int, AcceleratedNode> TempAcceleratedNodes = new Dictionary<int, AcceleratedNode>();

        //public Dictionary<int, EngineNS.Bricks.Particle.ColorBaseNode> TempParticleTrailNodes = new Dictionary<int, EngineNS.Bricks.Particle.ColorBaseNode>();
        #endregion

        #region MacrossCall
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        //[Editor.MacrossPanelPathAttribute("粒子系统/粒子系统对象(CGfxParticleSystem)/InitParticlePool")]
        //[Editor.DisplayParamName("初始化粒子池")]
        public bool InitParticlePool(CRenderContext rc)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;

            if (MaxParticle == 0 || TempSubStates.Count == 0)
                return false;

            //根据最大的粒子数量和粒子发射器形状数量初始化数据
            if (SDK_GfxParticleSystem_InitParticlePool(CoreObject, rc.CoreObject, MaxParticle, TempSubStates.Count) == false)
                return false;

            //填充粒子形状数据
            SubStates = new CGfxParticleSubState[TempSubStates.Count];

            var effectorobject = Effector.Get(false);
            effectorobject.ParticleData.ParticleSystem = this;
            for (int i = 0; i < TempSubStates.Count; i++)
            {
                var ptr = SDK_GfxParticleSystem_GetSubState(CoreObject, i);
                SubStates[i] = new CGfxParticleSubState(ptr);
                TempSubStates[i].SetEmitter(SubStates[i]);
                SubStates[i].Shape = TempSubStates[i];

                if(i < TempParticleSubStateNodes.Count)
                {
                    SubStates[i].MacrossNode = TempParticleSubStateNodes[i];

                    ParticleStateNode ParticleStateNode;
                    if (TempParticleStateNodes.TryGetValue(i, out ParticleStateNode))
                    {
                        SubStates[i].MacrossParticleStateNode = ParticleStateNode;
                    }

                    ColorBaseNode ColorNode;
                    if (TempParticleColorNodes.TryGetValue(i, out ColorNode))
                    {
                        SubStates[i].ColorNode = ColorNode;
                    }

                    ParticleScaleNode ScaleNode;
                    if (TempParticleScaleNodes.TryGetValue(i, out ScaleNode))
                    {
                        SubStates[i].ParticleScaleNode = ScaleNode;
                    }

                    ParticleVelocityByCenterNode VelocityByCenterNode;
                    if (TempVelocityByCenterNodes.TryGetValue(i, out VelocityByCenterNode))
                    {
                        SubStates[i].VelocityByCenterNode = VelocityByCenterNode;
                    }

                    ParticleVelocityByTangentNode VelocityByTangentNode;
                    if (TempVelocityByTangentNodes.TryGetValue(i, out VelocityByTangentNode))
                    {
                        SubStates[i].VelocityByTangentNode = VelocityByTangentNode;
                    }

                    List<ParticleVelocityNode> VelocityNodes;
                    if (TempParticleVelocityNodes.TryGetValue(i, out VelocityNodes))
                    {
                        SubStates[i].ParticleVelocityNodes = VelocityNodes;
                    }

                    List<RotationNode> RotationNodes;
                    if (TempParticleRotationNodes.TryGetValue(i, out RotationNodes))
                    {
                        SubStates[i].RotationNodes = RotationNodes;
                    }

                    TransformNode transformNode;
                    if (TempParticleTransformNodes.TryGetValue(i, out transformNode))
                    {
                        SubStates[i].TransformNode = transformNode;
                        SubStates[i].TransformNode.StartTick = EngineNS.CEngine.Instance.EngineElapseTimeSecond;
                    }

                    SubStates[i].Shape.IsRandomDirection = false;
                    RandomDirectionNode directionNode;
                    if (TempRandomDirectionNodes.TryGetValue(i, out directionNode))
                    {
                        SubStates[i].RandomDirectionNode = directionNode;

                        SubStates[i].RandomDirectionNode.GetValue(SubStates[i].Shape);
                    }

                    AcceleratedNode acceleratedNode;
                    if (TempAcceleratedNodes.TryGetValue(i, out acceleratedNode))
                    {
                        SubStates[i].AcceleratedNode = acceleratedNode;
                    }
                }
                

                effectorobject.DoParticleSubStateBorn(this, SubStates[i], i);
                
            }

            //实例化粒子需要的数据对象 位置 旋转 缩放 颜色
            mPosVB = new CVertexBuffer(SDK_GfxParticleSystem_GetPosVB(CoreObject));
            mPosVB.Core_AddRef();
            mScaleVB = new CVertexBuffer(SDK_GfxParticleSystem_GetScaleVB(CoreObject));
            mScaleVB.Core_AddRef();
            mRotateVB = new CVertexBuffer(SDK_GfxParticleSystem_GetRotateVB(CoreObject));
            mRotateVB.Core_AddRef();
            mColorVB = new CVertexBuffer(SDK_GfxParticleSystem_GetColorVB(CoreObject));
            mColorVB.Core_AddRef();

            return true;
        }
        #endregion

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_GfxParticleSystem_InitParticlePool(NativePointer self, CRenderContext.NativePointer rc, int maxNum, int state);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxParticleSubState.NativePointer SDK_GfxParticleSystem_GetSubState(NativePointer self, int index);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_Simulate(NativePointer self, float elaspedTime);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxParticleSystem_FireParticles(NativePointer self, int num);
      
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal extern static unsafe void SDK_GfxParticleSystem_GetParticlePool(NativePointer self, CGfxParticle** pParticles, int* num, int* stride);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal extern static unsafe void SDK_GfxParticleSystem_GetParticles(NativePointer self, CGfxParticle*** ppParticles, int* num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal extern static unsafe void SDK_GfxParticleSystem_GetDeathParticles(NativePointer self, CGfxParticle*** ppParticles, int* num);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_GfxParticleSystem_GetPosVB(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_GfxParticleSystem_GetScaleVB(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_GfxParticleSystem_GetRotateVB(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CVertexBuffer.NativePointer SDK_GfxParticleSystem_GetColorVB(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_Flush2VB(NativePointer self, CCommandList.NativePointer cmd, vBOOL bImm);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxParticleSystem_GetParticleNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_GfxParticleSystem_Face2(NativePointer self, CGfxParticlePose* pose, BILLBOARDTYPE type, Graphics.CGfxCamera.NativePointer camera, CoordinateSpace coord, Matrix* worldMatrix, vBOOL bind, vBOOL isBillBoard, Vector3* prepos);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleSystem_GetLiveTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_SetLiveTime(NativePointer self, float time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleSystem_GetCurLiveTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_SetCurLiveTime(NativePointer self, float time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleSystem_GetPrevFireTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_SetPrevFireTime(NativePointer self, float time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_GfxParticleSystem_GetFireInterval(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_SetFireInterval(NativePointer self, float time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxParticleSystem_GetFireCountPerTime(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static int SDK_GfxParticleSystem_SetFireCountPerTime(NativePointer self, int time);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static bool SDK_GfxParticleSystem_GetIsTrail(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_SetIsTrail(NativePointer self, vBOOL istrail);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxParticleSystem_ClearParticles(NativePointer self);

        #endregion
    }

}
