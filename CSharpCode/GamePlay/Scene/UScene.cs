using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Rtti.Meta]
    public class USceneAMeta : IO.IAssetMeta
    {
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
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
                mAsset = Rtti.UTypeDescManager.CreateInstance(TypeSlt.SelectedType, new USceneData()) as IO.IAsset;
                PGAsset.Target = mAsset;
            }
        }
        public UScene(UNodeData data)
            : base(data, EBoundVolumeType.Box, typeof(UPlacement))
        {
            
        }
        int PrevAllocId = 0;
        private UNode[] ContainNodes = new UNode[UInt16.MaxValue];
        public void AllocId(UNode node)
        {
            if (node is ULightWeightNodeBase)
                return;
            for (int i = PrevAllocId; i < ContainNodes.Length; i++)
            {
                if (ContainNodes[i] == null)
                {
                    ContainNodes[i] = node;
                    node.Id = (UInt32)i;
                    node.ParentScene = this;
                    PrevAllocId = i;
                    return;
                }
            }
            for (int i = 0; i < ContainNodes.Length; i++)
            {
                if(ContainNodes[i]==null)
                {
                    ContainNodes[i] = node;
                    node.Id = (UInt32)i;
                    node.ParentScene = this;
                    PrevAllocId = i;
                    return;
                }
            }
            System.Diagnostics.Debug.Assert(false);
        }
        public void FreeId(UNode node)
        {
            if (node is ULightWeightNodeBase)
                return;
            if (node.Id >= UInt16.MaxValue)
                return;
            ContainNodes[node.Id] = null;
            node.Id = UInt32.MaxValue;
        }
        public UNode NewNode(string nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ntype = Rtti.UTypeDesc.TypeOf(nodeType);
            return NewNode(ntype.SystemType, data, bvType, placementType);
        }
        public UNode NewNode(Type nodeType, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var args = new object[] { data, bvType, placementType };
            var node = Rtti.UTypeDescManager.CreateInstance(nodeType, args) as UNode;
            if (node != null)
            {
                AllocId(node);
            }
            return node;
        }
        public USceneData SceneData 
        { 
            get
            {
                return NodeData as USceneData;
            }
        }

        public RName AssetName { get; set; }
        public const uint SceneDescAttributeFlags = 1;
        public void SaveAssetTo(RName name)
        {
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

            SaveXndNode(this, xnd.mCoreObject, node.mCoreObject);

            xndHolder.SaveXnd(name.Address);
        }
        internal unsafe static UScene LoadScene(RName name)
        {
            using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
            {
                var descAttr = xnd.RootNode.mCoreObject.FindFirstAttributeByFlags(SceneDescAttributeFlags);
                if (descAttr.NativePointer == IntPtr.Zero)
                {
                    return null;
                }

                USceneData nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(descAttr.Name)) as USceneData;
                UScene scene = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(xnd.RootNode.Name), nodeData) as UScene;
                if (scene == null)
                    return null;

                var ar = descAttr.GetReader(null);
                ar.Tag = scene;
                IO.ISerializer desc = nodeData;
                ar.ReadTo(desc, scene);
                ar.Tag = null;
                descAttr.ReleaseReader(ref ar);

                scene.AssetName = name;
                if (scene.LoadSceneImpl(xnd.RootNode) == false)
                    return null;
                return scene;
            }   
        }
        private unsafe bool LoadSceneImpl(IO.CXndNode rootNode)
        {
            return LoadXndNode(this, rootNode.mCoreObject);
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
        }
    }

    public class USceneManager : UModule<UEngine>
    {
        public override void Cleanup(UEngine host)
        {
            Scenes.Clear();
        }
        public Dictionary<RName, WeakReference<UScene>> Scenes { get; } = new Dictionary<RName, WeakReference<UScene>>();
        public async System.Threading.Tasks.Task<UScene> GetScene(RName name)
        {
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

            scene = await UEngine.Instance.EventPoster.Post(() =>
            {
                return UScene.LoadScene(name);
            }, Thread.Async.EAsyncTarget.AsyncIO);
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
