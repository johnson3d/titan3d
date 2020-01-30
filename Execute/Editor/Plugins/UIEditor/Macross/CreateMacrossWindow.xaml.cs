using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using EditorCommon.Resources;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace UIEditor.UIMacross
{
    public class ClassData : DependencyObject
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ClassData), new UIPropertyMetadata(""));
        public string HightLightString
        {
            get { return (string)GetValue(HightLightStringProperty); }
            set { SetValue(HightLightStringProperty, value); }
        }
        public static readonly DependencyProperty HightLightStringProperty = DependencyProperty.Register("HightLightString", typeof(string), typeof(ClassData), new UIPropertyMetadata(""));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ClassData), new UIPropertyMetadata(null));

        public Type ClassType;

        public void CopyTo(ClassData data)
        {
            data.Name = Name;
            data.Icon = Icon;
            data.ClassType = ClassType;
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CreateMacrossWindow : ResourceLibrary.WindowBase, EditorCommon.Resources.ICustomCreateDialog
    {
        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
        public string ResourceName
        {
            get { return (string)GetValue(ResourceNameProperty); }
            set { SetValue(ResourceNameProperty, value); }
        }

        public static readonly DependencyProperty ResourceNameProperty =
            DependencyProperty.Register("ResourceName", typeof(string), typeof(CreateMacrossWindow), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnResourceNameChanged)));
        public static void OnResourceNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as CreateMacrossWindow;
            win.mCreateData.ResourceName = (string)e.NewValue;
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(CreateMacrossWindow), new FrameworkPropertyMetadata(""));

        public string FilterString
        {
            get { return (string)GetValue(FilterStringProperty); }
            set { SetValue(FilterStringProperty, value); }
        }

        public static readonly DependencyProperty FilterStringProperty = DependencyProperty.Register("FilterString", typeof(string), typeof(CreateMacrossWindow), new UIPropertyMetadata("", OnFilterStringPropertyChanged));
        private static void OnFilterStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var win = sender as CreateMacrossWindow;
            win.ShowItemWithFilter();
        }

        string mResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;

        ResourceInfo mMacrossResourceInfo = null;
        public ResourceInfo HostResourceInfo
        {
            set
            {
                mMacrossResourceInfo = value;
                var type = mMacrossResourceInfo.GetType();
                var att = Attribute.GetCustomAttribute(type,typeof(ResourceInfoAttribute)) as EditorCommon.Resources.ResourceInfoAttribute;
                mResourceInfoType = att.ResourceInfoType;
                if(mResourceInfoType == EngineNS.Editor.Editor_RNameTypeAttribute.AnimationMacross)
                {
                }
            }
        }
        ResourceCreateData mCreateData = new ResourceCreateData();

        List<string> mMacrossTypeNames = new List<string>();

        public CreateMacrossWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitClassShow();

            var bindExp = TextBox_Name.GetBindingExpression(TextBox.TextProperty);
            if(bindExp != null)
            {
                if(bindExp.ParentBinding.ValidationRules.Count > 0)
                {
                    var rule = bindExp.ParentBinding.ValidationRules[0] as InputWindow.RequiredRule;
                    if(rule != null)
                    {
                        rule.OnValidateCheck = (object value, System.Globalization.CultureInfo cultureInfo) =>
                        {
                            var val = (string)value;
                            if (string.IsNullOrEmpty(val))
                                return new ValidationResult(false, "名称不能为空!");
                            if(EditorCommon.Program.IsValidRName(val) == false)
                                return new ValidationResult(false, "名称不合法!");

                            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(FolderData.AbsFolder, val + EngineNS.CEngineDesc.MacrossExtension + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
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
            mCreateData.Description = Description;
            return mCreateData;
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            if(TextBox_Name != null)
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
            catch(System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Create Resource Exception", ex.ToString());
            }
            return false;
        }

        Dictionary<Type, ClassData> mClassDataDic = new Dictionary<Type, ClassData>();
        ObservableCollection<ClassData> mAllClasses = new ObservableCollection<ClassData>();
        ObservableCollection<ClassData> mViewClasses = new ObservableCollection<ClassData>();
        void InitClassShow()
        {
            mClassDataDic.Clear();
            mAllClasses.Clear();
            mViewClasses.Clear();

            var assemblys = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblys(EngineNS.ECSType.Client);
            foreach(var assembly in assemblys)
            {
                foreach(var type in assembly.GetTypes())
                {
                    var atts = type.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_BaseElementAttribute), false);
                    if (atts.Length == 0)
                        continue;

                    var att = atts[0] as EngineNS.UISystem.Editor_BaseElementAttribute;
                    var classData = new ClassData()
                    {
                        Name = type.Name,
                        ClassType = type,
                        Icon = new BitmapImage(new Uri($"/UIEditor;component/Icons/{att.Icon}", UriKind.Relative)),
                    };
                    mClassDataDic[type] = classData;
                    mAllClasses.Add(classData);
                }
            }

            ShowItemWithFilter();
            if (mViewClasses.Count > 0)
                ListBox_Bases.SelectedIndex = 0;
        }
        void ShowItemWithFilter()
        {
            ListBox_Bases.ItemsSource = null;
            mViewClasses.Clear();
            ShowItemWithFilter(mAllClasses, ref mViewClasses, FilterString);
            ListBox_Bases.ItemsSource = mViewClasses;
        }
        bool ShowItemWithFilter(ObservableCollection<ClassData> srcDatas, ref ObservableCollection<ClassData> tagDatas, string filterString)
        {
            tagDatas.Clear();

            bool retValue = false;
            foreach (var data in srcDatas.OfType<ClassData>())
            {
                ClassData tagData = null;
                if (string.IsNullOrEmpty(filterString))
                {
                    tagData = new ClassData();
                    data.CopyTo(tagData);
                    tagData.HightLightString = filterString;
                    tagDatas.Add(tagData);
                    retValue = true;
                }
                else if (data.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    tagData = new ClassData();
                    data.CopyTo(tagData);
                    tagData.HightLightString = filterString;
                    tagDatas.Add(tagData);
                    retValue = true;
                }
            }

            return retValue;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = ListBox_Bases.SelectedItem as ClassData;
            if(data != null)
            {
                mCreateData.ClassType = data.ClassType;
            }
        }
    }
}
