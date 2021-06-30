using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class INodeExpr : EGui.Controls.NodeGraph.NodeBase
    {
        public bool HasError = false;
        public GraphException CodeExcept;
        public ClassGraph Graph;
        public EGui.Controls.NodeGraph.PinIn BeforeExec { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public EGui.Controls.NodeGraph.PinOut AfterExec { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public INodeExpr()
        {
            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";

            BeforeExec.Link = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.Link = MacrossStyles.Instance.NewExecPinDesc();
        }
        public virtual IExpression GetExpr(FunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            return null;
        }
        public IExpression GetNextExpr(FunctionGraph funGraph, ICodeGen cGen)
        {
            if (AfterExec.HasLinker())
            {
                var links = new List<EGui.Controls.NodeGraph.PinLinker>();
                funGraph.FindOutLinker(AfterExec, links);
                var nextNode = links[0].InNode as INodeExpr;
                return nextNode.GetExpr(funGraph, cGen, false);
            }
            return null;
        }
        public override void OnLinkedTo(PinOut oPin, NodeBase InNode, PinIn iPin)
        {
            var funcGraph = ParentGraph as FunctionGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }

            if (oPin.Link.CanLinks.Contains("Exec"))// || oPin.Link.CanLinks.Contains("Bool"))
            {
                funcGraph.RemoveLinkedOutExcept(oPin, InNode, iPin.Name);
                //funcGraph.AddLink(this, oPin.Name, InNode, iPin.Name, false);
            }
        }
        public override void OnLinkedFrom(PinIn iPin, NodeBase OutNode, PinOut oPin)
        {
            var funcGraph = ParentGraph as FunctionGraph;
            if (funcGraph == null || oPin.Link == null || iPin.Link == null)
            {
                return;
            }
            if (iPin.Link.CanLinks.Contains("Value"))
            {
                funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
                //funcGraph.AddLink(OutNode, oPin.Name, this, iPin.Name, false);
            }
        }
        protected virtual object GetPropertyEditObject()
        {
            return null;
        }
        public override void OnLClicked(NodePin clickedPin)
        {
            if (HasError)
            {
                if (Graph != null)
                {
                    Graph.NodePropGrid.Target = CodeExcept;
                }
                return;
            }
            if (GetPropertyEditObject() == null)
            {
                if (Graph != null)
                    Graph.NodePropGrid.Target = null;
                return;
            }

            Graph.NodePropGrid.Target = GetPropertyEditObject();
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            //EGui.Controls.CtrlUtility.DrawHelper($"Stay:{stayPin.Name}");
        }
        public virtual System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return null;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var klsGraph = tagObject as ClassGraph;
            if (klsGraph == null)
                return;

            Graph = klsGraph;
        }
        protected override void OnAfterDraw(NodeGraphStyles styles, ref ImDrawList cmdlist)
        {
            float fScale = ParentGraph.ScaleFactor;
            var ScaledNodeSize = this.Size * fScale;

            if (HasError)
            {
                var nameSize = ImGuiAPI.CalcTextSize("Error", false, -1.0f);
                var drawPos = this.DrawPosition + ScaledNodeSize * 0.5f - nameSize * 0.5f;
                cmdlist.AddText(ref drawPos, 0xFF0000FF, "Error", null);
            }
        }
    }
}
