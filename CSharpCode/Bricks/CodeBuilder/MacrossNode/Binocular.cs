using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class Binocular : UNodeBase
    {
        public Rtti.UTypeDesc LeftType;
        public UBinaryOperatorExpression.EBinaryOperation Op { get; set; }

        [Rtti.Meta]
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
        
        public PinIn Left { get; set; } = new PinIn();
        public PinIn Right { get; set; } = new PinIn();
        public PinOut Result { get; set; } = new PinOut();
        public Binocular(UBinaryOperatorExpression.EBinaryOperation InOp, string name)
        {
            Op = InOp;
            Name = name;

            Left.Name = " L";
            Right.Name = " R";
            Result.Name = "= ";

            Left.Link = MacrossStyles.Instance.NewInOutPinDesc();
            Right.Link = MacrossStyles.Instance.NewInOutPinDesc();
            Result.Link = MacrossStyles.Instance.NewInOutPinDesc();
            
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);

            Left.Link.CanLinks.Add("Value");
            Right.Link.CanLinks.Add("Value");
            Result.Link.CanLinks.Add("Value");

            EditObject = new BinocularEditObject();
            EditObject.Host = this;
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return LeftType;
        }
        BinocularEditObject EditObject;
        private class BinocularEditObject
        {
            public Binocular Host;
            public string VarName
            {
                get { return Host.Name; }
            }
        }
        public override object GetPropertyEditObject()
        {
            return EditObject;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin != null && pin != Result)
                return null;
            var binOp = new UBinaryOperatorExpression()
            {
                Operation = this.Op,
                Left = data.NodeGraph.GetOppositePinExpression(Left, ref data),
                Right = data.NodeGraph.GetOppositePinExpression(Right, ref data)
            };
            return binOp;
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
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
    [ContextMenu("add,+", "Operation\\+", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class AddNode : ValueOpNode
    {
        public AddNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Add, "+")
        {
        }
    }
    [ContextMenu("subtraction,-", "Operation\\-", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class SubNode : ValueOpNode
    {
        public SubNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Subtract, "-")
        {
        }
    }
    [ContextMenu("multiplication,*", "Operation\\*", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class MulNode : ValueOpNode
    {
        public MulNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Multiply, "*")
        {
        }
    }
    [ContextMenu("division,/", "Operation\\/", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class DivNode : ValueOpNode
    {
        public DivNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Divide, "/")
        {
        }
    }
    [ContextMenu("mod,%", "Operation\\%", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class ModNode : ValueOpNode
    {
        public ModNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Modulus, "%")
        {
        }
    }
    [ContextMenu("bitand,&", "Operation\\&", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseAnd, "&")
        {
         
        }
    }
    [ContextMenu("bitor,|", "Operation\\|", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseOr, "|")
        {
        }
    }
    [ContextMenu("bitxor,^", "Operation\\^", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BitXorNode : ValueOpNode
    {
        public BitXorNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseXOR, "^")
        {
        }
    }
    [ContextMenu("bitleftshift,|", "Operation\\<<", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BitLeftshiftNode : ValueOpNode
    {
        public BitLeftshiftNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseLeftShift, "<<")
        {
        }
    }
    [ContextMenu("bitrightshift,|", "Operation\\>>", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class BitRightshiftNode : ValueOpNode
    {
        public BitRightshiftNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BitwiseRightShift, ">>")
        {
        }
    }

    #endregion

    #region Cmp
    public partial class CmpNode : Binocular
    {
        public CmpNode(UBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
            EditObject = new CmpEditObject();
            EditObject.Host = this;
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.UTypeDesc.TypeOf(typeof(bool));
        }
        public override void OnMouseStayPin(NodePin stayPin)
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
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
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
                return UCodeGeneratorBase.CanConvert(testType, LeftType);
            }
            return true;
        }
        CmpEditObject EditObject;
        private class CmpEditObject
        {
            public CmpNode Host;
            public string VarName
            {
                get { return Host.Name; }
            }
            public string LeftType
            {
                get
                {
                    if (Host.LeftType == null)
                        return null;
                    return Host.LeftType.FullName;
                }
            }
        }
        public override object GetPropertyEditObject()
        {
            return EditObject;
        }
    }
    [ContextMenu("equal,==", "Bool Operation\\==", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class EqualNode : CmpNode
    {
        public EqualNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.Equality, "==")
        {
            
        }
    }
    [ContextMenu("notequal,!=", "Bool Operation\\!=", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class NotEqualNode : CmpNode
    {
        public NotEqualNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.NotEquality, "!=")
        {
            
        }
    }
    [ContextMenu("greate,>", "Bool Operation\\>", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class GreateNode : CmpNode
    {
        public GreateNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.GreaterThan, ">")
        {
            
        }
    }
    [ContextMenu("greateequal,>=", "Bool Operation\\>=", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class GreateEqualNode : CmpNode
    {
        public GreateEqualNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.GreaterThanOrEqual, ">=")
        {
            
        }
    }
    [ContextMenu("less,<", "Bool Operation\\<", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class LessNode : CmpNode
    {
        public LessNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.LessThan, "<")
        {
            
        }
    }
    [ContextMenu("lessequal,<=", "Bool Operation\\<=", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class LessEqualNode : CmpNode
    {
        public LessEqualNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.LessThanOrEqual, "<=")
        {
            
        }
    }
    #endregion

    #region Bool
    public partial class BoolNode : Binocular
    {
        public BoolNode(UBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
            LeftType = Rtti.UTypeDescGetter<bool>.TypeDesc;
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.UTypeDesc.TypeOf(typeof(bool));
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
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
    [ContextMenu("and,&&", "Bool Operation\\&&", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class AndNode : BoolNode
    {
        public AndNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BooleanAnd, "&&")
        {
        }
    }
    [ContextMenu("or,||", "Bool Operation\\||", UMacross.MacrossEditorKeyword, ShaderNode.UMaterialGraph.MaterialEditorKeyword)]
    public partial class OrNode : BoolNode
    {
        public OrNode()
            : base(UBinaryOperatorExpression.EBinaryOperation.BooleanOr, "||")
        {
        }
    }
    #endregion
}
