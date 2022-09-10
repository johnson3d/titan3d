using System;
using System.Collections.Generic;
using System.Reflection;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Operator
{
    public class Binocular : UNodeBase
    {
        public Rtti.UTypeDesc LeftType;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public UBinaryOperatorExpression.EBinaryOperation Op { get; set; }
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
        public PinIn Left { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn Right { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut Result { get; set; } = new PinOut();
        public Binocular(UBinaryOperatorExpression.EBinaryOperation InOp, string name)
        {
            Op = InOp;
            Name = name;

            Left.Name = " L";
            Right.Name = " R";
            Result.Name = "= ";

            Left.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            Right.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            Result.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return LeftType;
        }
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var binOp = new BinocularOp();
        //    binOp.Op = this.Op;
        //    var links = new List<UPinLinker>();
        //    funGraph.FindInLinker(Left, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, Left, $"Left link error : {links.Count}");
        //    }
        //    var leftNode = links[0].OutNode;
        //    var leftExpr = leftNode.GetExpr(funGraph, cGen, links[0].OutPin, true) as OpExpress;
        //    var leftType = leftNode.GetOutPinType(links[0].OutPin);
        //    binOp.Left = leftExpr;

        //    links.Clear();
        //    funGraph.FindInLinker(Right, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, Left, $"Right link error : {links.Count}");
        //    }
        //    var rightNode = links[0].OutNode;
        //    var rightExpr = rightNode.GetExpr(funGraph, cGen, links[0].OutPin, true) as OpExpress;
        //    var rightType = rightNode.GetOutPinType(links[0].OutPin);
        //    if (rightType != leftType)
        //    {
        //        var cvtExpr = new ConvertTypeOp();
        //        cvtExpr.TargetType = cGen.GetTypeString(leftType);
        //        cvtExpr.ObjExpr = rightExpr;
        //        binOp.Right = cvtExpr;
        //    }
        //    else
        //    {
        //        binOp.Right = rightExpr;
        //    }

        //    return binOp;
        //}
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            if (data.NodeGraph.PinHasLinker(Left))
                data.NodeGraph.GetOppositePinNode(Left).BuildStatements(ref data);
            if(data.NodeGraph.PinHasLinker(Right))
                data.NodeGraph.GetOppositePinNode(Right).BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin != Result)
                return null;
            var binOp = new UBinaryOperatorExpression()
            {
                Operation = this.Op,
                Left = data.NodeGraph.GetOppositePinExpression(Left, ref data),
                Right = data.NodeGraph.GetOppositePinExpression(Right, ref data),
            };
            return binOp;
        }
    }

    #region ValueOp
    public class ValueOpNode : Binocular
    {
        public ValueOpNode(UBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (LeftType != null)
                EGui.Controls.CtrlUtility.DrawHelper(LeftType.FullName);
        }
        public override void OnRemoveLinker(UPinLinker linker)
        {
            if (linker.InPin == Left)
            {
                LeftType = null;
            }
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == Right)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);

                if (testType.IsEqual(typeof(float)))
                    return true;
                return UCodeGeneratorBase.CanConvert(testType, LeftType);
            }
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == Left)
            {//二元运算，左值决定输出类型
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return;

                var newType = nodeExpr.GetOutPinType(oPin);
                if (LeftType != null && LeftType != newType)
                {//类型改变，所有输入输出都需要断开
                    this.ParentGraph.RemoveLinkedOut(this.Result);
                    this.ParentGraph.RemoveLinkedIn(this.Right);
                }
                LeftType = newType;
            }
        }
    }
    [ContextMenu("add,+", "Operation\\+", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class AddNode : ValueOpNode
    {
        public AddNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Add, "+")
        {
        }
    }
    [ContextMenu("subtraction,-", "Operation\\-", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class SubNode : ValueOpNode
    {
        public SubNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Subtract, "-")
        {
        }
    }
    [ContextMenu("multiplication,*", "Operation\\*", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class MulNode : ValueOpNode
    {
        public MulNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Multiply, "*")
        {
        }
    }
    [ContextMenu("division,/", "Operation\\/", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class DivNode : ValueOpNode
    {
        public DivNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Divide, "/")
        {
        }
    }
    [ContextMenu("mod,%", "Operation\\%", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class ModNode : ValueOpNode
    {
        public ModNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Modulus, "%")
        {
        }
    }
    [ContextMenu("bitand,&", "Operation\\&", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseAnd, "&")
        {

        }
    }
    [ContextMenu("bitor,|", "Operation\\|", ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseOr, "|")
        {
        }
    }
    #endregion
}
