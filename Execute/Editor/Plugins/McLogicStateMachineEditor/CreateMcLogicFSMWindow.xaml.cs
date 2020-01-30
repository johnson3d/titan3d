using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using EditorCommon.Resources;
using System.Windows.Controls;
using System.Windows.Data;
using CodeDomNode;
using System.ComponentModel;
using EditorCommon.Controls.ResourceBrowser;

namespace McLogicStateMachineEditor
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CreateMcLogicFSMWindow : ResourceLibrary.WindowBase, ICustomCreateDialog, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateMcLogicFSMWindow), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as CreateMcLogicFSMWindow;
            win.mCreateData.ResourceName = (string)e.NewValue;
        }
        public bool OKButtonEnable
        {
            get { return (bool)GetValue(OKButtonEnableProperty); }
            set { SetValue(OKButtonEnableProperty, value); }
        }

        public static readonly DependencyProperty OKButtonEnableProperty =
            DependencyProperty.Register("OKButtonEnable", typeof(bool), typeof(CreateMcLogicFSMWindow), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnOKButtonEnableChanged)));
        public static void OnOKButtonEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        string mResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;

        ResourceInfo mMacrossResourceInfo = null;
        public ResourceInfo HostResourceInfo
        {
            set
            {
                mMacrossResourceInfo = value;
                var type = mMacrossResourceInfo.GetType();
                var att = Attribute.GetCustomAttribute(type, typeof(ResourceInfoAttribute)) as EditorCommon.Resources.ResourceInfoAttribute;
                mResourceInfoType = att.ResourceInfoType;
                if (mResourceInfoType == EngineNS.Editor.Editor_RNameTypeAttribute.AnimationMacross)
                {
                }
            }
        }
        ResourceInfos.McLFSMResourceCreateData mCreateData = new ResourceInfos.McLFSMResourceCreateData();

        Dictionary<Type, Macross.ClassData> mClassDataDic = new Dictionary<Type, Macross.ClassData>();
        ObservableCollection<Macross.ClassData> mAllClasses = new ObservableCollection<Macross.ClassData>();
        ObservableCollection<Macross.ClassData> mViewClasses = new ObservableCollection<Macross.ClassData>();
        List<string> mMacrossTypeNames = new List<string>();

        public CreateMcLogicFSMWindow()
        {
            InitializeComponent();
            CenterDataNameCtrl.RNameResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;
            CenterDataNameCtrl.MacrossBaseType = typeof(EngineNS.GamePlay.Actor.GCenterData);
            CenterDataNameCtrl.OnResourceNameChanged += CenterDataNameCtrl_OnResourceNameChanged;
            mCreateData.HostDialog = this;
        }
        bool mIsCenterDataSelect = false;
        private void CenterDataNameCtrl_OnResourceNameChanged(object sender, WPG.Themes.TypeEditors.RNameControl.ResourceNameChangedEventArgs e)
        {
            if (e.Name != EngineNS.RName.EmptyName)
            {
                mIsCenterDataSelect = true;
            }
            else
            {
                mIsCenterDataSelect = false;

            }
            if (mIsCenterDataSelect)
            {
                OKButtonEnable = true;
            }
            else
            {
                OKButtonEnable = false;
            }
            mCreateData.CenterDataTypeName = e.Name;
        }
     
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mCreateData.ClassType = typeof(EngineNS.GamePlay.Component.StateMachine.McStateMachineComponent);
            mCreateData.IsMacrossType = false;
            mCreateData.CSType = EngineNS.ECSType.All;

            var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
            if (bindExp != null)
            {
                if (bindExp.ParentBinding.ValidationRules.Count > 0)
                {
                    var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                    if (rule != null)
                    {
                        rule.OnValidateCheck = (object value, System.Globalization.CultureInfo cultureInfo) =>
                        {
                            var val = (string)value;
                            if (string.IsNullOrEmpty(val))
                                return new ValidationResult(false, "名称不能为空!");
                            if (EditorCommon.Program.IsValidRName(val) == false)
                                return new ValidationResult(false, "名称不合法!");

                            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, val + EngineNS.CEngineDesc.MacrossExtension + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
                            if (files.Count > 0)
                                return new ValidationResult(false, "已包含同名Macross");

                            return new ValidationResult(true, null);
                        };
                    }
                }
            }
        }

        public IResourceCreateData GetCreateData()
        {
            return mCreateData;
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Name != null)
            {
                if (Validation.GetHasError(TextBox_Name))
                    return;

                var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = null;
                        }
                    }
                }

                var bindingExpression = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                    bindingExpression.UpdateSource();
            }
            if (mCreateData.ClassType == null)
                return;
            DialogResult = true;
            this.Close();
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public bool? ShowDialog(InputWindow.Delegate_ValidateCheck onCheck)
        {
            try
            {
                if (TextBox_Name != null)
                {
                    var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
                    if (bindExp != null)
                    {
                        if (bindExp.ParentBinding.ValidationRules.Count > 0)
                        {
                            var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                            if (rule != null)
                                rule.OnValidateCheck = onCheck;
                        }
                    }
                }

                return this.ShowDialog();
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            }
            return false;
        }
        EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData ICustomCreateDialog.FolderData { get; set; }
    }
}
