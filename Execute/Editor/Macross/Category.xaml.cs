using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace Macross
{
    public interface ICategoryItemPropertyClass : EngineNS.IO.Serializer.ISerializer
    {
        INodesContainerDicKey GetNodesContainerDicKey(Guid keyId);
    }
    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [EngineNS.Rtti.MetaClass]
    public class VariableCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, ICategoryItemPropertyClass, CodeDomNode.IVariableTypeChangeProcessClass
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region ISerializer
        public void ReadObject(IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }
        public void ReadObject(IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void ReadObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObject(IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }

        public void WriteObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        public INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }

        public delegate bool Delegate_IsNewVariableNameValid(string newName);
        public Delegate_IsNewVariableNameValid OnIsNewVariableNameValid;
        public delegate void Delegate_ProcessTypeChanged(CodeDomNode.VariableType varType);
        public Delegate_ProcessTypeChanged OnProcessTypeChanged;

        //public delegate void Delegate_VariableNameChanged(string name);
        //public Delegate_VariableNameChanged OnVariableNameChanged;

        //static void SVariableNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    //var ctrl = sender as VariableCategoryItemPropertys;
        //    //ctrl.OnVariableNameChanged?.Invoke(ctrl.VariableName);
        //}

        [EngineNS.Rtti.MetaData]
        public bool VariableTypeReadOnly
        {
            get;
            set;
        } = false;

        [EngineNS.Rtti.MetaData]
        public string VariableName
        {
            get { return (string)GetValue(VariableNameProperty); }
            set { SetValue(VariableNameProperty, value); }
        }
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(VariableCategoryItemPropertys), new PropertyMetadata(""));

        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("TypeSetterTemplate")]
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.VariableType VariableType
        {
            get { return (CodeDomNode.VariableType)GetValue(VariableTypeProperty); }
            set { SetValue(VariableTypeProperty, value); }
        }
        public static readonly DependencyProperty VariableTypeProperty = DependencyProperty.Register("VariableType", typeof(CodeDomNode.VariableType), typeof(VariableCategoryItemPropertys), new PropertyMetadata());

        public void OnVariableTypeChanged(CodeDomNode.VariableType variableType)
        {
            OnProcessTypeChanged?.Invoke(variableType);
        }

        [EngineNS.Rtti.MetaData]
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(VariableCategoryItemPropertys), new UIPropertyMetadata(""));

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(VariableCategoryItemPropertys), new UIPropertyMetadata(""));

        bool mIsNeedMetaAttr;
        [DisplayName("变量信息是否保存")]
        public bool IsNeedMetaAttr
        {
            get
            {
                return mIsNeedMetaAttr;
            }
            set
            {
                mIsNeedMetaAttr = value;
                if (mIsNeedMetaAttr)
                {
                    AddAttribute(typeof(EngineNS.Rtti.MetaDataAttribute).FullName);
                }
                else
                {
                    DeleteAttribute(typeof(EngineNS.Rtti.MetaDataAttribute).FullName);
                }
                OnPropertyChanged("IsNeedMetaAttr");
            }
        }

        bool mMacossKey;
        [DisplayName("变量是否为显示主键")]
        public bool IsMacossKey
        {
            get
            {
                return mMacossKey;
            }
            set
            {
                mMacossKey = value;
                if (mMacossKey)
                {
                    AddAttribute(typeof(EngineNS.Editor.MacrossClassKeyAttribute).FullName);
                }
                else
                {
                    DeleteAttribute(typeof(EngineNS.Editor.MacrossClassKeyAttribute).FullName);
                }
                OnPropertyChanged("IsMacossKey");
            }
        }

        void AttributeTypeProxysAddAction()
        {

        }
        void AttributeTypeProxysRemoveAction()
        {

        }

        public class CustomAddRemoveAttributeTypeProxysPrivider : EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute.AddRemoveActionProviderBase
        {
            public override object[] Add()
            {
                var window = new CreateAttribute(CreateAttribute.Attribute.Property);
                window.ShowDialog();
                if (window.CurrentAttributeTypes == null || window.CurrentAttributeTypes.Count == 0)
                    return null;

                //ObservableCollection<AttributeType> attributes = sender as ObservableCollection<AttributeType>;
                return window.CurrentAttributeTypes.ToArray();
            }

            public override object[] Insert()
            {
                return Add();
            }

            public override bool Remove(object item)
            {
                return true;
            }
        }

        ObservableCollection<AttributeType> mAttributeTypeProxys = new ObservableCollection<AttributeType>();
        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute(typeof(CustomAddRemoveAttributeTypeProxysPrivider))]
        public ObservableCollection<AttributeType> AttributeTypeProxys
        {
            get
            {
                return mAttributeTypeProxys;
            }
            set
            {
                mAttributeTypeProxys = value;
                foreach (var i in mAttributeTypeProxys)
                {
                    if (i.AttributeName.Equals(typeof(EngineNS.Rtti.MetaDataAttribute).FullName))
                    {
                        mIsNeedMetaAttr = true;
                        OnPropertyChanged("IsNeedMetaAttr");
                    }
                    else if (i.AttributeName.Equals(typeof(EngineNS.Editor.MacrossClassKeyAttribute).FullName))
                    {
                        mMacossKey = true;
                        OnPropertyChanged("IsMacossKey");
                    }
                }
            }
        }

        public VariableCategoryItemPropertys()
        {
            VariableType = new CodeDomNode.VariableType();
            //mAttributeTypeProxys.SetProxy(mAttributeTypeProxys);
            mAttributeTypeProxys.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }
        public VariableCategoryItemPropertys(EngineNS.ECSType csType)
        {
            VariableType = new CodeDomNode.VariableType(csType);
            //mAttributeTypeProxys.SetProxy(mAttributeTypeProxys);
            mAttributeTypeProxys.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        static AttributeManager AttributeManager = new AttributeManager();
        public void AddAttribute(string name)
        {
            foreach (var i in AttributeManager.PropertyAttribute)
            {
                if (i.Value.AttributeName.Equals(name))
                {
                    AttributeTypeProxys.Add(i.Value);
                    break;
                }
            }
            OnPropertyChanged("AttributeTypeProxys");
        }

        public void DeleteAttribute(string name)
        {
            foreach (var i in mAttributeTypeProxys)
            {
                if (i.AttributeName.Equals(name))
                {
                    AttributeTypeProxys.Remove(i);
                    break;
                }
            }
            OnPropertyChanged("AttributeTypeProxys");
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AttributeType attributetype = e.NewItems[0] as AttributeType;
                if (attributetype != null)
                {
                    if (attributetype.AttributeName.Equals(typeof(EngineNS.Rtti.MetaDataAttribute).FullName))
                    {
                        mIsNeedMetaAttr = true;

                        OnPropertyChanged("IsNeedMetaAttr");
                    }
                    else if (attributetype.AttributeName.Equals(typeof(EngineNS.Editor.MacrossClassKeyAttribute).FullName))
                    {
                        mMacossKey = true;
                        OnPropertyChanged("IsMacossKey");
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                AttributeType attributetype = e.OldItems[0] as AttributeType;
                if (attributetype != null)
                {
                    if (attributetype.AttributeName.Equals(typeof(EngineNS.Rtti.MetaDataAttribute).FullName))
                    {
                        mIsNeedMetaAttr = false;

                        OnPropertyChanged("IsNeedMetaAttr");
                    }
                    else if (attributetype.AttributeName.Equals(typeof(EngineNS.Editor.MacrossClassKeyAttribute).FullName))
                    {
                        mMacossKey = false;
                        OnPropertyChanged("IsMacossKey");
                    }
                }
            }

        }
    }

    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [EngineNS.Rtti.MetaClass]
    public class GraphCategoryItemPropertys : DependencyObject, ICategoryItemPropertyClass, InputWindow.IInputErrorCheckClass
    {
        #region ISerializer
        public void ReadObject(IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }
        public void ReadObject(IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void ReadObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObject(IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }

        public void WriteObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        #region inputErrorCheck
        public ValidationResult IsInputValidate(object value, CultureInfo cultureInfo)
        {
            var nameVal = (string)value;
            if (HostCategoryItem != null)
            {
                foreach (var item in HostCategoryItem.ParentCategory.Items)
                {
                    if (item == HostCategoryItem)
                        continue;
                    if (item.Name == nameVal)
                        return new ValidationResult(false, "已经包含同名图");
                }
            }

            return ValidationResult.ValidResult;
        }

        #endregion

        public INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.Editor_InputWithErrorCheckAttribute]
        public string GraphName
        {
            get { return (string)GetValue(GraphNameProperty); }
            set { SetValue(GraphNameProperty, value); }
        }
        public static readonly DependencyProperty GraphNameProperty = DependencyProperty.Register("GraphName", typeof(string), typeof(GraphCategoryItemPropertys), new UIPropertyMetadata("", null));

        [EngineNS.Rtti.MetaData]
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(GraphCategoryItemPropertys), new UIPropertyMetadata(""));

        public CategoryItem HostCategoryItem;
    }

    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [EngineNS.Rtti.MetaClass]
    public class AttributeCategoryItemPropertys : DependencyObject, ICategoryItemPropertyClass
    {
        #region ISerializer
        public void ReadObject(IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }
        public void ReadObject(IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void ReadObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObject(IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }

        public void WriteObjectXML(XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion
        //[EngineNS.Rtti.MetaData]
        //public string VariableName
        //{
        //    get { return (string)GetValue(VariableNameProperty); }
        //    set { SetValue(VariableNameProperty, value); }
        //}
        //public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(VariableCategoryItemPropertys), new PropertyMetadata(""));

        //public static readonly DependencyProperty VariableTypeProperty = DependencyProperty.Register("VariableType", typeof(CodeDomNode.VariableType), typeof(VariableCategoryItemPropertys), new PropertyMetadata());

        //public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(VariableCategoryItemPropertys), new UIPropertyMetadata(""));

        //public List<AttributeType.Constructors> AttributeParams
        //{
        //    get;
        //    set;
        //}
        public INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }

        [Browsable(false)]
        private AttributeType mAttributeType;

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public AttributeType AttributeType
        {
            get
            {
                return mAttributeType;
            }
            set
            {
                mAttributeType = value;
                //重新设置下Constructors的host
                if (mAttributeType != null)
                {
                    foreach (var i in mAttributeType.ConstructorParamers)
                    {
                        i.Host = mAttributeType;
                    }
                }
            }
        }

        public AttributeCategoryItemPropertys()
        {
        }

        public bool LoadXnd(EngineNS.IO.XndNode node)
        {
            bool result = true;
            if (AttributeType != null)
            {
                foreach (Constructors c in AttributeType.ConstructorParamers)
                {
                    foreach (var p in c.Paramers)
                    {
                        result = result | p.LoadXnd(node);

                    }
                }
            }

            return result;
        }

        public void Save2Xnd(EngineNS.IO.XndNode node)
        {
            //var 
            if (AttributeType != null)
            {
                foreach (Constructors c in AttributeType.ConstructorParamers)
                {
                    foreach (var p in c.Paramers)
                    {
                        p.Save2Xnd(node);

                    }
                }
            }

        }
    }

    [EngineNS.Rtti.MetaClass]
    public class CategoryItem : DependencyObject, EditorCommon.DragDrop.IDragAbleObject, CodeDomNode.CustomMethodInfo.IFunctionOutParamOperation, INotifyPropertyChanged, INodesContainerDicKey
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region INodesContainerDicKey
        public EngineNS.ECSType CSType
        {
            get
            {
                return ParentCategory.HostControl.HostControl.CSType;
            }
        }
        [EngineNS.Rtti.MetaData]
        public Guid Id
        {
            get;
            set;
        } = Guid.NewGuid();

        [EngineNS.Rtti.MetaData]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(CategoryItem), new UIPropertyMetadata("", OnNamePropertyChanged, OnNamePropertyCVCallback));
        private static void OnNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as CategoryItem;
            
            if (ctrl.isLoading)
                return;
            var newValue = (string)e.NewValue;
            var oldValue = (string)e.OldValue;
            ctrl.OnNameChangedEvent?.Invoke(ctrl, newValue, oldValue);
            ctrl.OnPropertyChanged("ShowName");
        }

        async Task ProcessNamePropertyChanged(string oldValue, string newValue)
        {
            foreach (var data in InstanceNodes)
            {
                var container = await ParentCategory.HostControl.HostControl.GetNodesContainer(data.ContainerKeyId, true);
                var node = container.FindControl(data.NodeId);
                var param = node.CSParam as CodeDomNode.PropertyNode.PropertyNodeConstructionParams;
                param.PropertyInfo.PropertyName = newValue;
            }
        }

        string mShowName;
        public string ShowName
        {
            get
            {
                if (string.IsNullOrEmpty(mShowName))
                    return Name;
                return mShowName;
            }
            set
            {
                mShowName = value;
                OnPropertyChanged("ShowName");
            }
        }
        [EngineNS.Rtti.MetaData]
        public enCategoryItemType CategoryItemType
        {
            get;
            set;
        } = enCategoryItemType.Unknow;
        public void ProcessOnNodesContainerShow(NodesControlAssist.ProcessData processData)
        {
            switch(CategoryItemType)
            {
                case enCategoryItemType.CustomFunction:
                case enCategoryItemType.OverrideFunction:
                //case enCategoryItemType.Property:
                    {
                        ParentCategory.HostControl.ProcessOnNodesContainerShow(this, processData);
                    }
                    break;
            }
        }

        public void ProcessOnNodesContainerHide(NodesControlAssist.ProcessData processData)
        {
            switch(CategoryItemType)
            {
                case enCategoryItemType.CustomFunction:
                case enCategoryItemType.OverrideFunction:
                //case enCategoryItemType.Property:
                    {
                        ParentCategory.HostControl.ProcessOnNodesContainerHide(this, processData);
                    }
                    break;
            }
        }
        #endregion

        public bool IsShow
        {
            get { return (bool)GetValue(IsShowProperty); }
            set { SetValue(IsShowProperty, value); }
        }

        Visibility mCheckVisibility = Visibility.Collapsed;
        public Visibility CheckVisibility
        {
            get => mCheckVisibility;
            set
            {
                mCheckVisibility = value;
                OnPropertyChanged("CheckVisibility");
            }
        }

        [EngineNS.Rtti.MetaData]
        public string CommentName
        {
            get;
            set;
        }

        public delegate void OnNameChangedDelegate(CategoryItem item, string newvalue, string oldvalue);
        public event OnNameChangedDelegate OnNameChangedEvent;

        public delegate void OnIsShowChangedDelegate(CategoryItem item, bool newvalue, bool oldvalue);
        public event OnIsShowChangedDelegate OnIsShowChanged;

        public static readonly DependencyProperty IsShowProperty = DependencyProperty.Register("IsShow", typeof(bool), typeof(CategoryItem), new UIPropertyMetadata(true, OnIsShowPropertyChanged));


        private static void OnIsShowPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as CategoryItem;

            if (ctrl.isLoading)
                return;
            var newValue = (bool)e.NewValue;
            var oldValue = (bool)e.OldValue;
            
            ctrl.OnIsShowChanged?.Invoke(ctrl, newValue, oldValue);
        }
        private static object OnNamePropertyCVCallback(DependencyObject d, object value)
        {
            var ctrl = d as CategoryItem;
            string strvalue = (string)value;
            string ctrname = ctrl.Name;
            if (string.IsNullOrEmpty(ctrl.CommentName) == false)
            {
                if (strvalue.IndexOf(ctrl.CommentName) == -1)
                {
                    strvalue = strvalue + "(" + ctrl.CommentName + ")";
                }


                if (ctrname.IndexOf(ctrl.CommentName) == -1)
                {
                    ctrname = ctrname + "(" + ctrl.CommentName + ")";
                }
            }

            if (ctrl.isLoading)
                return strvalue;


            var newName = (string)value;

            string result;
            
            if (string.IsNullOrEmpty(ctrl.ToolTips) == false)
            {
                newName = newName.Replace(ctrl.ToolTips, "");
            }


            if (string.IsNullOrEmpty(ctrl.CommentName) == false)
            {
                newName = newName.Replace(ctrl.CommentName, "");
            }
            //else
            //{
            //    string pattern = "[\u4e00-\u9fbb]";
            //    if (System.Text.RegularExpressions.Regex.IsMatch(newName, pattern))
            //    {
            //        var index = newName.IndexOf("(");
            //        var index2 = newName.LastIndexOf(")");
            //        ctrl.CommentName = newName.Substring(index + 1, newName.Length - 1 - index);
            //    }

            //    if (string.IsNullOrEmpty(ctrl.CommentName) == false)
            //    {
            //        newName = newName.Replace(ctrl.CommentName, "");
            //    }
            //}

            if (ctrl.CheckNameFormat(newName, out result) == false)
            {
                EditorCommon.MessageBox.Show(result);
                return ctrname;
            }

            if (ctrl.CheckNameValid(newName) == true)
            {
                return strvalue;
            }
            else
            {
                EditorCommon.MessageBox.Show($"无法改名为{newName}, 已包含同名变量");
                return ctrname;
            }
        }

        bool CheckNameFormat(string name, out string result)
        {
            if (name.IndexOf(" ") != -1)
            {
                result = "名字中不能包含空格！";
                return false;
            }


            string pattern = "[\u4e00-\u9fbb]";
            if (System.Text.RegularExpressions.Regex.IsMatch(name, pattern))
            {
                result = "名字中不能包含中文！";
                return false;
            }

            int num;
            if (name != "")
            {
                if (int.TryParse(name.Substring(0, 1), out num))
                {
                    result = "名字中首字母不能是数字！";
                    return false;
                }
            }
            result = "";
            return true;
        }
        bool CheckNameValid(string name)
        {
            foreach (var item in ParentCategory.Items)
            {
                if (item == this)
                    continue;
                if (item.Name == name)
                    return false;
            }
            return true;
        }

        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(CategoryItem), new UIPropertyMetadata(""));
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(CategoryItem), new UIPropertyMetadata(null));
        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(CategoryItem), new UIPropertyMetadata(Visibility.Visible));
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CategoryItem), new UIPropertyMetadata(true));
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(CategoryItem), new UIPropertyMetadata(false));

        //[EngineNS.Rtti.MetaData]
        //public bool Editable
        //{
        //    get { return (bool)GetValue(EditableProperty); }
        //    set { SetValue(EditableProperty, value); }
        //}
        //public static readonly DependencyProperty EditableProperty = DependencyProperty.Register("Editable", typeof(bool), typeof(CategoryItem), new UIPropertyMetadata(true, OnEditablePropertyChanged));
        //private static void OnEditablePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = sender as CategoryItem;
        //    var newValue = (bool)e.NewValue;
        //}
        [EngineNS.Rtti.MetaData]
        public string ToolTips
        {
            get { return (string)GetValue(ToolTipsProperty); }
            set { SetValue(ToolTipsProperty, value); }
        }
        public static readonly DependencyProperty ToolTipsProperty = DependencyProperty.Register("ToolTips", typeof(string), typeof(CategoryItem), new UIPropertyMetadata(""));

        [EngineNS.Rtti.MetaData]
        public ICategoryItemPropertyClass PropertyShowItem;

        [EngineNS.Rtti.MetaClass]
        public class InstanceData : EngineNS.IO.Serializer.Serializer
        {
            [EngineNS.Rtti.MetaData]
            public Guid NodeId { get; set; }
            [EngineNS.Rtti.MetaData]
            public Guid ContainerKeyId { get; set; }
        }

        [EngineNS.Rtti.MetaData]
        public List<InstanceData> InstanceNodes
        {
            get;
            set;
        } = new List<InstanceData>();

        // 枚举存盘，请勿随意修改
        public enum enCategoryItemType
        {
            Unknow               = 0,
            MainGraph            = 1,
            Graph                = 2,
            Event                = 3,
            OverrideFunction     = 4,
            CustomFunction       = 5,
            Variable             = 6,
            VariableCategory     = 7,
            BehaviorTree         = 8,
            Atrribute            = 9,
            ParticleEmitter      = 10,
            LogicAnimGraph       = 11,
            LogicAnimPostProcess = 12,
            LogicAnimGraphLayer  = 13,
            Property             = 14,
            FunctionVariable     = 15,
        }

        // 与节点图相关联
        public bool HasLinkedNodesContainer = true;

        [EngineNS.Rtti.MetaData]
        public bool CanDrag
        {
            get;
            set;
        } = false;
        private CategoryItem mParent;
        public CategoryItem Parent
        {
            get => mParent;
        }
        public ObservableCollection<CategoryItem> Children
        {
            get;
            set;
        } = new ObservableCollection<CategoryItem>();

        ContextMenu mCategoryItemContextMenu;
        public ContextMenu CategoryItemContextMenu
        {
            get => mCategoryItemContextMenu;
            set
            {
                mCategoryItemContextMenu = value;
                OnPropertyChanged("ContextMenu");
            }
        }

        public delegate void Delegate_OnDoubleClick(CategoryItem item);
        public event Delegate_OnDoubleClick OnDoubleClick;

        internal void InvokeOnDoubleClick()
        {
            OnDoubleClick?.Invoke(this);
        }

        Category mParentCategory;
        public Category ParentCategory
        {
            get
            {
                if (mParent == null)
                    return mParentCategory;
                else
                    return mParent.ParentCategory;
            }
        }
        public void SetParentCategory(Category parent)
        {
            mParentCategory = parent;
        }
        public CategoryItem(CategoryItem parent, Category parentCategory)
        {
            mParent = parent;
            mParentCategory = parentCategory;
        }
        void CategoryItem_CtrlNodesControlDeleteNode(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            for(int i=InstanceNodes.Count - 1; i>=0; i--)
            {
                if (InstanceNodes[i].NodeId == node.Id)
                    InstanceNodes.RemoveAt(i);
            }
        }
        //void CustomFunction_CustomMethodInfo_OnAddParam(CodeDomNode.CustomMethodInfo.FunctionParam param)
        public async Task OnAddedOutParam(CodeDomNode.CustomMethodInfo.FunctionParam param)
        {
            var nodesContainer = await mHostLinkControl.GetNodesContainer(this, true);
            var item = PropertyShowItem as CodeDomNode.CustomMethodInfo;
            // 没有Return的话增加Return
            bool hasReturn = false;
            for (int i = 0; i < nodesContainer.NodesControl.CtrlNodeList.Count; i++)
            {
                if (nodesContainer.NodesControl.CtrlNodeList[i] is CodeDomNode.ReturnCustom)
                {
                    hasReturn = true;
                    break;
                }
            }
            if (!hasReturn)
            {
                var nodeType = typeof(CodeDomNode.ReturnCustom);
                var csParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = nodesContainer.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    MethodInfo = item,
                    ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var node = nodesContainer.NodesControl.AddNodeControl(nodeType, csParam, 300, 0);
            }
        }
