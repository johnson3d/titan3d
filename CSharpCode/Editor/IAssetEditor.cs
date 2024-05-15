using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class UAssetEditorAttribute : Attribute
    {
        public Type EditorType;
    }
    public interface IProgressBar
    {
        float LoadingPercent { get; set; }
        string ProgressText { get; set; }
    }
    public interface IAssetEditor : IProgressBar
    {
        System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg);
        void OnCloseEditor();
        RName AssetName { get; set; }
        bool Visible { get; set; }
        void OnDraw();
        void OnEvent(in Bricks.Input.Event e);
        IRootForm GetRootForm();
        System.Threading.Tasks.Task<bool> Initialize();
    }
    public class TtAssetEditorOpenProgress : IRootForm
    {
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose() { }
        bool mVisible = true;
        public bool Visible
        {
            get => mVisible;
            set => mVisible = value;
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public IAssetEditor CurrentEditor;
        public void OnDraw()
        {
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm($"Progress: Open {CurrentEditor.AssetName}", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                ImGuiAPI.Button($"Loading:{CurrentEditor.LoadingPercent}->{CurrentEditor.ProgressText}");
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
    public class UAssetEditorManager
    {
        TtAssetEditorOpenProgress mAssetEditorOpenProgress = null;
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            mAssetEditorOpenProgress = new TtAssetEditorOpenProgress();
            await mAssetEditorOpenProgress.Initialize();
            return true;
        }
        public List<IAssetEditor> OpenedEditors { get; } = new List<IAssetEditor>();
        public IAssetEditor CurrentActiveEditor = null;
        public async System.Threading.Tasks.Task OpenEditor(UMainEditorApplication mainEditor, Type editorType, RName name, object arg)
        {
            IAssetEditor editor = null;
            foreach(var i in OpenedEditors)
            {
                if (i.AssetName == name)
                {
                    editor = i;
                    return;
                }
            }
            if (editor == null)
            {
                editor = Rtti.UTypeDescManager.CreateInstance(editorType) as IAssetEditor;
            }
            editor.AssetName = name;
            mAssetEditorOpenProgress.CurrentEditor = editor;
            this.ShowTopMost(mAssetEditorOpenProgress);

            bool ok = false;
            try
            {
                if (await editor.Initialize() == false)
                    return;
                ok = await editor.OpenEditor(mainEditor, name, arg);
            }
            catch (Exception exp)
            {
                Profiler.Log.WriteException(exp);
                mAssetEditorOpenProgress.Visible = false;
                mAssetEditorOpenProgress.CurrentEditor = null;
                return;
            }
            mAssetEditorOpenProgress.Visible = false;
            mAssetEditorOpenProgress.CurrentEditor = null;
            if (ok == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Editor", $"AssetEditor {name} open failed");
            }
            else
            {
                OpenedEditors.Add(editor);
                var form = editor.GetRootForm();
                if (form != null)
                {
                    if (CurrentActiveEditor != null && CurrentActiveEditor.GetRootForm() != null)
                    {
                        form.DockId = CurrentActiveEditor.GetRootForm().DockId;
                        form.DockCond = ImGuiCond_.ImGuiCond_Appearing;
                    }
                    else
                    {
                        var application = UEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.UMainEditorApplication;
                        form.DockId = EGui.UIProxy.DockProxy.MainFormDockClass.ClassId;
                        form.DockCond = ImGuiCond_.ImGuiCond_Appearing;
                    }
                }

                if (CurrentActiveEditor == null)
                    CurrentActiveEditor = editor;
            }
        }
        public void CloseAll()
        {
            for (int i = 0; i < OpenedEditors.Count; i++)
            {
                OpenedEditors[i].OnCloseEditor();
            }
            OpenedEditors.Clear();
            System.GC.Collect();
        }
        public void OnDraw()
        {
            var wr = OnDrawImpl();

            if (wr != null)
            {
                int count = 5;
                while (wr.IsAlive && count>0)
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    count--;
                }
            }
        }
        private WeakReference OnDrawImpl()
        {
            WeakReference wr = null;
            for (int i = 0; i < OpenedEditors.Count; i++)
            {
                if (wr == null && OpenedEditors[i].Visible == false)
                {
                    if (CurrentActiveEditor == OpenedEditors[i])
                    {
                        CurrentActiveEditor = null;
                    }
                    wr = new WeakReference(OpenedEditors[i]);
                    OpenedEditors[i].OnCloseEditor();
                    OpenedEditors.RemoveAt(i);
                    i--;
                    System.GC.Collect();
                }
                else
                {
                    OpenedEditors[i].OnDraw();
                }
            }
            return wr;
        }

        private List<IRootForm> TopMostForms { get; } = new List<IRootForm>();
        public void ShowTopMost(IRootForm form)
        {
            if (TopMostForms.Contains(form))
                return;
            form.Visible = true;
            TopMostForms.Add(form);
        }
        public void OnDrawTopMost()
        {
            for (int i = 0; i < TopMostForms.Count; i++)
            {
                if (TopMostForms[i].Visible)
                {
                    TopMostForms[i].OnDraw();
                }
                else
                {
                    TopMostForms.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}

