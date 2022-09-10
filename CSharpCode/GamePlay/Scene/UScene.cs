using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Rtti.Meta]
    public class USceneAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UScene.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            base.OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddText(in start, 0xFFFFFFFF, "scene", null);
        }
    }
    public class USceneData : UNodeData
    {
        
    }
    [UScene.SceneCreateAttribute]
    [IO.AssetCreateMenu(MenuName = "Scene")]
    public partial class UScene : UNode, IO.IAsset
    {
        public const string AssetExt = ".scene";
        public class SceneCreateAttribute : IO.CommonCreateAttribute
        {
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                //mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType, new USceneData()) as IO.IAsset;
                mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType) as IO.IAsset;
                var task = (mAsset as UScene).InitializeNode(null, new USceneData(), EBoundVolumeType.Box, typeof(UPlacement));
                PGAsset.Target = mAsset;
            }
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new USceneData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            World = world;
            //ParentScene = GetNearestParentScene();

            var task = mMemberTickables.InitializeMembers(this);
            return true;
        }
        public UScene()
        {
            mMemberTickables.CollectMembers(this);
        }
        ~UScene()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            ClearChildren();
            UEngine.Instance?.SceneManager.UnloadScene(this.AssetName);
        }
        public USceneData SceneData
        {
            get
            {
                return NodeData as USceneData;
            }
        }
        public UWorld World;
        #region Allocator
        int PrevAllocId = 0;
        private UNode[] ContainNodes = new UNode[UInt16.MaxValue];
        public void AllocId(UNode node)
        {
            lock (ContainNodes)
            {
                if (node is ULightWeightNodeBase)
                    return;
                for (int i = PrevAllocId; i < ContainNodes.Length; i++)
                {
                    if (ContainNodes[i] == null)
                    {
                        ContainNodes[i] = node;
                        node.SceneId = (UInt32)i;
                        PrevAllocId = i;
                        return;
                    }
                }
                for (int i = 0; i < ContainNodes.Length; i++)
                {
                    if (ContainNodes[i] == null)
                    {
                        ContainNodes[i] = node;
                        node.SceneId = (UInt32)i;
                        PrevAllocId = i;
                        return;
                    }
                }
                System.Diagnostics.Debug.Assert(false);
            }
        }
        public void FreeId(UNode node)
        {
            if (node is ULightWeightNodeBase)
                return;
            lock (ContainNodes)
            {
                if (node.SceneId >= UInt16.MaxValue)
                    return;
                System.Diagnostics.Debug.Assert(ContainNodes[node.SceneId] == node);
                ContainNodes[node.SceneId] = null;
                node.SceneId = UInt32.MaxValue;
            }
        }
        public async System.Threading.Tasks.Task<USceneActorNode> NewNode(GamePlay.UWorld world, string nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType, bool isSceneManaged = false)
        {
            var ntype = Rtti.UTypeDesc.TypeOf(nodeType);
            return await NewNode(world, ntype.SystemType, data, bvType, placementType, false);
        }
        public async System.Threading.Tasks.Task<USceneActorNode> NewNodeSimple(GamePlay.UWorld world, Type nodeType, UNodeData data, bool isSceneManaged = false)
        {
            return await NewNode(world, nodeType, data, EBoundVolumeType.Box, typeof(GamePlay.UPlacement), isSceneManaged);
        }
        public async System.Threading.Tasks.Task<USceneActorNode> NewNode(GamePlay.UWorld world, Type nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType, bool isSceneManaged = false)
        {
            var node = Rtti.UTypeDescManager.CreateInstance(nodeType) as USceneActorNode;
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
                ameta.SaveAMeta();
            }

            var typeStr = Rtti.UTypeDesc.TypeStr(GetType());
            var xndHolder = new EngineNS.IO.CXndHolder(typeStr, 1, 0);
            var xnd = xndHolder;
            var node = xndHolder.RootNode;
            if (SceneData != null)
            {
                using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(SceneData.GetType()), 1, SceneDescAttributeFlags))
                {
                    node.AddAttribute(dataAttr);
                    var ar = dataAttr.GetWriter((ulong)SceneData.GetStructSize() * 2);
                    ar.Write(SceneData);
                    dataAttr.ReleaseWriter(ref ar);
                }
            }

            SaveChildNode(this, xnd.mCoreObject, node.mCoreObject);

            xndHolder.SaveXnd(name.Address);
        }
        internal static async System.Threading.Tasks.Task<UScene> LoadScene(GamePlay.UWorld world, RName name)
        {
            using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
            {
                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(SceneDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return null;
                }

                USceneData nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(descAttr.Name)) as USceneData;

                //UScene don't have construct with params
                //UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name), nodeData) as UScene;
                UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name)) as UScene;
                if (scene == null)
                    return null;

                var ar = descAttr.GetReader(null);
                ar.Tag = scene;
                IO.ISerializer desc = nodeData;
                try
                {
                    ar.ReadTo(desc, scene);
                    if (await scene.InitializeNode(world, nodeData, EBoundVolumeType.None, null) == false)
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Scene", $"InitializeNode failed: NodeDataType={descAttr.Name}, NodeData={xnd.RootNode.Name}");
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"SceneData({scene.AssetName}): load failed");
                }
                ar.Tag = null;
                descAttr.ReleaseReader(ref ar);

                scene.AssetName = name;
                if (await scene.LoadChildNode(world, scene, xnd.RootNode.mCoreObject) == false)
                    return null;

                scene.DFS_VisitNodeTree((UNode inNode, object inArg) =>
                {
                    inNode.OnSceneLoaded();
                    return false;
                }, null);
                return scene;
            }   
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new USceneAMeta();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            UpdateNodeAssetReferences(this, ameta);
        }
        protected void UpdateNodeAssetReferences(UNode node, IO.IAssetMeta ameta)
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
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            mMemberTickables.TickLogic(this, UEngine.Instance.ElapseTickCount);
            return true;
        }
        public override unsafe bool IsTreeContain(DVector3* localStart, DVector3* dir, DBoundingBox* pBox)
        {
            return true;
        }

    }

    public class USceneManager : UModule<UEngine>
    {
        public override void Cleanup(UEngine host)
        {
            Scenes.Clear();
        }
        public Dictionary<RName, WeakReference<UScene>> Scenes { get; } = new Dictionary<RName, WeakReference<UScene>>();
        public async System.Threading.Tasks.Task<UScene> GetScene(GamePlay.UWorld world, RName name)
        {
            //return await UScene.LoadScene(world, name);
            System.GC.Collect();
            UScene scene;
            WeakReference<UScene> result;
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

            scene = await UScene.LoadScene(world, name);
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
            Scenes.Add(name, new WeakReference<UScene>(scene));
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
    partial class UEngine
    {
        public GamePlay.Scene.USceneManager SceneManager { get; } = new GamePlay.Scene.USceneManager();
    }
}
