using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta]
    public partial class UMaterialInstanceAMeta : IO.IAssetMeta
    {
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void ResetSnapshot()
        {
            HasSnapshot = true;
            OnShowIconTimout(0);
        }
        System.Threading.Tasks.Task<Editor.USnapshot> Task;
        IntPtr SnapshotPtr;
        public override unsafe void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            if (SnapshotPtr == IntPtr.Zero && HasSnapshot == true)
            {
                if (Task == null)
                {
                    Task = Editor.USnapshot.Load(GetAssetName().Address + ".snap");
                }
                else if (Task.IsCompleted)
                {
                    if (Task.Result == null)
                    {
                        HasSnapshot = false;
                    }
                    else
                    {
                        SnapshotPtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(Task.Result.mTextureRSV));
                    }
                    Task = null;
                }
            }

            var start = ImGuiAPI.GetItemRectMin();
            var end = start + sz;

            var name = IO.FileManager.GetPureName(GetAssetName().Name);
            var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
            Vector2 tpos;
            tpos.Y = start.Y + sz.Y - tsz.Y;
            tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
            ImGuiAPI.PushClipRect(ref start, ref end, true);

            end.Y -= tsz.Y;
            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            if (SnapshotPtr != IntPtr.Zero)
            {
                cmdlist.AddImage(SnapshotPtr.ToPointer(), ref start, ref end, ref uv0, ref uv1, 0xFFFFFFFF);
            }

            cmdlist.AddText(ref tpos, 0xFFFF00FF, name, null);
            ImGuiAPI.PopClipRect();
        }
        public override void OnShowIconTimout(int time)
        {
            if (SnapshotPtr != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(SnapshotPtr);
                handle.Free();
                SnapshotPtr = IntPtr.Zero;
                Task = null;
            }
        }
    }
    [Rtti.Meta]
    [UMaterialInstance.MaterialInstanceImport]
    [IO.AssetCreateMenu(MenuName = "MaterialInstance")]
    public partial class UMaterialInstance : UMaterial
    {
        public new const string AssetExt = ".uminst";

        public class MaterialInstanceImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as UMaterialInstance;
                if (material == null)
                    return false;

                if (material.ParentMaterial == null)
                    return false;

                return true;
            }
        }
        #region IAsset
        public override IO.IAssetMeta CreateAMeta()
        {
            var result = new UMaterialInstanceAMeta();
            return result;
        }
        public override IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public override void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.CXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("MaterialInstance", 0, 0))
            {
                var ar = attr.GetWriter(512);
                ar.Write(this);
                attr.ReleaseWriter(ref ar);
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
        }
        public static bool ReloadXnd(UMaterialInstance material, UMaterialInstanceManager manager, IO.CXndNode node)
        {
            var attr = node.TryGetAttribute("MaterialInstance");
            if (attr.NativePointer != IntPtr.Zero)
            {
                var ar = attr.GetReader(null);
                try
                {
                    ar.ReadTo(material, null);
                    material.SerialId++;
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                attr.ReleaseReader(ref ar);
            }
            return true;
        }
        public static UMaterialInstance LoadXnd(UMaterialInstanceManager manager, IO.CXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("MaterialInstance");
            if (attr.NativePointer != IntPtr.Zero)
            {
                var ar = attr.GetReader(null);
                try
                {
                    ar.Read(out result, null);
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                attr.ReleaseReader(ref ar);
            }

            var material = result as UMaterialInstance;
            if (material != null)
            {
                return material;
            }
            return null;
        }
        #endregion        
        public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public RName MaterialName { get; set; }
        }
        [Rtti.Meta(Order = 1)]
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.MaterialName = MaterialName;
                return tmp;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                System.Action exec = async () =>
                {
                    ParentMaterial = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value.MaterialName);
                    bool isMatch = true;
                    #region UniformVars match parent
                    if (mUsedUniformVars.Count != ParentMaterial.UsedUniformVars.Count)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < mUsedUniformVars.Count; i++)
                        {
                            if (mUsedUniformVars[i].Name != ParentMaterial.UsedUniformVars[i].Name ||
                                    mUsedUniformVars[i].VarType != ParentMaterial.UsedUniformVars[i].VarType)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    if (isMatch == false)
                    {
                        var uniformVars = new List<NameValuePair>();
                        for (int i = 0; i < ParentMaterial.UsedUniformVars.Count; i++)
                        {
                            var uuv = this.FindVar(ParentMaterial.UsedUniformVars[i].Name);
                            if (uuv != null && uuv.VarType == ParentMaterial.UsedUniformVars[i].VarType)
                            {
                                uniformVars.Add(uuv);
                            }
                            else
                            {
                                uniformVars.Add(ParentMaterial.UsedUniformVars[i]);
                            }
                        }
                        mUsedUniformVars = uniformVars;
                    }
                    #endregion

                    #region SRV match parent
                    isMatch = true;
                    if (mUsedRSView.Count != ParentMaterial.UsedRSView.Count)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < mUsedRSView.Count; i++)
                        {
                            if (mUsedRSView[i].Name != ParentMaterial.UsedRSView[i].Name)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    if (isMatch == false)
                    {
                        var srvs = new List<NameRNamePair>();
                        for (int i = 0; i < ParentMaterial.UsedRSView.Count; i++)
                        {
                            var uuv = this.FindSRV(ParentMaterial.UsedRSView[i].Name);
                            if (uuv != null)
                            {
                                srvs.Add(uuv);
                            }
                            else
                            {
                                srvs.Add(ParentMaterial.UsedRSView[i]);
                            }
                        }
                        mUsedRSView = srvs;
                    }
                    #endregion
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
                
            }
        }
        [RName.PGRName(FilterExts = UMaterial.AssetExt)]
        public RName MaterialName
        {
            get
            {
                if (ParentMaterial == null)
                    return null;
                return ParentMaterial.AssetName;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                System.Action exec = async () =>
                {
                    ParentMaterial = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value);
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
            }
        }
        UMaterial mParentMaterial;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public override UMaterial ParentMaterial
        {
            get => mParentMaterial;
            protected set
            {
                mParentMaterial = value;
                if (value != null)
                    ParentMaterialSerialId = value.SerialId;
            }
        }
        public override Hash160 MaterialHash
        {
            get
            {
                if (ParentMaterial == null)
                    return Hash160.Emtpy;
                return ParentMaterial.MaterialHash;
            }
        }
        internal uint ParentMaterialSerialId;
        public override uint SerialId
        {
            get 
            {
                if (ParentMaterial != null && ParentMaterialSerialId != ParentMaterial.SerialId)
                {
                    ParentMaterialSerialId = ParentMaterial.SerialId;
                    mSerialId++;
                    this.SaveAssetTo(this.AssetName);
                }
                return mSerialId;
            }
            set
            {
                mSerialId = value;
            }
        }
        #region RHIResource        
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public override RHI.CRasterizerState RasterizerState
        {
            get
            {
                if (ParentMaterial != null && mRasterizerState == null)
                    return ParentMaterial.RasterizerState;
                return mRasterizerState;
            }
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public override RHI.CDepthStencilState DepthStencilState
        {
            get
            {
                if (ParentMaterial != null && mDepthStencilState == null)
                    return ParentMaterial.DepthStencilState;
                return mDepthStencilState;
            }
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public override RHI.CBlendState BlendState
        {
            get
            {
                if (ParentMaterial != null && mBlendState == null)
                    return ParentMaterial.BlendState;
                return mBlendState;
            }
        }
        #endregion
    }
    public class UMaterialInstanceManager
    {
        public void Cleanup()
        {
            Materials.Clear();
        }
        public Dictionary<RName, UMaterialInstance> Materials { get; } = new Dictionary<RName, UMaterialInstance>();
        public UMaterialInstance FindMaterialInstance(RName rn)
        {
            if (rn == null)
                return null;

            UMaterialInstance result;
            if (Materials.TryGetValue(rn, out result))
                return result;
            return null;
        }
        public async System.Threading.Tasks.Task<UMaterialInstance> CreateMaterialInstance(RName rn)
        {
            UMaterialInstance result;
            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UMaterialInstance.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return result;
        }
        public async System.Threading.Tasks.Task<bool> ReloadMaterialInstance(RName rn)
        {
            UMaterialInstance result;
            if (Materials.TryGetValue(rn, out result)==false)
                return true;

            var ok = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return UMaterialInstance.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return ok;
        }
        public async System.Threading.Tasks.Task<UMaterialInstance> GetMaterialInstance(RName rn)
        {
            if (rn == null)
                return null;

            UMaterialInstance result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UMaterialInstance.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                Materials[rn] = result;
                return result;
            }

            return null;
        }
    }
}
