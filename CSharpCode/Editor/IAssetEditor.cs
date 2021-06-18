using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class UAssetEditorAttribute : Attribute
    {
        public Type EditorType;
    }
    public interface IAssetEditor
    {
        System.Threading.Tasks.Task<bool> OpenEditor(UMainEditorApplication mainEditor, RName name, object arg);
        void OnCloseEditor();
        RName AssetName { get; set; }
        bool Visible { get; set; }
        void OnDraw();
        void OnEvent(ref SDL2.SDL.SDL_Event e);
        Graphics.Pipeline.IRootForm GetRootForm();
    }
    public class UAssetEditorManager
    {
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
                    break;
                }
            }
            if (editor == null)
            {
                editor = Rtti.UTypeDescManager.CreateInstance(editorType) as IAssetEditor;
                editor.AssetName = name;
                OpenedEditors.Add(editor);
            }
            var ok = await editor.OpenEditor(mainEditor, name, arg);
            if (ok == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Editor", $"AssetEditor {name} open failed");
            }
            else
            {
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
                        var application = UEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
                        form.DockId = application.CenterDockId;
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
        }
    }
}

