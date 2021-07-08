using EngineNS.EGui.Controls.NodeGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Operator
{
    public class Binocular : IBaseNode
    {
        public Rtti.UTypeDesc LeftType;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EBinocularOp Op { get; set; }
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public string LeftTypeString
        {
            get
            {
                if (LeftType != null)
                    return Rtti.UTypeDesc.TypeStr(LeftType);
                return "";
            }
            set
            {
                LeftType = Rtti.UTypeDesc.TypeOf(value);
            }
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Left { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinIn Right { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut Result { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public Binocular(EBinocularOp InOp)
        {
            Op = InOp;

            Left.Name = " L";
            Right.Name = " R";
            Result.Name = "= ";

            Left.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            Right.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            Result.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = 0xFF204020;
            Background.Color = 0x80808080;
            Name = Bricks.CodeBuilder.CSharp.CSGen.GetOp(InOp);

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return LeftType.SystemType;
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        {
            var binOp = new BinocularOp();
            binOp.Op = this.Op;
            var links = new List<EGui.Controls.NodeGraph.PinLinker>();
            funGraph.FindInLinker(Left, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Left link error : {links.Count}");
            }
            var leftNode = links[0].OutNode as IBaseNode;
            var leftExpr = leftNode.GetExpr(funGraph, cGen, links[0].Out, true) as OpExpress;
            var leftType = leftNode.GetOutPinType(links[0].Out);
            binOp.Left = leftExpr;

            links.Clear();
            funGraph.FindInLinker(Right, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Right link error : {links.Count}");
            }
            var rightNode = links[0].OutNode as IBaseNode;
            var rightExpr = rightNode.GetExpr(funGraph, cGen, links[0].Out, true) as OpExpress;
            var rightType = rightNode.GetOutPinType(links[0].Out);
            if (rightType != leftType)
            {
                var cvtExpr = new ConvertTypeOp();
                cvtExpr.TargetType = cGen.GetTypeString(leftType);
                cvtExpr.ObjExpr = rightExpr;
                binOp.Right = cvtExpr;
            }
            else
            {
                binOp.Right = rightExpr;
            }

            return binOp;
        }
    }

    #region ValueOp
    public class ValueOpNode : Binocular
    {
        public ValueOpNode(EBinocularOp op)
            : base(op)
        {
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (LeftType != null)
                EGui.Controls.CtrlUtility.DrawHelper(LeftType.FullName);
        }
        public override void OnRemoveLinker(EGui.Controls.NodeGraph.PinLinker linker)
        {
            if (linker.In == Left)
            {
                LeftType = null;
            }
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == Right)
            {
                var nodeExpr = OutNode as IBaseNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);

                if (testType == typeof(float))
                    return true;
                return ICodeGen.CanConvert(testType, LeftType.SystemType);
            }
            return true;
        }
        public override void OnLinkedFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == Left)
            {//二元运算，左值决定输出类型
                var nodeExpr = OutNode as IBaseNode;
                if (nodeExpr == null)
                    return;

                var newType = nodeExpr.GetOutPinType(oPin);
                if (LeftType != null && LeftType.SystemType != newType)
                {//类型改变，所有输入输出都需要断开
                    this.ParentGraph.RemoveLinkedOut(this.Result);
                    this.ParentGraph.RemoveLinkedIn(this.Right);
                }
                LeftType = Rtti.UTypeDesc.TypeOf(newType);
            }
        }
    }
    public class AddNode : ValueOpNode
    {
        public AddNode()
            : base(EBinocularOp.Add)
        {
        }
    }
    public class SubNode : ValueOpNode
    {
        public SubNode()
            : base(EBinocularOp.Sub)
        {
        }
    }
    public class MulNode : ValueOpNode
    {
        public MulNode()
            : base(EBinocularOp.Mul)
        {
        }
    }
    public class DivNode : ValueOpNode
    {
        public DivNode()
            : base(EBinocularOp.Div)
        {
        }
    }
    public class ModNode : ValueOpNode
    {
        public ModNode()
            : base(EBinocularOp.Mod)
        {
        }
    }
    public class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(EBinocularOp.BitAnd)
        {

        }
    }
    public class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(EBinocularOp.BitOr)
        {
        }
    }
    #endregion
}
