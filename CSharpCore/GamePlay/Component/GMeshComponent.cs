using EngineNS.Graphics;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Graphics.View;
using System.ComponentModel;
using EngineNS.IO.Serializer;

namespace EngineNS.GamePlay.Component
{
    [Rtti.MetaClassAttribute]

    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMeshComponentInitializer), "模型组件", "Mesh", "MeshComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/staticmesh_64x.txpic", RName.enRNameType.Editor)]
    public partial class GMeshComponent : GVisualComponent, IComponentHostSelectOperation, Bricks.Animation.Skeleton.ISocketable, IPlaceable
    {
        [Rtti.MetaClass]
        public class GMeshComponentInitializer : GComponentInitializer
        {
            public GMeshComponent HostMeshComp;
            private RName mMeshName;
            [Rtti.MetaData]
            [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]
            [Editor.Editor_PackData()]
            public RName MeshName
            {
                get
                {
                    return mMeshName;
                }
                set
                {
                    mMeshName = value;
                    if (HostMeshComp == null)
                        return;
                    //if (mMtlList == null || (HostMeshComp.SceneMesh != null && HostMeshComp.SceneMesh.Name != value))
                    //{
                    //    var nouse = UpdateMtlInst();
                    //}
                    if (HostMeshComp.SceneMesh != null && HostMeshComp.SceneMesh.Name != value)
                    {
                        var nouse = UpdateMtlInst();
                    }
                }
            }
            [Rtti.MetaData]
            public bool Visible { get; set; } = true;
            [Rtti.MetaData]
            public bool HideInGame { get; set; } = false;
            internal async System.Threading.Tasks.Task UpdateMtlInst()
            {
                var mesh = await CEngine.Instance.MeshManager.CreateMeshAsync(CEngine.Instance.RenderContext, mMeshName);
                if (mesh == null)
                    return;
                HostMeshComp.SetSceneMesh(CEngine.Instance.RenderContext.ImmCommandList, mesh);
                HostMeshComp.OnPlacementChanged(HostMeshComp.Placement);
                ////////////////////临时用来 修正错误 mMtlList
                if (mMtlList != null && mesh.MtlMeshArray.Length == mMtlList.Count)
                {
                    bool validIndex = false;
                    for (int i = 0; i < mMtlList.Count; ++i)
                    {
                        for (int j = i; j < mMtlList.Count; ++j)
                        {
                            if (mMtlList[i].Index == mMtlList[j].Index)
                            {
                                validIndex = true;
                                break;
                            }
                        }
                    }
                    if (validIndex)
                    {
                        for (int i = 0; i < mMtlList.Count; ++i)
                        {
                            mMtlList[i].Index = (uint)i;
                        }
                    }
                    return;
                }
                ////////////////////临时用来 修正错误 mMtlList

                if (mMtlList != null && mesh.MtlMeshArray.Length == mMtlList.Count)
                    return;
                mMtlList = new List<MtlInst>();
                for (int i = 0; i < mesh.MtlMeshArray.Length; i++)
                {
                    var elem = new MtlInst();
                    elem.Index = (UInt32)i;
                    elem.MeshComp = HostMeshComp;
                    //elem.Name = mesh.MaterialNames[i];
                    mMtlList.Add(elem);
                }


            }
            [Rtti.MetaClass]
            public class MtlInst : IO.Serializer.Serializer, Editor.Editor_RNameTypeObjectBind
            {
                public override void BeforeRead()
                {

                }
                public override void BeforeWrite()
                {

                }

                GMeshComponent mMeshComp;
                [Browsable(false)]
                public GMeshComponent MeshComp
                {
                    get => mMeshComp;
                    set
                    {
                        mMeshComp = value;
                        if(value==null)
                        {
                            return;
                        }
                    }
                }
                private RName mName;
                [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.MaterialInstance, true)]
                [Rtti.MetaData]
                [Editor.Editor_PackData()]
                public RName Name
                {
                    get
                    {
                        if ((mName == null || mName == RName.EmptyName) && MeshComp != null)
                        {
                            var rc = CEngine.Instance.RenderContext;
                            var origMeshTask = CEngine.Instance.MeshManager.GetMeshOrigion(rc, MeshComp.MeshName);
                            if (origMeshTask.IsCompleted)
                            {
                                var oriMtl = origMeshTask.Result.GetMaterialName(Index);
                                return oriMtl;
                            }
                            return null;
                        }
                        else
                        {
                            return mName;
                        }
                    }
                    set
                    {
                        mName = value;
                        if (MeshComp == null)
                            return;
                        Action action = async () =>
                        {
                            var rc = CEngine.Instance.RenderContext;
                            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, value);
                            if (mtl != null)
                            {
                                MeshComp.SetMaterialInstance(rc, Index, mtl, null);
                            }
                            else
                            {
                                var origMesh = await CEngine.Instance.MeshManager.GetMeshOrigion(rc, MeshComp.MeshName);
                                var oriMtl = origMesh.GetMaterialName(Index);
                                if (oriMtl != RName.EmptyName)
                                {
                                    mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, oriMtl);
                                    MeshComp.SetMaterialInstance(rc, Index, mtl, null);
                                }
                            }
                        };
                        action();
                    }
                }
                public bool IsSetName()
                {
                    return mName != null;
                }
                [Rtti.MetaData]
                [ReadOnly(true)]
                public UInt32 Index { get; set; }

                public void invoke(object param)
                {
                    EngineNS.Graphics.Mesh.CGfxMtlMesh mtlmesh = MeshComp.SceneMesh.MtlMeshArray[Index];
                    if (mtlmesh != null)
                        mtlmesh.Visible = (bool)param;
                }
            }
            private List<GMeshComponentInitializer.MtlInst> mMtlList = null;
            [Rtti.MetaData]
            public List<GMeshComponentInitializer.MtlInst> MtlList
            {
                get
                {
                    return mMtlList;
                }
                set
                {
                    mMtlList = value;
                }
            }
            [Rtti.MetaData]
            public GPlacementComponent.GPlacementComponentInitializer PlacementComponentInitializer { get; set; }
            [Rtti.MetaData]
            public string SocketName { get; set; }
        }
        public override bool Visible
        {
            get
            {
                var initializer = this.Initializer as GMeshComponentInitializer;
                return initializer.Visible;
            }
            set
            {
                var initializer = this.Initializer as GMeshComponentInitializer;
                initializer.Visible = value;
            }
        }
        public bool HideInGame
        {
            get
            {
                var initializer = this.Initializer as GMeshComponentInitializer;
                return initializer.HideInGame;
            }
            set
            {
                var initializer = this.Initializer as GMeshComponentInitializer;
                initializer.HideInGame = value;
            }
        }
        public GMeshComponent()
        {
            this.Initializer = new GMeshComponentInitializer();
            Placement.HostContainer = this;
            OnlyForGame = false;
        }
        public override void Cleanup()
        {
            if (mSceneMesh != null)
            {
                mSceneMesh.Dispose();
                mSceneMesh = null;
            }
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GMeshComponentInitializer;
            if (init == null)
                return false;
            Visible = init.Visible;
            init.HostMeshComp = this;
            EngineNS.Graphics.Mesh.CGfxMesh mesh;
            if (init.MeshName != null)
            {
                if (init.MeshName.Name.Equals("@createplane"))
                {
                    var meshpri = CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
                    Graphics.Mesh.CGfxMeshCooker.MakeRect3D(rc, meshpri);
                    mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshpri);
                }
                else
                {
                    mesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, init.MeshName);
                }

                await init.UpdateMtlInst();
            }
            else
            {
                var meshpri = CEngine.Instance.MeshPrimitivesManager.NewMeshPrimitives(rc, EngineNS.Graphics.Mesh.CGfxMeshCooker.CookBoxName, 1);
                Graphics.Mesh.CGfxMeshCooker.MakeBox(rc, meshpri, -0.5f, -0.5f, -0.5f, 1, 1, 1);
                mesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshpri);
            }

            if (mesh != null)
            {
                SetSceneMesh(rc.ImmCommandList, mesh);

                if (init.MtlList != null)
                {
                    for (int i = 0; i < init.MtlList.Count; i++)
                    {
                        var mtl = init.MtlList[i];
                        if (mtl == null)
                            continue;
                        mtl.MeshComp = this;
                        if (mtl.Name == null)
                            continue;
                        var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, mtl.Name);
                        if (mtlInst == null)
                            continue;

                        //await this.SetMaterial(rc, (UInt32)i, mtlInst, null);
                        this.SetMaterialInstance(rc, (UInt32)i, mtlInst, null);
                    }
                }
            }
            else
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"MeshComponent Init failed: {init.MeshName}");
            }
            if (init.SocketName != null)
            {
                SocketName = init.SocketName;
            }
            if (init.PlacementComponentInitializer != null)
            {
                await Placement.SetInitializer(rc, host, this, init.PlacementComponentInitializer);
            }
            return true;
        }
        [Editor.Editor_PackData()]
        [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]
        public RName MeshName
        {
            get
            {
                var init = this.Initializer as GMeshComponentInitializer;
                if (init == null)
                    return RName.GetRName("");
                return init.MeshName;
            }
            set
            {
                if (SceneMesh.Name == value)
                    return;
                var init = this.Initializer as GMeshComponentInitializer;
                init.MeshName = value;
            }
        }
        public int MaterialNumber
        {
            get
            {
                if (mSceneMesh == null)
                    return 0;
                return mSceneMesh.MtlMeshArray.Length;
            }
        }
        protected Graphics.Mesh.CGfxMesh mSceneMesh;

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Graphics.Mesh.CGfxMesh SceneMesh
        {
            get
            {
                return mSceneMesh;
            }
        }


        ////public CPass[] mPassArray;
        //protected Graphics.Mesh.CGfxMtlMesh[] mRLAtomArray;
        //[Browsable(false)]
        //public Graphics.Mesh.CGfxMtlMesh[] RLAtomArray
        //{
        //    get { return mRLAtomArray; }
        //}
        [Editor.Editor_ExpandedInPropertyGridAttribute(true)]
        public List<GMeshComponentInitializer.MtlInst> MtlList
        {
            get
            {
                var init = Initializer as GMeshComponentInitializer;
                if (init == null)
                    return null;
                return init.MtlList;
            }
        }
        public async System.Threading.Tasks.Task<bool> SetMaterialInstance(CRenderContext RHICtx, UInt32 index, RName materialName, CGfxShadingEnv[] envs)
        {
            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(RHICtx, materialName);
            if (mtl == null)
                return false;
            var ret = SetMaterialInstance(RHICtx, index, mtl, envs);
            await SceneMesh.AwaitEffects();
            return ret;
        }
        public async System.Threading.Tasks.Task<bool> SetMaterialInstanceAsync(CRenderContext RHICtx, UInt32 index,
            Graphics.CGfxMaterialInstance material, CGfxShadingEnv[] envs, bool preUseEffect = false)
        {
            var ret = SetMaterialInstance(RHICtx, index, material, envs, preUseEffect);
            await SceneMesh.AwaitEffects();
            return ret;
        }
        public bool SetMaterialInstance(CRenderContext RHICtx, UInt32 index, Graphics.CGfxMaterialInstance material, CGfxShadingEnv[] envs, bool preUseEffect = false)
        {
            if (index >= mSceneMesh.MtlMeshArray.Length)
                return false;
            SceneMesh.SetMaterialInstance(RHICtx, index, material, envs, preUseEffect);
            return true;
        }
        #region ISocketable
        string mSocketName = "";
        [Editor.Editor_SocketSelect]
        public string SocketName
        {
            get => mSocketName;
            set
            {
                if (value == "None" || string.IsNullOrEmpty(value))
                {
                    mSocketName = "";
                    if (ParentSocket != null)
                    {
                        ParentSocket = null;
                        ((Bricks.Animation.Skeleton.GSocketPlacement)Placement).SocketCharacterSpaceMatrix = Matrix.Identity;
                        UpdateSocketMatrix(Matrix.Identity);
                    }
                }
                else if (mHostContainer is GMeshComponent)
                {
                    mSocketName = value;
                    var hostContainer = mHostContainer as GMeshComponent;
                    var skin = hostContainer?.SceneMesh?.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
                    if (skin == null || skin.MeshSpaceAnimPose == null)
                        return;
                    var bone = skin.MeshSpaceAnimPose.FindBonePose(value);
                    if (bone == null)
                        return;
                    ParentSocket = bone;
                    UpdateSocketMatrix(ParentSocket.MeshSpaceMatrix);
                }
                var init = this.Initializer as GMeshComponentInitializer;
                if (init != null)
                    init.SocketName = value;
            }
        }

        private void UpdateSocketMatrix(Matrix matrix)
        {
            Placement.DrawTransform = Placement.Transform * matrix;
            OnPlacementChanged(Placement);
        }

        [Browsable(false)]
        public Bricks.Animation.Skeleton.IScocket ParentSocket { get; set; } = null;

        void KeepParentSocketValid()
        {
            if (ParentSocket == null)
            {
                //var hostContainer = mHostContainer as GMeshComponent;
                //var skin = hostContainer.SceneMesh.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
                //if (skin == null || skin.MeshSpaceAnimPose == null)
                //    return;
                //var socket = ParentSocket as Bricks.Animation.Pose.CGfxBonePose;
                //var bone = skin.MeshSpaceAnimPose.FindBonePose(socket.ReferenceBone.BoneDesc.NameHash);
                //if (bone != socket)
                //{
                //    ParentSocket = bone;
                //}
                if (mSocketName != "" && mSocketName != "null")
                {
                    var hostContainer = mHostContainer as GMeshComponent;
                    if (hostContainer == null)
                        return;
                    var skin = hostContainer.SceneMesh.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
                    if (skin == null || skin.MeshSpaceAnimPose == null)
                        return;
                    var bone = skin.MeshSpaceAnimPose.FindBonePose(mSocketName);
                    if (bone == null)
                        return;
                    ParentSocket = bone;
                    UpdateSocketMatrix(ParentSocket.MeshSpaceMatrix);
                }
            }

        }

        #endregion ISocketable
        #region Placement
        GPlacementComponent mPlacement = new Bricks.Animation.Skeleton.GSocketPlacement();
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public GPlacementComponent Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
                var init = this.Initializer as GMeshComponentInitializer;
                if (init != null)
                    init.PlacementComponentInitializer = value.Initializer as GPlacementComponent.GPlacementComponentInitializer;
            }
        }
        //partial void PlacementChangedCallback();
        public void OnPlacementChanged(GPlacementComponent placement)
        {
            var init = this.Initializer as GMeshComponentInitializer;
            if (init != null)
            {
                if (init.PlacementComponentInitializer == null)
                {
                    init.PlacementComponentInitializer = Placement.Initializer as GPlacementComponent.GPlacementComponentInitializer;
                }
            }
            if (ParentSocket != null)
            {
                ((Bricks.Animation.Skeleton.GSocketPlacement)Placement).SocketCharacterSpaceMatrix = ParentSocket.MeshSpaceMatrix;
            }
            Placement.DrawTransform = Placement.WorldMatrix;
            OnDrawMatrixChanged();

            if (Components != null)
            {
                for (int i = 0; i < Components.Count; ++i)
                {
                    var comp = Components[i];
                    var cld = Components[i];
                    if (cld is IPlaceable)
                    {
                        if (((IPlaceable)cld).Placement != null)
                        {
                            ((IPlaceable)cld).OnPlacementChanged(placement);
                        }
                    }
                }
            }

            //PlacementChangedCallback();
        }
        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {

        }
        private void OnDrawMatrixChanged()
        {
            OnUpdateDrawMatrix(ref Placement.mDrawTransform);
        }
        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            if (SceneMesh == null)
                return;
            if (IsIdentityDrawTransform)
            {
                SceneMesh.mMeshVars.WorldMtx = Matrix.mIdentity;
            }
            else
            {
                SceneMesh.mMeshVars.WorldMtx = drawMatrix;
            }
            base.OnUpdateDrawMatrix(ref drawMatrix);
        }
        #endregion
        private void RefreshSceneMeshData(CRenderContext RHICtx)
        {
            //mRLAtomArray = new Graphics.Mesh.CGfxMtlMesh[SceneMesh.MtlMeshArray.Length];

            for (UInt32 i = 0; i < SceneMesh.MtlMeshArray.Length; i++)
            {
                var MtlMesh = SceneMesh.MtlMeshArray[i];
                if (MtlMesh == null || MtlMesh.MtlInst == null)
                    continue;

                //bool shaderVarSet = false;
                for (int MPI = 0; MPI < (int)PrebuildPassIndex.PPI_Num; MPI++)
                {
                    var refPass = MtlMesh.GetPass((PrebuildPassIndex)MPI);
                    if (refPass == null)
                    {
                        continue;
                    }
                    var effect = refPass.Effect;
                    if (effect == null)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Graphics", $"Effect load failed: Mtl={MtlMesh.MtlInst.Name}");
                        return;
                    }
                    if (effect.IsValid == false)
                    {
                        //材质模板存盘后，effect变化了，后面的设置PreUse会做
                        continue;
                    }

                    //refPass.BindCBuffer(effect.ShaderProgram, effect.CBID_Mesh, mSceneMesh.CBuffer);
                    MtlMesh.FillPassData(RHICtx, refPass, effect, refPass.PassIndex, true);

                    //if (shaderVarSet == false && effect.PerInstanceId != UInt32.MaxValue)
                    //{
                    //    MtlMesh.ReCreateCBuffer(RHICtx, effect, true);
                    //    MtlMesh.MeshVarVersion = UInt32.MaxValue;
                    //    shaderVarSet = true;
                    //}

                    //refPass.Effect = effect;
                    //MtlMesh.MtlInst.BindTextures(refPass.ShaderResources, effect.ShaderProgram);
                    //refPass.BindCBuffer(effect.ShaderProgram, effect.PerInstanceId, MtlMesh.CBuffer);
                }

                MtlMesh.MtlInstVersion = MtlMesh.MtlInst.Version;
            }
        }

        public void SetSceneMesh(CCommandList cmd, Graphics.Mesh.CGfxMesh value)
        {
            mSceneMesh = value;

            if (mSceneMesh != null)
            {
                RefreshSceneMeshData(CEngine.Instance.RenderContext);
            }

            if (value != null)
            {
                ((GMeshComponentInitializer)Initializer).MeshName = value.Name;
            }
        }
        System.Threading.Tasks.Task<bool> EditerModeTask = null;
        private void UpdateSceneMeshForEditerMode(CCommandList cmd)
        {
            if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
            {
                return;
            }

            if (EditerModeTask != null && EditerModeTask.IsCompleted == false)
            {
                return;
            }

            if (mSceneMesh.Editor_NeedUpdateOrigionMesh())
            {
                var rc = CEngine.Instance.RenderContext;
                EditerModeTask = mSceneMesh.UpdatedOrigionMesh(rc, MtlList);
            }
            else
            {
                for (UInt32 i = 0; i < SceneMesh.MtlMeshArray.Length; i++)
                {
                    var MtlMesh = mSceneMesh.MtlMeshArray[i];
                    if (MtlMesh == null)
                    {
                        continue;
                    }

                    if (MtlMesh.MtlInstVersion != MtlMesh.MtlInst.Version)
                    {
                        //MtlMesh.UpdateMaterialCBuffer(rc, true);
                        SetSceneMesh(cmd, mSceneMesh);
                        //MtlMesh.mNeedToRefreshCBOnly = true;

                        //MtlMesh.Init()
                        return;
                    }
                }
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IsIdentityDrawTransform
        {
            get;
            set;
        } = false;
        public static Profiler.TimeScope ScopeCommitMesh = Profiler.TimeScopeManager.GetTimeScope(typeof(GMeshComponent), nameof(GatherVisibleSceneMesh));
        //when the scene graph is ready,we need to rewrite this part;
        public void GatherVisibleSceneMesh(CCommandList cmd, Graphics.CGfxCamera ViewerCamera, SceneGraph.CheckVisibleParam SceneCullDesc)
        {
            using(new Profiler.TimeScopeHelper(ScopeCommitMesh))
            {
                if (mSceneMesh == null || Host == null)
                {
                    return;
                }

#if PWindow
                UpdateSceneMeshForEditerMode(cmd);
#endif

                mSceneMesh.UpdatePerSceneMeshVars(cmd, Host.HitProxyId, Host.PVSId, ViewerCamera);

                mSceneMesh.MeshPrimitives.PreUse(false);
                if (mSceneMesh.MeshPrimitives.ResourceState.StreamState != EStreamingState.SS_Valid)
                {
                    return;
                }

                mSceneMesh.HostActor = Host;
                if (SceneCullDesc.ForShadow)
                {
                    ViewerCamera.mVisibleSceneMeshArray_Shadow.Add(mSceneMesh);
                }
                else
                {
                    ViewerCamera.mVisibleSceneMeshArray.Add(mSceneMesh);
                }
            }
        }

        public override void CommitVisual(CCommandList cmd, Graphics.CGfxCamera Camera, SceneGraph.CheckVisibleParam SceneCullDesc)
        {
            if (!Visible)
                return;
            if (this.HideInGame && CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Editor)
                return;
            GatherVisibleSceneMesh(cmd, Camera, SceneCullDesc);
            base.CommitVisual(cmd, Camera, SceneCullDesc);
        }
        public override void OnAdded()
        {
            if (Host == null)
                return;

            if (mSceneMesh != null)
            {
                var aabb = mSceneMesh.MeshPrimitives.AABB;
                BoundingBox.Merge(ref Host.LocalBoundingBox, ref aabb, out Host.LocalBoundingBox);
                OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            }
            base.OnAdded();
        }
        public override Profiler.TimeScope GetTickTimeScope()
        {
            return ScopeTick;
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GMeshComponent), nameof(Tick));
        public override void Tick(GPlacementComponent placement)
        {
            KeepParentSocketValid();
            SceneMesh?.MdfQueue.TickLogic(CEngine.Instance.RenderContext, SceneMesh, CEngine.Instance.EngineElapseTime);
            if (ParentSocket != null)
            {
                UpdateSocketMatrix(ParentSocket.MeshSpaceMatrix);
            }
            base.Tick(placement);
        }

        public bool LineCheck(ref Vector3 start, ref Vector3 end, ref SceneGraph.VHitResult rst)
        {
            return false;
        }

        public void OnHostSelected(bool isSelect)
        {
            if (SceneMesh != null)
                SceneMesh.mSelected = isSelect;
        }

        public override void PreUse(bool force)
        {
            SceneMesh.PreUse(force);
        }
        public override async Task<bool> LoadXnd(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer, XndNode node)
        {
            var result = await base.LoadXnd(rc, host, hostContainer, node);
            OnPlacementChanged(host.Placement);
            return result;
        }
    }
}
