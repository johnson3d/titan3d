using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EngineNS.Bricks.Animation.AnimStateMachine;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace McBehaviorTreeEditor
{
    #region BTCategoryItemProperty
    [MetaClass]
    public class BTGraphCategoryItemInitData : Macross.CategoryItem.InitializeData
    {

    }
    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [MetaClass]
    public class BTGraphCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, Macross.ICategoryItemPropertyClass
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

        public Macross.INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return CategoryItem; }
        [Browsable(false)]
        public Macross.CategoryItem CategoryItem { get; set; }
        [EngineNS.Rtti.MetaData]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(BTGraphCategoryItemPropertys), new PropertyMetadata(""));

        [EngineNS.Rtti.MetaData]
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(BTGraphCategoryItemPropertys), new UIPropertyMetadata(""));
    }
    #endregion BTGraphCategoryItem
}
