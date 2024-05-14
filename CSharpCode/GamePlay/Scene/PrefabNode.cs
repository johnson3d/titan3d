using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Assimp;
using NPOI.Util;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Prefab", "Prefab", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(TtPrefabNode.TtPrefabNodeData), DefaultNamePrefix = "Prefab")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPrefabNode : GamePlay.Scene.USceneActorNode
    {
        public class TtPrefabNodeData : UNodeData
        {
            RName mPrefabName = null;
            [Rtti.Meta(Order = 3)]
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
                if (PrefabNodeData == null)
                    return;
                var save = PrefabNodeData.PrefabName;
                _ = UpdatePrefab(save, value);
                PrefabNodeData.PrefabName = value;
            }
        }
        private async Thread.Async.TtTask UpdatePrefab(RName save, RName value)
        {
            if (save != null)
            {
                var oldfab = await UEngine.Instance.PrefabManager.GetPrefab(save);
                if (oldfab != null)
                {
                    oldfab.RemovePrefabChildren(this);
                }
            }
            var prefab = await UEngine.Instance.PrefabManager.GetPrefab(value);
            if (prefab != null)
                await prefab.ConcreatePrefab(this.GetWorld(), this);
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);
            _ = UpdatePrefab(null, PrefabName);
        }
    }
    [Rtti.Meta]
    public class TtPrefabAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UScene.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Prefab";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            //return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
            return null;
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
    public partial class TtPrefab : IO.IAsset
    {
        public TtPrefabNode Root;
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
        public class TtPrefabNodeData : UNodeData
        {
        }
        public const string AssetExt = ".prefab";

        public static async Thread.Async.TtTask CreatePrefab(TtPrefabNode node, RName assetName)
        {
            var prefab = new TtPrefab();
            prefab.Root = node;
            prefab.Root.PrefabNodeData.PrefabName = assetName;
            prefab.Root.NodeName = "Prefab";

            prefab.SaveAssetTo(assetName);
            await UEngine.Instance.PrefabManager.ReloadPrefab(assetName);
        }
        
        #region IAsset
        public class PrefabCreateAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                var world = new UWorld(null);
                await world.InitWorld();
                PGAsset.Target = mAsset;

                mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                
                var prefab = mAsset as TtPrefab;
                prefab.Root = new TtPrefabNode();
                _ = prefab.Root.InitializeNode(world, new TtPrefabNode.TtPrefabNodeData() { PrefabName = dir, }, EBoundVolumeType.Box, typeof(GamePlay.UIdentityPlacement));
            }
        }
        [Category("Option")]
        public RName AssetName { get; set; }
        public const uint PrefabDescAttributeFlags = 1;
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }

            var typeStr = Rtti.UTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.TtXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (Root != null)
            {
                Root.PrefabNodeData.PrefabName = name;
                using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(Root.NodeData.GetType()), 1, PrefabDescAttributeFlags))
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
        }
        internal static async System.Threading.Tasks.Task<TtPrefab> LoadPrefab(GamePlay.UWorld world, RName name)
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

                var nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(descAttr.Name)) as UNodeData;

                var prefab = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name)) as TtPrefab;
                if (prefab == null)
                    return null;
                var node = new TtPrefabNode();
                await node.InitializeNode(world, new TtPrefabNode.TtPrefabNodeData(), EBoundVolumeType.Box, typeof(GamePlay.UIdentityPlacement));
                prefab.Root = node;
                prefab.AssetName = name;

                using (var ar = descAttr.GetReader(node))
                {
                    IO.ISerializer desc = nodeData;
                    try
                    {
                        ar.ReadTo(desc, node);
                        nodeData.IsDirty = true;
                        if (await node.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Scene", $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Prefab({prefab.AssetName}): load failed");
                    }
                }

                if (await node.LoadChildNode(world, node, xnd.RootNode.mCoreObject, false) == false)
                    return null;

                node.DFS_VisitNodeTree((UNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                return prefab;
            }
        }
        internal static async System.Threading.Tasks.Task ReLoadPrefab(GamePlay.UWorld world, TtPrefab prefab, RName name)
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
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Scene", $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Prefab({prefab.AssetName}): load failed");
                    }
                }

                if (await node.LoadChildNode(world, node, xnd.RootNode.mCoreObject, true) == false)
                    return;

                node.DFS_VisitNodeTree((UNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                return;
            }
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtPrefabAMeta();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            UpdateNodeAssetReferences(this.Root, ameta);
        }
        protected void UpdateNodeAssetReferences(UNode node, IO.IAssetMeta ameta)
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

        private static async Thread.Async.TtTask<UNode> ConcreateNode(UWorld world, UNode tarNode, UNode node)
        {
            UNode result = tarNode;
            if (result == null)
            {
                result = Rtti.UTypeDescManager.CreateInstance(node.GetType()) as UNode;
            }
            else
            {
                System.Diagnostics.Debug.Assert(tarNode.GetType() == node.GetType());
            }
            var nd = Rtti.UTypeDescManager.CreateInstance(node.NodeData.GetType()) as UNodeData;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.UTypeDesc.TypeOf(node.NodeData.GetType()));
            meta.CopyObjectMetaField(nd, node.NodeData);
            await result.InitializeNode(world, nd, node.BoundVolumeType, node.Placement.GetType());
            result.SetPrefabTemplate(node.NodeData);
            
            foreach (var i in node.Children)
            {
                var cnodeTar = result.FindFirstChild(i.NodeName, i.GetType());
                var cnode = await ConcreateNode(world, cnodeTar, i);
                cnode.Parent = result;
            }

            return result;
        }

        public async Thread.Async.TtTask<UNode> ConcreatePrefab(UWorld world, UNode tarNode)
        {
            return await ConcreateNode(world, tarNode, Root);
        }

        private static void RemovePrefabChildren(UNode tarNode, UNode node)
        {
            foreach (var i in node.Children)
            {
                var cnodeTar = tarNode.FindFirstChild(i.NodeName, i.GetType());
                RemovePrefabChildren(cnodeTar, i);
                if (cnodeTar != null)
                    cnodeTar.Parent = null;
            }
        }
        public void RemovePrefabChildren(UNode tarNode)
        {
            RemovePrefabChildren(tarNode, Root);
        }
    }
    public class TtPrefabManager : UModule<UEngine>
    {
        public override void Cleanup(UEngine host)
        {
            Prefabs.Clear();
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
        {
            PrefabWorld = new UWorld(null);
            return await PrefabWorld.InitWorld();
        }
        public Dictionary<RName, TtPrefab> Prefabs { get; } = new Dictionary<RName, TtPrefab>();
        public GamePlay.UWorld PrefabWorld;
        public async System.Threading.Tasks.Task<TtPrefab> GetPrefab(RName name)
        {
            TtPrefab scene;
            TtPrefab result;
            if (Prefabs.TryGetValue(name, out result))
            {
                return result;
            }

            scene = await TtPrefab.LoadPrefab(PrefabWorld, name);
            if (scene == null)
                return null;

            if (Prefabs.TryGetValue(name, out result))
            {
                return result;
            }
            Prefabs.Add(name, scene);
            return scene;
        }
        public async System.Threading.Tasks.Task<TtPrefab> ReloadPrefab(RName name)
        {
            TtPrefab scene;
            TtPrefab result;
            if (Prefabs.TryGetValue(name, out result))
            {
                await TtPrefab.ReLoadPrefab(PrefabWorld, result, name);
                return result;
            }
            scene = await TtPrefab.LoadPrefab(PrefabWorld, name);
            if (scene == null)
                return null;

            if (Prefabs.TryGetValue(name, out result))
            {
                return result;
            }
            Prefabs.Add(name, scene);
            return scene;
        }
        public async System.Threading.Tasks.Task<TtPrefab> CreatePrefab(RName name)
        {
            var scene = await TtPrefab.LoadPrefab(PrefabWorld, name);
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

    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPrefabCreateForm
    {
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await SettingsPropGrid.Initialize();

            SettingsPropGrid.Target = this;
            
            return true;
        }

        bool mVisible;
        public bool Visible
        {
            get => mVisible;
            set
            {
                mVisible = value;
            }
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public TtPrefabNode RootNode;
        [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
        [Category("Option")]
        public RName PrefabName { get; set; }

        public EGui.Controls.PropertyGrid.PropertyGrid SettingsPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            var visible = true;
            var size = Vector2.Zero;// new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            if (mVisible)
                ImGuiAPI.OpenPopup($"PrefabCreator", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            if (ImGuiAPI.BeginPopupModal($"PrefabCreator", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                SettingsPropGrid.OnDraw(false, false, false);

                if (PrefabName != null && ImGuiAPI.Button("Create"))
                {
                    _ = TtPrefab.CreatePrefab(RootNode, PrefabName);
                    
                    mVisible = false;
                    ImGuiAPI.CloseCurrentPopup();
                    PrefabName = null;
                    RootNode = null;
                }
                if (ImGuiAPI.Button("Cancel"))
                {
                    PrefabName = null;
                    mVisible = false;
                    RootNode = null;
                    ImGuiAPI.CloseCurrentPopup();
                }
                ImGuiAPI.EndPopup();
            }
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public GamePlay.Scene.TtPrefabManager PrefabManager { get; } = new GamePlay.Scene.TtPrefabManager();
    }
}
