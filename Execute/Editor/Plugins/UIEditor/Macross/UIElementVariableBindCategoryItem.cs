using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;

namespace UIEditor.UIMacross
{
    [EngineNS.Rtti.MetaClass]
    public class UIElementVariableBindCategoryItemInitData : Macross.CategoryItem.InitializeData, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public EngineNS.UISystem.UIElement UIElement;
        [EngineNS.Rtti.MetaData]
        public int UIElementId { get; set; }
        [EngineNS.Rtti.MetaData]
        public string PropertyName { get; set; }
        [EngineNS.Rtti.MetaData]
        public Type PropertyType { get; set; }

        public EngineNS.UISystem.UIElement TargetUIElement;
        [EngineNS.Rtti.MetaData]
        public int TargetUIElementId { get; set; }
        [EngineNS.Rtti.MetaData]
        public string TargetPropertyName { get; set; }
        [EngineNS.Rtti.MetaData]
        public Type TargetPropertyType { get; set; }
        [EngineNS.Rtti.MetaData]
        public string FunctionName { get; set; }
    }

    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    public class UIElementVariableBindCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, Macross.ICategoryItemPropertyClass
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

        public CodeDomNode.CustomMethodInfo MethodInfo = new CodeDomNode.CustomMethodInfo();
    }
}
