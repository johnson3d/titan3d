using EngineNS.EGui.Slate;
using EngineNS.GamePlay.Scene;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public partial class UWorld : IDisposable
    {
        public void Dispose()
        {
            //Root.ClearChildren();
            Root.DisposeWithChildren();
            mBoundingDebugMaterial = null;

            mMemberTickables.CleanupMembers(this);
        }
        public UWorld(Graphics.Pipeline.TtViewportSlate viewport)
        {
            mMemberTickables.CollectMembers(this);

            mOnVisitNode_GatherVisibleMeshesAll = this.OnVisitNode_GatherVisibleMeshesAll;
            mOnVisitNode_GatherBoundShapes = this.OnVisitNode_GatherBoundShapes;

            mRoot = new Scene.UScene();
            mRoot.World = this;
        }
        UMemberTickables mMemberTickables = new UMemberTickables();
        Graphics.Pipeline.Shader.UMaterialInstance mBoundingDebugMaterial;
        public async System.Threading.Tasks.Task<bool> InitWorld()
        {
            Scene.UNodeData data = new Scene.UNodeData();
            await mRoot.InitializeNode(this, data, Scene.EBoundVolumeType.Box, typeof(UPlacement));
            mRoot.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);

            mBoundingDebugMaterial = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));

            await mMemberTickables.InitializeMembers(this);
            return true;
        }
        public List<UNode> ActiveNodes { get; } = new List<UNode>();
        public void RegActiveNode(UNode node)
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
        Scene.UScene mRoot;
        public Scene.UScene Root
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
            public UWorld World;

            public DBoundingBox AABB;
            public List<Graphics.Pipeline.FVisibleMesh> VisibleMeshes = new List<Graphics.Pipeline.FVisibleMesh>();
            public List<GamePlay.Scene.UNode> VisibleNodes = new List<UNode>();
            
            public delegate bool FOnVisitNode(Scene.UNode node, UVisParameter arg);
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
                VisibleMeshes.Add(new Graphics.Pipeline.FVisibleMesh() { Mesh = mesh });
            }
            public NxRHI.TtTransientBuffer TransientVB = null;
            public NxRHI.TtTransientBuffer TransientIB = null;
            public void AddAABB(in Aabb aabb, in Color4f color, in FTransform transform)
            {
                BoundingBox box = new BoundingBox(-aabb.Extent, aabb.Extent);
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeBox(in box, color.ToArgb());
                meshProvider.TransientVB = TransientVB;
                meshProvider.TransientIB = TransientIB;
                var mesh = meshProvider.ToDrawMesh(UEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                var localTrans = FTransform.CreateTransform(aabb.Minimum, in Vector3.One, in Quaternion.Identity);
                FTransform trans;
                FTransform.Multiply(out trans, in transform, in localTrans);
                mesh.SetWorldTransform(in trans, this.World, true);
                AddVisibleMesh(mesh);
            }
            public void AddBoundingBox(in BoundingBox box, in Color4f color)
            {
                var meshProvider = Graphics.Mesh.UMeshDataProvider.MakeBox(in box, color.ToArgb());
                var mesh = meshProvider.ToDrawMesh(UEngine.Instance.GfxDevice.MaterialInstanceManager.WireVtxColorMateria);
                mesh.SetWorldTransform(in FTransform.Identity, this.World, true);
                AddVisibleMesh(mesh);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeGatherVisibleMeshes = Profiler.TimeScopeManager.GetTimeScope(typeof(UWorld), nameof(GatherVisibleMeshes));
        public virtual void GatherVisibleMeshes(UVisParameter rp)
        {
            rp.CullCamera.VisParameter = rp;
            using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes))
            {
                rp.Reset();

                OnVisitNode_GatherVisibleMeshes(Root, rp);
            }   
        }
        private unsafe bool OnVisitNode_GatherVisibleMeshes(Scene.UNode node, object arg)
        {
            var rp = arg as UVisParameter;

            if (rp.OnVisitNode != null)
            {
                if (rp.OnVisitNode(node, rp) == false)
                    return false;
            }
            
            CONTAIN_TYPE type;
            if (node.HasStyle(Scene.UNode.ENodeStyles.VisibleFollowParent))
            {
                type = CONTAIN_TYPE.CONTAIN_TEST_REFER;
            }
            else
            {
                //var matrix = node.Placement.AbsTransform.ToMatrixNoScale();
                var absAABB = DBoundingBox.TransformNoScale(in node.AABB, in node.Placement.AbsTransform);
                type = rp.CullCamera.WhichContainTypeFast(this, in absAABB, true);
                //这里还没想明白，把Frustum的6个平面变换到AABB所在坐标为啥不行
                //type = frustom->whichContainTypeFast(ref node.AABB, ref node.Placement.AbsTransformInv, 1);
            }
            switch (type)
            {
                case CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_INNER:
                    node.DFS_VisitNodeTree(mOnVisitNode_GatherVisibleMeshesAll, rp);
                    break;
                case CONTAIN_TYPE.CONTAIN_TEST_REFER:
                    {
                        if(!node.HasStyle(Scene.UNode.ENodeStyles.SelfInvisible))
                            node.OnGatherVisibleMeshes(rp);
                        if (!node.HasStyle(Scene.UNode.ENodeStyles.ChildrenInvisible))
                        {
                            foreach (var i in node.Children)
                            {
                                OnVisitNode_GatherVisibleMeshes(i, arg);
                            }
                        }
                    }
                    break;
            }
            return false;
        }
        Scene.UNode.FOnVisitNode mOnVisitNode_GatherVisibleMeshesAll;
        private unsafe bool OnVisitNode_GatherVisibleMeshesAll(Scene.UNode node, object arg)
        {
            var rp = arg as UVisParameter;
            
            node.OnGatherVisibleMeshes(rp);
            return false;
        }
        #endregion

        #region DebugAssist
        
        public void GatherBoundShapes(List<Graphics.Pipeline.FVisibleMesh> boundVolumes, Scene.UNode node = null)
        {
            if (node == null)
                node = Root;
            node.DFS_VisitNodeTree(mOnVisitNode_GatherBoundShapes, boundVolumes);
        }
        Scene.UNode.FOnVisitNode mOnVisitNode_GatherBoundShapes;
        private unsafe bool OnVisitNode_GatherBoundShapes(Scene.UNode node, object arg)
        {
            if (node.HasStyle(Scene.UNode.ENodeStyles.HideBoundShape))
                return false;

            var bvs = arg as List<Graphics.Pipeline.FVisibleMesh>;

            ref var aabb = ref node.AABB;
            var size = aabb.GetSize();
            var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe((float)aabb.Minimum.X, (float)aabb.Minimum.Y, (float)aabb.Minimum.Z,
                (float)size.X, (float)size.Y, (float)size.Z).ToMesh();
            var mesh2 = new Graphics.Mesh.TtMesh();

            var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
            materials1[0] = mBoundingDebugMaterial;// UEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
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
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UWorld), nameof(TickLogic));
        private UNode.TtNodeTickParameters NodeTickParameters = new UNode.TtNodeTickParameters();
        public virtual void TickLogic(Graphics.Pipeline.URenderPolicy policy, float ellapse)
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
                //UEngine.Instance.EventPoster.ParrallelFor(ActiveNodes.Count, static (Index, obj1, obj2) =>
                //{
                //    var pThis = (UWorld)obj1;
                //    pThis.ActiveNodes[Index].TickLogic(pThis.NodeTickParameters);
                //}, this);
                //foreach (var i in ActiveNodes)
                //{
                //    i.TickLogic(NodeTickParameters);
                //}

                mMemberTickables.TickLogic(this, UEngine.Instance.ElapseTickCountMS);

                foreach (var i in mAfterTicks)
                {
                    i();
                }
                mAfterTicks.Clear();
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
