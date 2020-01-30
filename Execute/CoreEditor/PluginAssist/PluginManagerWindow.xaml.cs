using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Controls;

namespace CoreEditor.PluginAssist
{
    /// <summary>
    /// Interaction logic for PluginManagerWindow.xaml
    /// </summary>
    public partial class PluginManagerWindow : DockControl.Controls.DockAbleWindowBase
    {
        ObservableCollection<EditorCommon.PluginAssist.PluginItem> mPluginItems = new ObservableCollection<EditorCommon.PluginAssist.PluginItem>();
        public ObservableCollection<EditorCommon.PluginAssist.PluginItem> PluginItems
        {
            get { return mPluginItems; }
            set
            {
                mPluginItems = value;
            }
        }

        public PluginManagerWindow()
        {
            InitializeComponent();

            LayoutManaged = false;
            CanClose = false;
        }
        
        public bool NeedClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!NeedClose)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListView_Plugins.SelectedIndex < 0 || ListView_Plugins.SelectedIndex >= PluginItems.Count)
                return;

            Grid_Instruction.Children.Clear();
            Grid_Instruction.Children.Add(PluginItems[ListView_Plugins.SelectedIndex].InstructionControl);

            pluginInfo.Instance = PluginItems[ListView_Plugins.SelectedIndex];
        }

        private void Button_Click_Delete(object sender, System.Windows.RoutedEventArgs e)
        {
            if (EditorCommon.MessageBox.enMessageBoxResult.Yes == EditorCommon.MessageBox.Show("确定要删除？", "提示", EditorCommon.MessageBox.enMessageBoxButton.YesNo))
            {
                var btn = sender as Button;
                var id = (Guid)btn.Tag;
                if (EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey.ContainsKey(id))
                {
                    foreach (var i in EditorCommon.PluginAssist.PluginManager.Instance.Catalog.Catalogs)
                    {
                        var cata = i as AssemblyCatalog;
                        if (cata == null)
                            continue;
                        if (cata.Assembly.Location == EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].AssemblyPath)
                        {
                            EditorCommon.PluginAssist.PluginManager.Instance.Catalog.Catalogs.Remove(cata);
                            break;
                        }
                    }

                    if (EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].HostMeuItem != null)
                    {
                        var menuItem = EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].HostMeuItem.Parent as MenuItem;
                        if (menuItem != null)
                            menuItem.Items.Remove(EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].HostMeuItem);
                    }

                    if (EditorCommon.PluginAssist.PluginManager.Instance.PluginDicWithTypeKey.ContainsKey(EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].PluginData.PluginType))
                    {
                        EditorCommon.PluginAssist.PluginManager.Instance.PluginDicWithTypeKey[EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].PluginData.PluginType].Plugins.Remove(id);
                    }

                    var delObj = EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id].PluginObject as EditorCommon.PluginAssist.IEditorPluginOperation;
                    if (delObj != null)
                    {
                        delObj.Delete();                                                               
                    }

                    mPluginItems.Remove(EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey[id] as EditorCommon.PluginAssist.PluginItem);
                    EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey.Remove(id);

                    EditorCommon.PluginAssist.PluginManager.Instance.AddDelAssembly(delObj.AssemblyPath);
                    EditorCommon.PluginAssist.PluginManager.Instance.SavePluginInfo();                 
                }
            }
        }

        private void Button_Click_New(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "|*.dll";               
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {                            
                if (EditorCommon.PluginAssist.PluginManager.Instance.AssemblyCatalogExist(ofd.FileName))
                {
                    EditorCommon.MessageBox.Show("该插件已加载！","提示");
                    return;
                }

                var file = EngineNS.CEngine.Instance.FileManager.Bin + "Plugins/" + ofd.SafeFileName;
                if (!System.IO.File.Exists(file))
                {
                    System.IO.File.Copy(ofd.FileName, file);
                }                

                var cata = new AssemblyCatalog(System.Reflection.Assembly.LoadFrom(file));
                EditorCommon.PluginAssist.PluginManager.Instance.Catalog.Catalogs.Add(cata);

                EditorCommon.PluginAssist.PluginManager.Instance.DelAssembly(ofd.FileName);
                EditorCommon.PluginAssist.PluginManager.Instance.GeneratePlugins(0, null);
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach(var item in EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey)
            {
                if (item.Value == null)
                    continue;
                PluginItems.Add(item.Value);
            }
        }
    }
}
