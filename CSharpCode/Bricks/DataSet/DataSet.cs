using EngineNS.Algorithm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EngineNS.Bricks.DataSet
{
    [Rtti.Meta("")]
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
        public override async Thread.Async.TtTask<IO.IAsset> GetAsset(params object[] args)
        {
            return null;
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
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            DeleteFile(address + ".xlsx");

            base.DeleteAsset(name, type);
        }
        [Rtti.Meta("")]
        public Rtti.TtTypeDesc DataType { get; set; }
    }

    [TtDataSet.TtDataSetImport]
    [IO.AssetCreateMenu(MenuName = "DataSet")]
    [Editor.TtAssetEditor(EditorType = typeof(TtDataSetEditor))]
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
                EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
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
                EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
                if (!visible)
                    retValue = true;
                return retValue;
            }

            public bool ImportDataSet()
            {
                var dataset = mAsset as TtDataSet;
                var rn = GetAssetRName();
                IO.TtFileManager.CopyFile(mSourceFile, rn.Address + ".xlsx", true);
                rn.AMeta.AddAssetFile(rn.Address + ".xlsx");
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address + ".xlsx", true);
                dataset.LoadDataSet(rn, dataset.DataType.SystemType);
                dataset.SaveAssetTo(rn);

                var ameta = new TtDataSetAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = IO.IAssetMeta.AcquireAssetId(rn);
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtDataSet)).TypeString;
                ameta.Description = $"This is a {typeof(TtDataSet).FullName}\n";
                ameta.DataType = dataset.DataType;
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
            name.AMeta.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        [Rtti.Meta("")]
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
        public System.Reflection.PropertyInfo FindPropByHeadName(string name)
        {
            var props = this.DataType.GetProperties();
            foreach(var prop in props)
            {
                var attr = prop.GetCustomAttribute<TtDataColumnAttribute>();
                if (attr != null)
                {
                    if (attr.HeadName == name)
                        return prop;
                }
            }
            return null;
        }
        public bool LoadDataSet(RName name, Type objType)
        {
            var internalResult = InternalLoadDataSet(name, objType);
            if(internalResult)
            {
                var attr = DataType.SystemType.GetCustomAttribute<TtDataTableAttribute>();
                if (Tables.TryGetValue(attr.SheetName, out MainTable) == false)
                    return false;
            }
            return internalResult;
        }
        public bool InternalLoadDataSet(RName name, Type objType)
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
                            byte[] hash;
                            string SheetName;
                            int HeadRow;
                            int DataStartRow;
                            using (var ar = attr.GetReader(attr))
                            {
                                ar.Read(out hash);
                                ar.Read(out SheetName);
                                ar.Read(out HeadRow);
                                ar.Read(out DataStartRow);
                            }
                            var dtAttr = objType.GetCustomAttribute<TtDataTableAttribute>();
                            if (IsEqual(hash, XlsMd5) && dtAttr.SheetName == SheetName && dtAttr.HeadRow == HeadRow && dtAttr.DataStartRow == DataStartRow)
                            {
                                LoadDataSetFromXnd(xnd.RootNode);
                                return true;
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
                        name.AMeta.AddAssetFile(name.Address);
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
                var dtAttr = DataType.SystemType.GetCustomAttribute<TtDataTableAttribute>();
                ar.Write(dtAttr.SheetName);
                ar.Write(dtAttr.HeadRow);
                ar.Write(dtAttr.DataStartRow);
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
        public bool SaveDataSetToExcel(string name)
        {
            bool ok = false;
            SaveToExcel(ref ok, name);
            return ok;
        }
        partial void SaveToExcel(ref bool isOk, string filepath);
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

        public T GetData(string propName, object key,
            bool bSorted = true,
            [Rtti.MetaParameter(FilterType = typeof(TtDataProvider), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type type = null)
        {
            if (MainTable == null)
                return null;

            var Result = MainTable.FindByKey(propName, key, bSorted, type) as T;
            if (Result == null)
            {
                Profiler.Log.WriteLine<Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, $"GetData({propName},{key}) not found");
            }
            return Result;
        }
    }

    public class TtDataSetEditor : Editor.IAssetEditor, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public RName AssetName { get; set; }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Controls.PropertyGrid.TtPropertyGrid DataPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();        

        #region 统一Undo/Redo(开门)
        // 控制门就是是否new出历史栈: 需回退旧流程时把mEditorHistory改为null即可
        public bool EnableUndoRedo => EditorHistory != null;
        public Editor.Infrastructure.TtEditorHistory EditorHistory => mEditorHistory;
        Editor.Infrastructure.TtEditorHistory mEditorHistory = new Editor.Infrastructure.TtEditorHistory();
        Editor.Infrastructure.TtEditorHistoryPanel mHistoryPanel = new Editor.Infrastructure.TtEditorHistoryPanel();
        #endregion

        ~TtDataSetEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            DataPropGrid.Target = null;
            DataPropGrid.HistoryHost = null;
            mEditorHistory?.Clear();
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await DataPropGrid.Initialize();
            return true;
        }
        public IRootForm GetRootForm()
        {
            return this;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        public TtDataSet DataSet;
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            AssetName = name;
            var ameta = name.AMeta as TtDataSetAMeta;
            if (ameta == null)
                return false;
            DataSet = new TtDataSet();
            DataSet.DataType = ameta.DataType;
            DataSet.LoadDataSet(name, ameta.DataType.SystemType);
            mEditorHistory?.Clear();
            DataPropGrid.HistoryHost = mEditorHistory;
            return true;
        }
        public void OnCloseEditor()
        {
            Dispose();
        }
        #region DrawUI
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawDataSets();
            DrawData();
            if (mEditorHistory != null)
            {
                mHistoryPanel.OnDraw(in mDockKeyClass, "History", mEditorHistory);
            }
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("DataSets", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("History", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Data", mDockKeyClass), middleId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save XLSX", in btSize))
            {
                DataSet.SaveDataSetToExcel(AssetName.Address + ".xlsx");
                mEditorHistory?.SetSavePoint();
            }
            ImGuiAPI.SameLine(0, -1);
            // mEditorHistory为null时按钮/快捷键均为空操作
            Editor.Infrastructure.EditorUndoUtils.DrawUndoRedoButtons(mEditorHistory);
            Editor.Infrastructure.EditorUndoUtils.HandleUndoShortcut(mEditorHistory);
            ImGuiAPI.SameLine(0, -1);
        }
        //bool ShowDataSets = true;
        protected void DrawDataSets()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "DataSets", ref ShowDataPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var attr = DataSet.DataType.SystemType.GetCustomAttribute<TtDataTableAttribute>();
                var prop = DataSet.FindPropByHeadName(attr.KeyName);
                int n = 0;
                foreach (var i in DataSet.MainTable.DataProviders)
                {
                    if (i==null)
                        continue;
                    if (prop != null)
                        ImGuiAPI.Text($"{prop.GetValue(i)}");
                    else
                        ImGuiAPI.Text($"{n}");
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        DataPropGrid.Target = i;
                    }
                    n++;
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowDataPropGrid = true;
        protected void DrawData()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Data", ref ShowDataPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                DataPropGrid.OnDraw(true, false, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoScrollbar,
                    ImGuiChildFlags_.ImGuiChildFlags_AlwaysAutoResize | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeX | ImGuiChildFlags_.ImGuiChildFlags_AutoResizeY);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        #endregion

        public void OnEvent(in Bricks.Input.Event e)
        {

        }
    }
}
