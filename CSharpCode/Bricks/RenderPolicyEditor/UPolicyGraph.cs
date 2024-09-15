using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    public partial class UPolicyNode : UNodeBase//, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public UPolicyNode()
        {
            this.Icon.Size = new Vector2(20, 20);
            TitleColor = 0xffff00ff;
        }
        Graphics.Pipeline.TtRenderGraphNode mGraphNode;
        [Rtti.Meta(Order = 1)]
        public Graphics.Pipeline.TtRenderGraphNode GraphNode 
        {
            get => mGraphNode;
        }
        [Rtti.Meta(Order = 0)]
        public string GraphNodeTypeString
        {
            get
            {
                if (GraphNode == null)
                    return "";
                return Rtti.TtTypeDesc.TypeOf(GraphNode.GetType()).TypeString;
            }
            set
            {
                var typeDesc = Rtti.TtTypeDesc.TypeOf(value);
                if (typeDesc != null)
                {
                    var rgNode = Rtti.TtTypeDescManager.CreateInstance(typeDesc) as Graphics.Pipeline.TtRenderGraphNode;
                    rgNode.InitNodePins();
                    InitNode(rgNode);
                }
            }
        }
        [Rtti.Meta]
        public override string Name
        {
            get
            {
                if (GraphNode != null)
                {
                    return GraphNode.Name;
                }
                return base.Name;
            }
            set
            {
                base.Name = value;
                if (GraphNode != null)
                {
                    GraphNode.Name = value;
                }
            }
        }
        
        public void InitNode(Graphics.Pipeline.TtRenderGraphNode node)
        {
            mGraphNode = node;
            TitleColor = node.GetTileColor().ToArgb();

            Inputs.Clear();
            for (int i = 0; i < GraphNode.NumOfInput; i++)
            {
                var pin = GraphNode.GetInput(i);

                var iPin = new PinIn();
                iPin.Name = pin.Name;
                iPin.LinkDesc = NewInOutPinDesc("GraphNode");
                iPin.MultiLinks = true;
                AddPinIn(iPin);
            }
            Outputs.Clear();
            for (int i = 0; i < GraphNode.NumOfOutput; i++)
            {
                var pin = GraphNode.GetOutput(i);

                var oPin = new PinOut();
                oPin.Name = pin.Name;
                oPin.LinkDesc = NewInOutPinDesc("GraphNode");
                oPin.MultiLinks = true;
                AddPinOut(oPin);
            }
        }
        private LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            var graph = this.ParentGraph as UPolicyGraph;

            if (graph != null && graph.PolicyEditor != null)
            {
                graph.PolicyEditor.NodePropGrid.Target = this;
            }
        }


        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);
            ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
        }
        //#region PG
        //[Browsable(false)]
        //public bool IsPropertyVisibleDirty { get; set; } = false;
        //public void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        //{
        //    var thisType = Rtti.TtTypeDesc.TypeOf(this.GetType());
        //    var pros = System.ComponentModel.TypeDescriptor.GetProperties(this);

        //    //collection.InitValue(this,  pros, parentIsValueType);
        //    foreach (System.ComponentModel.PropertyDescriptor prop in pros)
        //    {
        //        //var p = this.GetType().GetProperty(prop.Name);
        //        //if (p == null)
        //        //    continue;
        //        if (prop.ComponentType != typeof(UPolicyNode) && !prop.ComponentType.IsSubclassOf(typeof(UPolicyNode)))
        //        {
        //            continue;
        //        }
        //        var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
        //        proDesc.InitValue(this, thisType, prop, parentIsValueType);
        //        if (!proDesc.IsBrowsable)
        //        {
        //            proDesc.ReleaseObject();
        //            continue;
        //        }
        //        collection.Add(proDesc);
        //    }
        //}

        //public object GetPropertyValue(string propertyName)
        //{
        //    var proInfo = this.GetType().GetProperty(propertyName);
        //    if (proInfo != null)
        //        return proInfo.GetValue(this);
        //    var fieldInfo = this.GetType().GetField(propertyName);
        //    if (fieldInfo != null)
        //        return fieldInfo.GetValue(this);
        //    return null;
        //}

        //public void SetPropertyValue(string propertyName, object value)
        //{
        //    var proInfo = this.GetType().GetProperty(propertyName);
        //    if (proInfo != null)
        //        proInfo.SetValue(this, value);
        //    var fieldInfo = this.GetType().GetField(propertyName);
        //    if (fieldInfo != null)
        //        fieldInfo.SetValue(this, value);
        //}
        //#endregion
    }
    public class UPolicyGraph : UNodeGraph
    {
        public const string RGDEditorKeyword = "RDG";
        public UPolicyGraph()
        {
            PolicyType = Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Pipeline.TtDeferredPolicyBase));
            UpdateCanvasMenus();
            UpdateNodeMenus();
            UpdatePinMenus();
        }
        Graphics.Pipeline.TtRenderPolicy mRenderPolicy;
        [Rtti.Meta]
        public Graphics.Pipeline.TtRenderPolicy RenderPolicy
        {
            get => mRenderPolicy;
            set
            {
                mRenderPolicy = value;
            }
        }
        Rtti.TtTypeDesc mPolicyType;
        [Rtti.Meta]
        [PGTypeEditor(typeof(Graphics.Pipeline.TtRenderPolicy))]
        public Rtti.TtTypeDesc PolicyType
        {
            get => mPolicyType;
            set
            {
                mPolicyType = value;
                mRenderPolicy = Rtti.TtTypeDescManager.CreateInstance(mPolicyType, null) as Graphics.Pipeline.TtRenderPolicy;
            }
        }
        
        public TtPolicyEditor PolicyEditor;
        List<Rtti.TtTypeDesc> mGraphNodeTypes = null;
        public List<Rtti.TtTypeDesc> GraphNodeTypes
        {
            get
            {
                if (mGraphNodeTypes == null)
                {
                    mGraphNodeTypes = new List<Rtti.TtTypeDesc>();
                    foreach (var i in Rtti.TtTypeDescManager.Instance.Services.Values)
                    {
                        foreach (var j in i.Types)
                        {
                            if (j.Value.SystemType.IsSubclassOf(typeof(Graphics.Pipeline.TtRenderGraphNode)))
                            {
                                mGraphNodeTypes.Add(j.Value);
                            }
                        }
                    }
                }
                return mGraphNodeTypes;
            }
        }
        public override void UpdateCanvasMenus()
        {
            CanvasMenus.SubMenuItems.Clear();
            CanvasMenus.Text = "Canvas";

            foreach (var i in GraphNodeTypes)
            {
                //CanvasMenus.AddMenuItem(
                //$"{i.FullName}", null,
                //(UMenuItem item, object sender) =>
                //{
                //    var node = new UPolicyNode();
                //    var rgNode = Rtti.UTypeDescManager.CreateInstance(i) as Graphics.Pipeline.TtRenderGraphNode;
                //    //rgNode.RenderGraph = this;
                //    rgNode.InitNodePins();
                //    node.InitNode(rgNode);
                //    node.Name = rgNode.Name;
                //    node.Position = PopMenuPosition;
                //    this.AddNode(node);
                //});
                var atts = i.GetCustomAttributes(typeof(ContextMenuAttribute), true);
                if (atts.Length > 0)
                {
                    var parentMenu = CanvasMenus;
                    var att = atts[0] as ContextMenuAttribute;
                    if (!att.HasKeyString(RGDEditorKeyword))
                        continue;
                    for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                    {
                        var menuStr = att.MenuPaths[menuIdx];
                        var menuName = GetMenuName(menuStr);
                        if (menuIdx < att.MenuPaths.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuName, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuName, att.FilterStrings, null,
                                (TtMenuItem item, object sender) =>
                                {
                                    var node = new UPolicyNode();
                                    var rgNode = Rtti.TtTypeDescManager.CreateInstance(i) as Graphics.Pipeline.TtRenderGraphNode;
                                    //rgNode.RenderGraph = this;
                                    rgNode.InitNodePins();
                                    node.InitNode(rgNode);
                                    node.Name = rgNode.Name;
                                    node.Position = PopMenuPosition;
                                    this.AddNode(node);
                                });
                        }
                    }
                }
            }   
        }
    }
}
