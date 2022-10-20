using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta]
    public class UMaterialMeshAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UMaterialMesh.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "UMS";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "ums", null);
        //}
        protected override Color GetBorderColor()
        {
            return Color.OrangeRed;
        }
        public override void OnDragTo(Graphics.Pipeline.UViewportSlate vpSlate)
        {
            var worldViewport = vpSlate as EGui.Slate.UWorldViewportSlate;
            if (worldViewport != null)
            {
                var start = worldViewport.CameraController.Camera.GetPosition();
                Vector3 dir = Vector3.Zero;
                var msPt = ImGuiAPI.GetMousePos();
                msPt = worldViewport.Window2Viewport(msPt);
                worldViewport.CameraController.Camera.GetPickRay(ref dir, msPt.X, msPt.Y, worldViewport.ClientSize.X, worldViewport.ClientSize.Y);
                var end = start + dir.AsDVector() * 1000.0f;
                VHitResult htResult = new VHitResult();
                if (worldViewport.World.Root.LineCheck(in start, in end, ref htResult))
                {
                    return;
                }
            }
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
        public bool Initialize(UMeshPrimitives mesh, Pipeline.Shader.UMaterial[] materials)
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
        [RName.PGRName(FilterExts = UMeshPrimitives.AssetExt)]
        [ReadOnly(true)]
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
                    if (Mesh.mCoreObject.IsValidPointer == false)
                    {
                        AssetState = IO.EAssetState.LoadFailed;
                        return;
                    }
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
            }
        }
        UMeshPrimitives mMesh;
        [Browsable(false)]
        public UMeshPrimitives Mesh
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
            RName.PGRNameAttribute mRNameEditor;

            public PGMaterialsAttribute()
            {
                FullRedraw = false;
            }
            protected override async Task<bool> Initialize_Override()
            {
                mRNameEditor = new RName.PGRNameAttribute();
                await mRNameEditor.Initialize();
                return await base.Initialize_Override();
            }
            protected override void Cleanup_Override()
            {
                mRNameEditor?.Cleanup();
                base.Cleanup_Override();
            }
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                bool valueChanged = false;
                newValue = info.Value;
                var multiValue = newValue as EGui.Controls.PropertyGrid.PropertyMultiValue;
                if (multiValue != null)
                {
                    ImGuiAPI.Text(multiValue.MultiValueString);
                }
                else
                {
                    ImGuiAPI.Text(info.Type.ToString());
                    var lst = info.Value as Pipeline.Shader.UMaterial[];
                    if(lst != null)
                        Expandable = lst.Length > 0;
                    if (info.Expand)
                    {
                        if (OnArray(info, lst))
                        {
                            valueChanged = true;
                        }
                    }
                }

                return valueChanged;
            }
            private bool OnArray(EditorInfo info, Pipeline.Shader.UMaterial[] materials)
            {
                if (materials == null)
                    return false;
                bool valueChanged = false;
                var sz = new Vector2(0, 0);
                ImGuiTableRowData rowData;
                unsafe
                {
                    rowData = new ImGuiTableRowData()
                    {
                        IndentTextureId = info.HostPropertyGrid.IndentDec.GetImagePtrPointer().ToPointer(),
                        MinHeight = 0,
                        CellPaddingYEnd = info.HostPropertyGrid.EndRowPadding,
                        CellPaddingYBegin = info.HostPropertyGrid.BeginRowPadding,
                        IndentImageWidth = info.HostPropertyGrid.Indent,
                        IndentTextureUVMin = Vector2.Zero,
                        IndentTextureUVMax = Vector2.One,
                        IndentColor = info.HostPropertyGrid.IndentColor,
                        HoverColor = EGui.UIProxy.StyleConfig.Instance.PGItemHoveredColor,
                        Flags = ImGuiTableRowFlags_.ImGuiTableRowFlags_None,
                    };
                }
                var umesh = info.ObjectInstance as UMaterialMesh;
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                for (int i = 0; i < materials.Length; i++)
                {
                    ImGuiAPI.TableNextRow(in rowData);

                    var name = i.ToString();
                    ImGuiAPI.TableSetColumnIndex(0);
                    ImGuiAPI.AlignTextToFramePadding();
                    var treeNodeRet = ImGuiAPI.TreeNodeEx(name, flags | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf, name);
                    ImGuiAPI.TableNextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var old = materials[i]?.AssetName;

                    mRNameEditor.FilterExts = Pipeline.Shader.UMaterial.AssetExt + "," + Pipeline.Shader.UMaterialInstance.AssetExt;
                    object newValue;
                    info.Value = materials[i]?.AssetName;
                    mRNameEditor.OnDraw(in info, out newValue);
                    RName rn = (RName)newValue;
                    //rn = EGui.Controls.CtrlUtility.DrawRName(old, name, Pipeline.Shader.UMaterial.AssetExt, info.Readonly, null);

                    if (rn != old)
                    {
                        if (rn == null)
                        {
                            materials[i] = UEngine.Instance.GfxDevice.MaterialManager.PxDebugMaterial;
                        }
                        else
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
                                    var mesh = (UMaterialMesh)info.HostPropertyGrid.Target;
                                };
                                exec();
                            }
                        }
                    }
                    if (treeNodeRet)
                        ImGuiAPI.TreePop();
                }
                return valueChanged;
            }
        }
        [PGMaterials]
        public Pipeline.Shader.UMaterial[] Materials
        {
            get;
            private set;
        }
        [Browsable(false)]
        public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            public List<RName> Materials { get; } = new List<RName>();
        }
        [Rtti.Meta(Order = 1)]
        [Browsable(false)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.MeshName = this.Mesh.AssetName;
                for (int i = 0; i < Materials.Length; i++)
                {
                    if (Materials[i] == null)
                        tmp.Materials.Add(RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine));
                    else
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
                            if (value.Materials[i].ExtName == Graphics.Pipeline.Shader.UMaterial.AssetExt)
                            {
                                Materials[i] = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value.Materials[i]);
                            }
                            else if (value.Materials[i].ExtName == Graphics.Pipeline.Shader.UMaterialInstance.AssetExt)
                            {
                                Materials[i] = await mtlMgr.GetMaterialInstance(value.Materials[i]);
                            }
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
        public UMaterialMesh TryGetMaterialMesh(RName name)
        {
            if (name == null)
                return null;
            UMaterialMesh result;
            if (Meshes.TryGetValue(name, out result))
                return result;
            return null;
        }
        public async System.Threading.Tasks.Task<UMaterialMesh> GetMaterialMesh(RName name)
        {
            if (name == null)
                return null;
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
