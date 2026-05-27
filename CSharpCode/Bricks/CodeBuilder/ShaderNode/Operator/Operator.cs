using System;
using System.Collections.Generic;
using System.Reflection;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Operator
{
    public class Binocular : TtShadeBaseNode
    {
        public Rtti.TtTypeDesc LeftType;
        [Browsable(false)]
        public TtBinaryOperatorExpression.EBinaryOperation Op { get; set; }
        [Rtti.Meta("")]
        [Browsable(false)]
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
        [Browsable(false)]
        public PinIn Left { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn Right { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut Result { get; set; } = new PinOut();
        public Binocular(TtBinaryOperatorExpression.EBinaryOperation InOp, string name)
        {
            Op = InOp;
            Name = name;

            Left.Name = " L";
            Right.Name = " R";
            Result.Name = "= ";
            Result.MultiLinks = true;

            Left.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Right.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Result.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
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
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (data.NodeGraph.PinHasLinker(Left))
            {
                var opPin = data.NodeGraph.GetOppositePin(Left);
                data.NodeGraph.GetOppositePinNode(Left).BuildStatements(opPin, ref data);
            }
            if(data.NodeGraph.PinHasLinker(Right))
            {
                var opPin = data.NodeGraph.GetOppositePin(Right);
                if (opPin != null)
                {
                    data.NodeGraph.GetOppositePinNode(Right).BuildStatements(opPin, ref data);
                }
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin != Result)
                return null;
            var binOp = new TtBinaryOperatorExpression()
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
        public ValueOpNode(TtBinaryOperatorExpression.EBinaryOperation op, string name)
            : base(op, name)
        {
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            if (LeftType != null)
                EGui.Controls.CtrlUtility.DrawHelper(LeftType.FullName);
        }
        public override void OnRemoveLinker(TtPinLinker linker)
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

                if (testType.IsEqual(typeof(float)))
                    return true;
                return TtCodeGeneratorBase.CanConvert(testType, LeftType);
            }
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin, linker);

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
    [ContextMenu("add,+", "Operation\\+", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class AddNode : ValueOpNode
    {
        public AddNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Add, "+")
        {
        }
    }
    [ContextMenu("subtraction,-", "Operation\\-", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class SubNode : ValueOpNode
    {
        public SubNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Subtract, "-")
        {
        }
    }
    [ContextMenu("multiplication,*", "Operation\\*", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class MulNode : ValueOpNode
    {
        public MulNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Multiply, "*")
        {
        }
    }
    [ContextMenu("division,/", "Operation\\/", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class DivNode : ValueOpNode
    {
        public DivNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Divide, "/")
        {
        }
    }
    [ContextMenu("mod,%", "Operation\\%", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class ModNode : ValueOpNode
    {
        public ModNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.Modulus, "%")
        {
        }
    }
    [ContextMenu("bitand,&", "Operation\\&", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseAnd, "&")
        {

        }
    }
    [ContextMenu("bitor,|", "Operation\\|", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(TtBinaryOperatorExpression.EBinaryOperation.BitwiseOr, "|")
        {
        }
    }
    #endregion

    #region Monocular
    public class Monocular : TtShadeBaseNode
    {
        public Rtti.TtTypeDesc InputType;
        [Rtti.Meta("")]
        [Browsable(false)]
        public string FunctionName { get; set; }
        [Rtti.Meta("")]
        [Browsable(false)]
        public string InputTypeString
        {
            get
            {
                if (InputType != null)
                    return Rtti.TtTypeDesc.TypeStr(InputType);
                return "";
            }
            set
            {
                InputType = Rtti.TtTypeDesc.TypeOf(value);
            }
        }
        [Browsable(false)]
        public PinIn Input { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut Result { get; set; } = new PinOut();
        public Monocular(string functionName, string displayName)
        {
            FunctionName = functionName;
            Name = displayName;

            Input.Name = " In";
            Result.Name = "= ";
            Result.MultiLinks = true;

            Input.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Result.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(Input);
            AddPinOut(Result);
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return InputType;
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (data.NodeGraph.PinHasLinker(Input))
            {
                var opPin = data.NodeGraph.GetOppositePin(Input);
                data.NodeGraph.GetOppositePinNode(Input).BuildStatements(opPin, ref data);
            }
        }
        protected virtual string GetReturnValueName()
        {
            return $"tmp_r_{FunctionName}_{(uint)NodeId.GetHashCode()}";
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == null || pin != Result)
                return null;

            var retValName = GetReturnValueName();
            var methodInvoke = new TtMethodInvokeStatement()
            {
                MethodName = FunctionName,
            };

            if (InputType != null)
            {
                methodInvoke.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(InputType),
                    VariableName = retValName,
                    InitValue = new TtDefaultValueExpression(InputType),
                };
                if (!data.MethodDec.HasLocalVariable(retValName))
                    data.MethodDec.AddLocalVar(methodInvoke.ReturnValue);
            }

            var inputExp = data.NodeGraph.GetOppositePinExpression(Input, ref data);
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                OperationType = EMethodArgumentAttribute.Default,
                Expression = inputExp,
            });

            if (!data.CurrentStatements.Contains(methodInvoke))
                data.CurrentStatements.Add(methodInvoke);

            return new TtVariableReferenceExpression(retValName);
        }
    }

    public class FunctionOpNode : Monocular
    {
        public FunctionOpNode(string functionName, string displayName)
            : base(functionName, displayName)
        {
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            if (InputType != null)
                EGui.Controls.CtrlUtility.DrawHelper(InputType.FullName);
        }
        public override void OnRemoveLinker(TtPinLinker linker)
        {
            if (linker.InPin == Input)
            {
                InputType = null;
            }
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin, linker);

            if (iPin == Input)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return;

                var newType = nodeExpr.GetOutPinType(oPin);
                if (InputType != null && InputType != newType)
                {
                    this.ParentGraph.RemoveLinkedOut(this.Result);
                }
                InputType = newType;
            }
        }
    }

    // Derivative
    [ContextMenu("ddx", "Operation\\Derivative\\ddx", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class DdxNode : FunctionOpNode
    {
        public DdxNode()
            : base("ddx", "ddx")
        {
        }
    }
    [ContextMenu("ddy", "Operation\\Derivative\\ddy", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class DdyNode : FunctionOpNode
    {
        public DdyNode()
            : base("ddy", "ddy")
        {
        }
    }
    [ContextMenu("fwidth", "Operation\\Derivative\\fwidth", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class FwidthNode : FunctionOpNode
    {
        public FwidthNode()
            : base("fwidth", "fwidth")
        {
        }
    }

    // Trigonometric
    [ContextMenu("sin", "Operation\\Trigonometric\\sin", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class SinNode : FunctionOpNode
    {
        public SinNode()
            : base("sin", "sin")
        {
        }
    }
    [ContextMenu("cos", "Operation\\Trigonometric\\cos", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class CosNode : FunctionOpNode
    {
        public CosNode()
            : base("cos", "cos")
        {
        }
    }
    [ContextMenu("tan", "Operation\\Trigonometric\\tan", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class TanNode : FunctionOpNode
    {
        public TanNode()
            : base("tan", "tan")
        {
        }
    }
    [ContextMenu("asin", "Operation\\Trigonometric\\asin", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class AsinNode : FunctionOpNode
    {
        public AsinNode()
            : base("asin", "asin")
        {
        }
    }
    [ContextMenu("acos", "Operation\\Trigonometric\\acos", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class AcosNode : FunctionOpNode
    {
        public AcosNode()
            : base("acos", "acos")
        {
        }
    }
    [ContextMenu("atan", "Operation\\Trigonometric\\atan", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class AtanNode : FunctionOpNode
    {
        public AtanNode()
            : base("atan", "atan")
        {
        }
    }

    // Math
    [ContextMenu("abs", "Operation\\Math\\abs", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class AbsNode : FunctionOpNode
    {
        public AbsNode()
            : base("abs", "abs")
        {
        }
    }
    [ContextMenu("sign", "Operation\\Math\\sign", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class SignNode : FunctionOpNode
    {
        public SignNode()
            : base("sign", "sign")
        {
        }
    }
    [ContextMenu("floor", "Operation\\Math\\floor", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class FloorNode : FunctionOpNode
    {
        public FloorNode()
            : base("floor", "floor")
        {
        }
    }
    [ContextMenu("ceil", "Operation\\Math\\ceil", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class CeilNode : FunctionOpNode
    {
        public CeilNode()
            : base("ceil", "ceil")
        {
        }
    }
    [ContextMenu("round", "Operation\\Math\\round", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class RoundNode : FunctionOpNode
    {
        public RoundNode()
            : base("round", "round")
        {
        }
    }
    [ContextMenu("frac", "Operation\\Math\\frac", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class FracNode : FunctionOpNode
    {
        public FracNode()
            : base("frac", "frac")
        {
        }
    }
    [ContextMenu("sqrt", "Operation\\Math\\sqrt", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class SqrtNode : FunctionOpNode
    {
        public SqrtNode()
            : base("sqrt", "sqrt")
        {
        }
    }
    [ContextMenu("rsqrt", "Operation\\Math\\rsqrt", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class RsqrtNode : FunctionOpNode
    {
        public RsqrtNode()
            : base("rsqrt", "rsqrt")
        {
        }
    }
    [ContextMenu("exp", "Operation\\Math\\exp", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class ExpNode : FunctionOpNode
    {
        public ExpNode()
            : base("exp", "exp")
        {
        }
    }
    [ContextMenu("exp2", "Operation\\Math\\exp2", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class Exp2Node : FunctionOpNode
    {
        public Exp2Node()
            : base("exp2", "exp2")
        {
        }
    }
    [ContextMenu("log", "Operation\\Math\\log", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class LogNode : FunctionOpNode
    {
        public LogNode()
            : base("log", "log")
        {
        }
    }
    [ContextMenu("log2", "Operation\\Math\\log2", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class Log2Node : FunctionOpNode
    {
        public Log2Node()
            : base("log2", "log2")
        {
        }
    }
    [ContextMenu("rcp", "Operation\\Math\\rcp", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class RcpNode : FunctionOpNode
    {
        public RcpNode()
            : base("rcp", "rcp")
        {
        }
    }

    // Vector
    [ContextMenu("normalize", "Operation\\Vector\\normalize", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class NormalizeNode : FunctionOpNode
    {
        public NormalizeNode()
            : base("normalize", "normalize")
        {
        }
    }
    [ContextMenu("length", "Operation\\Vector\\length", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class LengthNode : FunctionOpNode
    {
        public LengthNode()
            : base("length", "length")
        {
        }
    }

    // Value
    [ContextMenu("saturate", "Operation\\Value\\saturate", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class SaturateNode : FunctionOpNode
    {
        public SaturateNode()
            : base("saturate", "saturate")
        {
        }
    }
    [ContextMenu("trunc", "Operation\\Value\\trunc", ShaderNode.TtMaterialGraph.MaterialEditorKeyword)]
    public class TruncNode : FunctionOpNode
    {
        public TruncNode()
            : base("trunc", "trunc")
        {
        }
    }
    #endregion
}
