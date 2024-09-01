using EngineNS.EGui.Slate;
using EngineNS.GamePlay.Scene;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class TtWorld : IDisposable
    {
        public void Dispose()
        {
            //Root.ClearChildren();
            Root.DisposeWithChildren();
            mBoundingDebugMaterial = null;

            mMemberTickables.CleanupMembers(this);
        }
        public TtWorld(Graphics.Pipeline.TtViewportSlate viewport)
        {
            mMemberTickables.CollectMembers(this);

            mOnVisitNode_GatherBoundShapes = this.OnVisitNode_GatherBoundShapes;

            mRoot = new Scene.TtScene();
            mRoot.World = this;
        }
        UMemberTickables mMemberTickables = new UMemberTickables();
        Graphics.Pipeline.Shader.TtMaterialInstance mBoundingDebugMaterial;
        public async System.Threading.Tasks.Task<bool> InitWorld()
        {
            Scene.TtNodeData data = new Scene.TtNodeData();
            await mRoot.InitializeNode(this, data, Scene.EBoundVolumeType.Box, typeof(TtPlacement));
            mRoot.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);

            mBoundingDebugMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));

            await mMemberTickables.InitializeMembers(this);
            return true;
        }
        public List<TtNode> ActiveNodes { get; } = new List<TtNode>();
        public void RegActiveNode(TtNode node)
        {
            lock (ActiveNodes)
            {
                ActiveNodes.Add(node);
            }
        }
        public void ResetActiveNodes()
        {
            lock (ActiveNodes)
            {
                ActiveNodes.Clear();
            }
        }
        internal DVector3 mCameraOffset = DVector3.Zero;
        internal uint CameralOffsetSerialId = 1;
        public DVector3 CameraOffset 
        {
            get => mCameraOffset;
            set
            {
                mCameraOffset = value;
                CameralOffsetSerialId++;
            }
        }
        Scene.TtScene mRoot;
        public Scene.TtScene Root
        {
            get => mRoot;
            set => mRoot = value;
        }
        internal List<Scene.TtSunNode> mSuns = new List<Scene.TtSunNode>();
        public Scene.TtSunNode GetSun(int index = 0)
        {
            if (index < 0 || index >= mSuns.Count)
                return null;
            return mSuns[index];
        }
        TtDirectionLight mDirectionLight;
        public TtDirectionLight DirectionLight 
        {
            get
            {
                var result = GetSun();
                if (result != null)
                {
                    return result.DirectionLight;
                }
                else
                {
                    if (mDirectionLight == null)
                    {
                        mDirectionLight = new TtDirectionLight();
                    }
                    return mDirectionLight;
                }
            } 
        }
        #region Culling
        public class UVisParameter
        {
            public enum EVisCull
            {
                Normal,
                Shadow,
            }
            [Flags]
            public enum EVisCullFilter : uint
            {
                GameObject = 1,
                LightDebug = (1 << 1),
                PhyxDebug = (1 << 2),
                UtilityDebug = (1 << 3),
                FilterTypeCount = 4,

                EditorObject = LightDebug | PhyxDebug | UtilityDebug,
                All = 0xFFFFFFFF,
                None = 0,
            }
            public const string FilterTypeCountAs = "PhyxDebug";
            public Graphics.Pipeline.UCamera CullCamera;

            public EVisCull CullType = EVisCull.Normal;
            public EVisCullFilter CullFilters = EVisCullFilter.None;// EVisCullFilter.All;
            public bool IsBuildAABB = false;
            public TtWorld World;

            public DBoundingBox AABB;
            public List<Graphics.Pipeline.FVisibleMesh> VisibleMeshes = new List<Graphics.Pipeline.FVisibleMesh>();
            public List<GamePlay.Scene.TtNode> VisibleNodes { get; } = new List<TtNode>();
            public bool IsGatherVisibleNodes { get; set; } = false;
            
            public delegate bool FOnVisitNode(Scene.TtNode node, UVisParameter arg);
            public FOnVisitNode OnVisitNode = null;
            public void Reset()
            {
                AABB.InitEmptyBox();
                VisibleMeshes?.Clear();
                VisibleNodes?.Clear();
            }
            public void MergeAABB(in DBoundingBox aabb)
            {
                if (IsBuildAABB)
                {
                    AABB = DBoundingBox.Merge(in AABB, in aabb);
                }
            }
            public void AddVisibleMesh(Graphics.Mesh.TtMesh mesh, bool bAABB = true)
            {
                if (CullType == EVisCull.Shadow && mesh.IsCastShadow == false)
                {
                    return;
                }
                if (IsBuildAABB && bAABB)
                {
                    AABB = DBoundingBox.Merge(in AABB, mesh.WorldAABB);
                }
                lock (VisibleMeshes)
                {
                    VisibleMeshes.Add(new Graphics.Pipeline.FVisibleMesh() { Mesh = mesh });
                }
            }
            public void AddVisibleNode(TtNode node)
            {
                if (IsGatherVisibleNodes == false && node.IsForceGatherNode == false)
                    return;
                lock (VisibleNodes)
                {
                    VisibleNodes.Add(node);
                }
            }
            public NxRHI.TtTransientBuffer TransientVB = null;
            public NxRHI.TtTransientBuffer TransientIB = null;
            public void AddAABB(in Aabb aabb, in Color4f color, in FTransform transform)
            {
                BoundingBox box = new BoundingBox(-aabb.Extent, aabb.Extent);
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeBox(in box, color.ToArgb());
                meshProvider.TransientVB = TransientVB;
                meshProvider.TransientIB = TransientIB;
                var mesh = meshProvider.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                var localTrans = FTransform.CreateTransform(aabb.Minimum, in Vector3.One, in Quaternion.Identity);
                FTransform trans;
                FTransform.Multiply(out trans, in transform, in localTrans);
                mesh.SetWorldTransform(in trans, this.World, true);
                AddVisibleMesh(mesh);
            }
            public void AddBoundingBox(in BoundingBox box, in Color4f color)
            {
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeBox(in box, color.ToArgb());
                var mesh = meshProvider.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                mesh.SetWorldTransform(in FTransform.Identity, this.World, true);
                AddVisibleMesh(mesh);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeGatherVisibleMeshes;
        private static Profiler.TimeScope ScopeGatherVisibleMeshes
        {
            get
            {
                if (mScopeGatherVisibleMeshes == null)
                    mScopeGatherVisibleMeshes = new Profiler.TimeScope(typeof(TtWorld), nameof(GatherVisibleMeshes));
                return mScopeGatherVisibleMeshes;
            }
        }
        public virtual void GatherVisibleMeshes(UVisParameter rp)
        {
            rp.CullCamera.VisParameter = rp;
            using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes))
            {
                rp.Reset();

                OnVisitNode_GatherVisibleMeshes(Root, rp);
            }   
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeGatherVisibleMeshes_Cull;
        private static Profiler.TimeScope ScopeGatherVisibleMeshes_Cull
        {
            get
            {
                if (mScopeGatherVisibleMeshes_Cull == null)
                    mScopeGatherVisibleMeshes_Cull = new Profiler.TimeScope(typeof(TtWorld), nameof(GatherVisibleMeshes) + ".Cull");
                return mScopeGatherVisibleMeshes_Cull;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeOnVisitNode;
        private static Profiler.TimeScope ScopeOnVisitNode
        {
            get
            {
                if (mScopeOnVisitNode == null)
                    mScopeOnVisitNode = new Profiler.TimeScope(typeof(TtWorld), nameof(GatherVisibleMeshes) + ".OnVisitNode");
                return mScopeOnVisitNode;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeOnGatherVisibleMeshes;
        private static Profiler.TimeScope ScopeOnGatherVisibleMeshes
        {
            get
            {
                if (mScopeOnGatherVisibleMeshes == null)
                    mScopeOnGatherVisibleMeshes = new Profiler.TimeScope(typeof(TtNode), "OnGatherVisibleMeshes");
                return mScopeOnGatherVisibleMeshes;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeChildren;
        private static Profiler.TimeScope ScopeChildren
        {
            get
            {
                if (mScopeChildren == null)
                    mScopeChildren = new Profiler.TimeScope(typeof(TtWorld), nameof(GatherVisibleMeshes) + ".Children");
                return mScopeChildren;
            }
        }
        internal unsafe static bool OnVisitNode_GatherVisibleMeshes(Scene.TtNode node, UVisParameter rp)
        {
            if (rp.OnVisitNode != null)
            {
                using (new Profiler.TimeScopeHelper(ScopeOnVisitNode))
                {
                    if (rp.OnVisitNode(node, rp) == false)
                        return false;
                }   
            }
            
            CONTAIN_TYPE type;

            using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes_Cull))
            {
                if (node.HasStyle(Scene.TtNode.ENodeStyles.VisibleFollowParent))
                {
                    type = CONTAIN_TYPE.CONTAIN_TEST_REFER;
                }
                else
                {
                    type = rp.CullCamera.WhichContainTypeFast(rp.World, in node.AbsAABB, true);

                    //var absAABB = DBoundingBox.TransformNoScale(in node.AABB, in node.Placement.AbsTransform);
                    //type = rp.CullCamera.WhichContainTypeFast(this, in absAABB, true);
                    
                    //这里还没想明白，把Frustum的6个平面变换到AABB所在坐标为啥不行
                    //type = frustom->whichContainTypeFast(ref node.AABB, ref node.Placement.AbsTransformInv, 1);
                }
            }

            switch (type)
            {
                case CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_INNER:
                    {
                        if (node.TreeGatherVisibleMeshes(rp))
                        {
                            node.DFS_VisitNodeTree(mOnVisitNode_GatherVisibleMeshesAll, rp);
                        }
                    }
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_REFER:
                    {
                        if (node.TreeGatherVisibleMeshes(rp))
                        {
                            if (!node.HasStyle(Scene.TtNode.ENodeStyles.SelfInvisible))
                            {
                                using (new Profiler.TimeScopeHelper(ScopeOnGatherVisibleMeshes))
                                {
                                    node.OnGatherVisibleMeshes(rp);
                                }
                            }
                            if (!node.HasStyle(Scene.TtNode.ENodeStyles.ChildrenInvisible))
                            {
                                using (new Profiler.TimeScopeHelper(ScopeChildren))
                                {
                                    if (TtEngine.Instance.Config.IsParrallelWorldGather)
                                    {
                                        var numTask = TtEngine.Instance.EventPoster.NumOfPool;
                                        numTask = Math.Min(node.Children.Count, numTask);
                                        TtEngine.Instance.EventPoster.ParrallelFor(numTask, static (int index, object arg1, object arg2, Thread.Async.TtAsyncTaskStateBase state) =>
                                        {
                                            var node = arg1 as TtNode;
                                            var rp = arg2 as UVisParameter;
                                            int stride = node.Children.Count / (int)state.UserArguments.NumOfParrallelFor + 1;
                                            var start = index * stride;
                                            for (int n = 0; n < stride; n++)
                                            {
                                                var nn = start + n;
                                                if (nn >= node.Children.Count)
                                                    break;
                                                OnVisitNode_GatherVisibleMeshes(node.Children[nn], rp);
                                            }
                                        }, node, rp);
                                    }
                                    else
                                    {
                                        foreach (var i in node.Children)
                                        {
                                            OnVisitNode_GatherVisibleMeshes(i, rp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            return false;
        }
        static Scene.TtNode.FOnVisitNode mOnVisitNode_GatherVisibleMeshesAll = OnVisitNode_GatherVisibleMeshesAll;
        private unsafe static bool OnVisitNode_GatherVisibleMeshesAll(Scene.TtNode node, object arg)
        {
            var rp = arg as UVisParameter;

            using (new Profiler.TimeScopeHelper(ScopeOnGatherVisibleMeshes))
            {
                node.OnGatherVisibleMeshes(rp);
            }
                
            return false;
        }
        #endregion

        #region DebugAssist
        
        public void GatherBoundShapes(List<Graphics.Pipeline.FVisibleMesh> boundVolumes, Scene.TtNode node = null)
        {
            if (node == null)
                node = Root;
            node.DFS_VisitNodeTree(mOnVisitNode_GatherBoundShapes, boundVolumes);
        }
        Scene.TtNode.FOnVisitNode mOnVisitNode_GatherBoundShapes;
        private unsafe bool OnVisitNode_GatherBoundShapes(Scene.TtNode node, object arg)
        {
            if (node.HasStyle(Scene.TtNode.ENodeStyles.HideBoundShape))
                return false;

            var bvs = arg as List<Graphics.Pipeline.FVisibleMesh>;

            ref var aabb = ref node.AABB;
            var size = aabb.GetSize();
            var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe((float)aabb.Minimum.X, (float)aabb.Minimum.Y, (float)aabb.Minimum.Z,
                (float)size.X, (float)size.Y, (float)size.Z).ToMesh();
            var mesh2 = new Graphics.Mesh.TtMesh();

            var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
            materials1[0] = mBoundingDebugMaterial;// TtEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
            if (materials1[0] == null)
            {
                //System.Diagnostics.Debug.Assert(false);
                return false;
            }
            mesh2.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            mesh2.SetWorldTransform(in node.Placement.AbsTransform, this, true);
            mesh2.IsAcceptShadow = false;
            mesh2.IsUnlit = true;

            bvs.Add(new Graphics.Pipeline.FVisibleMesh() { Mesh = mesh2 });

            return false;
        }
        #endregion

        #region GamePlay
        private int mTimeMillisecond = 0;
        public int TimeMillisecond { get=> mTimeMillisecond; } 
        public float TimeSecond { get => ((float)mTimeMillisecond) * 0.001f; }

        private int mDeltaTimeMillisecond = 0;
        public int DeltaTimeMillisecond { get => mDeltaTimeMillisecond; }
        public float DeltaTimeSecond { get => ((float)mDeltaTimeMillisecond) * 0.001f; }

        private int mRealtimeMillisecondSinceStartup = 0;
        public int RealtimeMillisecondSinceStartup { get=> mRealtimeMillisecondSinceStartup; }
        public float RealtimeSecondSinceStartup { get => ((float)mRealtimeMillisecondSinceStartup) * 0.001f; }

        public bool Pause { get; set; } = false;
        public float TimeScale { get; set; } = 1.0f;

        public void TickTime(float ellapseMillisecond)
        {
            int scaledTime = (int)Math.Truncate(ellapseMillisecond * TimeScale);
            mRealtimeMillisecondSinceStartup += scaledTime;
            if (!Pause)
            {
                mTimeMillisecond += scaledTime;
                mDeltaTimeMillisecond = scaledTime;
            }
            else
            {
                mDeltaTimeMillisecond = 0;
            }
        }
        public void ResetTime()
        {
            mTimeMillisecond = 0;
            mRealtimeMillisecondSinceStartup = 0;
            mDeltaTimeMillisecond = 0;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtWorld), nameof(TickLogic));
                return mScopeTick;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick_After;
        private static Profiler.TimeScope ScopeTick_After
        {
            get
            {
                if (mScopeTick_After == null)
                    mScopeTick_After = new Profiler.TimeScope(typeof(TtWorld), nameof(TickLogic) + ".After");
                return mScopeTick_After;
            }
        } 
        private TtNode.TtNodeTickParameters NodeTickParameters = new TtNode.TtNodeTickParameters();
        public virtual void TickLogic(Graphics.Pipeline.TtRenderPolicy policy, float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                TickTime(ellapse);

                if (Pause)
                    return;

                NodeTickParameters.World = this;
                NodeTickParameters.Policy = policy;
                NodeTickParameters.IsTickChildren = true;
                Root.TickLogic(NodeTickParameters);

                //NodeTickParameters.IsTickChildren = false;
                //TtEngine.Instance.EventPoster.ParrallelFor(ActiveNodes.Count, static (Index, obj1, obj2) =>
                //{
                //    var pThis = (UWorld)obj1;
                //    pThis.ActiveNodes[Index].TickLogic(pThis.NodeTickParameters);
                //}, this);
                //foreach (var i in ActiveNodes)
                //{
                //    i.TickLogic(NodeTickParameters);
                //}

                mMemberTickables.TickLogic(this, TtEngine.Instance.ElapseTickCountMS);

                using (new Profiler.TimeScopeHelper(ScopeTick_After))
                {
                    foreach (var i in mAfterTicks)
                    {
                        i();
                    }
                    mAfterTicks.Clear();
                }
            }
        }
        private List<System.Action> mAfterTicks = new List<System.Action>();
        public void RegAfterTickAction(System.Action action)
        {
            mAfterTicks.Add(action);
        }
        #endregion
    }
}
