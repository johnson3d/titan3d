using DockControl;
using EngineNS.Bricks.ExcelTable;
using EditorCommon.Resources;
using EngineNS.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EngineNS.Macross;
using EngineNS.Editor;

namespace ExcelViewEditor
{
    /// <summary>
    /// ExcelViewPanel.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "ExcelViewEditor")]
    [Guid("10F78615-3F00-4296-9E85-B905F4902E31")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ExcelViewPanel : UserControl, EditorCommon.PluginAssist.IEditorPlugin, INotifyPropertyChanged
    {
        #region pluginInterface
        public string PluginName
        {
            get { return "ExcelViewEditor"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        public bool OnActive()
        {
            return true;
        }
        public bool OnDeactive()
        {
            return true;
        }

        public void Tick()
        {

        }

        public object[] GetObjects(object[] param)
        {
            return null;
        }

        public bool RemoveObjects(object[] param)
        {
            return false;
        }

        public string Title { get; }
        public ImageSource Icon { get; }
        public Brush IconBrush { get; }

        void IDockAbleControl.StartDrag()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.EndDrag()
        {
            //throw new NotImplementedException();
        }

        bool? IDockAbleControl.CanClose()
        {
            return true;
            //throw new NotImplementedException();
        }

        void IDockAbleControl.Closed()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        IDockAbleControl IDockAbleControl.LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

        ExcelResourceInfo ResourceInfo;
        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (context.ResInfo == null)
                return;

            ResourceInfo = context.ResInfo as ExcelResourceInfo;
            if (ResourceInfo == null)
                return;

            //UIPanel.Children.Clear();
            var filename = ResourceInfo.AbsInfoFileName.Replace(".rinfo", "");
            FileName = filename;
            if (System.IO.File.Exists(filename))
            {
                ExcelImporter import = new ExcelImporter();
                import.Init(filename);
                var method = import.GetType().GetMethod("Table2Objects");

                //var assembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Common, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
                var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;// Assembly.LoadFile(EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll");
                try
                {
                    MacrossName = ResourceInfo.MacrossName;
                    var type = EngineNS.Macross.MacrossFactory.Instance.GetMacrossType(MacrossName);
                    ObjectList = method.MakeGenericMethod(type).Invoke(import, null) as IList;
                    RefreshDatas();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

            }
            else
            {

                //ClassInfo.AddNode(_ClassType.ClassName, _ClassType);

            }

        }

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public UIElement InstructionControl => throw new NotImplementedException();
        #endregion

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region binddata
        public class BindData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string ObjectName
            {
                get { return (string)GetValue(ObjectNameProperty); }
                set { SetValue(ObjectNameProperty, value); }
            }
            public static readonly DependencyProperty ObjectNameProperty = DependencyProperty.Register("ObjectName", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(null));

            public string ObjectIndex
            {
                get { return (string)GetValue(ObjectIndexProperty); }
                set { SetValue(ObjectIndexProperty, value); }
            }
            public static readonly DependencyProperty ObjectIndexProperty = DependencyProperty.Register("ObjectIndex", typeof(string), typeof(BindData), new FrameworkPropertyMetadata(null));

            public Object Value
            {
                get;
                set;
            }

            public BindData()
            {
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }
        #endregion

        string mFilterString = "";
        string mLowerFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                mLowerFilterString = value.ToLower();
                // ShowItemWithFilter(TreeViewItemsNodes, mFilterString);
                OnPropertyChanged("FilterString");
                RefreshDatas();
                //RefreshActors();
            }
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();
        public ExcelViewPanel()
        {
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(ExcelResourceInfo));
            InitializeComponent();

            UITrees.TreeListItemsSource = TreeViewItemsNodes;
            UIProperty.DeleteObject -= DeleteData;
            UIProperty.DeleteObject += DeleteData;
        }

        IList ObjectList;
        EngineNS.RName MacrossName;
        string FileName = "";

        public void RefreshDatas()
        {
            if (ObjectList == null)
                return;
            TreeViewItemsNodes.Clear();

            PropertyInfo propertytype = null;
            if (ObjectList.Count > 0)
            {
                var obj = ObjectList[0];
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();
                foreach (var ptype in properties)
                {
                    MacrossClassKeyAttribute attr = ptype.GetCustomAttribute<MacrossClassKeyAttribute>();
                    if (attr != null)
                    {
                        propertytype = ptype;
                        break;
                    }
                }

                //MacrossClassKeyAttribute attr = type.GetCustomAttribute<MacrossClassKeyAttribute>();
            }

            for (int i = 0; i < ObjectList.Count; i++)
            {
                string ObjectName = i.ToString();
                object value = null;
                if (propertytype != null)
                {
                    value = propertytype.GetValue(ObjectList[i]);
                    if (value != null)
                    {
                        ObjectName = value.ToString();
                    }
                    else
                    {
                        propertytype.SetValue(ObjectList[i], ObjectName);
                    }
                }
                if (mLowerFilterString.Equals("") || ObjectName.ToLower().IndexOf(mLowerFilterString) != -1)
                {
                    BindData bd = new BindData();
                    bd.ObjectIndex = i.ToString();
                    if (propertytype != null)
                    {
                        BindingOperations.SetBinding(bd, BindData.ObjectNameProperty, new Binding(propertytype.Name) { Source = ObjectList[i] });
                    }
                    else
                    {
                        bd.ObjectName = ObjectName;
                    }

                    bd.Value = ObjectList[i];
                    TreeViewItemsNodes.Add(bd);
                }
            }
        }

        public void AddData()
        {
            if (MacrossName == null || ObjectList == null)
                return;

            //ObjectList.Add(Activator.CreateInstance(ObjectType));
            var getter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<object>(this.MacrossName);
            ObjectList.Add(getter.Get(false));

            RefreshDatas();
        }

        public void DeleteData(Object obj)
        {
            if (ObjectList.Contains(obj) == false)
                return;

            ObjectList.Remove(obj);

            RefreshDatas();
        }
        private void UIAdd_Click(object sender, RoutedEventArgs e)
        {
            AddData();
        }

        private bool DontProcessProperty(System.Reflection.PropertyInfo prop, Type type)
        {
            if (type.Assembly.FullName.Contains("PresentationCore"))
                return true;
            else if (type.Assembly.FullName.Contains("PresentationFramework"))
                return true;
            else if (type.Assembly.FullName.Contains("WindowsBase"))
                return true;
            else if (type.Assembly.FullName.Contains("System.Windows.Forms"))
                return true;
            else if (type.Assembly.FullName.Contains("System.Drawing"))
                return true;
            else if (type.Assembly.FullName.Contains("mscorlib"))
                return true;
            return false;

            //if (type.Assembly.FullName.Contains("CoreClient.Windows.dll"))
            //    return false;
            //if (type.Assembly.FullName.Contains("Game.Windows"))
            //    return false;
            //return true;
        }

        private bool DontProcessObject(Object obj, Type type)
        {
            if (type.Equals(typeof(EngineNS.CConstantBuffer)))
                return true;
            else if (type.FullName.Contains("System.RuntimeType"))
                return true;
            else if(type.Equals(typeof(string)) || type.IsPrimitive)
                return true;
            else if (type.IsValueType)
                return true;
            return false;
        }

        private bool DontProcessNameSpace(Object obj, Type type)
        {
            if (type.Namespace.IndexOf("System.") == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsArray(Type type)
        {
            Type[] types = type.GetInterfaces();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].Name.Equals("IList") || types[i].Name.Equals("IDictionary"))
                    return true;
            }

            return false;
        }

        public void RecordObject(Object obj, ExcelResourceInfo info)
        {
            if (obj == null)
                return;

            var type = obj.GetType();
            if (DontProcessObject(obj, type))
            {
                return;
            }

            if (DontProcessNameSpace(obj, type))
            {
                return;
            }



            //处理字段集
            FieldInfo[] ActorFields = type.GetFields();

            for (int i = 0; i < ActorFields.Length; i++)
            {

                object value;
                try
                {
                    value = ActorFields[i].GetValue(obj);
                }
                catch (Exception e)

                {
                    value = null;
                }

                if (value == null)
                    continue;

                {
                    //看是否使用宏图文件
                    EngineNS.RName rname = value as EngineNS.RName;
                    if (rname != null)
                    {
                        //Record..
                        info.ReferenceRNameList.Add(rname);
                    }
                }
               

                Type pp = value.GetType();
                if (IsArray(pp))
                {
                    Type[] args = pp.GetGenericArguments();
                    //Array List
                    if (args.Length == 1 || args.Length == 0)
                    {
                        var tt = (System.Collections.IEnumerable)value;
                        if (tt != null)
                        {
                            foreach (var v in tt)
                            {
                                EngineNS.RName rname = v as EngineNS.RName;
                                if (rname != null)
                                {
                                    //Record..
                                    info.ReferenceRNameList.Add(rname);
                                }
                                else
                                {
                                    RecordObject(v, info);
                                }
                            }

                        }
                    }
                    //Dictionary
                    else if (args.Length == 2)
                    {
                        var tt = (System.Collections.IDictionary)value;
                        if (tt != null)
                        {
                            foreach (var v in tt.Values)
                            {

                                EngineNS.RName rname = v as EngineNS.RName;
                                if (rname != null)
                                {
                                    //Record..
                                    info.ReferenceRNameList.Add(rname);
                                }
                                else
                                {
                                    RecordObject(v, info);
                                }
                            }

                        }
                    }
                }
                else
                {
                    RecordObject(value, info);
                }
            }

            //处理属性集
            PropertyInfo[] ActorProperties = type.GetProperties();
            for (int i = 0; i < ActorProperties.Length; i++)
            {

                object value;
                try
                {
                    value = ActorProperties[i].GetValue(obj);
                }
                catch (Exception e)

                {
                    value = null;
                }

                if (value == null)
                    continue;

                //看是否使用宏图文件
                {
                    EngineNS.RName rname = value as EngineNS.RName;
                    if (rname != null)
                    {
                        //Record..
                        info.ReferenceRNameList.Add(rname);
                    }

                }

                Type pp = value.GetType();
                if (IsArray(pp))
                {
                    Type[] args = pp.GetGenericArguments();
                    //Array List
                    if (args.Length == 1 || args.Length == 0)
                    {
                        var tt = (System.Collections.IEnumerable)value;
                        if (tt != null)
                        {
                            foreach (var v in tt)
                            {
                                EngineNS.RName rname = v as EngineNS.RName;
                                if (rname != null)
                                {
                                    //Record..
                                    info.ReferenceRNameList.Add(rname);
                                }
                                else
                                {
                                    RecordObject(v, info);
                                }

                            }

                        }
                    }
                    //Dictionary
                    else if (args.Length == 2)
                    {
                        var tt = (System.Collections.IDictionary)value;
                        if (tt != null)
                        {
                            foreach (var v in tt.Values)
                            {

                                EngineNS.RName rname = v as EngineNS.RName;
                                if (rname != null)
                                {
                                    //Record..
                                    info.ReferenceRNameList.Add(rname);
                                }
                                else
                                {
                                    RecordObject(v, info);
                                }
                            }

                        }
                    }
                }
                else
                {
                    RecordObject(value, info);
                }
            }
        }


        private void UISave_Click(object sender, RoutedEventArgs e)
        {
            if (ObjectList == null || ObjectList.Count == 0)
                return;


            ExcelExporter newexport = new ExcelExporter();
            var type = EngineNS.Macross.MacrossFactory.Instance.GetMacrossType(this.MacrossName);
            newexport.Init("", type);
            List<Object> lst = new List<Object>();

            ResourceInfo.ReferenceRNameList.Clear();
            foreach (var i in ObjectList)
            {
                lst.Add(i);
                RecordObject(i, ResourceInfo);
            }

            var test = ResourceInfo.Save();

            //data._Type.
            newexport.Objects2Table(lst, type);
            newexport.Save(FileName);

            EngineNS.Bricks.DataProvider.GDataSet.Save2Xnd(FileName+".dataset", lst);
        }

        private void TreeViewActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UITrees.SelectedIndex < 0)
                return;


            EditorCommon.TreeListView.TreeNode node = UITrees.SelectedItem as EditorCommon.TreeListView.TreeNode;
            if (node == null)
                return;

            BindData data = node.Tag as BindData;
            if (data == null)
                return;

            UIProperty.SetValue(data.Value);
        }
    }
}
