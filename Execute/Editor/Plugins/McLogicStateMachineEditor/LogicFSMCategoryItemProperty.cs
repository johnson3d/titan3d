using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CodeDomNode.Animation;
using EngineNS.Bricks.Animation.AnimStateMachine;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;
using Macross;
using McLogicStateMachineEditor.Controls;

namespace McLogicStateMachineEditor
{
    public class NameCheck
    {
        static bool NameRepetition(string name, CategoryItem categoryItem)
        {
            if (categoryItem.Name == name)
                return true;
            foreach (var cItem in categoryItem.Children)
            {
                if (NameRepetition(name, cItem))
                    return true;
            }
            return false;
        }
        public static bool CheckValid(string name, Category category)
        {
            string validName = name;
            bool repetition = false;
            foreach (var cItem in category.Items)
            {
                if (NameRepetition(validName, cItem))
                {
                    repetition = true;
                    break;
                }
            }
            return !repetition;
        }
    }

    #region LAAnimGraphCategoryItem
    [MetaClass]
    public class LFSMGraphCategoryItemInitData : Macross.CategoryItem.InitializeData
    {
        [MetaData]
        public string Name { get; set; } = "LinkNode";
        [MetaData]
        public string ToolTips { get; set; } = "";
    }
    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [MetaClass]
    public class LFSMGraphCategoryItemPropertys : DependencyObject, INotifyPropertyChanged, Macross.ICategoryItemPropertyClass
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
        [Browsable(false)]
        public McLogicFSMLinkControl LinkControl { get; set; } = null;
        public Macross.INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return CategoryItem; }
        [Browsable(false)]
        public Macross.CategoryItem CategoryItem { get; set; }
        [EngineNS.Rtti.MetaData]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(LFSMGraphCategoryItemPropertys), new PropertyMetadata("", OnNamePropertyChanged, new CoerceValueCallback(CoerceHeader)));
        public static void OnNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var layerP = d as LFSMGraphCategoryItemPropertys;
            if (layerP.CategoryItem == null)
                return;
            var item = layerP.CategoryItem;
            Action rename = async () =>
            {
                Category laCat = null;
                layerP.LinkControl.MacrossOpPanel.CategoryDic.TryGetValue(McLogicFSMMacrossPanel.LogicStateMachineCategoryName, out laCat);
                if (laCat != null)
                {
                    for (int i = 0; i < laCat.Items.Count; ++i)
                    {
                        var layerItem = laCat.Items[i];
                        for (int k = 0; k < layerItem.Children.Count; ++k)
                        {
                            if (layerItem.Children[k] != item)
                            {
                                var otherContainer = await layerP.LinkControl.GetNodesContainer(layerItem.Children[k], true, true);
                                if (otherContainer == null)
                                    return;
                                for (int j = 0; j < otherContainer.NodesControl.CtrlNodeList.Count; ++j)
                                {
                                    if (otherContainer.NodesControl.CtrlNodeList[j] is LogicFSMGraphNodeControl)
                                    {
                                        var node = otherContainer.NodesControl.CtrlNodeList[j] as LogicFSMGraphNodeControl;
                                        if (node.NodeName == (string)e.OldValue)
                                        {
                                            node.SetName((string)e.NewValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                var nodeContainer = await layerP.LinkControl.GetNodesContainer(item, true, true);
                if (nodeContainer == null)
                    return;
                nodeContainer.LinkedCategoryItemName = (string)e.NewValue;
                for (int i = 0; i < nodeContainer.NodesControl.CtrlNodeList.Count; ++i)
                {
                    if (nodeContainer.NodesControl.CtrlNodeList[i] is LogicFSMGraphNodeControl)
                    {
                        var node = nodeContainer.NodesControl.CtrlNodeList[i] as LogicFSMGraphNodeControl;
                        if (node.IsSelfGraphNode)
                        {
                            node.SetName((string)e.NewValue);
                        }
                    }
                }
            };
            rename.Invoke();
        }
        private static object CoerceHeader(DependencyObject d, object value)
        {
            var layerP = d as LFSMGraphCategoryItemPropertys;
            if (layerP.CategoryItem == null)
                return value;
            var cat = layerP.CategoryItem.ParentCategory;
            if (NameCheck.CheckValid((string)value, cat))
                return value;
            else
                return layerP.Name;
        }
        [EngineNS.Rtti.MetaData]
        public string ToolTips
        {
            get { return (string)GetValue(ToolTipsProperty); }
            set { SetValue(ToolTipsProperty, value); }
        }
        public static readonly DependencyProperty ToolTipsProperty = DependencyProperty.Register("ToolTips", typeof(string), typeof(LFSMGraphCategoryItemPropertys), new UIPropertyMetadata(""));
    }
    #endregion LAAnimGraphCategoryItem
}
