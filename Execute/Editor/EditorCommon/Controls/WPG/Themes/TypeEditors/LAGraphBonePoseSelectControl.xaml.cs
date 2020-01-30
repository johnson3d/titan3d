using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using EditorCommon.Resources;
using EngineNS.Bricks.Animation.Skeleton;

namespace WPG.Themes.TypeEditors
{
    public interface ISkeletonControl
    {
        string SkeletonAsset { get; }
    }
    public class TreeItemBone
    {
        public string Name { get; set; } = "";
        ObservableCollection<TreeItemBone> mChildren = new ObservableCollection<TreeItemBone>();
        public ObservableCollection<TreeItemBone> Children
        {
            get => mChildren;
        }
        public TreeItemBone(CGfxSkeleton skeleton, CGfxBone bone, CGfxBone parentBone)
        {
            if (bone == null)
                Name = "None";
            else
            {
                Name = bone.BoneDesc.Name;
                if (bone.ChildNumber > 0)
                {
                    for (uint i = 0; i < bone.ChildNumber; ++i)
                    {
                        var childIndex = bone.GetChild(i);
                        var child = skeleton.GetBone(childIndex);
                        mChildren.Add(new TreeItemBone(skeleton, child, bone));
                    }
                }
            }
        }
    }
    /// <summary>
    /// Interaction logic for SocketSelectControl.xaml
    /// </summary>
    public partial class LAGraphBonePoseSelectControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        static string None = "None";
        ObservableCollection<TreeItemBone> mSkeletonList = new ObservableCollection<TreeItemBone>();
        public ObservableCollection<TreeItemBone> SkeletonList
        {
            get { return mSkeletonList; }
            set
            {
                mSkeletonList = value;
                OnPropertyChanged("BoneList");
            }
        }


        public string BoneName
        {
            get { return (string)GetValue(BoneNameProperty); }
            set { SetValue(BoneNameProperty, value); }
        }

        public static readonly DependencyProperty BoneNameProperty =
            DependencyProperty.Register("BoneName", typeof(string), typeof(LAGraphBonePoseSelectControl), new PropertyMetadata(new PropertyChangedCallback(OnBoneNameChangedCallback)));

        static void OnBoneNameChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAGraphBonePoseSelectControl;
            var name = e.NewValue as string;
            if (name != None && !string.IsNullOrEmpty(name))
            {
                ctrl.ResetBtnVisible = Visibility.Visible;
            }
            else
            {
                ctrl.ResetBtnVisible = Visibility.Collapsed;
            }
        }
        public Visibility ResetBtnVisible
        {
            get { return (Visibility)GetValue(ResetBtnVisibleProperty); }
            set { SetValue(ResetBtnVisibleProperty, value); }
        }
        public static readonly DependencyProperty ResetBtnVisibleProperty =
                            DependencyProperty.Register("ResetBtnVisible", typeof(Visibility), typeof(LAGraphBonePoseSelectControl), new UIPropertyMetadata(Visibility.Collapsed));

        #region 筛选

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                TreeView_Skeleton.Items.Filter = new Predicate<object>((object obj) =>
                {
                    var item = obj as TreeItemBone;
                    var boneName = item.Name;
                    if (boneName.ToLower().Contains(mFilterString.ToLower()))
                        return true;
                    return false;
                });
                if (string.IsNullOrEmpty(mFilterString))
                {
                    TreeView_Skeleton.Items.Filter = null;
                }
                OnPropertyChanged("FilterString");
            }
        }

        #endregion
        public LAGraphBonePoseSelectControl()
        {
            InitializeComponent();
        }
        private void IconTextBtn_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            SkeletonList.Clear();
            var type = BindInstance.GetType();
            var field = type.GetField("HostNode");
            if (field == null)
                return;
            var skeletonCtrl = field.GetValue(BindInstance) as ISkeletonControl;
            if (skeletonCtrl == null)
                return;
            var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, EngineNS.RName.GetRName(skeletonCtrl.SkeletonAsset));
            if (skeleton == null)
                SkeletonList.Add(new TreeItemBone(null, null, null));
            else
                SkeletonList.Add(new TreeItemBone(skeleton, skeleton.Root, null));
            TreeView_Skeleton.ItemsSource = SkeletonList;
            SearchBoxCtrl.FocusInput();
        }
        private void TreeView_Skeleton_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = TreeView_Skeleton.SelectedItem;
            if (item == null)
                return;
            else
            {
                var itemBone = item as TreeItemBone;
                BoneName = itemBone.Name;
                if (BoneName != None && !string.IsNullOrEmpty(BoneName))
                {
                    ResetBtnVisible = Visibility.Visible;
                }
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            BoneName = None;
            ResetBtnVisible = Visibility.Collapsed;
        }
        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(LAGraphBonePoseSelectControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));
        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LAGraphBonePoseSelectControl;
            //var newValue = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            //var att = newValue.Attributes[typeof(EngineNS.Editor.Editor_RNameTypeAttribute)] as EngineNS.Editor.Editor_RNameTypeAttribute;
            //if (att != null)
            //{
            //    ctrl.mRNameResourceType = att.RNameType;
            //    ctrl.ShowActiveBtn = att.ShowActiveBtn ? Visibility.Visible : Visibility.Hidden;
            //    var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(ctrl.mRNameResourceType);
            //    if (meta != null)
            //    {
            //        ctrl.ResourceBrush = meta.ResInfo.ResourceTypeBrush;
            //    }
            //}
            //var macrossAtt = newValue.Attributes[typeof(EngineNS.Editor.Editor_RNameMacrossType)] as EngineNS.Editor.Editor_RNameMacrossType;
            //if (macrossAtt != null)
            //{
            //    ctrl.mRNameResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;
            //    ctrl.mMacrossBaseType = macrossAtt.MacrossBaseType;
            //    var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(ctrl.mRNameResourceType);
            //    if (meta != null)
            //    {
            //        ctrl.ResourceBrush = meta.ResInfo.ResourceTypeBrush;
            //    }
            //}
        }
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(LAGraphBonePoseSelectControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }


    }
}
