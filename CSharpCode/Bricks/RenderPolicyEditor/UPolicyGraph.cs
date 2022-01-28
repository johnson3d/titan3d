using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RenderPolicyEditor
{
    public partial class UPolicyNode : EGui.Controls.NodeGraph.NodeBase
    {
        public Graphics.Pipeline.Common.URenderGraphNode GraphNode 
        { 
            get; 
            protected set; 
        }
        [Rtti.Meta(Order = 0)]
        public string GraphNodeTypeString
        {
            get
            {
                if (GraphNode == null)
                    return "";
                return Rtti.UTypeDesc.TypeOf(GraphNode.GetType()).TypeString;
            }
            set
            {
                var typeDesc = Rtti.UTypeDesc.TypeOf(value);
                if (typeDesc != null)
                {
                    var rgNode = Rtti.UTypeDescManager.CreateInstance(typeDesc) as Graphics.Pipeline.Common.URenderGraphNode;
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
        
        public void InitNode(Graphics.Pipeline.Common.URenderGraphNode node)
        {
            GraphNode = node;            
            Inputs.Clear();
            for (int i = 0; i < GraphNode.NumOfInput; i++)
            {
                var pin = GraphNode.GetInput(i);

                var iPin = new EGui.Controls.NodeGraph.PinIn();
                iPin.Name = pin.Name;
                iPin.Link = NewInOutPinDesc("GraphNode");
                AddPinIn(iPin);
            }
            Outputs.Clear();
            for (int i = 0; i < GraphNode.NumOfOutput; i++)
            {
                var pin = GraphNode.GetOutput(i);

                var oPin = new EGui.Controls.NodeGraph.PinOut();
                oPin.Name = pin.Name;
                oPin.Link = NewInOutPinDesc("GraphNode");
                AddPinOut(oPin);
            }
        }
        private EGui.Controls.NodeGraph.NodePin.LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new EGui.Controls.NodeGraph.NodePin.LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
        public override void OnLClicked(EGui.Controls.NodeGraph.NodePin clickedPin)
        {
            var graph = this.ParentGraph as UPolicyGraph;

            if (graph != null && graph.PolicyEditor != null)
            {
                graph.PolicyEditor.NodePropGrid.Target = this;
            }
        }
        public Vector2 Measure(EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            Vector2 size = new Vector2(0);
            var nameSize = ImGuiAPI.CalcTextSize(Name, false, -1.0f);

            size.Y += nameSize.Y;
            SetIfBigger(ref size.X, nameSize.X);
            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                float lineWidth = 0;
                float lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].Link != null)
                    {
                        inIcon = Inputs[i].Link.Icon;
                    }
                    lineWidth += styles.PinInStyle.Offset;
                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;

                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                    SetIfBigger(ref lineHeight, inIcon.Size.Y);
                    SetIfBigger(ref lineHeight, nameSize.Y);

                    if (Inputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Inputs[i].Link.ExtPadding);
                    }
                }
                if (i < Outputs.Count)
                {
                    var inIcon = styles.PinOutStyle.Image;
                    if (Outputs[i].Link != null)
                    {
                        inIcon = Outputs[i].Link.Icon;
                    }
                    lineWidth += styles.PinOutStyle.Offset;
                    lineWidth += inIcon.Size.X;
                    lineWidth += styles.PinSpacing;

                    nameSize = ImGuiAPI.CalcTextSize(Outputs[i].Name, false, -1.0f);
                    SetIfBigger(ref lineHeight, inIcon.Size.Y);
                    SetIfBigger(ref lineHeight, nameSize.Y);

                    if (Outputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding);
                    }
                }
                SetIfBigger(ref size.X, lineWidth);
                size.Y += (lineHeight + styles.PinPadding + extPadding);
            }

            size.X += PreviewWidth;
            SetIfBigger(ref size.Y, PreviewWidth + nameSize.Y);
            return size;
        }
        public virtual void OnPreviewDraw(ref Vector2 prevStart, ref Vector2 prevEnd)
        {

        }
        public float PreviewWidth = 0;
        public unsafe override void OnDraw(EGui.Controls.NodeGraph.NodeGraphStyles styles)
        {
            this.Size = Measure(styles);
            float fScale = ParentGraph.ScaleFactor;
            var ScaledNodeSize = this.Size * fScale;

            var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
            ImGuiAPI.SetWindowFontScale(fScale);

            var nameSize = ImGuiAPI.CalcTextSize(Name, false, -1.0f);

            var start = DrawPosition;
            var nodeMin = start;
            var nodeMax = nodeMin + new Vector2(ScaledNodeSize.X, nameSize.Y);
            TitleImage.OnDraw(ref cmdlist, in nodeMin, in nodeMax, 0);
            cmdlist.AddText(in nodeMin, Icon.Color, Name, null);

            nodeMax = nodeMin + ScaledNodeSize;
            nodeMin.Y += nameSize.Y;
            Background.OnDraw(ref cmdlist, in nodeMin, in nodeMax, 0);

            {
                var prevStart = nodeMin;
                prevStart.X += (this.Size.X - PreviewWidth) * fScale / 2;
                var prevEnd = prevStart + new Vector2(PreviewWidth * fScale);

                OnPreviewDraw(ref prevStart, ref prevEnd);
            }

            var rectMin = new Vector2(nodeMin.X, nodeMin.Y);
            var lines = Math.Max(Inputs.Count, Outputs.Count);
            for (int i = 0; i < lines; i++)
            {
                rectMin = nodeMin;
                float lineHeight = 0;
                float extPadding = 0;
                if (i < Inputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Inputs[i].Link != null)
                    {
                        inIcon = Inputs[i].Link.Icon;
                    }

                    var rectMax = rectMin + inIcon.Size * fScale;
                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                    Inputs[i].Offset = (rectMin - start) / ParentGraph.ScaleFactor;

                    nameSize = ImGuiAPI.CalcTextSize(Inputs[i].Name, false, -1.0f);
                    rectMin.X -= nameSize.X;
                    cmdlist.AddText(in rectMin, Icon.Color, Inputs[i].Name, null);

                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                    if (Inputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Inputs[i].Link.ExtPadding * fScale);
                    }
                }
                if (i < Outputs.Count)
                {
                    var inIcon = styles.PinInStyle.Image;
                    if (Outputs[i].Link != null)
                    {
                        inIcon = Outputs[i].Link.Icon;
                    }

                    rectMin.X = nodeMax.X - inIcon.Size.X * fScale;
                    var rectMax = rectMin + inIcon.Size * fScale;
                    inIcon.OnDraw(ref cmdlist, in rectMin, in rectMax, 0);
                    Outputs[i].Offset = (rectMin - start) / ParentGraph.ScaleFactor;

                    rectMin.X = rectMax.X;
                    cmdlist.AddText(in rectMin, Icon.Color, Outputs[i].Name, null);

                    SetIfBigger(ref lineHeight, inIcon.Size.Y * fScale);
                    if (Outputs[i].Link != null)
                    {
                        SetIfBigger(ref extPadding, Outputs[i].Link.ExtPadding * fScale);
                    }
                }
                nodeMin.Y += (lineHeight + styles.PinPadding * fScale + extPadding);
            }

            if (Selected)
            {
                cmdlist.AddRect(in start, in nodeMax, styles.SelectedColor, 0, ImDrawFlags_.ImDrawFlags_RoundCornersAll, 2);
            }
            ImGuiAPI.SetWindowFontScale(1.0f);
        }
    }
    public class UPolicyGraph : EGui.Controls.NodeGraph.NodeGraph
    {
        public UPolicyEditor PolicyEditor;
        List<Rtti.UTypeDesc> mGraphNodeTypes = null;
        public List<Rtti.UTypeDesc> GraphNodeTypes
        {
            get
            {
                if (mGraphNodeTypes == null)
                {
                    mGraphNodeTypes = new List<Rtti.UTypeDesc>();
                    foreach (var i in Rtti.UTypeDescManager.Instance.Services.Values)
                    {
                        foreach (var j in i.Types)
                        {
                            if (j.Value.SystemType.IsSubclassOf(typeof(Graphics.Pipeline.Common.URenderGraphNode)))
                            {
                                mGraphNodeTypes.Add(j.Value);
                            }
                        }
                    }
                }
                return mGraphNodeTypes;
            }
        }
        protected override void ShowAddNode(Vector2 posMenu)
        {
            if (ImGuiAPI.BeginMenu("Operation", true))
            {
                foreach (var i in GraphNodeTypes)
                {
                    if (ImGuiAPI.MenuItem($"{i.FullName}", null, false, true))
                    {
                        var node = new UPolicyNode();
                        //node.Graph = this;
                        var rgNode = Rtti.UTypeDescManager.CreateInstance(i) as Graphics.Pipeline.Common.URenderGraphNode;
                        rgNode.InitNodePins();
                        node.InitNode(rgNode);
                        node.Name = rgNode.Name;
                        node.Position = View2WorldSpace(ref posMenu);
                        this.AddNode(node);
                    }
                }
                
                ImGuiAPI.EndMenu();
            }
        }
    }
}
