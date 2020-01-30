using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Macross
{
    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [EngineNS.Rtti.MetaClass]
    public class CategoryItemProperty_Property : DependencyObject, INotifyPropertyChanged, ICategoryItemPropertyClass, CodeDomNode.IVariableTypeChangeProcessClass
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region ISerializer
        public void ReadObject(EngineNS.IO.Serializer.IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
        }
        public void ReadObject(EngineNS.IO.Serializer.IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
        }

        public void ReadObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
        }

        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }

        public void WriteObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        public INodesContainerDicKey GetNodesContainerDicKey(Guid keyId)
        {
            if (GetMethodNodesKey.Id == keyId)
                return GetMethodNodesKey;
            if (SetMethodNodesKey.Id == keyId)
                return SetMethodNodesKey;
            return null;
        }

        public delegate bool Delegate_IsNewPropertyNameValid(string newName);
        public Delegate_IsNewPropertyNameValid OnIsNewPropertyNameValid;
        public delegate void Delegate_ProcessTypeChanged(CodeDomNode.VariableType varType);
        public Delegate_ProcessTypeChanged OnProcessTypeChanged;

        [EngineNS.Rtti.MetaData]
        public bool VariableTypeReadOnly
        {
            get;
            set;
        } = false;

        [EngineNS.Rtti.MetaData]
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }
        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(CategoryItemProperty_Property), new PropertyMetadata(""));

        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("TypeSetterTemplate")]
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.VariableType PropertyType
        {
            get { return (CodeDomNode.VariableType)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }
        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register("PropertyType", typeof(CodeDomNode.VariableType), typeof(CategoryItemProperty_Property), new PropertyMetadata());

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
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(CategoryItemProperty_Property), new UIPropertyMetadata(""));

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(string), typeof(CategoryItemProperty_Property), new UIPropertyMetadata(""));

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.CustomMethodInfo GetMethodInfo
        {
            get;
            set;
        }
        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.CustomMethodInfo SetMethodInfo
        {
            get;
            set;
        }

        [EngineNS.Rtti.MetaClass]
        public class NodesContainerDicKey : EngineNS.IO.Serializer.Serializer, Macross.INodesContainerDicKey, INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            [EngineNS.Rtti.MetaData]
            public EngineNS.ECSType CSType { get; set; }
            [EngineNS.Rtti.MetaData]

            public Guid Id { get; set; } = Guid.Empty;
            string mName;
            [EngineNS.Rtti.MetaData]
            public string Name
            {
                get => mName;
                set
                {
                    mName = value;
                    OnPropertyChanged("Name");
                }
            }
            string mShowName;
            [EngineNS.Rtti.MetaData]
            public string ShowName
            {
                get => mShowName;
                set
                {
                    mShowName = value;
                    OnPropertyChanged("ShowName");
                }
            }
            [EngineNS.Rtti.MetaData]
            public CategoryItem.enCategoryItemType CategoryItemType { get; set; }

            public CategoryItem HostCategoryItem;

            #region INodesContainerDicKey

            public void ProcessOnNodesContainerShow(NodesControlAssist.ProcessData processData)
            {
                HostCategoryItem.ParentCategory.HostControl.ProcessOnNodesContainerShow(this, processData);
            }
            public void ProcessOnNodesContainerHide(NodesControlAssist.ProcessData processData)
            {
                HostCategoryItem.ParentCategory.HostControl.ProcessOnNodesContainerHide(this, processData);
            }

            #endregion
        }
        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public NodesContainerDicKey GetMethodNodesKey { get; set; }
        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public NodesContainerDicKey SetMethodNodesKey { get; set; }

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute(typeof(VariableCategoryItemPropertys.CustomAddRemoveAttributeTypeProxysPrivider))]
        public ObservableCollection<AttributeType> AttributeTypeProxys
        {
            get;
            set;
        } = new ObservableCollection<AttributeType>();

        public CategoryItemProperty_Property()
        {
            PropertyType = new CodeDomNode.VariableType();
        }
        public CategoryItemProperty_Property(EngineNS.ECSType csType)
        {
            PropertyType = new CodeDomNode.VariableType(csType);
        }
    }
}
