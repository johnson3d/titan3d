using EngineNS.Algorithm;
using Mono.CompilerServices.SymbolWriter;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    [Rtti.Meta]
    public class TtDataSetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtDataSet.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //物理材质不会引用别的资产
            return false;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterial(GetAssetName());
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            TtEngine.Instance.EditorInstance.PhyMaterialIcon?.OnDraw(cmdlist, in start, in end, 0);
            //cmdlist.AddText(in start, 0xFFFFFFFF, "PhyMtl", null);
        }
        public override string GetAssetTypeName()
        {
            return "DataSet";
        }
    }

    [TtDataSet.TtDataSetImport]
    [IO.AssetCreateMenu(MenuName = "DataSet")]
    public partial class TtDataSet : IO.IAsset
    {
        public const string AssetExt = ".dataset";
        public string TypeExt { get => AssetExt; }
        public class TtDataSetImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = Rtti.TtTypeDescGetter<TtDataProvider>.TypeDesc;
                TypeSlt.SelectedType = TypeSlt.BaseType;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = new TtDataSet();
                PGAsset.Target = mAsset;
            }
            string mSourceFile;
            bool bFileExisting = false;
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import DataSet", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                bool retValue = false;
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Import DataSet", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var saved = TypeSlt.SelectedType;
                    TypeSlt.OnDraw(-1, 6);
                    if (TypeSlt.SelectedType != saved)
                    {
                        var dataset = mAsset as TtDataSet;
                        dataset.DataType = TypeSlt.SelectedType;
                    }
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select XLS", in sz))
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".xlsx", ".");
                    }
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            mSourceFile = mFileDialog.GetFilePathName();
                            mName = IO.TtFileManager.GetPureName(mSourceFile);
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }

                    if (bFileExisting)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();
                    using (var buffer = BigStackBuffer.CreateInstance(128))
                    {
                        buffer.SetTextUtf8(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsTextUtf8();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.TtFileManager.FileExists(mDir.Address + mName + TtDataSet.AssetExt);
                        }
                    }

                    var btSz = Vector2.Zero;
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", in btSz))
                        {
                            if (ImportDataSet())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in btSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.EndPopup();
                }
                if (!visible)
                    retValue = true;
                return retValue;
            }

            public bool ImportDataSet()
            {
                var dataset = mAsset as TtDataSet;
                var rn = GetAssetRName();
                IO.TtFileManager.CopyFile(mSourceFile, rn.Address + ".xlsx", true);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address + ".xlsx", true);
                dataset.LoadDataSet(rn, dataset.DataType.SystemType);
                dataset.SaveAssetTo(rn);

                var ameta = new TtDataSetAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtDataSet));
                ameta.Description = $"This is a {typeof(TtDataSet).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                return true;
            }
            protected override bool CheckAsset()
            {
                return true;
            }
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtDataSetAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            var savexnd = new IO.TtXndHolder(DataType.TypeString, 0, 0);
            SaveDataSetToXnd(savexnd.RootNode, XlsMd5);
            savexnd.SaveXnd(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        [Rtti.Meta]
        [RName.PGRName(ReadOnly = true)]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public TtDataProviderBinderManager BinderManager = new TtDataProviderBinderManager();
        public Rtti.TtTypeDesc DataType { get; set; }
        public byte[] XlsMd5;
        public Dictionary<string, TtTable> Tables { get; } = new Dictionary<string, TtTable>();
        public TtTable MainTable;
        private static bool IsEqual(byte[] lh, byte[] rh)
        {
            if (lh == null || rh == null)
                return false;
            if (lh.Length != rh.Length)
                return false;

            for (int i = 0; i < lh.Length; i++)
            {
                if (lh[i] != rh[i])
                    return false;
            }
            return true;
        }
        public bool LoadDataSet(RName name, Type objType)
        {
            if (IO.TtFileManager.FileExists(name.Address + ".xlsx"))
            {
                bool bSaveXnd = false;
                XlsMd5 = IO.TtFileManager.GetMD5HashFromFile(name.Address + ".xlsx").Hash;
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var attr = xnd.RootNode.TryGetAttribute("Desc");
                        if (attr.IsValidPointer)
                        {
                            using (var ar = attr.GetReader(attr))
                            {
                                byte[] hash;
                                ar.Read(out hash);
                                if (IsEqual(hash, XlsMd5))
                                {
                                    LoadDataSetFromXnd(xnd.RootNode);
                                    return true;
                                }
                            }
                        }
                        bSaveXnd = true;
                    }
                    else
                    {
                        bSaveXnd = true;
                    }
                }   
                bool isOk = false;
                LoadDataSet_Exel(ref isOk, name.Address + ".xlsx", objType);
                if (isOk)
                {
                    if (bSaveXnd)
                    {
                        var savexnd = new IO.TtXndHolder(Rtti.TtTypeDesc.TypeOf(objType).TypeString, 0, 0);
                        SaveDataSetToXnd(savexnd.RootNode, XlsMd5);
                        savexnd.SaveXnd(name.Address);
                        TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
                    }
                    return true;
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"{name.Address}.xlsx load failed, Is it opened?");
                }
            }
            else
            {
                var xnd = IO.TtXndHolder.LoadXnd(name.Address);
                if (xnd != null)
                {
                    var attr = xnd.RootNode.TryGetAttribute("Desc");
                    if (attr.IsValidPointer)
                    {
                        using (var ar = attr.GetReader(null))
                        {
                            ar.Read(out XlsMd5);
                        }
                            
                        LoadDataSetFromXnd(xnd.RootNode);
                        return true;
                    }
                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"{name.Address} load failed");
                }
            }
            return false;
        }
        public bool LoadDataSetFromXnd(IO.TtXndNode node)
        {
            Tables.Clear();
            var cd = node.TryGetChildNode("Tables");
            if (cd.IsValidPointer == false)
                return false;
            for (uint i = 0; i < cd.GetNumOfAttribute(); i++)
            {
                var tab = cd.GetAttribute(i);
                using (var ar = tab.GetReader(null))
                {
                    TtTable obj;
                    ar.ReadObject(out obj);
                    Tables.Add(tab.Name, obj);
                }
            }
            return true;
        }
        public void SaveDataSetToXnd(IO.TtXndNode node, byte[] md5Hash)
        {
            var attr = node.GetOrAddAttribute("Desc", 0, 0);
            using (var ar = attr.GetWriter(20))
            {
                ar.Write(md5Hash);
            }
            var tables = node.GetOrAddNode("Tables", 0, 0, true);
            foreach (var i in Tables)
            {
                attr = tables.GetOrAddAttribute(i.Key, 0, 0, true);
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(i.Value);
                }
            }
        }
        public bool LoadFromDatabase(Type objType)
        {
            return false;
        }
        partial void LoadDataSet_Exel(ref bool isOk, string name, Type objType);
        public TtTable GetTable(string name)
        {
            TtTable result;
            if (Tables.TryGetValue(name, out result))
                return result;
            return null;
        }
        private void CheckSheetLinks()
        {
            if (MainTable == null)
                return;
            for (int i = 0; i < MainTable.Count; i++)
            {
                MainTable.CheckSheetLinks(this, i);
            }
        }
    }


    public class TtDataManager<T> : TtDataSet where T : TtDataProvider
    {
        public TtDataManager()
        {
            DataType = Rtti.TtTypeDescGetter<T>.TypeDesc;
        }

        public bool LoadDataSet(RName name)
        {
            return LoadDataSet(name, typeof(T));
        }
        public T GetData(int index)
        {
            return MainTable.GetData(index) as T;
        }
    }
}
