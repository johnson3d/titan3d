using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace EditorCommon.PluginAssist
{
    public delegate void Delegate_OnSaveObject(object[] obj);
    // 第一个参数为编辑器类型名称（string），其余参数为编辑器设置所需参数
    public delegate System.Threading.Tasks.Task Delegate_OnOpenEditor(EditorCommon.Resources.ResourceEditorContext context);

    public interface IRefreshSaveFiles
    {
        System.Threading.Tasks.Task RefreshSaveFiles();
    }

    public interface IUsbDeviceChangelistener
    {
        void UsbDeviceChanged();
    }

    public interface IObjectEditorOperation
    {
        event Delegate_OnOpenEditor OnOpenEditor;
    }

    public interface IEditorDockManagerDragDropOperation
    {
        void StartDrag();
        void EndDrag();
    }
    /// <summary>
    /// 插件关闭时的处理
    /// </summary>
    public interface IEditorCloseAction
    {
        void OnClose();
    }

    public interface IEditorPlugin : DockControl.IDockAbleControl
    {
        string PluginName { get; }
        string Version { get; }      

        string Title { get; }
        ImageSource Icon { get; }
        Brush IconBrush { get; }
                
        System.Windows.UIElement InstructionControl { get; }

        bool OnActive();
        bool OnDeactive();
        
        /// <summary>
        /// 向编辑器设置指定的对象以供编辑
        /// </summary>
        /// <param name="obj"></param>
        System.Threading.Tasks.Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context);
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EditorPluginAttribute : ExportAttribute
    {
        public EditorPluginAttribute()
            : base(typeof(IEditorPlugin))
        {

        }

        public EditorPluginAttribute(string contractName)
            : base(contractName, typeof(IEditorPlugin))
        {

        }

        public string PluginType { get; set; }
    }

    public interface IEditorPluginData
    {
        string PluginType { get; }
    }

    public interface IEditorPluginOperation
    {        
        string AssemblyPath { get; }

        void Delete();
    }
}
