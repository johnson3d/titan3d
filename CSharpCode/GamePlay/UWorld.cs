using EngineNS.EGui.Slate;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtWorld : IDisposable
    {
        static int mNodeAliveNumber = 0;
        public static int NodeAliveNumber
        {
            get => mNodeAliveNumber;
        }
        public void Dispose()
        {
            //Root.ClearChildren();
            Root?.DisposeWithChildren();
            mBoundingDebugMaterial = null;

            mMemberTickables.CleanupMembers(this);
        }
        public TtWorld(Graphics.Pipeline.TtViewportSlate viewport)
        {
            mMemberTickables.CollectMembers(this);

            mOnVisitNode_GatherBoundShapes = this.OnVisitNode_GatherBoundShapes;

            //mRoot = new Scene.TtScene();
            //mRoot.SetWorld(this);
            System.Threading.Interlocked.Increment(ref mNodeAliveNumber);
        }
        ~TtWorld()
        {
            Dispose();
            System.Threading.Interlocked.Decrement(ref mNodeAliveNumber);
        }
        public bool IsGameWorld { get; set; } = false;
        TtMemberTickables mMemberTickables = new TtMemberTickables();
        Graphics.Pipeline.Shader.TtMaterialInstance mBoundingDebugMaterial;
        public async System.Threading.Tasks.Task<bool> InitWorld()
        {
            Scene.TtNodeData data = new Scene.TtNodeData();
            mRoot = await TtNode.SpawnNode<Scene.TtScene>(null, null, data, Scene.EBoundVolumeType.Box, typeof(TtPlacement), this);
            mRoot.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);

            mBoundingDebugMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));

            await mMemberTickables.InitializeMembers(this);
            return true;
        }
        internal DVector3 mCameraOffset = DVector3.Zero;
        internal uint CameralOffsetSerialId = 1;
        [Category("Option")]
        [ReadOnly(true)]
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
        [Rtti.Meta("")]
        public Scene.TtScene Root
        {
            get => mRoot;
            set => mRoot = value;
        }
        internal List<Scene.TtSunNode> mSuns = new List<Scene.TtSunNode>();
        [Rtti.Meta("")]
        public Scene.TtSunNode GetSun(int index = 0)
        {
            if (index < 0 || index >= mSuns.Count)
                return null;
            return mSuns[index];
        }
        TtDirectionLight mDirectionLight;
        [Category("Option")]
        [Rtti.Meta("")]
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
        public class TtVisParameter
        {
            public TtVisParameter()
            {

            }
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
                NavMesh = (1 << 4),
                FilterTypeCount = 5,

                EditorObject = LightDebug | PhyxDebug | UtilityDebug | NavMesh,
                All = 0xFFFFFFFF,
                None = 0,
            }
            public const string FilterTypeCountAs = "PhyxDebug";
            public Graphics.Pipeline.TtCamera CullCamera = null;

            public EVisCull CullType = EVisCull.Normal;
            public EVisCullFilter CullFilters = EVisCullFilter.None;// EVisCullFilter.All;
            public bool IsBuildAABB = false;
            public DBoundingBox CullBox;
            public TtWorld World;

            public DBoundingBox AABB;
            public List<Graphics.Pipeline.FVisibleMesh> VisibleMeshes = new List<Graphics.Pipeline.FVisibleMesh>();
            public List<GamePlay.Scene.TtNode> VisibleNodes { get; } = new List<TtNode>();
            public bool IsGatherVisibleNodes { get; set; } = false;
            
            public delegate bool FOnVisitNode(Scene.TtNode node, TtVisParameter arg);
            public FOnVisitNode OnVisitNode = null;
            public FOnVisitNode IsGatherVisibleMeshes = null;
            public void ClearVisibles()
            {
                AABB.InitEmptyBox();
                VisibleMeshes?.Clear();
                VisibleNodes?.Clear();
            }
            public void Reset()
            {
                ClearVisibles();
                World = null;
                OnVisitNode = null;
                IsGatherVisibleMeshes = null;
                TransientVB = null;
                TransientIB = null;
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
                var meshProvider = Graphics.Mesh.TtMeshDataProvider.MakeBox(in box, color.ToArgb());
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
                var meshProvider = Graphics.Mesh.TtMeshDataProvider.MakeBox(in box, color.ToArgb());
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
        public virtual void GatherVisibleMeshes(TtVisParameter rp)
        {
            using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes))
            {
                rp.ClearVisibles();

                GatherVisibleMeshes(rp, Root);
            }
        }
        public virtual void GatherVisibleMeshes(TtVisParameter rp, TtNode rootNode)
        {
            rootNode.IterateNodesBFS(static (node, arg) =>
            {
                var rp = arg as TtVisParameter;
                if (rp.OnVisitNode != null)
                {
                    using (new Profiler.TimeScopeHelper(ScopeOnVisitNode))
                    {
                        if (rp.OnVisitNode(node, rp) == false)
                            return false;
                    }
                }
                CONTAIN_TYPE type = CONTAIN_TYPE.CONTAIN_TEST_OUTER;
                if (node.HasStyle(Scene.TtNode.ENodeStyles.VisibleFollowParent))
                {
                    type = CONTAIN_TYPE.CONTAIN_TEST_REFER;
                }
                else if (rp.CullCamera != null)
                {
                    using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes_Cull))
                    {
                        type = rp.CullCamera.WhichContainTypeFast(rp.World, in node.AbsAABB, true);
                    }
                }
                else
                {
                    var ct = DBoundingBox.Contains(in rp.CullBox, in node.AbsAABB);
                    switch (ct)
                    {
                        case ContainmentType.Contains:
                            type = CONTAIN_TYPE.CONTAIN_TEST_INNER;
                            break;
                        case ContainmentType.Intersects:
                            type = CONTAIN_TYPE.CONTAIN_TEST_REFER;
                            break;
                        case ContainmentType.Disjoint:
                            type = CONTAIN_TYPE.CONTAIN_TEST_OUTER;
                            break;
                        default:
                            type = CONTAIN_TYPE.CONTAIN_TEST_REFER;
                            break;
                    }
                }
                if (type == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                {
                    return false;
                }
                //else if (type == CONTAIN_TYPE.CONTAIN_TEST_INNER)
                //{
                //    node.IterateNodes(static (node, arg) =>
                //    {
                //        var rp = arg as TtVisParameter;
                //        using (new Profiler.TimeScopeHelper(ScopeOnGatherVisibleMeshes))
                //        {
                //            node.OnGatherVisibleMeshes(rp);
                //        }
                //        return true;
                //    }, rp);
                //    return false;
                //}
                else if (node.HasStyle(TtNode.ENodeStyles.Invisible) == false)
                {
                    using (new Profiler.TimeScopeHelper(ScopeOnGatherVisibleMeshes))
                    {
                        if (rp.IsGatherVisibleMeshes == null || rp.IsGatherVisibleMeshes(node, rp))
                            node.OnGatherVisibleMeshes(rp);
                    }
                }
                return true;
            }, rp, 8);
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
            var cookedMesh = Graphics.Mesh.TtMeshDataProvider.MakeBoxWireframe((float)aabb.Minimum.X, (float)aabb.Minimum.Y, (float)aabb.Minimum.Z,
                (float)size.X, (float)size.Y, (float)size.Z).ToMesh();
            var mesh2 = new Graphics.Mesh.TtMesh();

            var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
            materials1[0] = mBoundingDebugMaterial;// TtEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
            if (materials1[0] == null)
            {
                //System.Diagnostics.Debug.Assert(false);
                return false;
            }
            mesh2.Initialize(cookedMesh, materials1, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
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
        private static Profiler.TimeScope mScopeTick_Iterate;
        private static Profiler.TimeScope ScopeTick_Iterate
        {
            get
            {
                if (mScopeTick_Iterate == null)
                    mScopeTick_Iterate = new Profiler.TimeScope(typeof(TtWorld), nameof(TickLogic) + ".Iterate");
                return mScopeTick_Iterate;
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
        private List<TtNode> TickNodes = new List<TtNode>();
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
                //Root.TickLogic(NodeTickParameters);

                TickNodes.Clear();
                using (new Profiler.TimeScopeHelper(ScopeTick_Iterate))
                {
                    Root.IterateNodes(static (nd, arg) =>
                    {
                        var tp = (List<TtNode>)arg;
                        tp.Add(nd);
                        return true;
                    }, TickNodes);
                }

                foreach (var i in TickNodes)
                {
                    if (i.IsNoTick)
                        continue;

                    using (new Profiler.TimeScopeHelper(i.GetScopeTickLogic()))
                    {
                        i.OnTickLogic(NodeTickParameters);
                    }
                }
                TickNodes.Clear();

                using (new Profiler.TimeScopeHelper(ScopeTick_After))
                {
                    mMemberTickables.TickLogic(this, TtEngine.Instance.ElapseTickCountMS);

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
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay
{
	partial class TtWorld
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetSun_1880456938 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtWorld->Scene.TtSunNode GetSun(int index)");
		public unsafe Scene.TtSunNode macross_GetSun (string nodeName, int index) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":index", index);
				}
			}
			var _return_value = GetSun(index);
			macross_break_GetSun_1880456938.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross