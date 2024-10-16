using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class Binocular : TtNodeBase
    {
        public Rtti.TtTypeDesc LeftType;
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }

        [Rtti.Meta]
        public string LeftTypeString
        {
            get
            {
                if (LeftType != null)
                    return Rtti.TtTypeDesc.TypeStr(LeftType);
                return "";
            }
            set
            {
                LeftType = Rtti.TtTypeDesc.TypeOf(value);
            }
        }
        
        public PinIn Left { get; set; } = new PinIn();
        public PinIn Right { get; set; } = new PinIn();
        public PinOut Result { get; set; } = new PinOut()
        {
            MultiLinks = true,
        };
        public Binocular(TtBinaryOperatorExpression.EBinaryOperation InOp, string name)
        {
            Op = InOp;
            Name = name;

            Left.Name = " L";
            Right.Name = " R";
            Result.Name = "= ";

            Left.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            Right.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            Result.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);

            Left.LinkDesc.CanLinks.Add("Value");
            Right.LinkDesc.CanLinks.Add("Value");
            Result.LinkDesc.CanLinks.Add("Value");
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return LeftType;
        }
        public override object GetPropertyEditObject()
        {
            return null;
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin != Result)
                return null;
            var binOp = new TtBinaryOperatorExpression()
            {
                Operation = this.Op,
                Left = data.NodeGraph.GetOppositePinExpression(Left, ref data),
                Right = data.NodeGraph.GetOppositePinExpression(Right, ref data)
            };
            return binOp;
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(Left.HasLinker())
            {
                var opPin = data.NodeGraph.GetOppositePin(Left);
                var pinNode = data.NodeGraph.GetOppositePinNode(Left);
                pinNode.BuildStatements(opPin, ref data);
            }
            if(Right.HasLinker())
            {
                var opPin = data.NodeGraph.GetOppositePin(Right);
                var pinNode = data.NodeGraph.GetOppositePinNode(Right);
                pinNode.BuildStatements(opPin, ref data);
            }
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    var binOp = new BinocularOp();
        //    binOp.Op = this.Op;
        //    var links = new List<UPinLinker>();
        //    funGraph.FindInLinker(Left, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, Left, $"Left link error : {links.Count}");
        //    }
        //    var leftNode = links[0].OutNode as UNodeExpr;
        //    var leftExpr = leftNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //    var leftType = leftNode.GetOutPinType(links[0].OutPin);
        //    binOp.Left = leftExpr;

        //    links.Clear();
        //    funGraph.FindInLinker(Right, links);
        //    if (links.Count != 1)
        //    {
        //        throw new GraphException(this, Left, $"Right link error : {links.Count}");
        //    }
        //    var rightNode = links[0].OutNode as UNodeExpr;
        //    var rightExpr = rightNode.GetExpr(funGraph, cGen, true) as OpExpress;
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
    }

    #region ValueOp
    public partial class ValueOpNode : Binocular
    {
        public ValueOpNode(TtBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
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
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == Right)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return TtCodeGeneratorBase.CanConvert(testType, LeftType);
            }
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
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
    [ContextMenu("add,+", "Operation\\+", TtMacross.MacrossEditorKeyword)]
    public partial class AddNode : ValueOpNode
    {
        public AddNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Add, "+")
        {
        }
    }
    [ContextMenu("subtraction,-", "Operation\\-", TtMacross.MacrossEditorKeyword)]
    public partial class SubNode : ValueOpNode
    {
        public SubNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Subtract, "-")
        {
        }
    }
    [ContextMenu("multiplication,*", "Operation\\*", TtMacross.MacrossEditorKeyword)]
    public partial class MulNode : ValueOpNode
    {
        public MulNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Multiply, "*")
        {
        }
    }
    [ContextMenu("division,/", "Operation\\/", TtMacross.MacrossEditorKeyword)]
    public partial class DivNode : ValueOpNode
    {
        public DivNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Divide, "/")
        {
        }
    }
    [ContextMenu("mod,%", "Operation\\%", TtMacross.MacrossEditorKeyword)]
    public partial class ModNode : ValueOpNode
    {
        public ModNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Modulus, "%")
        {
        }
    }
    [ContextMenu("bitand,&", "Operation\\&", TtMacross.MacrossEditorKeyword)]
    public partial class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseAnd, "&")
        {
         
        }
    }
    [ContextMenu("bitor,|", "Operation\\|", TtMacross.MacrossEditorKeyword)]
    public partial class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseOr, "|")
        {
        }
    }
    [ContextMenu("bitxor,^", "Operation\\^", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class BitXorNode : ValueOpNode
    {
        public BitXorNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseXOR, "^")
        {
        }
    }
    [ContextMenu("bitleftshift,|", "Operation\\<<", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class BitLeftshiftNode : ValueOpNode
    {
        public BitLeftshiftNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseLeftShift, "<<")
        {
        }
    }
    [ContextMenu("bitrightshift,|", "Operation\\>>", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class BitRightshiftNode : ValueOpNode
    {
        public BitRightshiftNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseRightShift, ">>")
        {
        }
    }

    #endregion

    #region Cmp
    public partial class CmpNode : Binocular
    {
        public CmpNode(TtBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.TtTypeDesc.TypeOf(typeof(bool));
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            if (Result == stayPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper(typeof(bool).FullName);
            }
            else
            {
                if (LeftType != null)
                    EGui.Controls.CtrlUtility.DrawHelper(LeftType.FullName);
            }
        }
        public override void OnRemoveLinker(UPinLinker linker)
        {
            if (linker.InPin == Left)
            {
                LeftType = null;
            }
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == Left)
            {//比较运算，左值决定比较类型
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return;

                var newType = nodeExpr.GetOutPinType(oPin);
                if (LeftType != newType)
                {//类型改变，所有输入输出都需要断开
                    this.ParentGraph.RemoveLinkedOut(this.Result);
                    this.ParentGraph.RemoveLinkedIn(this.Right);
                }
                LeftType = newType;
            }
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == Right)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return TtCodeGeneratorBase.CanConvert(testType, LeftType);
            }
            return true;
        }
    }
    [ContextMenu("equal,==", "Bool Operation\\==", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class EqualNode : CmpNode
    {
        public EqualNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Equality, "==")
        {
            
        }
    }
    [ContextMenu("notequal,!=", "Bool Operation\\!=", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class NotEqualNode : CmpNode
    {
        public NotEqualNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.NotEquality, "!=")
        {
            
        }
    }
    [ContextMenu("greate,>", "Bool Operation\\>", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class GreateNode : CmpNode
    {
        public GreateNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.GreaterThan, ">")
        {
            
        }
    }
    [ContextMenu("greateequal,>=", "Bool Operation\\>=", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class GreateEqualNode : CmpNode
    {
        public GreateEqualNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual, ">=")
        {
            
        }
    }
    [ContextMenu("less,<", "Bool Operation\\<", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class LessNode : CmpNode
    {
        public LessNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.LessThan, "<")
        {
            
        }
    }
    [ContextMenu("lessequal,<=", "Bool Operation\\<=", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class LessEqualNode : CmpNode
    {
        public LessEqualNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual, "<=")
        {
            
        }
    }
    #endregion

    #region Bool
    public partial class BoolNode : Binocular
    {
        public BoolNode(TtBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
            LeftType = Rtti.TtTypeDescGetter<bool>.TypeDesc;
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.TtTypeDesc.TypeOf(typeof(bool));
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode;
            if (nodeExpr == null)
                return true;
            if (!nodeExpr.GetOutPinType(oPin).IsEqual(typeof(bool)))
            {
                return false;
            }
            return true;
        }
    }
    [ContextMenu("and,&&", "Bool Operation\\&&", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class AndNode : BoolNode
    {
        public AndNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BooleanAnd, "&&")
        {
        }
    }
    [ContextMenu("or,||", "Bool Operation\\||", TtMacross.MacrossEditorKeyword, ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public partial class OrNode : BoolNode
    {
        public OrNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BooleanOr, "||")
        {
        }
    }
    #endregion
}
