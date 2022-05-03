using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    //public partial class IBaseNode : UNodeBase
    //{
        //internal UMaterialGraph Graph;
        //public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        //{
        //    base.OnPreRead(tagObject, hostObject, fromXml);
        //    var editor = tagObject as UShaderEditor;
        //    if (editor != null)
        //    {
        //        UserData = editor.MaterialGraph;
        //    }
        //}
        //[Obsolete]
        //public virtual void OnMaterialEditorGenCode(Graphics.Pipeline.Shader.UMaterial material)
        //{

        //}

        //public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        //{
        //    var funcGraph = ParentGraph as UMaterialGraph;
        //    if (funcGraph == null || oPin.Link == null || iPin.Link == null)
        //    {
        //        return;
        //    }
        //    if (iPin.Link.CanLinks.Contains("Value"))
        //    {
        //        funcGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
        //    }
        //}
        //public override void OnLButtonClicked(NodePin clickedPin)
        //{
        //    //if (HasError)
        //    //{
        //    //    if (Graph != null)
        //    //    {
        //    //        Graph.ShaderEditor.NodePropGrid.SingleTarget = CodeExcept;
        //    //    }
        //    //    return;
        //    //}
        //    var graph = UserData as UMaterialGraph;
        //    graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = null;

        //    if (GetPropertyEditObject() == null)
        //    {
        //        if (graph != null)
        //        {
        //            graph.ShaderEditor.NodePropGrid.Target = null;
        //        }
        //        return;
        //    }

        //    graph.ShaderEditor.NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<IBaseNode>.TypeDesc;
        //    graph.ShaderEditor.NodePropGrid.Target = GetPropertyEditObject();
        //}
        //protected virtual object GetPropertyEditObject()
        //{
        //    return this;
        //}
        //[Obsolete]
        //public virtual void PreGenExpr()
        //{

        //}
        //[Obsolete]
        //public virtual IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    return null;
        //}
    //}
}
