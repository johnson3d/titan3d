using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class TtShaderAssetAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtShaderAsset.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Nebula";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return TtShaderAsset.LoadAsset(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        [Rtti.Meta]
        public string ShaderType { get; set; }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = TtShaderAsset.AssetExt)]
        public RName TemplateName
        {
            get;
            set;
        }
    }

    [TtShaderAsset.TtShaderAssetImport]
    [IO.AssetCreateMenu(MenuName = "FX/Shader")]
    [EngineNS.Editor.UAssetEditor(EditorType = typeof(TtShaderAssetEditor))]
    public class TtShaderAsset : IO.IAsset
    {
        public const string AssetExt = ".shader";
        public string TypeExt { get => AssetExt; }
        public class TtShaderAssetImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as TtShaderAsset;
                if (material == null)
                    return false;

                return true;
            }
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = new TtShaderAsset() { ShaderCode = "//This is a ShaderAsset"};
                PGAsset.Target = this;
            }
            protected override bool DoImportAsset()
            {
                base.DoImportAsset();

                var ameta = mAsset.GetAMeta() as TtShaderAssetAMeta;
                ameta.ShaderType = ShaderType;
                if (TemplateName != null)
                {
                    ameta.TemplateName = TemplateName;
                    var templateAMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(TemplateName) as TtShaderAssetAMeta;
                    ameta.ShaderType = templateAMeta.ShaderType;

                    var shader = (mAsset as TtShaderAsset);
                    shader.ShaderCode = IO.TtFileManager.ReadAllText(TemplateName.Address);
                }
                mAsset.SaveAssetTo(mAsset.AssetName);

                return true;
            }
            [Category("Option")]
            public string ShaderType
            {
                get;
                set;
            }
            [Category("Option")]
            [RName.PGRName(FilterExts = AssetExt)]
            public RName TemplateName
            {
                get;
                set;
            }
        }
        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtShaderAssetAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
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
            if (ShaderCode != null)
            {
                IO.TtFileManager.WriteAllText(name.Address, ShaderCode);
            }
            TtEngine.Instance.SourceControlModule.AddFile(name.Address, true);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        public static TtShaderAsset LoadAsset(RName name)
        {
            TtShaderAsset result = new TtShaderAsset();
            result.AssetName = name;
            result.ShaderCode = result.GetShaderCode();
            return result;
        }

        [RName.PGRName(FilterExts = AssetExt)]
        public RName TemplateName
        {
            get 
            {
                var ameta = (GetAMeta() as TtShaderAssetAMeta);
                if (ameta != null)
                    return ameta.TemplateName;
                return null;
            }
        }
        public string ShaderType
        {
            get
            {
                var ameta = (GetAMeta() as TtShaderAssetAMeta);
                if (ameta != null)
                    return ameta.ShaderType;
                return null;
            }
        }
        public string ShaderCode { get; set; } = null;
        public string GetShaderCode()
        {
            return IO.TtFileManager.ReadAllText(AssetName.Address);
        }
    }
    public partial class TtShaderAssetEditor : EngineNS.Editor.IAssetEditor, IRootForm
    {
        #region IAssetEditor
        public RName AssetName { get; set; }
        public EGui.Controls.PropertyGrid.PropertyGrid AssetPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public float LeftWidth = 0;
        public TtShaderAsset ShaderAsset;
        bool IsStarting = false;
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        public async Thread.Async.TtTask<bool> OpenEditor(EngineNS.Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            IsStarting = true;

            AssetName = name;
            IsStarting = false;

            await AssetPropGrid.Initialize();

            ShaderAsset = TtShaderAsset.LoadAsset(name);

            mShaderEditor.mCoreObject.ApplyLangDefine();
            mShaderEditor.mCoreObject.ApplyErrorMarkers();
            mShaderEditor.mCoreObject.SetText(ShaderAsset.ShaderCode);

            AssetPropGrid.Target = ShaderAsset;
            return true;
        }
        public void OnCloseEditor()
        {
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //PreviewViewport.OnEvent(ref e);
        }
        #endregion

        public void Dispose()
        {
            
        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public IRootForm GetRootForm()
        {
            return this;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
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
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawNodeDetails();
            DrawTextEditor();
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

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextEditor", mDockKeyClass), middleId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                var blob = new Support.TtBlobObject();
                mShaderEditor.mCoreObject.GetText(blob.mCoreObject);
                ShaderAsset.ShaderCode = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)blob.mCoreObject.GetData(), (int)blob.mCoreObject.GetSize());
                ShaderAsset.SaveAssetTo(AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Undo", in btSize))
            {
                mShaderEditor.mCoreObject.Undo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Redo", in btSize))
            {
                mShaderEditor.mCoreObject.Redo();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Copy", in btSize))
            {
                mShaderEditor.mCoreObject.Copy();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Paste", in btSize))
            {
                mShaderEditor.mCoreObject.Paste();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Snapshot", in btSize))
            {
                var presentWindow = ImGuiAPI.GetWindowViewportData();
                Editor.USnapshot.Save(AssetName, ShaderAsset.GetAMeta(), presentWindow.SwapChain.mCoreObject.GetBackBuffer(0),
                            (uint)DrawOffset.X, (uint)DrawOffset.Y, (uint)GraphSize.X, (uint)GraphSize.Y, Editor.USnapshot.ESnapSide.Left);
            }
        }

        bool ShowMeshPropGrid = true;
        protected void DrawNodeDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref ShowMeshPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                AssetPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        EGui.TtCodeEditor mShaderEditor = new EGui.TtCodeEditor();
        Vector2 DrawOffset;
        protected Vector2 GraphSize;
        bool ShowTextEditor = true;
        protected void DrawTextEditor()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextEditor", ref ShowTextEditor, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                GraphSize = sz;
                mShaderEditor.mCoreObject.Render(AssetName.Name, in Vector2.Zero, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        #endregion
    }

    [Rtti.Meta]
    public partial class TtMacrossShaderUtility
    {
        [Rtti.Meta(ShaderName = "InterlockedAdd")]
        public static void InterlockedAddUInt32(ref uint location1, uint value, out uint oriValue)
        {
            oriValue = System.Threading.Interlocked.Add(ref location1, value);
        }
        [Rtti.Meta(ShaderName = "InterlockedAdd")]
        public static void InterlockedAddInt32(ref int location1, int value, out int oriValue)
        {
            oriValue = System.Threading.Interlocked.Add(ref location1, value);
        }
    }
}
