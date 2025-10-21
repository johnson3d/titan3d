using EngineNS.Thread.Async;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPOI.HPSF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Prefab", "Prefab", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPrefabNode.TtPrefabNodeData), DefaultNamePrefix = "Prefab")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPrefabNode : GamePlay.Scene.TtSceneActorNode, IPooledObject
    {
        public class TtPrefabNodeData : TtNodeData
        {
            RName mPrefabName = null;
            [Rtti.Meta("",Order = 3)]
            public RName PrefabName
            {
                get => mPrefabName;
                set
                {
                    mPrefabName = value;
                }
            }
        }
        public TtPrefabNodeData PrefabNodeData
        {
            get
            {
                return NodeData as TtPrefabNodeData;
            }
        }
        [Category("Option")]
        [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
        public RName PrefabName
        {
            get
            {
                return PrefabNodeData?.PrefabName;
            }
            set
            {
                if (PrefabName == value)
                    return;
                if (PrefabNodeData == null)
                    return;
                var save = PrefabNodeData.PrefabName;
                UpdatePrefab(save, value).WaitCompletedAndDispose();
                PrefabNodeData.PrefabName = value;
            }
        }

        public bool IsAlloc { get; set; }
        protected async override TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            return ret;
        }

        private async Thread.Async.TtTask UpdatePrefab(RName save, RName value)
        {
            if (save != null)
            {
                var oldfab = await TtEngine.Instance.PrefabManager.GetPrefab(save);
                if (oldfab != null)
                {
                    oldfab.RemovePrefabChildren(this);
                }
            }
            var prefab = await TtEngine.Instance.PrefabManager.GetPrefab(value);
            if (prefab != null)
                await prefab.ConcreatePrefab(this.GetWorld(), this);
        }
        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent)
        {
            await base.OnPostInitNode(parent);
            if (PrefabName!=null)
            {
                await UpdatePrefab(null, PrefabName);
            }
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            base.OnParentChanged(prev, cur);
        }

        internal static async Thread.Async.TtTask<TtPrefab> LoadPrefab(GamePlay.TtWorld world, RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                    return null;

                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(PrefabDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return null;
                }

                var nodeData = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(descAttr.Name)) as TtNodeData;

                var prefab = Rtti.TtTypeDescManager.CreateInstance(Rtti.TtTypeDesc.TypeOf(xnd.RootNode.Name)) as TtPrefab;
                if (prefab == null)
                    return null;
                var node = await TtNode.SpawnNode<TtPrefabNode>(world.Root, null, new TtPrefabNode.TtPrefabNodeData(), EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
                prefab.Root = node;
                prefab.AssetName = name;

                using (var ar = descAttr.GetReader(node))
                {
                    IO.ISerializer desc = nodeData;
                    try
                    {
                        ar.ReadTo(desc, node);
                        node.IsDirty = true;
                        if (await node.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"Prefab({prefab.AssetName}): load failed");
                    }
                }

                if (await node.LoadChildNode(world, node, xnd.RootNode.mCoreObject, false) == false)
                    return null;

                node.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                return prefab;
            }
        }
        public const uint PrefabDescAttributeFlags = 1;
        internal static async System.Threading.Tasks.Task ReLoadPrefab(GamePlay.TtWorld world, TtPrefab prefab, RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                    return;

                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(PrefabDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return;
                }

                var nodeData = prefab.Root.NodeData;
                var node = prefab.Root;

                using (var ar = descAttr.GetReader(node))
                {
                    IO.ISerializer desc = nodeData;
                    try
                    {
                        ar.ReadTo(desc, node);
                        if (await node.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(Profiler.ELogTag.Warning, $"Prefab({prefab.AssetName}): load failed");
                    }
                }

                if (await node.LoadChildNode(world, node, xnd.RootNode.mCoreObject, true) == false)
                    return;

                node.DFS_VisitNodeTree((TtNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                return;
            }
        }
        
    }
    [Rtti.Meta("")]
    public class TtPrefabAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtPrefab.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Prefab";
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.PrefabBoderColor;
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            return await TtEngine.Instance.PrefabManager.CreatePrefab(args[0] as TtWorld, GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.PrefabManager.CreatePrefab(args[0] as TtWorld, GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return true;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "scene", null);
        //}
    }

    [TtPrefab.PrefabCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Prefab")]
    public partial class TtPrefab : IO.IAsset, IPooledObject
    {
        public TtPrefabNode Root { get; set; }
        public static bool TryParsePrefabPath(string prefabPath, out RName name, out string[] path)
        {
            name = null;
            path = null;
            var segs = prefabPath.Split('$');
            if (segs.Length != 2)
                return false;
            name = RName.ParseFrom(segs[0]);
            path = segs[0].Split('/');
            return false;
        }
        public const string AssetExt = ".prefab";
        public string TypeExt { get => AssetExt; }
        [Category("Option")]
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
        public RName RPolicyName
        {
            get;
            set;
        }

        public static async Thread.Async.TtTask CreatePrefab(TtPrefabNode node, RName assetName)
        {
            var prefab = new TtPrefab();
            prefab.Root = node;
            prefab.Root.PrefabNodeData.PrefabName = assetName;
            prefab.Root.NodeName = "Prefab";

            prefab.SaveAssetTo(assetName);
            await TtEngine.Instance.PrefabManager.ReloadPrefab(assetName);
        }
        
        #region IAsset
        public class PrefabCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                var world = new TtWorld(null);
                await world.InitWorld();
                PGAsset.Target = mAsset;

                mAsset = Rtti.TtTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                
                var prefab = mAsset as TtPrefab;
                prefab.Root = await TtNode.SpawnNode<TtPrefabNode>(null, null,
                    new TtPrefabNode.TtPrefabNodeData() { PrefabName = dir, }
                    , EBoundVolumeType.Box, typeof(GamePlay.TtPlacement), world);
            }
        }
        [Category("Option")]
        public RName AssetName { get; set; }
        public bool IsAlloc { get ; set ; }

        public void SaveAssetTo(RName name)
        {
            name.AMeta.ClearAssetFiles();
            
            var typeStr = Rtti.TtTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.TtXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (Root != null)
            {
                Root.PrefabNodeData.PrefabName = name;
                using (var dataAttr = xnd.NewAttribute(Rtti.TtTypeDesc.TypeStr(Root.NodeData.GetType()), 1, TtPrefabNode.PrefabDescAttributeFlags))
                {
                    node.AddAttribute(dataAttr);
                    using (var ar = dataAttr.GetWriter((ulong)Root.NodeData.GetStructSize() * 2))
                    {
                        ar.Write(Root.NodeData);
                    }
                }
                Root.SaveChildNode(Root, xnd.mCoreObject, node.mCoreObject);
            }

            xndHolder.SaveXnd(name.Address);
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
            TtEngine.Instance.PrefabManager.UnloadPrefab(name);

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
        }
        
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtPrefabAMeta();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            UpdateNodeAssetReferences(this.Root, ameta);
        }
        protected void UpdateNodeAssetReferences(TtNode node, IO.IAssetMeta ameta)
        {
            if (node == null)
                return;
            node.AddAssetReferences(ameta);
            foreach (var i in node.Children)
            {
                i.AddAssetReferences(ameta);
                UpdateNodeAssetReferences(i, ameta);
            }
        }
        #endregion

        public async Thread.Async.TtTask<TtNode> ConcreatePrefab(TtWorld world, TtNode tarNode)
        {
            return await TtNode.ConcreateNode(world, tarNode, Root);
        }

        private static void RemovePrefabChildren(TtNode tarNode, TtNode node)
        {
            foreach (var i in node.Children)
            {
                var cnodeTar = tarNode.FindFirstChild(i.NodeName, i.GetType());
                RemovePrefabChildren(cnodeTar, i);
                if (cnodeTar != null)
                    cnodeTar.Parent = null;
            }
        }
        public void RemovePrefabChildren(TtNode tarNode)
        {
            RemovePrefabChildren(tarNode, Root);
        }
    }
    public class TtPrefabManager : TtModule<TtEngine>
    {
        public override void Cleanup(TtEngine host)
        {
            foreach(var i in Prefabs.Values)
            {
                i.Root.DisposeWithChildren();
                i.Root.Dispose();
                i.Root = null;
            }
            Prefabs.Clear();
            CoreSDK.DisposeObject(ref PrefabWorld);
        }
        public override async Thread.Async.TtTask<bool> Initialize(TtEngine host)
        {
            PrefabWorld = new TtWorld(null, false);
            return await PrefabWorld.InitWorld();
        }
        public Dictionary<RName, TtPrefab> Prefabs { get; } = new Dictionary<RName, TtPrefab>();
        public GamePlay.TtWorld PrefabWorld;
        private Thread.TtAwaitSessionManager<RName, TtPrefab> mCreatingSession = new Thread.TtAwaitSessionManager<RName, TtPrefab>();
        public async Thread.Async.TtTask<TtPrefab> GetPrefab(RName name)
        {
            TtPrefab result;
            if (Prefabs.TryGetValue(name, out result))
            {
                return result;
            }

            bool isNewSession;
            var session = mCreatingSession.GetOrNewSession(name, out isNewSession);
            if (isNewSession == false)
            {
                return await session.Await();
            }

            result = await TtPrefabNode.LoadPrefab(PrefabWorld, name);
            if (result == null)
                return null;

            Prefabs.Add(name, result);
            mCreatingSession.FinishSession(name, session, result);

            return result;
        }
        public async Thread.Async.TtTask<TtPrefabNode> CreatePrefabNode(TtWorld world, RName name)
        {
            var prefab = await GetPrefab(name);
            if (prefab == null)
                return null;
            return await prefab.Root.CloneNode(world) as TtPrefabNode;
        }
        public async Thread.Async.TtTask<TtPrefab> ReloadPrefab(RName name)
        {
            TtPrefab scene;
            TtPrefab result;
            if (Prefabs.TryGetValue(name, out result))
            {
                await TtPrefabNode.ReLoadPrefab(PrefabWorld, result, name);
                return result;
            }
            scene = await TtPrefabNode.LoadPrefab(PrefabWorld, name);
            if (scene == null)
                return null;

            if (Prefabs.TryGetValue(name, out result))
            {
                return result;
            }
            Prefabs.Add(name, scene);
            return scene;
        }
        public async Thread.Async.TtTask<TtPrefab> CreatePrefab(TtWorld world, RName name)
        {
            var scene = await TtPrefabNode.LoadPrefab(world, name);
            if (scene == null)
                return null;

            return scene;
        }
        public void UnloadPrefab(RName name)
        {
            if (name == null)
                return;
            if (Prefabs.ContainsKey(name))
                Prefabs.Remove(name);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public GamePlay.Scene.TtPrefabManager PrefabManager { get; } = new GamePlay.Scene.TtPrefabManager();
    }
}
