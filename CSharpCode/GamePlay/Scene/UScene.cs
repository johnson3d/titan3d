using Assimp;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Graphics.Pipeline;
using EngineNS.Macross;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.USceneAMeta@EngineCore" })]
    public class TtSceneAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtScene.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Scene";
        }
        public override Color4b GetBorderColor()
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
        [Rtti.Meta]
        public int NumOfNodes { get; set; }
    }
    [TtScene.SceneCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Scene")]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UScene@EngineCore" })]
    public partial class TtScene : TtNode, IO.IAsset
    {
        public const string AssetExt = ".scene";
        public string TypeExt { get => AssetExt; }
        public override string ToString()
        {
            return this.AssetName.ToString();
        }
        public class SceneCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                //mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType, new USceneData()) as IO.IAsset;
                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                var world = new TtWorld(null);
                await world.InitWorld();
                var task = (mAsset as TtScene).InitializeNode(world, new TtSceneData(), EBoundVolumeType.Box, typeof(TtPlacement));
                PGAsset.Target = mAsset;
            }
        }
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtSceneData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            SetWorld(world);
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
        protected override void UnsafeNullParent()
        {
            base.UnsafeNullParent();
            SetWorld(null);
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
        [Rtti.Meta]
        public async Thread.Async.TtTask<TtRenderPolicy> SetRenderPolicyToViewport(TtViewportSlate slate)
        {
            TtRenderPolicy policy = null;
            var rpAsset = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.LoadAsset(RPolicyName);
            if (rpAsset != null)
            {
                policy = rpAsset.CreateRenderPolicy(slate);
                await policy.Initialize(null);
                if (slate.Viewport.Width > 1 && slate.Viewport.Height > 1)
                    policy.OnResize(slate.Viewport.Width, slate.Viewport.Height);
                slate.RenderPolicy = policy;
                return policy;
            }
            return null;
        }
        WeakReference<TtWorld> mWorld;
        public void SetWorld(TtWorld world)
        {
            if (world == null)
            {
                mWorld = null;
                return;
            }
            mWorld = new WeakReference<TtWorld>(world);
        }
        public TtWorld World
        {
            get
            {
                if (mWorld == null)
                    return null;
                if (mWorld.TryGetTarget(out var result))
                    return result;
                return null;
            }
        }
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
        public async Thread.Async.TtTask<T> SpawnSceneActor<T>(TtNode parent, TtNode.FPostSpawnNode postAction, TtNodeData data = null, EBoundVolumeType bvType = EBoundVolumeType.Box, Type placementType = null, TtWorld world = null, bool isSceneManaged = false)
            where T : TtSceneActorNode
        {
            return await SpawnSceneActor(parent, typeof(T), postAction, data, bvType, placementType, world, isSceneManaged) as T;
        }
        public async Thread.Async.TtTask<TtSceneActorNode> SpawnSceneActor(TtNode parent, Type nodeType, TtNode.FPostSpawnNode postAction, TtNodeData data = null, EBoundVolumeType bvType = EBoundVolumeType.Box, Type placementType = null, TtWorld world = null, bool isSceneManaged = false)
        {
            var node = await TtNode.SpawnNode(parent, nodeType, postAction, data, bvType, placementType, world) as TtSceneActorNode;
            if (node != null)
            {
                node.IsSceneManaged = isSceneManaged;
            }
            return node;
        }
        #endregion

        #region Macross
        Bricks.CodeBuilder.MacrossNode.TtMacrossEditor mMacrossEditor = null;
        [Browsable(false)]
        public Bricks.CodeBuilder.MacrossNode.TtMacrossEditor MacrossEditor
        {
            get
            {
                if (mMacrossEditor == null)
                {
                    mMacrossEditor = new Bricks.CodeBuilder.MacrossNode.TtMacrossEditor();
                    mMacrossEditor.AssetName = AssetName;
                    //mMacrossEditor.FolderExt = ".Macross";
                }
                return mMacrossEditor;
            }
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

            UpdateNumOfNodes();
            var typeStr = Rtti.TtTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.TtXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (SceneData != null)
            {
                using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(SceneData.GetType()), 1, SceneDescAttributeFlags))
                {
                    node.AddAttribute(dataAttr);
                    using (var ar = dataAttr.GetWriter((ulong)SceneData.GetStructSize() * 2))
                    {
                        this.OnBeforeSaveNodeData();
                        ar.Write(SceneData);
                    }
                }
            }

            SaveChildNode(this, xnd.mCoreObject, node.mCoreObject);

            if(IO.TtFileManager.FileExists(name.Address))
                IO.TtFileManager.DeleteFile(name.Address);

            var file = name.Address + "/" + name.PureName + AssetExt;
            xndHolder.SaveXnd(file);
            TtEngine.Instance.SourceControlModule.AddFile(file, true);

            // Macross
            MacrossEditor.AssetName = name;
            MacrossEditor.DefClass.ClassName = name.PureName;
            MacrossEditor.DefClass.Namespace = TtNamespaceDeclaration.GetNameSpaceFromRName(AssetName);
            MacrossEditor.DefClass.SupperClassNames.Clear();
            var baseClsName = typeof(TtSceneMacrossBase).FullName;
            if(!MacrossEditor.DefClass.SupperClassNames.Contains(baseClsName))
                MacrossEditor.DefClass.SupperClassNames.Add(baseClsName);
            MacrossEditor.SaveClassGraph(AssetName);
            MacrossEditor.GenerateCode();
            MacrossEditor.CompileCode();

            if (MacrossGetter != null)
            {
                MacrossGetter.Name = name;
                var mc = MacrossGetter.Get();
                if (mc != null)
                {
                    mc.Root = this;
                    mc.InitializeMacrossNodePropertyValues();
                }
            }
        }
        public int NumOfNodes 
        { 
            get
            {
                return GetNodeData<TtSceneData>().NumOfNodes;
            }
            protected set
            {
                GetNodeData<TtSceneData>().NumOfNodes = value;
            }
        }
        public int NumOfLoadedNode { get; internal set; } = 0;
        public void UpdateNumOfNodes()
        {
            NumOfNodes = 0;
            this.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
            {
                if (inNode.HasStyle(ENodeStyles.Transient))
                    return false;
                ((TtScene)inArg).NumOfNodes++;
                return false;
            }, this);
        }
        internal static async Thread.Async.TtTask<TtScene> LoadScene(GamePlay.TtWorld world, RName name)
        {
            var file = name.Address + "/" + name.PureName + AssetExt;
            if (IO.TtFileManager.FileExists(name.Address))
                file = name.Address;
            using (var xnd = IO.TtXndHolder.LoadXnd(file))
            {
                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(SceneDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return null;
                }

                TtSceneData nodeData = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(descAttr.Name)) as TtSceneData;

                //UScene don't have construct with params
                //UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(xnd.RootNode.Name), nodeData) as UScene;
                TtScene scene = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(xnd.RootNode.Name)) as TtScene;
                if (scene == null)
                    return null;

                scene.NumOfLoadedNode = 0;

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
                scene.NumOfLoadedNode++;

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
                if(scene.MacrossGetter != null)
                {
                    scene.MacrossGetter.Name = name;
                    var mc = scene.MacrossGetter.Get();
                    if (mc != null)
                    {
                        mc.Root = scene;
                        mc.InitializeMacrossNodePropertyValues();
                    }
                }
                return scene;
            }
        }

        TtMacrossGetter<TtSceneMacrossBase> mMacrossGetter;
        public TtMacrossGetter<TtSceneMacrossBase> MacrossGetter
        {
            get
            {
                if (mMacrossGetter == null)
                    mMacrossGetter = TtMacrossGetter<TtSceneMacrossBase>.NewInstance();
                return mMacrossGetter;
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

        TtMemberTickables mMemberTickables = new TtMemberTickables();
        public TtMemberTickables MemberTickables
        {
            get => mMemberTickables;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
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
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtScene>.Scope;
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
        public override bool TryTreeGatherVisibleMeshes(TtWorld.TtVisParameter rp)
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

    public partial class TtSceneMacrossBase
    {
        public TtScene Root;

        public TtNode FindSceneNode(in Guid id, bool bRecursive)
        {
            if (Root == null)
                return null;

            return Root.FindNode(in id, bRecursive);
        }

        public virtual void InitializeMacrossNodePropertyValues()
        {

        }
    }

    public class TtSceneManager : TtModule<TtEngine>
    {
        public override void Cleanup(TtEngine host)
        {
            Scenes.Clear();
        }
        public Dictionary<RName, WeakReference<TtScene>> Scenes { get; } = new Dictionary<RName, WeakReference<TtScene>>();
        private Thread.TtAwaitSessionManager<RName, TtScene> mCreatingSession = new Thread.TtAwaitSessionManager<RName, TtScene>();
        public async Thread.Async.TtTask<TtScene> GetScene(GamePlay.TtWorld world, RName name)
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

            bool isNewSession;
            var session = mCreatingSession.GetOrNewSession(name, out isNewSession);
            if (isNewSession == false)
            {
                return await session.Await();
            }

            scene = await TtScene.LoadScene(world, name);
            if (scene == null)
                return null;

            Scenes.Add(name, new WeakReference<TtScene>(scene));
            mCreatingSession.FinishSession(name, session, scene);
            return scene;
        }
        public async Thread.Async.TtTask<TtScene> CreateScene(GamePlay.TtWorld world, RName name)
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
        public GamePlay.Scene.TtSceneManager SceneManager { get; } = new GamePlay.Scene.TtSceneManager();
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay.Scene
{
	partial class TtScene
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_SetRenderPolicyToViewport_2302347641 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.Scene.TtScene->Thread.Async.TtTask<TtRenderPolicy> SetRenderPolicyToViewport(TtViewportSlate slate)");
		public async Thread.Async.TtTask<TtRenderPolicy> macross_SetRenderPolicyToViewport (string nodeName, TtViewportSlate slate) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":slate", slate);
				}
			}
			var _return_value = await SetRenderPolicyToViewport(slate);
			macross_break_SetRenderPolicyToViewport_2302347641.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross