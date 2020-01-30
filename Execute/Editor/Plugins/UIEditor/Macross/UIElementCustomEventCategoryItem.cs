using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UIEditor.UIMacross
{
    [EngineNS.Rtti.MetaClass]
    public class UIElementCustomEventCategoryItemInitData : Macross.CategoryItem.InitializeData
    {
        [EngineNS.Rtti.MetaData]
        public string EventName { get; set; }
        [EngineNS.Rtti.MetaData]
        public string DisplayName { get; set; }
    }

    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    public class UIElementCustomEventCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, Macross.ICategoryItemPropertyClass
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

        public Macross.INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }
        [EngineNS.Rtti.MetaData]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(UIElementCustomEventCategoryItemPropertys), new UIPropertyMetadata(""));

        CodeDomNode.CustomMethodInfo mMethodInfo = new CodeDomNode.CustomMethodInfo();
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.CustomMethodInfo MethodInfo
        {
            get => mMethodInfo;
            set
            {
                mMethodInfo = value;
                OnPropertyChanged("MethodInfo");
            }
        }
    }
}
