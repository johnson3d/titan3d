using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        public override void OnBeforeRenamedAsset(IO.IAsset asset, RName name)
        {
            CoreSDK.CheckResult(UEngine.Instance.GfxDevice.MaterialMeshManager.UnsafeRemove(name) == asset);
        }
        public override void OnAfterRenamedAsset(IO.IAsset asset, RName name)
        {
            UEngine.Instance.GfxDevice.MaterialMeshManager.UnsafeAdd(name, (UMaterialMesh)asset);
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override async Thread.Async.TtTask<bool> AutoGenSnap()
        {
            var renderer = new Graphics.Pipeline.TtOffscreenRenderer();
            await renderer.Initialize(RName.GetRName("graphics/deferred.rpolicy", RName.ERNameType.Engine));
            renderer.SetSize(256, 256);
            
            Graphics.Mesh.UMaterialMesh Mesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.CreateMaterialMesh(GetAssetName());
            var mesh = new Graphics.Mesh.TtMesh();
            var ok = mesh.Initialize(Mesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(renderer.World, renderer.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
                meshNode.IsSceneManaged = false;
                renderer.RenderPolicy.DefaultCamera.AutoZoom(in meshNode.AABB);
            }

            UEngine.Instance.GfxDevice.RenderSwapQueue.CaptureRenderDocFrame = true;
            UEngine.Instance.GfxDevice.RenderSwapQueue.BeginFrameCapture();

            renderer.ExecuteRender();

            UEngine.Instance.GfxDevice.RenderSwapQueue.EndFrameCapture($"AutoGen:{GetAssetName()}");

            renderer.TickSync();
            Editor.USnapshot.Save(this.GetAssetName(), this, renderer.RenderPolicy.GetFinalShowRSV());
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
        DVector3 CalculateDragPosition(Graphics.Pipeline.TtViewportSlate vpSlate)
        {
            var worldViewport = vpSlate as EGui.Slate.TtWorldViewportSlate;
            if (worldViewport != null)
            {
                var start = worldViewport.CameraController.Camera.GetPosition();
                Vector3 dir = Vector3.Zero;
                var msPt = new Vector2(UEngine.Instance.InputSystem.Mouse.GlobalMouseX, UEngine.Instance.InputSystem.Mouse.GlobalMouseY) - vpSlate.ViewportPos;
                msPt = worldViewport.Window2Viewport(msPt);
                worldViewport.CameraController.Camera.GetPickRay(ref dir, msPt.X, msPt.Y, worldViewport.ClientSize.X, worldViewport.ClientSize.Y);
                var end = start + dir.AsDVector() * 1000.0f;
                VHitResult hitResult = new VHitResult();
                DVector3 hitPos;
                if (worldViewport.World.Root.LineCheck(in start, in end, ref hitResult))
                {
                    hitPos = hitResult.Position;
                }
                else
                {
                    var ray = new DRay()
                    {
                        Position = start,
                        Direction = dir,
                    };
                    var plane = new DPlane(DVector3.Zero, DVector3.Up);
                    double distance;
                    if (DRay.Intersects(in ray, in plane, out distance))
                        hitPos = start + dir.AsDVector() * distance;
                    else
                        hitPos = start + dir.AsDVector() * 10.0f;
                }
                return hitPos;
            }
            return DVector3.Zero;
        }
        public override bool DraggingInViewport 
        { 
            get => base.DraggingInViewport;
            set
            {
                base.DraggingInViewport = value;
                if(!value && mPreviewNode != null)
                {
                    mPreviewNode.Parent = null;
                }
            }
        }
        public override async Thread.Async.TtTask OnDragTo(Graphics.Pipeline.TtViewportSlate vpSlate)
        {
            DraggingInViewport = false;
            var worldViewport = vpSlate as EGui.Slate.TtWorldViewportSlate;
            if (worldViewport != null)
            {
                var hitPos = CalculateDragPosition(vpSlate);
                var meshNodeData = new UMeshNode.UMeshNodeData();
                meshNodeData.Name = mAssetName.PureName;
                meshNodeData.MeshName = mAssetName;
                var node = await worldViewport.World.Root.ParentScene.NewNode(worldViewport.World, typeof(UMeshNode), meshNodeData, EBoundVolumeType.Box, typeof(UPlacement));
                node.Parent = worldViewport.World.Root;
                node.Placement.Position = hitPos;
                node.HitproxyType = Pipeline.UHitProxy.EHitproxyType.Root;

                if(mPreviewNode != null)
                {
                    mPreviewNode.Parent = null;
                }
            }
        }
        Thread.Async.TtTask<USceneActorNode>? mPreviewNodeTask;
        UMeshNode mPreviewNode;
        public override async Thread.Async.TtTask OnDragging(TtViewportSlate vpSlate)
        {
            var worldViewport = vpSlate as EGui.Slate.TtWorldViewportSlate;
            if (worldViewport == null)
                return;

            if (mPreviewNodeTask == null)
            {
                var meshNodeData = new UMeshNode.UMeshNodeData();
                meshNodeData.Name = mAssetName.PureName;
                meshNodeData.MeshName = mAssetName;
                mPreviewNodeTask = worldViewport.World.Root.ParentScene.NewNode(worldViewport.World, typeof(UMeshNode), meshNodeData, EBoundVolumeType.Box, typeof(UPlacement));
                return;
            }
            else if (mPreviewNodeTask.Value.IsCompleted == false)
                return;
            else
            {
                mPreviewNode = mPreviewNodeTask.Value.Result as UMeshNode;
            }

            if (mPreviewNode.Parent != worldViewport.World.Root)
                mPreviewNode.Parent = worldViewport.World.Root;

            var hitPos = CalculateDragPosition(vpSlate);
            mPreviewNode.Placement.Position = hitPos;
        }
    }
    [Rtti.Meta]
    [UMaterialMesh.Import]
    [IO.AssetCreateMenu(MenuName = "Mesh/MaterialMesh")]
    public partial class UMaterialMesh : IO.ISerializer, IO.IAsset
    {
        public const string AssetExt = ".ums";

        public class ImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                await base.DoCreate(dir, type, ext);
                var mesh = mAsset as UMaterialMesh;
                mesh.SubMeshes[0].MeshName = RName.GetRName("mesh/base/box.vms", RName.ERNameType.Engine);
            }
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

            foreach (var i in SubMeshes)
            {
                ameta.RefAssetRNames.Add(i.MeshName);
                if (i.Materials != null)
                {
                    foreach (var j in i.Materials)
                    {
                        if (j == null)
                            continue;
                        ameta.RefAssetRNames.Add(j.AssetName);
                    }
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
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("MaterialMesh", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(this);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }

                xnd.SaveXnd(name.Address);
                this.SerialId++;
            }
            UEngine.Instance.SourceControlModule.AddFile(name.Address);
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
        public bool Initialize(List<UMeshPrimitives> mesh, List<Pipeline.Shader.UMaterial[]> materials)
        {
            if (mesh.Count != materials.Count)
                return false;

            SubMeshes = new List<TtSubMaterialedMesh>();
            for (int i = 0; i < mesh.Count; i++)
            {
                SubMeshes.Add(new TtSubMaterialedMesh());
                if (false == SubMeshes[i].Initialize(mesh[i], materials[i]))
                    return false;
            }
            UpdateAABB();
            return true;
        }
        public bool Initialize(List<UMeshPrimitives> mesh, List<List<Pipeline.Shader.UMaterial>> materials)
        {
            if (mesh.Count != materials.Count)
                return false;

            //SubMeshes = new List<TtSubMaterialedMesh>();
            SubMeshes.Clear();
            for (int i = 0; i < mesh.Count; i++)
            {
                var sbMesh = new TtSubMaterialedMesh();
                if (false == sbMesh.Initialize(mesh[i], materials[i]))
                    return false;
                SubMeshes.Add(sbMesh);
            }
            UpdateAABB();
            return true;
        }
        public void UpdateSubMeshes()
        {
            foreach (var i in SubMeshes)
            {
                if (false == i.Initialize(i.Mesh, i.Materials))
                {

                }
            }
            UpdateAABB();
            SerialId++;
        }
        public unsafe static UMaterialMesh LoadXnd(UMaterialMeshManager manager, IO.TtXndNode node)
        {
            try
            {
                IO.ISerializer result = null;
                var attr = node.TryGetAttribute("MaterialMesh");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.Read(out result, manager);
                    }
                }

                var mesh = result as UMaterialMesh;
                if (mesh != null)
                {
                    mesh.UpdateAABB();
                    return mesh;
                }
                return null;
            }
            catch (Exception exp)
            {
                Profiler.Log.WriteException(exp);
                return null;
            }
        }
        public static bool ReloadXnd(UMaterialMesh mesh, UMaterialMeshManager manager, IO.TtXndNode node)
        {
            unsafe
            {
                var attr = node.TryGetAttribute("MaterialMesh");
                if ((IntPtr)attr.CppPointer != IntPtr.Zero)
                {
                    using (var ar = attr.GetReader(manager))
                    {
                        ar.ReadTo(mesh, null);
                    }
                }
                mesh.UpdateAABB();
                return true;
            }
        }
        public class TtSubMaterialedMesh : IO.BaseSerializer
        {
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
                        Materials.Clear();
                        Materials.Capacity = 0;
                        return;
                    }

                    if (Materials.Count != value.NumAtom)
                    {
                        Materials.Resize((int)value.NumAtom);
                    }
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
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"GetMeshPrimitive({value}) failed");
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        AssetState = IO.EAssetState.LoadFinished;
                    };
                    exec();
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
                        var lst = info.Value as List<Pipeline.Shader.UMaterial>;
                        if (lst != null)
                            Expandable = lst.Count > 0;
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
                private bool OnArray(EditorInfo info, List<Pipeline.Shader.UMaterial> materials)
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
                    var umesh = info.ObjectInstance as TtSubMaterialedMesh;
                    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                    for (int i = 0; i < materials.Count; i++)
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
            public List<Pipeline.Shader.UMaterial> Materials
            {
                get;
            } = new List<Pipeline.Shader.UMaterial>();
            [Browsable(false)]
            public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
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
            public bool Initialize(UMeshPrimitives mesh, List<Pipeline.Shader.UMaterial> materials)
            {
                if (mesh.mCoreObject.GetAtomNumber() != materials.Count)
                    return false;

                Mesh = mesh;

                for (int i = 0; i < materials.Count; i++)
                {
                    Materials[i] = materials[i];
                }

                return true;
            }

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
                    if (this.Mesh == null)
                        return tmp;
                    tmp.MeshName = this.Mesh.AssetName;
                    for (int i = 0; i < Materials.Count; i++)
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
                    System.Action exec = async () =>
                    {
                        Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value.MeshName);
                        if (Mesh == null)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Error, "IO", $"GetMeshPrimitive({value.MeshName}) failed");
                            AssetState = IO.EAssetState.LoadFailed;
                            return;
                        }
                        var mtlMgr = UEngine.Instance.GfxDevice.MaterialInstanceManager;
                        for (int i = 0; i < Materials.Count; i++)
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
        [Rtti.Meta()]
        public List<TtSubMaterialedMesh> SubMeshes { get; set; } = new List<TtSubMaterialedMesh>() { new TtSubMaterialedMesh() };
        public Graphics.Mesh.UMeshPrimitives GetMeshPrimitives(int index)
        {
            return SubMeshes[index].Mesh;
        }
        BoundingBox mAABB;
        public ref BoundingBox AABB
        {
            get => ref mAABB;
        }
        private void UpdateAABB()
        {
            mAABB.InitEmptyBox();
            foreach (var i in SubMeshes)
            {
                if (i.Mesh == null)
                    continue;
                BoundingBox.Merge(in mAABB, i.Mesh.mCoreObject.mAABB, out mAABB);
            }
        }

        [Browsable(false)]
        public uint SerialId
        {
            get;
            set;
        } = 0;
    }

    public class UMaterialMeshManager
    {
        public Dictionary<RName, UMaterialMesh> Meshes { get; } = new Dictionary<RName, UMaterialMesh>();
        public async Thread.Async.TtTask<UMaterialMesh> CreateMaterialMesh(RName name)
        {
            var result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
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
        public async Thread.Async.TtTask<bool> ReloadMaterialMesh(RName rn)
        {
            UMaterialMesh result;
            if (Meshes.TryGetValue(rn, out result) == false)
                return true;

            var ok = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
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
        internal UMaterialMesh UnsafeRemove(RName name)
        {
            lock (Meshes)
            {
                if (Meshes.TryGetValue(name, out var result))
                {
                    return result;
                }
                return null;
            }
        }
        internal void UnsafeAdd(RName name, UMaterialMesh obj)
        {
            lock (Meshes)
            {
                Meshes.Add(name, obj);
            }
        }
        public async Thread.Async.TtTask<UMaterialMesh> GetMaterialMesh(RName name)
        {
            if (name == null)
                return null;
            UMaterialMesh result;
            if (Meshes.TryGetValue(name, out result))
                return result;

            result = await CreateMaterialMesh(name);

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
