using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace EditorCommon.Controls.PropertyGrid
{
    /// <summary>
    /// MaterialTypeSetControl.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialTypeSetControl : UserControl, INotifyPropertyChanged
    {

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        //MeshPartControl m_ParentControl;

        //public string MaterialName
        //{
        //    get { return (string)GetValue(MaterialTechIdProperty); }
        //    set { SetValue(MaterialTechIdProperty, value); }
        //}
        //public static readonly DependencyProperty MaterialTechIdProperty =
        //    DependencyProperty.Register("MaterialName", typeof(string), typeof(MaterialTypeSetControl), new PropertyMetadata(new PropertyChangedCallback(OnMaterialTechIdCallback)));
        //static void OnMaterialCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    MaterialTypeSetControl control = sender as MaterialTypeSetControl;
        //    string tagId = (string)e.NewValue;
        //    control.TechName = EditorCommon.Material.MaterialFileAssist.GetTechniqueName(tagId);
        //}

        //public int IdxShow
        //{
        //    get { return mIdx + 1; }
        //}

        //int mIdx;
        //public int nIdx
        //{
        //    get { return mIdx; }
        //    set
        //    {
        //        mIdx = value;

        //        OnPropertyChanged("nIdx");
        //        OnPropertyChanged("IdxShow");
        //    }
        //}

        string m_MaterialTypeName;
        public string MaterialTypeName
        {
            get { return m_MaterialTypeName; }
            set
            {
                m_MaterialTypeName = value;
                OnPropertyChanged("MaterialTypeName");
            }
        }

        string mMaterialName;
        public string MaterialName
        {
            get { return mMaterialName; }
            set
            {
                mMaterialName = value;
                OnPropertyChanged("MaterialName");
            }
        }

        bool mEditable = true;
        public bool Editable
        {
            get { return mEditable; }
            set
            {
                mEditable = value;

                if (mEditable)
                {
                    Button_Set.Visibility = Visibility.Visible;
                }
                else
                {
                    Button_Set.Visibility = Visibility.Collapsed;
                }
            }
        }
        internal IEnumerable<EditorCommon.Resources.ResourceInfoMetaData> mResourceInfoProcessers = null;
        public MaterialTypeSetControl(IEnumerable<EditorCommon.Resources.ResourceInfoMetaData> processers)
        {
            InitializeComponent();
            mResourceInfoProcessers = processers;
        }
        //public MaterialTypeSetControl(MeshPartControl parent)
        //{
        //    InitializeComponent();

        //    m_ParentControl = parent;
        //}
        EditorCommon.Resources.ResourceInfo mCurrentMaterialInfo = null;
        public EditorCommon.Resources.ResourceInfo CurrentMaterialInfo
        {
            get { return mCurrentMaterialInfo; }
            set { mCurrentMaterialInfo = value; }
        }
        private async System.Threading.Tasks.Task UpdateParentMaterial(string materialAbsFile, bool resetTechnique)
        {
            var materialInfoAbsFile = materialAbsFile + EditorCommon.Program.ResourceInfoExt;
            if (mResourceInfoProcessers == null)
                return ;
            foreach (var processer in mResourceInfoProcessers)
            {
                try
                {
                    if (processer.ResourceInfoTypeStr == EngineNS.Editor.Editor_RNameTypeAttribute.Material)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            mCurrentMaterialInfo = System.Activator.CreateInstance(processer.ResourceInfoType) as EditorCommon.Resources.ResourceInfo;
                        });
                    }
                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
            // 材质不存在则使用默认材质
            if (string.IsNullOrEmpty(materialAbsFile) || !System.IO.File.Exists(materialInfoAbsFile))
                materialInfoAbsFile = EngineNS.CEngine.Instance.MaterialManager.DefaultMaterial.Name.Address;
            await mCurrentMaterialInfo.AsyncLoad(materialInfoAbsFile);
            MaterialNameTextBlock.Text = mCurrentMaterialInfo.ResourceName.PureName();
        }
        private void Button_Set_Click(object sender, RoutedEventArgs e)
        {
            var data = EditorCommon.PluginAssist.PropertyGridAssist.GetSelectedObjectData("Material");
            if (data == null)
                return;
            if (data.Length > 0)
            {
                var materialRelPath = (string)data[0];
                var noUse = UpdateParentMaterial(EngineNS.CEngine.Instance.FileManager._GetAbsPathFromRelativePath(materialRelPath), true);
            }
        }
       
        private void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            if (mCurrentMaterialInfo == null)
                return;

            //EditorCommon.PluginAssist.PluginOperation.SetObjectToPluginForEdit(new object[] { "ResourcesBrowser", mCurrentMaterialInfo.ResourceName });
        }

        enum enDropResult
        {
            Denial_UnknowFormat,
            Denial_NoDragAbleObject,
            Allow,
        }
        // 是否允许拖放
        enDropResult AllowResourceItemDrop(System.Windows.DragEventArgs e)
        {
            var formats = e.Data.GetFormats();
            if (formats == null || formats.Length == 0)
                return enDropResult.Denial_UnknowFormat;

            var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
            if (datas == null)
                return enDropResult.Denial_NoDragAbleObject;

            bool containMeshSource = false;
            foreach (var data in datas)
            {
                var resInfo = data as EditorCommon.Resources.ResourceInfo;
                if (resInfo.ResourceType == EngineNS.Editor.Editor_RNameTypeAttribute.Material)
                {
                    containMeshSource = true;
                    break;
                }
            }

            if (!containMeshSource)
                return enDropResult.Denial_NoDragAbleObject;

            return enDropResult.Allow;
        }

        EditorCommon.DragDrop.DropAdorner mDropAdorner;

        private void Rectangle_AddTech_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            mDropAdorner = new EditorCommon.DragDrop.DropAdorner(LayoutRoot);

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                mDropAdorner.IsAllowDrop = false;

                switch (AllowResourceItemDrop(e))
                {
                    case enDropResult.Allow:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "添加材质模板";

                            mDropAdorner.IsAllowDrop = true;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;

                    case enDropResult.Denial_NoDragAbleObject:
                    case enDropResult.Denial_UnknowFormat:
                        {
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "拖动内容不是材质模板";

                            mDropAdorner.IsAllowDrop = false;
                            var pos = e.GetPosition(element);
                            if (pos.X > 0 && pos.X < element.ActualWidth &&
                               pos.Y > 0 && pos.Y < element.ActualHeight)
                            {
                                var layer = AdornerLayer.GetAdornerLayer(element);
                                layer.Add(mDropAdorner);
                            }
                        }
                        break;
                }
            }
        }

        private void Rectangle_AddTech_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);
            }
        }

        private void Rectangle_AddTech_DragOver(object sender, DragEventArgs e)
        {

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void Rectangle_AddTech_Drop(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            if (EditorCommon.DragDrop.DragDropManager.Instance.DragType.Equals("ResourceItem"))
            {
                e.Handled = true;
                var layer = AdornerLayer.GetAdornerLayer(element);
                layer.Remove(mDropAdorner);

                if (AllowResourceItemDrop(e) == enDropResult.Allow)
                {
                    var formats = e.Data.GetFormats();
                    var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                    foreach (var data in datas)
                    {
                        var resInfo = data as EditorCommon.Resources.ResourceInfo;
                        if (resInfo == null)
                            continue;

                        var noUse = UpdateParentMaterial(resInfo.ResourceName.Address, true);
                    }
                }
            }
        }
    }
}

