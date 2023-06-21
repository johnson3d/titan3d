using EngineNS.EGui.Slate;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    public class UWorldRootNode : Scene.UScene
    {
        public UWorldRootNode()
        {
            
        }
        public override string NodeName
        {
            get
            {
                return "WorldRootNode";
            }
        }        
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
            //base.OnGatherVisibleMeshes(rp);
        }
    }

    public partial class UWorld : IDisposable
    {
        public void Dispose()
        {
            //Root.ClearChildren();
            Root.DisposeWithChildren();
            mBoundingDebugMaterial = null;

            mMemberTickables.CleanupMembers(this);
        }
        public Graphics.Pipeline.UViewportSlate CurViewport { get; set; }
        public UWorld(Graphics.Pipeline.UViewportSlate viewport)
        {
            CurViewport = viewport;
            mMemberTickables.CollectMembers(this);

            mOnVisitNode_GatherVisibleMeshesAll = this.OnVisitNode_GatherVisibleMeshesAll;
            mOnVisitNode_GatherBoundShapes = this.OnVisitNode_GatherBoundShapes;

            mRoot = new UWorldRootNode();
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
        UWorldRootNode mRoot;
        public UWorldRootNode Root
        {
            get => mRoot;
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
            }
            public const string FilterTypeCountAs = "PhyxDebug";
            public EVisCull CullType = EVisCull.Normal;
            public EVisCullFilter CullFilters = EVisCullFilter.All;
            public UWorld World;
            public Graphics.Pipeline.UCamera CullCamera;
            public List<Graphics.Mesh.UMesh> VisibleMeshes = null;// new List<Graphics.Mesh.UMesh>();
            public List<GamePlay.Scene.UNode> VisibleNodes = null;
            public delegate bool FOnVisitNode(Scene.UNode node, UVisParameter arg);
            public FOnVisitNode OnVisitNode = null;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeGatherVisibleMeshes = Profiler.TimeScopeManager.GetTimeScope(typeof(UWorld), nameof(GatherVisibleMeshes));
        public virtual void GatherVisibleMeshes(UVisParameter rp)
        {
            rp.CullCamera.VisParameter = rp;
            using (new Profiler.TimeScopeHelper(ScopeGatherVisibleMeshes))
            {
                rp.VisibleMeshes.Clear();
                rp.VisibleNodes?.Clear();

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
        
        public void GatherBoundShapes(List<Graphics.Mesh.UMesh> boundVolumes, Scene.UNode node = null)
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

            var bvs = arg as List<Graphics.Mesh.UMesh>;

            ref var aabb = ref node.AABB;
            var size = aabb.GetSize();
            var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe((float)aabb.Minimum.X, (float)aabb.Minimum.Y, (float)aabb.Minimum.Z,
                (float)size.X, (float)size.Y, (float)size.Z).ToMesh();
            var mesh2 = new Graphics.Mesh.UMesh();

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

            bvs.Add(mesh2);

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
        public virtual void TickLogic(Graphics.Pipeline.URenderPolicy policy, float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                TickTime(ellapse);

                if (Pause)
                    return;
                Root.TickLogic(this, policy);

                mMemberTickables.TickLogic(this, UEngine.Instance.ElapseTickCount);
            }
        }
        #endregion
    }
}
