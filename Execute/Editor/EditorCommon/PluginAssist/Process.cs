using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EditorCommon.PluginAssist
{
    public class Process
    {
        #region PluginProcess

        public static PluginControlContainer GetEditor(EngineNS.RName resourceName)
        {
            PluginControlContainer editCtrl;
            if (mControlsDic.TryGetValue(resourceName, out editCtrl))
            {
                return editCtrl;
            }
            return null;
        }
        // 
        static Dictionary<EngineNS.RName, PluginControlContainer> mControlsDic = new Dictionary<EngineNS.RName, PluginControlContainer>();
        public static Dictionary<EngineNS.RName, PluginControlContainer> ControlsDic
        {
            get => mControlsDic;
        }
        public static async System.Threading.Tasks.Task OnOpenEditor(EditorCommon.Resources.ResourceEditorContext context)
        {
            if (context == null)
                return;

            var resInfo = context.ResInfo;
            PluginControlContainer editCtrl;
            if(!mControlsDic.TryGetValue(resInfo.ResourceName, out editCtrl))
            {
                var pluginItem = EditorCommon.PluginAssist.PluginManager.Instance.GetPluginItem(context.EditorKeyName);
                if (pluginItem == null)
                    return;
                editCtrl = GetPluginControl(pluginItem.PluginObject, true);
                if(editCtrl != null)
                {
                    editCtrl.Context = context;
                    mControlsDic[resInfo.ResourceName] = editCtrl;
                    editCtrl.OnClosed += () =>
                    {
                        mControlsDic.Remove(resInfo.ResourceName);
                    };
                }
                else
                {
                    if(pluginItem.PluginObject is DependencyObject)
                    {
                        var dp = (DependencyProperty)(pluginItem.PluginObject.GetType().GetField("TitleProperty").GetValue(pluginItem.PluginObject));
                        if (dp != null)
                        {
                            BindingOperations.SetBinding(pluginItem.PluginObject as DependencyObject, dp, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                        }
                    }
                    else
                    {
                        var pro = pluginItem.PluginObject.GetType().GetProperty("Title");
                        if (pro.CanWrite)
                            pro.SetValue(pluginItem.PluginObject, context.ResInfo.ResourceName.PureName());
                    }
                    await pluginItem.PluginObject.SetObjectToEdit(context);
                    return;
                }
            }

            if (editCtrl.IsShowing)
            {
                var parentTabItem = EditorCommon.Program.GetParent(editCtrl, typeof(TabItem)) as TabItem;
                var parentTabControl = EditorCommon.Program.GetParent(editCtrl, typeof(TabControl)) as TabControl;
                if (parentTabControl != null && parentTabItem != null)
                {
                    parentTabControl.SelectedItem = parentTabItem;
                }

                // 将包含该控件的窗体显示到最前
                var parentWin = EditorCommon.Program.GetParent(editCtrl, typeof(Window)) as Window;

                ResourceLibrary.Win32.BringWindowToTop(new System.Windows.Interop.WindowInteropHelper(parentWin).Handle);
            }
            else
            {
                DockControl.Controls.DockAbleWindowBase win = null;
                foreach (var w in DockControl.DockManager.Instance.DockableWindows)
                {
                    if(w.IsActive)
                    {
                        win = w;
                        break;
                    }
                }
                var tabItem = new DockControl.Controls.DockAbleTabItem()
                {
                    Content = editCtrl
                };
                tabItem.DockGroup = editCtrl.DockGroup;
                tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.HeaderProperty, new Binding("Title") { Source = editCtrl.PluginObject });
                tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.IconProperty, new Binding("Icon") { Source = editCtrl.PluginObject });
                tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.IconBrushProperty, new Binding("IconBrush") { Source = editCtrl.PluginObject });
                if (win == null)
                {
                    var dockWin = new DockControl.DockAbleWindow();
                    dockWin.SetContent(tabItem);
                    dockWin.Show();
                }
                else
                {
                    win.TopSurface.AddChild(tabItem);
                }
                //CoreEditor.Program.MainWinInstance.CurrentSurface.AddChild(tabItem);
                editCtrl.IsShowing = true;
            }
            if (editCtrl.PluginObject is DependencyObject)
            {
                var TitleProperty = editCtrl.PluginObject.GetType().GetField("TitleProperty");
                if (TitleProperty != null)
                {
                    var dp = (DependencyProperty)(TitleProperty.GetValue(editCtrl.PluginObject));
                    if (dp != null)
                    {
                        BindingOperations.SetBinding(editCtrl.PluginObject as DependencyObject, dp, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
                    }
                }
                else
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Editor", $"TitleProperty is not a property");
                }
            }
            else
            {
                var pro = editCtrl.PluginObject.GetType().GetProperty("Title");
                if (pro.CanWrite)
                    pro.SetValue(editCtrl.PluginObject, context.ResInfo.ResourceName.PureName());
            }
            await editCtrl.PluginObject.SetObjectToEdit(context);
        }

        protected static Dictionary<EditorCommon.PluginAssist.IEditorPlugin, PluginControlContainer> mPluginControlsDictionary = new Dictionary<EditorCommon.PluginAssist.IEditorPlugin, PluginControlContainer>();
        public static Dictionary<EditorCommon.PluginAssist.IEditorPlugin, PluginControlContainer> PluginControlsDictionary
        {
            get { return mPluginControlsDictionary; }
        }
        static Dictionary<Guid, PluginControlContainer> mPluginControlGuids = new Dictionary<Guid, PluginControlContainer>();

        // 取得插件控件
        public static PluginControlContainer GetPluginControl(EditorCommon.PluginAssist.IEditorPlugin pluginObj, bool useExist = false)
        {
            if (!(pluginObj is System.Windows.FrameworkElement))
                return null;

            bool isMulti = false;
            var atts = pluginObj.GetType().GetCustomAttributes(typeof(PartCreationPolicyAttribute), false);
            if (atts.Length > 0)
            {
                switch (((PartCreationPolicyAttribute)(atts[0])).CreationPolicy)
                {
                    case CreationPolicy.NonShared:
                        isMulti = true;
                        break;
                }
            }

            EditorCommon.PluginAssist.IEditorPlugin tagPlugin = pluginObj;
            //atts = pluginObj.GetType().GetCustomAttributes(typeof(Export), false);
            //if (atts.Length <= 0)
            //    return null;

            //var contractName = ((Export)(atts[0])).Definition.ContractName;
            //tagPlugin = PluginAssist.PluginManagerWindow.Instance.CompositionContainer.GetExportedValue<EditorCommon.PluginAssist.IPlugin>(contractName);

            if (isMulti)// && !useExist)
            {
                tagPlugin = System.Activator.CreateInstance(pluginObj.GetType()) as EditorCommon.PluginAssist.IEditorPlugin;
            }

            PluginControlContainer retCtrl;
            if (mPluginControlsDictionary.TryGetValue(tagPlugin, out retCtrl))
                return retCtrl;
            var customAtts = tagPlugin.GetType().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);

            Guid pluginGuid = Guid.Empty;
            if (customAtts.Length > 0)
            {
                var cuat = customAtts[0] as System.Runtime.InteropServices.GuidAttribute;
                pluginGuid = EngineNS.Rtti.RttiHelper.GuidTryParse(cuat.Value);
            }

            if (useExist && !isMulti)
            {
                if (mPluginControlGuids.TryGetValue(pluginGuid, out retCtrl))
                    return retCtrl;
            }

            retCtrl = new PluginAssist.PluginControlContainer();
            retCtrl.Content = tagPlugin;
            retCtrl.PluginObject = tagPlugin;
            if (retCtrl.PluginObject is EditorCommon.PluginAssist.IObjectEditorOperation)
            {
                ((EditorCommon.PluginAssist.IObjectEditorOperation)(retCtrl.PluginObject)).OnOpenEditor += OnOpenEditor;
            }
            mPluginControlsDictionary[tagPlugin] = retCtrl;
            if (pluginGuid != Guid.Empty && !isMulti)
                mPluginControlGuids[pluginGuid] = retCtrl;


            return retCtrl;
        }

        #endregion

    }
}
