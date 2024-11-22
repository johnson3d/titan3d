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
        Thread.Async.TtTask<bool> OpenEditor(TtMainEditorApplication mainEditor, RName name, object arg);
        void OnCloseEditor();
        RName AssetName { get; set; }
        bool Visible { get; set; }
        void OnDraw();
        void OnEvent(in Bricks.Input.Event e);
        IRootForm GetRootForm();
        Thread.Async.TtTask<bool> Initialize();
        string GetWindowsName();
    }
    public class UAssetEditorManager
    {
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return true;
        }
        public List<IAssetEditor> OpenedEditors { get; } = new List<IAssetEditor>();
        public IAssetEditor CurrentActiveEditor = null;

        public async System.Threading.Tasks.Task OpenEditor(TtMainEditorApplication mainEditor, Type editorType, RName name, object arg)
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
                editor = Rtti.TtTypeDescManager.CreateInstance(editorType) as IAssetEditor;
            }
            await OpenEditor(mainEditor, editor, name, arg);
        }
        public async System.Threading.Tasks.Task OpenEditor(TtMainEditorApplication mainEditor, IAssetEditor editor, RName name, object arg)
        {
            editor.AssetName = name;
            editor.Visible = true;
            TtEngine.Instance.StopOperation($"{name}: OpenEditor");

            bool ok = false;
            try
            {
                if (await editor.Initialize() == false)
                    return;
                ok = await editor.OpenEditor(mainEditor, name, arg);
                TtMainEditorApplication.NeedFocusWindowName = editor.GetWindowsName();
            }
            catch (Exception exp)
            {
                Profiler.Log.WriteException(exp);
                TtEngine.Instance.ResumeOperation();
                return;
            }
            TtEngine.Instance.ResumeOperation();
            if (ok == false)
            {
                Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, "Editor", $"AssetEditor {name} open failed");
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
                        var application = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
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
            for (int i = 0; i < OpenedEditors.Count; i++)
            {
                if (OpenedEditors[i].Visible == false)
                {
                    if (CurrentActiveEditor == OpenedEditors[i])
                    {
                        CurrentActiveEditor = null;
                    }
                    OpenedEditors[i].OnCloseEditor();
                    OpenedEditors.RemoveAt(i);
                    i--;

                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
                }
                else
                {
                    OpenedEditors[i].OnDraw();
                }
            }
        }

        
    }
}

