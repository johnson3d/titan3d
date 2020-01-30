using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace UIEditor.UIMacross
{
    [MetaClass]
    public class UIElementVariableCategoryItemInitData : Macross.CategoryItem.InitializeData
    {
        public EngineNS.UISystem.UIElement UIElement;
        [MetaData]
        public int UIElementId
        {
            get;
            set;
        } = int.MaxValue;

        [MetaData]
        public Type ElementType
        {
            get;
            set;
        } = null;
    }

    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    public class UIElementVariableCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, Macross.ICategoryItemPropertyClass
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

        public Macross.INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }

        [ReadOnly(true)]
        [EngineNS.Rtti.MetaData]
        public string VariableName
        {
            get { return (string)GetValue(VariableNameProperty); }
            set { SetValue(VariableNameProperty, value); }
        }
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(UIElementVariableCategoryItemPropertys), new PropertyMetadata(""));

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public Type VariableType
        {
            get;
            set;
        }
        [ReadOnly(true)]
        [EngineNS.Rtti.MetaData]
        public int ElementId
        {
            get;
            set;
        }
        [EngineNS.Rtti.MetaData]
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(UIElementVariableCategoryItemPropertys), new UIPropertyMetadata(""));
    }
}
