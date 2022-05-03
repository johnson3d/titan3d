using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    //public partial class UNodeExpr : UNodeBase
    //{
        //public bool HasError = false;
        //public GraphException CodeExcept;
        //public void ResetErrors()
        //{
        //    HasError = false;
        //    CodeExcept = null;
        //}
        //public UMacrossEditor Editor;
        //public PinIn BeforeExec { get; set; } = new PinIn();
        //public PinOut AfterExec { get; set; } = new PinOut();
        //public UNodeExpr()
        //{
        //    BeforeExec.Name = " >>";
        //    AfterExec.Name = ">> ";

        //    BeforeExec.Link = MacrossStyles.Instance.NewExecPinDesc();
        //    AfterExec.Link = MacrossStyles.Instance.NewExecPinDesc();
        //}
        //[Obsolete]
        //public virtual IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    return null;
        //}
        //[Obsolete]
        //public IExpression GetNextExpr(UMacrossMethodGraph funGraph, ICodeGen cGen)
        //{
        //    if (AfterExec.HasLinker())
        //    {
        //        var links = new List<UPinLinker>();
        //        funGraph.FindOutLinker(AfterExec, links);
        //        var nextNode = links[0].InNode as UNodeExpr;
        //        return nextNode.GetExpr(funGraph, cGen, false);
        //    }
        //    return null;
        //}
        //public override void OnLinkedTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
        //{
        //    var funcGraph = ParentGraph as UMacrossMethodGraph;
        //    if (funcGraph == null || oPin.Link == null || iPin.Link == null)
        //    {
        //        return;
        //    }

        //    if (oPin.Link.CanLinks.Contains("Exec"))// || oPin.Link.CanLinks.Contains("Bool"))
        //    {
        //        funcGraph.RemoveLinkedOutExcept(oPin, InNode, iPin.Name);
        //        //funcGraph.AddLink(this, oPin.Name, InNode, iPin.Name, false);
        //    }
        //}
        //public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        //{
        //    var funcGraph = ParentGraph as UMacrossMethodGraph;
        //    if (funcGraph == null || oPin.Link == null || iPin.Link == null)
        //    {
        //        return;
        //    }
        //    if (iPin.Link.CanLinks.Contains("Value"))
        //    {
        //        funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
        //        //funcGraph.AddLink(OutNode, oPin.Name, this, iPin.Name, false);
        //    }
        //}
        //protected virtual object GetPropertyEditObject()
        //{
        //    return null;
        //}
        //public override void OnLButtonClicked(NodePin clickedPin)
        //{
        //    var editor = UserData as UMacrossEditor;
        //    if (HasError)
        //    {
        //        if (editor != null)
        //        {
        //            editor.NodePropGrid.Target = CodeExcept;
        //        }
        //        return;
        //    }
        //    if (GetPropertyEditObject() == null)
        //    {
        //        if (editor != null)
        //            editor.NodePropGrid.Target = null;
        //        return;
        //    }

        //    if(editor != null)
        //        editor.NodePropGrid.Target = GetPropertyEditObject();
        //}
        //public override void OnMouseStayPin(NodePin stayPin)
        //{
        //    //EGui.Controls.CtrlUtility.DrawHelper($"Stay:{stayPin.Name}");
        //}

        //public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        //{
        //    base.OnPreRead(tagObject, hostObject, fromXml);
        //    var klsGraph = tagObject as UMacrossEditor;
        //    if (klsGraph == null)
        //        return;

        //    UserData = klsGraph;
        //}
        //public override void OnAfterDraw(EngineNS.EGui.Controls.NodeGraph.NodeGraphStyles styles, ImDrawList cmdlist)
        //{
        //    //float fScale = ParentGraph.ScaleFactor;
        //    //var ScaledNodeSize = this.Size * fScale;

        //    //if (HasError)
        //    //{
        //    //    var nameSize = ImGuiAPI.CalcTextSize("Error", false, -1.0f);
        //    //    var drawPos = this.DrawPosition + ScaledNodeSize * 0.5f - nameSize * 0.5f;
        //    //    cmdlist.AddText(in drawPos, 0xFF0000FF, "Error", null);
        //    //}
        //}
    //}
}
