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

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for SocketSelectControl.xaml
    /// </summary>
    public partial class SocketSelectControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        static string None = "None";
        ObservableCollection<string> mBoneList = new ObservableCollection<string>();
        public ObservableCollection<string> BoneList
        {
            get { return mBoneList; }
            set
            {
                mBoneList = value;
                OnPropertyChanged("BoneList");
            }
        }


        public string SocketName
        {
            get { return (string)GetValue(SocketNameProperty); }
            set { SetValue(SocketNameProperty, value); }
        }

        public static readonly DependencyProperty SocketNameProperty =
            DependencyProperty.Register("SocketName", typeof(string), typeof(SocketSelectControl), new PropertyMetadata(new PropertyChangedCallback(OnSocketNameChangedCallback)));

        static void OnSocketNameChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as SocketSelectControl;
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
                            DependencyProperty.Register("ResetBtnVisible", typeof(Visibility), typeof(SocketSelectControl), new UIPropertyMetadata(Visibility.Collapsed));

        #region 筛选

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                ListBox_Sockets.Items.Filter = new Predicate<object>((object obj) =>
                {
                    var boneName = obj as string;
                    if (boneName.ToLower().Contains(mFilterString.ToLower()))
                        return true;
                    return false;
                });
                if (string.IsNullOrEmpty(mFilterString))
                {
                    ListBox_Sockets.Items.Filter = null;
                }
                OnPropertyChanged("FilterString");
            }
        }

        #endregion
        public SocketSelectControl()
        {
            InitializeComponent();
        }
        private void IconTextBtn_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            BoneList.Clear();
            var enumrableInterface = BindInstance.GetType().GetInterface(typeof(System.Collections.IEnumerable).FullName, false);
            if (enumrableInterface != null)
            {
                // 显示多个对象
                int count = 0;
                foreach (var objIns in (System.Collections.IEnumerable)BindInstance)
                {
                    if (objIns == null)
                        continue;
                    count++;
                }
                if (count == 0)
                {

                }
                else if(count == 1)
                {
                    foreach (var objIns in (System.Collections.IEnumerable)BindInstance)
                    {
                        if (objIns == null)
                            continue;
                        RefreshBoneList(objIns);
                    }
                }
                else
                {
                    string skeletonName = null;
                    bool haveSameSkeleton = true;
                    foreach (var objIns in (System.Collections.IEnumerable)BindInstance)
                    {
                        if (objIns == null)
                            continue;
                        var comp = objIns as EngineNS.GamePlay.Component.GComponent;
                        if (comp == null)
                            continue;
                        var hostMesh = comp.HostContainer as EngineNS.GamePlay.Component.GMeshComponent;
                        if (hostMesh != null)
                        {
                            var modifier = hostMesh.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                            if (modifier != null)
                            {
                                if (skeletonName == null)
                                    skeletonName = modifier.SkeletonAsset;
                                if (skeletonName != modifier.SkeletonAsset)
                                {
                                    haveSameSkeleton = false;
                                    return;
                                }
                               
                            }
                        }
                    }
                    if(haveSameSkeleton && !string.IsNullOrEmpty(skeletonName))
                    {
                        var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, EngineNS.RName.GetRName(skeletonName));
                        var skeletonPose = skeleton.CreateSkeletonPose();
                        BoneList.Add("None");
                        for (uint i = 0; i < skeletonPose.BoneNumber; ++i)
                        {
                            BoneList.Add(skeletonPose.GetBonePose(i).ReferenceBone.BoneDesc.Name);
                        }
                    }
                }
            }
            else
            {
                RefreshBoneList(BindInstance);
            }
            SearchBoxCtrl.FocusInput();
        }
        void RefreshBoneList(object instance)
        {
            var comp = instance as EngineNS.GamePlay.Component.GComponent;
            if (comp == null)
                return;
            var hostMesh = comp.HostContainer as EngineNS.GamePlay.Component.GMeshComponent;
            if (hostMesh != null)
            {
                var modifier = hostMesh.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                if (modifier != null)
                {
                    var skeleton = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, EngineNS.RName.GetRName(modifier.SkeletonAsset));
                    var skeletonPose = skeleton.CreateSkeletonPose();
                    BoneList.Add("None");
                    for (uint i = 0; i < skeletonPose.BoneNumber; ++i)
                    {
                        BoneList.Add(skeletonPose.GetBonePose(i).ReferenceBone.BoneDesc.Name);
                    }
                }
            }
        }
        private void ListBox_Sockets_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ListBox_Sockets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListBox_Sockets.SelectedItem;
            if (item == null)
                return;
            else
            {
                SocketName = item as string;
                if (SocketName != None && !string.IsNullOrEmpty(SocketName))
                {
                    ResetBtnVisible = Visibility.Visible;
                }
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SocketName = None;
            ResetBtnVisible = Visibility.Collapsed;
        }
        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(SocketSelectControl), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));
        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as SocketSelectControl;
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
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(SocketSelectControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }


    }
}
