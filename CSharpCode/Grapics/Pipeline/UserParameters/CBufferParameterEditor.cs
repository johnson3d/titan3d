using EngineNS.Editor;
using System;

namespace EngineNS.Graphics.Pipeline.UserParameters
{
    public class TtCBufferParameterEditor : IAssetEditor, IRootForm
    {
        public RName AssetName { get; set; }
        public bool Visible { get; set; }
        public uint DockId { get; set; }
        ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public TtCBufferParameterEditor()
        {
        }
        ~TtCBufferParameterEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            CBufferParameter = null;
            mParamPropGrid.Target = null;
            mShaderEditor = null;
        }

        #region IAssetEditor
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";

        public IRootForm GetRootForm()
        {
            return this;
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await mParamPropGrid.Initialize();
            mShaderEditor = new EGui.TtCodeEditor();
            mShaderEditor.mCoreObject.SetLanguage("HLSL");
            mShaderEditor.mCoreObject.ApplyLangDefine();
            mShaderEditor.mCoreObject.SetReadOnly(true);
            return true;
        }

        public async Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg, bool saveLayout)
        {
            CBufferParameter = await name.GetAsset<TtCBufferParameter>();
            AssetName = name;

            if (CBufferParameter != null)
            {
                mParamPropGrid.Target = CBufferParameter;
                mShaderEditor.mCoreObject.SetText(CBufferParameter.HLSLCode ?? "");
            }

            Visible = true;
            return true;
        }

        public void OnCloseEditor()
        {
            Dispose();
        }

        public void OnEvent(in Bricks.Input.Event e)
        {
        }
        #endregion

        TtCBufferParameter CBufferParameter;

        EGui.TtCodeEditor mShaderEditor;
        EGui.Controls.PropertyGrid.TtPropertyGrid mParamPropGrid = new EGui.Controls.PropertyGrid.TtPropertyGrid();

        bool mDockInitialized = false;
        bool mLeftShow = true;
        bool mRightShow = true;

        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);

        public string GetWindowsName()
        {
            return AssetName?.Name ?? "CBufferParameter";
        }

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
            uint leftId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.35f, ref leftId, ref rightId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Parameters", mDockKeyClass), leftId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("HLSLCode", mDockKeyClass), rightId);

            ImGuiAPI.DockBuilderFinish(id);
        }

        public unsafe void OnDraw()
        {
            if (Visible == false || CBufferParameter == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (result)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as TtMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(result);

            DrawParameters();
            DrawShaderCode();
        }

        protected unsafe void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                CBufferParameter.SaveAssetTo(AssetName);
                mShaderEditor.mCoreObject.SetText(CBufferParameter.HLSLCode ?? "");
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Refresh", in btSize))
            {
                CBufferParameter.UpdateHLSLCode();
                CBufferParameter.UpdateMethodMeta();
                mShaderEditor.mCoreObject.SetText(CBufferParameter.HLSLCode ?? "");
            }
        }

        protected unsafe void DrawParameters()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Parameters", ref mLeftShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (ImGuiAPI.CollapsingHeader("ParameterDefinitions", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    mParamPropGrid.OnDraw(true, false, false);
                }
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }

        protected unsafe void DrawShaderCode()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "HLSLCode", ref mRightShow, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                mShaderEditor.mCoreObject.Render("##cbshadercode", in Vector2.Zero, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
    }

    [Editor.TtAssetEditor(EditorType = typeof(TtCBufferParameterEditor))]
    public partial class TtCBufferParameter
    {
    }
}