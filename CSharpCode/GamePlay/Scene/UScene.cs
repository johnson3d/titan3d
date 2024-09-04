using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneAMeta@EngineCore" })]
    public class TtSceneAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtScene.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Scene";
        }
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.SceneBoderColor;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            //return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return null;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "scene", null);
        //}
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneData@EngineCore" })]
    public class TtSceneData : TtNodeData
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
        public RName RPolicyName { get; set; }
    }
    [TtScene.SceneCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Scene")]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UScene@EngineCore" })]
    public partial class TtScene : TtNode, IO.IAsset
    {
        public const string AssetExt = ".scene";
        public override string ToString()
        {
            return this.AssetName.ToString();
        }
        public class SceneCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                //mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType, new USceneData()) as IO.IAsset;
                mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                var world = new TtWorld(null);
                await world.InitWorld();
                var task = (mAsset as TtScene).InitializeNode(world, new TtSceneData(), EBoundVolumeType.Box, typeof(TtPlacement));
                PGAsset.Target = mAsset;
            }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtSceneData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            World = world;
            //ParentScene = GetNearestParentScene();

            var task = mMemberTickables.InitializeMembers(this);
            return true;
        }
        public TtScene()
        {
            mMemberTickables.CollectMembers(this);
        }
        ~TtScene()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            ClearChildren();
            TtEngine.Instance?.SceneManager.UnloadScene(this.AssetName);
        }
        public TtSceneData SceneData
        {
            get
            {
                return NodeData as TtSceneData;
            }
        }
        [Category("Option")]
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
        public RName RPolicyName
        {
            get => SceneData?.RPolicyName;
            set
            {
                if (SceneData == null)
                    return;
                SceneData.RPolicyName = value;
            }
        }
        public TtWorld World;
        #region Allocator
        int PrevAllocId = 0;
        private TtNode[] ManagedNodes = new TtNode[UInt16.MaxValue];
        public TtNode[] GetManagedNodes()
        {
            return ManagedNodes;
        }
        public bool AllocId(TtNode node)
        {
            lock (ManagedNodes)
            {
                if (node is TtLightWeightNodeBase)
                    return false;
                for (int i = PrevAllocId; i < ManagedNodes.Length; i++)
                {
                    if (ManagedNodes[i] == null)
                    {
                        ManagedNodes[i] = node;
                        node.SceneId = (UInt32)i;
                        PrevAllocId = i;

                        var notify = new FHostNotify();
                        notify.Info = "OnSceneAllocId";
                        notify.Parameter = node;
                        mMemberTickables.SendNotify(this, in notify);
                        return true;
                    }
                }
                for (int i = 0; i < ManagedNodes.Length; i++)
                {
                    if (ManagedNodes[i] == null)
                    {
                        ManagedNodes[i] = node;
                        node.SceneId = (UInt32)i;
                        PrevAllocId = i;

                        var notify = new FHostNotify();
                        notify.Info = "OnSceneAllocId";
                        notify.Parameter = node;
                        mMemberTickables.SendNotify(this, in notify);
                        return true;
                    }
                }
                System.Diagnostics.Debug.Assert(false);
                return false;
            }
        }
        public void FreeId(TtNode node)
        {
            if (node is TtLightWeightNodeBase)
                return;
            lock (ManagedNodes)
            {
                if (node.SceneId >= UInt16.MaxValue)
                    return;

                var notify = new FHostNotify();
                notify.Info = "OnSceneFreeId";
                notify.Parameter = node;
                mMemberTickables.SendNotify(this, in notify);

                System.Diagnostics.Debug.Assert(ManagedNodes[node.SceneId] == node);
                ManagedNodes[node.SceneId] = null;
                node.SceneId = UInt32.MaxValue;
            }
        }
        public async Thread.Async.TtTask<TtSceneActorNode> NewNode(GamePlay.TtWorld world, string nodeType, TtNodeData data, EBoundVolumeType bvType, Type placementType, bool isSceneManaged = false)
        {
            var ntype = Rtti.UTypeDesc.TypeOf(nodeType);
            return await NewNode(world, ntype.SystemType, data, bvType, placementType, false);
        }
        public async Thread.Async.TtTask<TtSceneActorNode> NewNodeSimple(GamePlay.TtWorld world, Type nodeType, TtNodeData data, bool isSceneManaged = false)
        {
            return await NewNode(world, nodeType, data, EBoundVolumeType.Box, typeof(GamePlay.TtPlacement), isSceneManaged);
        }
        public async Thread.Async.TtTask<TtSceneActorNode> NewNode(GamePlay.TtWorld world, Type nodeType, TtNodeData data, EBoundVolumeType bvType, Type placementType, bool isSceneManaged = false)
        {
            var node = Rtti.UTypeDescManager.CreateInstance(nodeType) as TtSceneActorNode;
            if (node != null)
            {
                if (await node.InitializeNode(world, data, bvType, placementType) == false)
                    return null;
                node.IsSceneManaged = isSceneManaged;
            }
            return node;
        }
        #endregion

        #region IAsset
        public RName AssetName { get; set; }
        public const uint SceneDescAttributeFlags = 1;
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            var typeStr = Rtti.UTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.TtXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (SceneData != null)
            {
                using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(SceneData.GetType()), 1, SceneDescAttributeFlags))
                {
                    node.AddAttribute(dataAttr);
                    using (var ar = dataAttr.GetWriter((ulong)SceneData.GetStructSize() * 2))
                    {
                        ar.Write(SceneData);
                    }
                }
            }

            SaveChildNode(this, xnd.mCoreObject, node.mCoreObject);

            xndHolder.SaveXnd(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        internal static async System.Threading.Tasks.Task<TtScene> LoadScene(GamePlay.TtWorld world, RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(SceneDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return null;
                }

                TtSceneData nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(descAttr.Name)) as TtSceneData;

                //UScene don't have construct with params
                //UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name), nodeData) as UScene;
                TtScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name)) as TtScene;
                if (scene == null)
                    return null;

                using (var ar = descAttr.GetReader(scene))
                {
                    IO.ISerializer desc = nodeData;
                    try
                    {
                        ar.ReadTo(desc, scene);
                        if (await scene.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"SceneData({scene.AssetName}): load failed");
                    }
                }

                scene.AssetName = name;
                if (await scene.LoadChildNode(world, scene, xnd.RootNode.mCoreObject, false) == false)
                    return null;

                scene.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                var notify = new FHostNotify();
                notify.Info = "OnSceneLoaded";
                scene.mMemberTickables.SendNotify(scene, in notify);
                return scene;
            }
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSceneAMeta();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            UpdateNodeAssetReferences(this, ameta);
        }
        protected void UpdateNodeAssetReferences(TtNode node, IO.IAssetMeta ameta)
        {
            node.AddAssetReferences(ameta);
            foreach (var i in node.Children)
            {
                i.AddAssetReferences(ameta);
                UpdateNodeAssetReferences(i, ameta);
            }
        }
        #endregion

        UMemberTickables mMemberTickables = new UMemberTickables();
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            mMemberTickables.TickLogic(this, TtEngine.Instance.ElapseTickCountMS);
            return true;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtScene), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override void TickLogic(TtNodeTickParameters args)
        {
            if (this.IsNoTick)
                return;
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (OnTickLogic(args.World, args.Policy) == false)
                    return;

                if (args.IsTickChildren)
                {
                    {
                        for (int i = 0; i < Children.Count; i++)
                        {
                            Children[i].TickLogic(args);
                        }
                    }
                }
            }

            //base.TickLogic(args);
        }
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            FHostNotify notify = new FHostNotify();
            notify.Info = "OnGatherVisibleMeshes";
            notify.Parameter = rp;
            mMemberTickables.SendNotify(this, in notify);
            base.OnGatherVisibleMeshes(rp);
        }
        [Category("Option")]
        public bool IsGatherVisibleByManagedNodes { get; set; } = false;
        public override bool TreeGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if (IsGatherVisibleByManagedNodes == false)
                return true;
            if (!this.HasStyle(Scene.TtNode.ENodeStyles.SelfInvisible))
            {
                this.OnGatherVisibleMeshes(rp);
            }
            if (!this.HasStyle(Scene.TtNode.ENodeStyles.ChildrenInvisible))
            {
                if (TtEngine.Instance.Config.IsParrallelWorldGather)
                {
                    var numTask = TtEngine.Instance.EventPoster.NumOfPool;
                    numTask = Math.Min(ManagedNodes.Length, numTask);
                    TtEngine.Instance.EventPoster.ParrallelFor(numTask, static (int index, object arg1, object arg2, Thread.Async.TtAsyncTaskStateBase state) =>
                    {
                        var node = arg1 as TtScene;
                        var rp = arg2 as TtWorld.TtVisParameter;
                        int stride = node.Children.Count / (int)state.UserArguments.NumOfParrallelFor + 1;
                        var start = index * stride;
                        for (int n = 0; n < stride; n++)
                        {
                            var nn = start + n;
                            if (nn >= node.ManagedNodes.Length)
                                break;
                            var i = node.ManagedNodes[nn];
                            if (i == null)
                                continue;
                            if (rp.OnVisitNode != null)
                            {
                                if (rp.OnVisitNode(i, rp) == false)
                                    continue;
                            }
                            var type = rp.CullCamera.WhichContainTypeFast(rp.World, in i.AbsAABB, false);
                            switch (type)
                            {
                                case CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                                    continue;
                                case CONTAIN_TYPE.CONTAIN_TEST_INNER:
                                case CONTAIN_TYPE.CONTAIN_TEST_REFER:
                                    {
                                        i.OnGatherVisibleMeshes(rp);
                                        //World.OnVisitNode_GatherVisibleMeshes(i, rp);
                                    }
                                    break;
                            }
                        }
                    }, this, rp);
                }
                else
                {
                    foreach (var i in ManagedNodes)
                    {
                        if (i == null)
                            continue;
                        if (rp.OnVisitNode != null)
                        {
                            if (rp.OnVisitNode(i, rp) == false)
                                continue;
                        }
                        var type = rp.CullCamera.WhichContainTypeFast(World, in i.AbsAABB, false);
                        switch (type)
                        {
                            case CONTAIN_TYPE.CONTAIN_TEST_OUTER:
                                continue;
                            case CONTAIN_TYPE.CONTAIN_TEST_INNER:
                            case CONTAIN_TYPE.CONTAIN_TEST_REFER:
                                {
                                    i.OnGatherVisibleMeshes(rp);
                                    //World.OnVisitNode_GatherVisibleMeshes(i, rp);
                                }
                                break;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class USceneManager : UModule<TtEngine>
    {
        public override void Cleanup(TtEngine host)
        {
            Scenes.Clear();
        }
        public Dictionary<RName, WeakReference<TtScene>> Scenes { get; } = new Dictionary<RName, WeakReference<TtScene>>();
        public async System.Threading.Tasks.Task<TtScene> GetScene(GamePlay.TtWorld world, RName name)
        {
            //return await UScene.LoadScene(world, name);
            System.GC.Collect();
            TtScene scene;
            WeakReference<TtScene> result;
            if (Scenes.TryGetValue(name, out result))
            {
                result.TryGetTarget(out scene);
                if (scene != null)
                {
                    return scene;
                }
                else
                {
                    Scenes.Remove(name);
                }
            }

            scene = await TtScene.LoadScene(world, name);
            if (scene == null)
                return null;

            if (Scenes.TryGetValue(name, out result))
            {
                result.TryGetTarget(out scene);
                if (scene != null)
                {
                    return scene;
                }
                else
                {
                    Scenes.Remove(name);
                }
            }
            Scenes.Add(name, new WeakReference<TtScene>(scene));
            return scene;
        }
        public async System.Threading.Tasks.Task<TtScene> CreateScene(GamePlay.TtWorld world, RName name)
        {
            System.GC.Collect();
            TtScene scene;
            scene = await TtScene.LoadScene(world, name);
            if (scene == null)
                return null;

            return scene;
        }
        public void UnloadScene(RName name)
        {
            if (name == null)
                return;
            if (Scenes.ContainsKey(name))
                Scenes.Remove(name);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public GamePlay.Scene.USceneManager SceneManager { get; } = new GamePlay.Scene.USceneManager();
    }
}
