using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using EditorCommon.Resources;
using System.Windows.Controls;
using System.Windows.Data;

namespace Macross
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
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ClassData), new UIPropertyMetadata(false));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ClassData), new UIPropertyMetadata(null));

        public Type ClassType;
        public EngineNS.ECSType CSType;
        public bool IsMacrossType;

        public ObservableCollection<ClassData> Children
        {
            get;
            set;
        } = new ObservableCollection<ClassData>();

        public void CopyTo(ClassData data)
        {
            data.Name = Name;
            data.Icon = Icon;
            data.ClassType = ClassType;
            data.CSType = CSType;
            data.IsMacrossType = IsMacrossType;
        }
    }

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class CreateMacrossWindow : ResourceLibrary.WindowBase, EditorCommon.Resources.ICustomCreateDialog
    {
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
        ResourceInfos.ResourceCreateData mCreateData = new ResourceInfos.ResourceCreateData();

        Dictionary<Type, ClassData> mClassDataDic = new Dictionary<Type, ClassData>();
        ObservableCollection<ClassData> mAllClasses = new ObservableCollection<ClassData>();
        ObservableCollection<ClassData> mViewClasses = new ObservableCollection<ClassData>();
        List<string> mMacrossTypeNames = new List<string>();

        public CreateMacrossWindow()
        {
            InitializeComponent();
        }

        void CollectClassType(Type classType, List<Type> topClassTypes, Dictionary<Type, ClassData> classDataDic, ClassData childClassData, Type topClassType, Dictionary<Type, Type> checkedClasses)
        {
            if (classType == null)
                return;
            if (checkedClasses.ContainsKey(classType))
            {
                ClassData checkedData;
                if(classDataDic.TryGetValue(classType, out checkedData))
                {
                    checkedData.Children.Add(childClassData);
                }
                else
                {
                    if (topClassType != null && !topClassTypes.Contains(topClassType))
                        topClassTypes.Add(topClassType);
                }
                return;
            }
            ClassData classData;
            if (classDataDic.TryGetValue(classType, out classData))
            {
                if(childClassData != null)
                    classData.Children.Add(childClassData);
                return;
            }
            var baseType = classType.BaseType;
            var atts = classType.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassAttribute), false);
            if (atts.Length > 0)
            {
                var att = atts[0] as EngineNS.Editor.Editor_MacrossClassAttribute;
                if(att.HasType(EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable))
                {
                    topClassType = classType;
                    var data = new ClassData()
                    {
                        Name = classType.Name,
                        ClassType = classType,
                    };
                    data.CSType = att.CSType;
                    if (childClassData != null)
                        data.Children.Add(childClassData);
                    var mtAtts = classType.GetCustomAttributes(typeof(EngineNS.Macross.MacrossTypeClassAttribute), false);
                    if (mtAtts.Length > 0)
                        data.IsMacrossType = true;
                    else
                        data.IsMacrossType = false;
                    classDataDic[classType] = data;
                    if (baseType == null)
                    {
                        topClassTypes.Add(topClassType);
                        return;
                    }
                    childClassData = data;
                }
            }

            checkedClasses.Add(classType, classType);
            CollectClassType(baseType, topClassTypes, classDataDic, childClassData, topClassType, checkedClasses);
        }
        void InitClassShow()
        {
            mClassDataDic.Clear();
            mAllClasses.Clear();
            mViewClasses.Clear();

            var assemblys = EngineNS.Rtti.RttiHelper.GetAnalyseAssemblys(EngineNS.ECSType.All);
            List<Type> topClassTypes = new List<Type>();
            Dictionary<Type, Type> checkedClasses = new Dictionary<Type, Type>();
            foreach (var assembly in assemblys)
            {
                foreach(var type in assembly.GetTypes())
                {
                    CollectClassType(type, topClassTypes, mClassDataDic, null, null, checkedClasses);
                }
            }

            // Macross
            ////var macrossAssembly = EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Common, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
            ////if(macrossAssembly != null)
            ////{
            ////    foreach (var type in macrossAssembly.GetTypes())
            ////    {
            ////        CollectClassType(type, topClassTypes, mClassDataDic, null, null, checkedClasses);
            ////    }
            ////}

            foreach (var topType in topClassTypes)
            {
                ClassData data;
                if(mClassDataDic.TryGetValue(topType, out data))
                {
                    mAllClasses.Add(data);
                }
            }
            ShowItemWithFilter();
        }

        void ShowItemWithFilter()
        {
            TreeView_Classes.ItemsSource = null;
            mViewClasses.Clear();
            ShowItemWithFilter(mAllClasses, ref mViewClasses, FilterString);
            TreeView_Classes.ItemsSource = mViewClasses;
        }
        bool ShowItemWithFilter(ObservableCollection<ClassData> srcDatas, ref ObservableCollection<ClassData> tagDatas, string filterString)
        {
            tagDatas.Clear();

            bool retValue = false;
            foreach(var data in srcDatas.OfType<ClassData>())
            {
                ClassData tagData = null;
                if(string.IsNullOrEmpty(filterString))
                {
                    tagData = new ClassData();
                    data.CopyTo(tagData);
                    tagData.HightLightString = filterString;
                    tagDatas.Add(tagData);
                    retValue = true;
                }
                else if(data.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    tagData = new ClassData();
                    data.CopyTo(tagData);
                    tagData.HightLightString = filterString;
                    tagDatas.Add(tagData);
                    retValue = true;
                }

                if(data.Children.Count > 0)
                {
                    var children = new ObservableCollection<ClassData>();
                    var bFind = ShowItemWithFilter(data.Children, ref children, filterString);
                    if(bFind)
                    {
                        if(tagData == null)
                        {
                            tagData = new ClassData();
                            data.CopyTo(tagData);
                            tagData.HightLightString = filterString;
                            tagDatas.Add(tagData);
                        }
                        tagData.Children = children;
                        tagData.IsExpanded = true;
                        retValue = true;
                    }
                }
            }

            return retValue;
        }

        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData { get; set; }
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

        private void TreeView_Classes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var data = e.NewValue as ClassData;
            if(data != null)
            {
                mCreateData.ClassType = data.ClassType;
                mCreateData.CSType = data.CSType;
                mCreateData.IsMacrossType = data.IsMacrossType;
            }
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
    }
}