#pragma warning disable 1998
        public async Task OnInsertOutParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam param) { }
        public async Task OnRemovedOutParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam param) { }
#pragma warning restore 1998
        IMacrossOperationContainer mHostLinkControl;
        public IMacrossOperationContainer HostLinkControl => mHostLinkControl;

        public void SetHostLinkControl(IMacrossOperationContainer ctr)
        {
            mHostLinkControl = ctr;
        }

        [MetaClass]
        public class InitializeData : EngineNS.IO.Serializer.Serializer
        {
            [MetaData]
            public bool Deleteable { get; set; }
            public void Reset()
            {
                Deleteable = true;
            }
        }
        [MetaData]
        public InitializeData InitData
        {
            get;
            set;
        } = new InitializeData();

        static Dictionary<string, Action<CategoryItem, IMacrossOperationContainer, InitializeData>> mInitActionDic = new Dictionary<string, Action<CategoryItem, IMacrossOperationContainer, InitializeData>>();
        public static void RegistInitAction(string typeStr, Action<CategoryItem, IMacrossOperationContainer, InitializeData> initAction)
        {
            mInitActionDic[typeStr] = initAction;
        }
        public static void UnRegistInitAction(string typeStr)
        {
            mInitActionDic.Remove(typeStr);
        }

        public struct stDropToNodesControlActionData
        {
            public CodeGenerateSystem.Base.INodesContainerHost NodesContainerHost;
            public Canvas DropCanvas;
            public Point DropPos;
        }
        public Action<stDropToNodesControlActionData> OnDropToNodesControlAction;
        public struct stDropVariableActionData
        {
            public CodeGenerateSystem.Base.INodesContainerHost ContainerHost;
            public CodeGenerateSystem.Controls.NodesContainerControl NodesContainer;
            public Point Pos;
        }
        public Action<stDropVariableActionData> OnDropVariableGetNodeControlAction;
        public Action<stDropVariableActionData> OnDropVariableSetNodeControlAction;

        [MetaData]
        public string InitTypeStr
        {
            get;
            set;
        } = "";

        public void Initialize(IMacrossOperationContainer ctrl, InitializeData data)
        {
            InitData = data;
            mHostLinkControl = ctrl;
            ctrl.NodesCtrlAssist.OnDeletedNodeControl -= CategoryItem_CtrlNodesControlDeleteNode;
            ctrl.NodesCtrlAssist.OnDeletedNodeControl += CategoryItem_CtrlNodesControlDeleteNode;

            if(CategoryItemContextMenu == null)
            {
                CategoryItemContextMenu = new ContextMenu();
                CategoryItemContextMenu.Style = ctrl.NodesCtrlAssist.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style;
            }

            var userCtrl = ctrl as UserControl;
            switch (CategoryItemType)
            {
                case enCategoryItemType.MainGraph:
                    {
                        Icon = userCtrl.TryFindResource("Icon_Graph") as ImageSource;
                        OnDoubleClick += (categoryItem) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(categoryItem);
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "MainGraphOpenGraph",
                            Header = "打开",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(this);
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);
                    }
                    break;
                case enCategoryItemType.Graph:
                    {
                        Icon = userCtrl.TryFindResource("Icon_Graph") as ImageSource;
                        OnDoubleClick += (categoryItem) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(categoryItem);
                        };

                        if (PropertyShowItem == null)
                            PropertyShowItem = new GraphCategoryItemPropertys();
                        var varItemPro = PropertyShowItem as GraphCategoryItemPropertys;
                        varItemPro.HostCategoryItem = this;
                        BindingOperations.SetBinding(varItemPro, GraphCategoryItemPropertys.GraphNameProperty, new Binding("Name") { Source = this, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(varItemPro, GraphCategoryItemPropertys.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });
                        var menuItem = new MenuItem()
                        {
                            Name = "GraphOpenGraph",
                            Header = "打开",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(this);
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);
                        if(data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "GraphDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                    ctrl.RemoveNodesContainer(this);
                                    var fileName = ctrl.GetGraphFileName(this.Name);
                                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);

                                    ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.CustomFunction:
                    {
                        if (Parent != null && Parent.CategoryItemType == enCategoryItemType.ParticleEmitter)
                        {
                            Visibility = Visibility.Collapsed;
                        }
                        Icon = userCtrl.TryFindResource("Icon_Function") as ImageSource;

                        Delegate_OnDoubleClick action = async (categoryItem) =>
                        {
                            var nodesContainer = await ctrl.ShowNodesContainer(categoryItem);
                            //for (int i = 0; i < nodesContainer.NodesControl.CtrlNodeList.Count; i++)
                            //{
                            //    var methodInfo = PropertyShowItem as CodeDomNode.CustomMethodInfo;
                            //    var node = nodesContainer.NodesControl.CtrlNodeList[i];
                            //    if (node is CodeDomNode.MethodCustom)
                            //    {
                            //        var methodNode = node as CodeDomNode.MethodCustom;
                            //        var param = methodNode.CSParam as CodeDomNode.MethodCustom.MethodCustomConstructParam;
                            //        if (param.MethodInfo != methodInfo)
                            //        {
                            //            methodNode.ResetMethodInfo(methodInfo);
                            //        }
                            //    }
                            //    else if (node is CodeDomNode.ReturnCustom)
                            //    {
                            //        var returnNode = node as CodeDomNode.ReturnCustom;
                            //        var param = returnNode.CSParam as CodeDomNode.ReturnCustom.ReturnCustomConstructParam;
                            //        if (param.MethodInfo != PropertyShowItem)
                            //        {
                            //            returnNode.ResetMethodInfo(methodInfo);
                            //        }
                            //    }
                            //}
                        };
                        OnDoubleClick += action;

                        if (PropertyShowItem == null)
                        {
                            PropertyShowItem = new CodeDomNode.CustomMethodInfo();
                        }
                        var itemPro = PropertyShowItem as CodeDomNode.CustomMethodInfo;
                        //itemPro.OnAddedOutParam -= CustomFunction_CustomMethodInfo_OnAddParam;
                        //itemPro.OnAddedOutParam += CustomFunction_CustomMethodInfo_OnAddParam;
                        itemPro.AddOutParamOperation(this);
                        itemPro.CSType = ctrl.CSType;
                        itemPro.MethodName = Name;
                        BindingOperations.SetBinding(this, NameProperty, new Binding("MethodName") { Source = itemPro, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(itemPro, CodeDomNode.CustomMethodInfo.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });

                        this.OnDropToNodesControlAction = (dropData) =>
                        {
                            var nodeCtrl = EditorCommon.Program.GetParent(dropData.DropCanvas, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
                            var nodeType = typeof(CodeDomNode.MethodCustomInvoke);
                            var csParam = new CodeDomNode.MethodCustomInvoke.MethodCustomInvokeConstructParam();
                            csParam.CSType = CSType;
                            csParam.HostNodesContainer = nodeCtrl;
                            csParam.ConstructParam = "";
                            csParam.MethodInfo = itemPro;
                            var pos = nodeCtrl._RectCanvas.TranslatePoint(dropData.DropPos, nodeCtrl._MainDrawCanvas);
                            var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                            this.AddInstanceNode(nodeCtrl.GUID, node);
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "CustomFunctionOpenGraph",
                            Header = "打开",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                            action.Invoke(this);
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);
                        if(data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "CustomFunctionDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                    ctrl.RemoveNodesContainer(this);
                                    var fileName = ctrl.GetGraphFileName(this.Name);
                                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);

                                    ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.OverrideFunction:
                    {
                        ctrl.MacrossOpPanel.CreatedOverrideMethods.Add(Name);
                        Icon = userCtrl.TryFindResource("Icon_OverrideFunction") as ImageSource;
                        OnDoubleClick += (categoryItem) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(categoryItem);
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "OverrideFunctionOpenGraph",
                            Header = "打开",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                            var noUse = ctrl.ShowNodesContainer(this);
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);
                        if(data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "OverrideFunctionDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                    ctrl.RemoveNodesContainer(this);
                                    ctrl.MacrossOpPanel.CreatedOverrideMethods.Remove(Name);
                                    var fileName = ctrl.GetGraphFileName(this.Name);
                                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);

                                    ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.Event:
                    {
                        ctrl.MacrossOpPanel.CreatedOverrideMethods.Add(Name);

                        Icon = userCtrl.TryFindResource("Icon_Event") as ImageSource;
                        OnDoubleClick += async (categoryItem) =>
                        {
                            // 显示到前台
                            var parent = categoryItem.Parent;
                            while (parent != null && parent.CategoryItemType != enCategoryItemType.MainGraph)
                            {
                                parent = parent.Parent;
                            }
                            if (parent != null)
                                await ctrl.ShowNodesContainer(parent);
                            else
                                throw new InvalidOperationException("这里不应该为空");
                            List<CodeGenerateSystem.Base.BaseNodeControl> nodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();
                            foreach (var idData in InstanceNodes)
                            {
                                var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                nodes.Add(container.FindControl(idData.NodeId));
                            }
                            ctrl.NodesCtrlAssist.NodesControl.FocusNodes(nodes.ToArray());
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "EventFocus",
                            Header = "聚焦",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        menuItem.Click += async (object sender, RoutedEventArgs e) =>
                        {
                            List<CodeGenerateSystem.Base.BaseNodeControl> nodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();
                            foreach (var idData in InstanceNodes)
                            {
                                var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                nodes.Add(container.FindControl(idData.NodeId));
                            }
                            ctrl.NodesCtrlAssist.NodesControl.FocusNodes(nodes.ToArray());
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);
                        if(data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "EventDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += async (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    this.mParent.Children.Remove(this);
                                    for (int i = InstanceNodes.Count - 1; i >= 0; i--)
                                    {
                                        var idData = InstanceNodes[i];
                                        var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                        var node = container.FindControl(idData.NodeId);
                                        if (node != null)
                                            ctrl.DeleteNode(node);
                                    }
                                    ctrl.MacrossOpPanel.CreatedOverrideMethods.Remove(Name);

                                    ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.Variable:
                case enCategoryItemType.FunctionVariable:
                    {
                        //OnDoubleClick += (categoryItem) =>
                        //{
                        //    List<CodeGenerateSystem.Base.BaseNodeControl> nodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();
                        //    foreach(var id in InstanceNodes)
                        //    {
                        //        nodes.Add(ctrl.NodesControl.FindControl(id));
                        //    }
                        //    ctrl.NodesControl.FocusNodes(nodes.ToArray());
                        //};
                        HasLinkedNodesContainer = false;
                        if (PropertyShowItem == null)
                        {
                            PropertyShowItem = new VariableCategoryItemPropertys(mParentCategory.HostControl.HostControl.CSType);
                        }

                        var varItemPro = PropertyShowItem as VariableCategoryItemPropertys;

                        varItemPro.OnIsNewVariableNameValid = (newName) =>
                        {
                            return CheckNameValid(newName);
                        };
                        varItemPro.OnProcessTypeChanged = async (CodeDomNode.VariableType varType) =>
                        {
                            foreach (var idData in InstanceNodes)
                            {
                                var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                var node = container.FindControl(idData.NodeId) as CodeDomNode.PropertyNode;
                                node?.ChangeValueType(varType);
                            }
                        };
                        OnNameChangedEvent += (categoryItem, newValue, oldValue) =>
                        {
                            var noUse = categoryItem.ProcessNamePropertyChanged(oldValue, newValue);
                        };
                        BindingOperations.SetBinding(varItemPro, VariableCategoryItemPropertys.VariableNameProperty, new Binding("Name") { Source = this, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(varItemPro, VariableCategoryItemPropertys.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });
                        switch (varItemPro.VariableType.ArrayType)
                        {
                            case CodeDomNode.VariableType.enArrayType.Array:
                                Icon = userCtrl.TryFindResource("Icon_PillArray") as ImageSource;
                                break;
                            case CodeDomNode.VariableType.enArrayType.Single:
                                Icon = userCtrl.TryFindResource("Icon_Pill") as ImageSource;
                                break;
                        }

                        this.OnDropToNodesControlAction = (dropData) =>
                        {
                            var nodesControlAssist = dropData.NodesContainerHost as Macross.NodesControlAssist;
                            nodesControlAssist.ShowGetButton = true;
                            nodesControlAssist.ShowSetButton = true;
                            nodesControlAssist.InitVariableDropShow(dropData.DropPos);
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "VariableFocus",
                            Header = "查找引用",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            IsEnabled = false,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);

                        if(data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "VariableDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += async (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                    for (int i = InstanceNodes.Count - 1; i >= 0; i--)
                                    {
                                        var idData = InstanceNodes[i];
                                        var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                        var node = container.FindControl(idData.NodeId);
                                        if (node != null)
                                            ctrl.DeleteNode(node);
                                    }
                                    
                                    ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.Property:
                    {
                        bool firstSet = false;
                        if(PropertyShowItem == null)
                        {
                            var varProItem = new CategoryItemProperty_Property(mParentCategory.HostControl.HostControl.CSType);
                            varProItem.GetMethodInfo = new CodeDomNode.CustomMethodInfo();
                            varProItem.SetMethodInfo = new CodeDomNode.CustomMethodInfo();
                            PropertyShowItem = varProItem;
                            firstSet = true;
                        }

                        var varItemPro = PropertyShowItem as CategoryItemProperty_Property;

                        varItemPro.OnIsNewPropertyNameValid = (newName) =>
                        {
                            return CheckNameValid(newName);
                        };

                        BindingOperations.SetBinding(varItemPro, CategoryItemProperty_Property.PropertyNameProperty, new Binding("Name") { Source = this, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(varItemPro, CategoryItemProperty_Property.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });
                        switch(varItemPro.PropertyType.ArrayType)
                        {
                            case CodeDomNode.VariableType.enArrayType.Array:
                                Icon = userCtrl.TryFindResource("Icon_PillArray") as ImageSource;
                                break;
                            case CodeDomNode.VariableType.enArrayType.Single:
                                Icon = userCtrl.TryFindResource("Icon_Pill") as ImageSource;
                                break;
                        }
                        var idStr = this.Id.ToString().Replace("-", "_");
                        if (varItemPro.GetMethodNodesKey == null)
                        {
                            varItemPro.GetMethodNodesKey = new CategoryItemProperty_Property.NodesContainerDicKey()
                            {
                                CSType = ctrl.CSType,
                                Id = Guid.NewGuid(),
                                Name = "ProGet_" + idStr,
                                ShowName = $"{Name}(Get)",
                                CategoryItemType = this.CategoryItemType
                            };
                        }
                        varItemPro.GetMethodNodesKey.HostCategoryItem = this;
                        BindingOperations.SetBinding(varItemPro.GetMethodInfo, CodeDomNode.CustomMethodInfo.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });
                        if(firstSet)
                        {
                            varItemPro.GetMethodInfo.CSType = ctrl.CSType;
                            varItemPro.GetMethodInfo.MethodName = varItemPro.GetMethodNodesKey.Name;
                            varItemPro.GetMethodInfo.DisplayName = varItemPro.GetMethodNodesKey.ShowName;
                            var retParam = new CodeDomNode.CustomMethodInfo.FunctionParam()
                            {
                                HostMethodInfo = varItemPro.GetMethodInfo,
                                ParamName = "value",
                                ParamType = varItemPro.PropertyType,
                            };
                            varItemPro.GetMethodInfo.OutParams.Add(retParam);
                        }

                        if (varItemPro.SetMethodNodesKey == null)
                        {
                            varItemPro.SetMethodNodesKey = new CategoryItemProperty_Property.NodesContainerDicKey()
                            {
                                CSType = ctrl.CSType,
                                Id = Guid.NewGuid(),
                                Name = "ProSet_" + idStr,
                                ShowName = $"{Name}(Set)",
                                CategoryItemType = this.CategoryItemType,
                            };
                        }
                        varItemPro.SetMethodNodesKey.HostCategoryItem = this;
                        BindingOperations.SetBinding(varItemPro.SetMethodInfo, CodeDomNode.CustomMethodInfo.TooltipProperty, new Binding("ToolTips") { Source = this, Mode = BindingMode.TwoWay });
                        if(firstSet)
                        {
                            varItemPro.SetMethodInfo.CSType = ctrl.CSType;
                            varItemPro.SetMethodInfo.MethodName = varItemPro.SetMethodNodesKey.Name;
                            varItemPro.SetMethodInfo.DisplayName = varItemPro.SetMethodNodesKey.ShowName;
                            var inParam = new CodeDomNode.CustomMethodInfo.FunctionParam()
                            {
                                HostMethodInfo = varItemPro.SetMethodInfo,
                                ParamName = "value",
                                ParamType = varItemPro.PropertyType,
                            };
                            varItemPro.SetMethodInfo.InParams.Add(inParam);
                        }

                        // 属性类型改变时调用改变
                        varItemPro.OnProcessTypeChanged = async (CodeDomNode.VariableType varType) =>
                        {
                            foreach (var idData in InstanceNodes)
                            {
                                var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                var node = container.FindControl(idData.NodeId) as CodeDomNode.PropertyNode;
                                node?.ChangeValueType(varType);
                            }

                            await ctrl.GetNodesContainer(varItemPro.GetMethodNodesKey, true);
                            varItemPro.GetMethodInfo.OutParams[0].OnVariableTypeChanged(varType);
                            await ctrl.GetNodesContainer(varItemPro.SetMethodNodesKey, true);
                            varItemPro.SetMethodInfo.InParams[0].OnVariableTypeChanged(varType);
                        };
                        OnNameChangedEvent += (categoryItem, newValue, oldValue) =>
                        {
                            var getName = $"{newValue}(Get)";
                            varItemPro.GetMethodInfo.DisplayName = getName;
                            varItemPro.GetMethodNodesKey.ShowName = getName;
                            var setName = $"{newValue}(Set)";
                            varItemPro.SetMethodInfo.DisplayName = setName;
                            varItemPro.SetMethodNodesKey.ShowName = setName;

                            var noUse = categoryItem.ProcessNamePropertyChanged(oldValue, newValue);
                        };

                        this.OnDropToNodesControlAction = (dropData) =>
                        {
                            var nodesControlAssist = dropData.NodesContainerHost as Macross.NodesControlAssist;
                            nodesControlAssist.ShowGetButton = true;
                            nodesControlAssist.ShowSetButton = true;
                            nodesControlAssist.InitVariableDropShow(dropData.DropPos);
                        };

                        var menuItem = new MenuItem()
                        {
                            Name = "PropertyFocus",
                            Header = "查找引用",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            IsEnabled = false,
                        };
                        menuItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                        };
                        CategoryItemContextMenu.Items.Add(menuItem);

                        var setMenuItem = new MenuItem()
                        {
                            Name = "SetFunction",
                            Header = "编写设置函数",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        setMenuItem.Click += async (object sender, RoutedEventArgs e) =>
                        {
                            await ctrl.ShowNodesContainer(varItemPro.SetMethodNodesKey);
                        };
                        CategoryItemContextMenu.Items.Add(setMenuItem);
                        var getMenuItem = new MenuItem()
                        {
                            Name = "GetFunction",
                            Header = "编写获取函数",
                            Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        };
                        getMenuItem.Click += async (object sender, RoutedEventArgs e) =>
                        {
                            await ctrl.ShowNodesContainer(varItemPro.GetMethodNodesKey);
                        };
                        CategoryItemContextMenu.Items.Add(getMenuItem);

                        if (data.Deleteable)
                        {
                            menuItem = new MenuItem()
                            {
                                Name = "VariableDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };
                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += async (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                    for (int i = InstanceNodes.Count - 1; i >= 0; i--)
                                    {
                                        var idData = InstanceNodes[i];
                                        var container = await ctrl.GetNodesContainer(idData.ContainerKeyId, true);
                                        if(container != null)
                                        {
                                            var node = container.FindControl(idData.NodeId);
                                            if (node != null)
                                                ctrl.DeleteNode(node);
                                        }
                                    }
                                    var getMethodFileName = ctrl.GetGraphFileName(varItemPro.GetMethodNodesKey.Name);
                                    EngineNS.CEngine.Instance.FileManager.DeleteFile(getMethodFileName);
                                    var setMethodFileName = ctrl.GetGraphFileName(varItemPro.SetMethodNodesKey.Name);
                                    EngineNS.CEngine.Instance.FileManager.DeleteFile(setMethodFileName);

                                    // 关闭打开的图
                                    ctrl.RemoveNodesContainer(varItemPro.GetMethodNodesKey);
                                    ctrl.RemoveNodesContainer(varItemPro.SetMethodNodesKey);
                                    // PropertyGrid
                                    ParentCategory.HostControl.HostControl.ShowItemPropertys(null);
                                    ParentCategory.HostControl.HostControl.CurrentResourceInfo.IsDirty = true;
                                }
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    }
                    break;
                case enCategoryItemType.Atrribute:
                    {
                        if (PropertyShowItem == null)
                            PropertyShowItem = new AttributeCategoryItemPropertys();

                        HasLinkedNodesContainer = false;
                        var attData = data as MacrossPanelBase.AttributeCategoryItemInitData;
                        var attrItemPro = PropertyShowItem as AttributeCategoryItemPropertys;
                        attrItemPro.AttributeType = attData.AttType;
                        if(data.Deleteable)
                        {
                            var menuItem = new MenuItem()
                            {
                                Name = "AttributeDelete",
                                Header = "删除",
                                Style = userCtrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                            };

                            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                            menuItem.Click += (object sender, RoutedEventArgs e) =>
                            {
                                if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                                {
                                    RemoveFromParent(this);
                                }

                                ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                            };
                            CategoryItemContextMenu.Items.Add(menuItem);
                        }
                    };
                    break;
            }

            if(!string.IsNullOrEmpty(InitTypeStr))
            {
                Action<CategoryItem, IMacrossOperationContainer, InitializeData> initAction;
                if (mInitActionDic.TryGetValue(InitTypeStr, out initAction))
                    initAction.Invoke(this, ctrl, data);
            }
        }
        public event Action<CategoryItem> OnRemove;
        void RemoveFromParent(CategoryItem item)
        {
            if (item.Parent != null)
                item.Parent.RemoveItem(item);
            else
                item.ParentCategory.Items.Remove(item);

            OnRemove?.Invoke(item);
        }
        public void RemoveFromParent()
        {
            RemoveFromParent(this);
        }
        public void AddInstanceNode(Guid nodesContainerId, CodeGenerateSystem.Base.BaseNodeControl node)
        {
            InstanceNodes.Add(new InstanceData()
            {
                ContainerKeyId = nodesContainerId,
                NodeId = node.Id,
            });
        }

        public void Save(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("data");
            att.Version = 0;
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();

            if (PropertyShowItem != null)
            {
                AttributeCategoryItemPropertys acip = PropertyShowItem as AttributeCategoryItemPropertys;
                if (acip != null)
                {
                    acip.Save2Xnd(xndNode);
                }
            }

            foreach (var child in Children)
            {
                var childNode = xndNode.AddNode("childNode", 0, 0);
                child.Save(childNode);

                //if (child.PropertyShowItem != null)
                //{
                //    AttributeCategoryItemPropertys acip = child.PropertyShowItem as AttributeCategoryItemPropertys;
                //    if (acip != null)
                //    {
                //        acip.Save2Xnd(xndNode);
                //    }
                //}
            }
        }

        bool isLoading = false;
        public void Load(EngineNS.IO.XndNode xndNode, IMacrossOperationContainer ctrl)
        {
            isLoading = true;
            var att = xndNode.FindAttrib("data");
            att.BeginRead();
            switch (att.Version)
            {
                case 0:
                    {
                        att.ReadMetaObject(this);
                    }
                    break;
            }
            att.EndRead();

            if (PropertyShowItem != null)
            {
                AttributeCategoryItemPropertys acip = PropertyShowItem as AttributeCategoryItemPropertys;
                if (acip != null)
                {
                    acip.LoadXnd(xndNode);
                }
            }

            var childNodes = xndNode.GetNodes();
            foreach (var childNode in childNodes)
            {
                var categoryItem = new CategoryItem(this, mParentCategory);
                categoryItem.Load(childNode, ctrl);

                //if (categoryItem.PropertyShowItem != null)
                //{
                //    AttributeCategoryItemPropertys acip = categoryItem.PropertyShowItem as AttributeCategoryItemPropertys;
                //    if (acip != null)
                //    {
                //        var text = acip.LoadXnd(xndNode);
                //    }
                //}
                var data = new Macross.CategoryItem.InitializeData();
                data.Reset();

                if (ctrl != null)
                {
                    categoryItem.Initialize(ctrl, data);
                }
                
                Children.Add(categoryItem);
            }
            isLoading = false;
        }

        public FrameworkElement GetDragVisual()
        {
            return mParentCategory?.GetContainerFromItem(this);
        }

        public CategoryItem FindItem(string name)
        {
            for(int i=Children.Count - 1; i>=0; i--)
            {
                if (Children[i].Name == name)
                    return Children[i];
                else
                {
                    var item = Children[i].FindItem(name);
                    if (item != null)
                        return item;
                }
            }
            return null;
        }
        public void RemoveItem(string name)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i].Name == name)
                    Children.RemoveAt(i);
                else
                {
                    Children[i].RemoveItem(name);
                }
            }
        }
        public void RemoveItem(CategoryItem item)
        {
            if (Children.Remove(item))
                return;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].RemoveItem(item);
            }
        }
    }

    /// <summary>
    /// Interaction logic for Category.xaml
    /// </summary>
    public partial class Category : UserControl
    {
        public string CategoryName
        {
            get { return (string)GetValue(CategoryNameProperty); }
            set { SetValue(CategoryNameProperty, value); }
        }

        public static readonly DependencyProperty CategoryNameProperty = DependencyProperty.Register("CategoryName", typeof(string), typeof(Category), new UIPropertyMetadata(""));

        ObservableCollection<CategoryItem> mItems = new ObservableCollection<CategoryItem>();
        public ObservableCollection<CategoryItem> Items
        {
            get => mItems;
        }
        MacrossPanelBase mHostControl;
        public MacrossPanelBase HostControl
        {
            get => mHostControl;
        }
        public Category(MacrossPanelBase hostControl)
        {
            InitializeComponent();
            TreeView_Items.ItemsSource = mItems;
            mHostControl = hostControl;
        }

        public delegate void SelectParticleItemDelegate(CategoryItem item, string name, string parentname);
        public event SelectParticleItemDelegate SelectParticleItem;
        FrameworkElement mTreeViewItemMouseDownElement;
        Point mTreeViewItemMouseDownPoint;
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var item = grid.DataContext as CategoryItem;
            if (item == null)
                return;

            mTreeViewItemMouseDownPoint = e.GetPosition(sender as FrameworkElement);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount > 1)
                {
                    item.InvokeOnDoubleClick();
                }
                else
                {
                    if (item.CategoryItemType == CategoryItem.enCategoryItemType.CustomFunction)
                    {
                        if (item.HostLinkControl != null)
                        {
                            //var nodesContainer = item.HostLinkControl.GetNodesContainer(item);
                            //if (nodesContainer != null)
                            //{
                            mHostControl.HostControl.ShowItemPropertys(item.PropertyShowItem);
                            mTreeViewItemMouseDownElement = sender as FrameworkElement;
                            //}
                        }
                    }
                    if (item.CategoryItemType == CategoryItem.enCategoryItemType.ParticleEmitter)
                    {
                        if (item.HostLinkControl != null)
                        {
                            mHostControl.HostControl.ShowItemPropertys(item.PropertyShowItem);
                            mTreeViewItemMouseDownElement = sender as FrameworkElement;
                        }

                        SelectParticleItem?.Invoke(item, item.Name, item.Parent != null ? item.Parent.Name : "");
                    }

                    else
                    {
                        mHostControl.HostControl.ShowItemPropertys(item.PropertyShowItem);
                        mTreeViewItemMouseDownElement = sender as FrameworkElement;
                    }
                }
            }
        }
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var item = grid.DataContext as CategoryItem;
            if (item == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed && mTreeViewItemMouseDownElement == sender)
            {
                if (item.CanDrag)
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(Program.MacrossCategoryItemDragDropTypeName, new EditorCommon.DragDrop.IDragAbleObject[] { item });
                }
            }
        }

        private void TreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var item = grid.DataContext as CategoryItem;
            if (item == null)
                return;

        }

        public Action<string> OnSelectedItemChanged;
        private void TreeView_Items_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (TreeView_Items.SelectedItem != null)
                OnSelectedItemChanged?.Invoke(CategoryName);
        }
        public void UnSelectAllItems()
        {
            var item = TreeView_Items.SelectedItem as CategoryItem;
            if (item != null)
                item.IsSelected = false;
        }

        public FrameworkElement GetContainerFromItem(CategoryItem item)
        {
            return TreeView_Items.ContainerFromElement(item) as FrameworkElement;
        }

        public CategoryItem FindItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            for(int i=Items.Count - 1; i>=0; i--)
            {
                if (Items[i].Name == name)
                    return Items[i];
                else
                {
                    var item = Items[i].FindItem(name);
                    if (item != null)
                        return item;
                }
            }
            return null;
        }
        public void RemoveItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].Name == name)
                    Items.RemoveAt(i);
                else
                    Items[i].RemoveItem(name);
            }
        }
    }
}
