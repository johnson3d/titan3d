using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta]
    public class UMaterialMeshAMeta : IO.IAssetMeta
    {
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            base.OnDraw(ref cmdlist, ref sz, ContentBrowser);
        }
    }
    [Rtti.Meta]
    [UMaterialMesh.Import]
    [IO.AssetCreateMenu(MenuName = "MaterialMesh")]
    public partial class UMaterialMesh : IO.ISerializer, IO.IAsset
    {
        public const string AssetExt = ".ums";

        public class ImportAttribute : IO.CommonCreateAttribute
        {
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UMaterialMeshAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            if (MeshName != null)
                ameta.RefAssetRNames.Add(MeshName);

            if (Materials != null)
            {
                foreach (var i in Materials)
                {
                    if (i == null)
                        continue;
                    ameta.RefAssetRNames.Add(i.AssetName);
                }
            }
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.CXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("MaterialMesh", 0, 0))
            {
                var ar = attr.GetWriter(512);
                ar.Write(this);
                attr.ReleaseWriter(ref ar);
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
        }
        [Rtti.Meta]
        [RName.PGRName(ReadOnly = true)]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var manager = tagObject as UMaterialMeshManager;
            if (manager == null)
                return;
        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        public bool Initialize(CMeshPrimitives mesh, Pipeline.Shader.UMaterial[] materials)
        {
            if (mesh.mCoreObject.GetAtomNumber() != materials.Length)
                return false;

            Mesh = mesh;

            for (int i = 0; i < materials.Length; i++)
            {
                Materials[i] = materials[i];
            }

            return true;
        }
        public static UMaterialMesh LoadXnd(UMaterialMeshManager manager, IO.CXndNode node)
        {
            unsafe
            {
                IO.ISerializer result = null;
                var attr = node.TryGetAttribute("MaterialMesh");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    var ar = attr.GetReader(manager);
                    ar.Read(out result, manager);
                    attr.ReleaseReader(ref ar);
                }

                var mesh = result as UMaterialMesh;
                if (mesh != null)
                {
                    return mesh;
                }
                return null;
            }
        }
        public static bool ReloadXnd(UMaterialMesh mesh, UMaterialMeshManager manager, IO.CXndNode node)
        {
            unsafe
            {
                var attr = node.TryGetAttribute("MaterialMesh");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    var ar = attr.GetReader(manager);
                    ar.ReadTo(mesh, null);
                    attr.ReleaseReader(ref ar);
                }
                return true;
            }
        }
        [RName.PGRName(FilterExts = CMeshPrimitives.AssetExt)]
        public RName MeshName
        {
            get
            {
                if (Mesh == null)
                    return null;
                return Mesh.AssetName;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                System.Action exec = async () =>
                {
                    Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value);
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
            }
        }
        CMeshPrimitives mMesh;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public CMeshPrimitives Mesh
        {
            get => mMesh;
            internal set
            {
                mMesh = value;
                if (value == null)
                {
                    Materials = null;
                    return;
                }

                if (Materials == null || Materials.Length != value.mCoreObject.GetAtomNumber())
                {
                    Materials = new Pipeline.Shader.UMaterial[value.mCoreObject.GetAtomNumber()];
                }
            }
        }

        public class PGMaterialsAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public PGMaterialsAttribute()
            {
                FullRedraw = true;
            }
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var umesh = info.ObjectInstance as UMaterialMesh;
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
                var materials = info.Value as Pipeline.Shader.UMaterial[];

                ImGuiAPI.GotoColumns(0);
                ImGuiAPI.SameLine(0, -1);
                var showChild = ImGuiAPI.TreeNode(info.Name, "");
                ImGuiAPI.NextColumn();
                ImGuiAPI.Text(materials.Length.ToString());
                ImGuiAPI.NextColumn();
                if (showChild)
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var name = i.ToString();
                        ImGuiAPI.AlignTextToFramePadding();
                        ImGuiAPI.TreeNodeEx(name, flags, name);
                        ImGuiAPI.NextColumn();
                        ImGuiAPI.SetNextItemWidth(-1);
                        var old = materials[i]?.AssetName;
                        RName rn;
                        if(materials[i] is Pipeline.Shader.UMaterialInstance)
                        {
                            rn = EGui.Controls.CtrlUtility.DrawRName(old, name, Pipeline.Shader.UMaterialInstance.AssetExt, info.Readonly);
                        }
                        else
                        {
                            rn = EGui.Controls.CtrlUtility.DrawRName(old, name, Pipeline.Shader.UMaterial.AssetExt, info.Readonly);
                        }
                        if (rn != old)
                        {
                            if (umesh.AssetState != IO.EAssetState.Loading)
                            {
                                umesh.AssetState = IO.EAssetState.Loading;
                                int IndexOfMaterial = i;
                                System.Action exec = async () =>
                                {
                                    if (rn.ExtName == Pipeline.Shader.UMaterialInstance.AssetExt)
                                    {
                                        materials[IndexOfMaterial] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(rn);
                                    }
                                    else if (rn.ExtName == Pipeline.Shader.UMaterial.AssetExt)
                                    {
                                        materials[IndexOfMaterial] = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(rn);
                                    }
                                    
                                    umesh.AssetState = IO.EAssetState.LoadFinished;
                                };
                                exec();
                            }
                        }
                        ImGuiAPI.NextColumn();
                    }
                    ImGuiAPI.TreePop();
                }

                return false;
            }
        }
        [PGMaterials]
        public Pipeline.Shader.UMaterial[] Materials
        {
            get;
            private set;
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            public List<RName> Materials { get; } = new List<RName>();
        }
        [Rtti.Meta(Order = 1)]
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.MeshName = this.Mesh.AssetName;
                for (int i = 0; i < Materials.Length; i++)
                {
                    tmp.Materials.Add(Materials[i].AssetName);
                }
                return tmp;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                Mesh = null;
                System.Action exec = async ()=>
                {
                    Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value.MeshName);
                    if(Mesh == null)
                    {
                        AssetState = IO.EAssetState.LoadFailed;
                        return;
                    }
                    var mtlMgr = UEngine.Instance.GfxDevice.MaterialInstanceManager;
                    for (int i = 0; i < Materials.Length; i++)
                    {
                        if (i < value.Materials.Count)
                        {
                            var mtl = await mtlMgr.GetMaterialInstance(value.Materials[i]);
                            Materials[i] = mtl;
                        }
                    }

                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
            }
        }
    }

    public class UMaterialMeshManager
    {
        public Dictionary<RName, UMaterialMesh> Meshes { get; } = new Dictionary<RName, UMaterialMesh>();
        public async System.Threading.Tasks.Task<UMaterialMesh> CreateMaterialMesh(RName name)
        {
            var result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = UMaterialMesh.LoadXnd(this, xnd.RootNode);
                        if (mesh == null)
                            return null;

                        return mesh;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return result;
        }
        public async System.Threading.Tasks.Task<bool> ReloadMaterialMesh(RName rn)
        {
            UMaterialMesh result;
            if (Meshes.TryGetValue(rn, out result) == false)
                return true;

            var ok = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return UMaterialMesh.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return ok;
        }
        public async System.Threading.Tasks.Task<UMaterialMesh> GetMaterialMesh(RName name)
        {
            UMaterialMesh result;
            if (Meshes.TryGetValue(name, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = UMaterialMesh.LoadXnd(this, xnd.RootNode);
                        if (mesh == null)
                            return null;

                        return mesh;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                Meshes[name] = result;
                return result;
            }

            return null;
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice
    {
        public Mesh.UMaterialMeshManager MaterialMeshManager { get; } = new Mesh.UMaterialMeshManager();
    }
}
