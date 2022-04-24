using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class Binocular : INodeExpr
    {
        public Rtti.UTypeDesc LeftType;
        public EBinocularOp Op { get; set; }

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
        public Binocular(EBinocularOp InOp)
        {
            Op = InOp;

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
            Name = Bricks.CodeBuilder.CSharp.CSGen.GetOp(InOp);

            if (Op == EBinocularOp.Assign)
            {
                AddPinIn(BeforeExec);
                AddPinOut(AfterExec);
            }

            AddPinIn(Left);
            AddPinIn(Right);
            AddPinOut(Result);

            Left.Link.CanLinks.Add("Value");
            Right.Link.CanLinks.Add("Value");
            Result.Link.CanLinks.Add("Value");

            EditObject = new BinocularEditObject();
            EditObject.Host = this;
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            return LeftType.SystemType;
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
        protected override object GetPropertyEditObject()
        {
            return EditObject;
        }
        public override IExpression GetExpr(UMacrossFunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            var binOp = new BinocularOp();
            binOp.Op = this.Op;
            var links = new List<UPinLinker>();
            funGraph.FindInLinker(Left, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Left link error : {links.Count}");
            }
            var leftNode = links[0].OutNode as INodeExpr;
            var leftExpr = leftNode.GetExpr(funGraph, cGen, true) as OpExpress;
            var leftType = leftNode.GetOutPinType(links[0].OutPin);
            binOp.Left = leftExpr;

            links.Clear();
            funGraph.FindInLinker(Right, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Right link error : {links.Count}");
            }
            var rightNode = links[0].OutNode as INodeExpr;
            var rightExpr = rightNode.GetExpr(funGraph, cGen, true) as OpExpress;
            var rightType = rightNode.GetOutPinType(links[0].OutPin);
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
    public partial class ValueOpNode : Binocular
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
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return ICodeGen.CanConvert(testType, LeftType.SystemType);
            }
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == Left)
            {//二元运算，左值决定输出类型
                var nodeExpr = OutNode as INodeExpr;
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
    public partial class AddNode : ValueOpNode
    {
        public AddNode()
            : base(EBinocularOp.Add)
        {
        }
    }
    public partial class SubNode : ValueOpNode
    {
        public SubNode()
            : base(EBinocularOp.Sub)
        {
        }
    }
    public partial class MulNode : ValueOpNode
    {
        public MulNode()
            : base(EBinocularOp.Mul)
        {
        }
    }
    public partial class DivNode : ValueOpNode
    {
        public DivNode()
            : base(EBinocularOp.Div)
        {
        }
    }
    public partial class ModNode : ValueOpNode
    {
        public ModNode()
            : base(EBinocularOp.Mod)
        {
        }
    }
    public partial class BitAndNode : ValueOpNode
    {
        public BitAndNode()
            : base(EBinocularOp.BitAnd)
        {
         
        }
    }
    public partial class BitOrNode : ValueOpNode
    {
        public BitOrNode()
            : base(EBinocularOp.BitOr)
        {
        }
    }
    #endregion

    #region Cmp
    public partial class CmpNode : Binocular
    {
        public CmpNode(EBinocularOp op)
            : base(op)
        {
            EditObject = new CmpEditObject();
            EditObject.Host = this;
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            return typeof(bool);
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
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return;

                var newType = nodeExpr.GetOutPinType(oPin);
                if (LeftType.SystemType != newType)
                {//类型改变，所有输入输出都需要断开
                    this.ParentGraph.RemoveLinkedOut(this.Result);
                    this.ParentGraph.RemoveLinkedIn(this.Right);
                }
                LeftType = Rtti.UTypeDesc.TypeOf(newType);
            }
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (iPin == Right)
            {
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return ICodeGen.CanConvert(testType, LeftType.SystemType);
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
        protected override object GetPropertyEditObject()
        {
            return EditObject;
        }
    }
    public partial class EqualNode : CmpNode
    {
        public EqualNode()
            : base(EBinocularOp.CmpEqual)
        {
            
        }
    }
    public partial class NotEqualNode : CmpNode
    {
        public NotEqualNode()
            : base(EBinocularOp.CmpNotEqual)
        {
            
        }
    }
    public partial class GreateNode : CmpNode
    {
        public GreateNode()
            : base(EBinocularOp.CmpGreate)
        {
            
        }
    }
    public partial class GreateEqualNode : CmpNode
    {
        public GreateEqualNode()
            : base(EBinocularOp.CmpGreateEqual)
        {
            
        }
    }
    public partial class LessNode : CmpNode
    {
        public LessNode()
            : base(EBinocularOp.CmpLess)
        {
            
        }
    }
    public partial class LessEqualNode : CmpNode
    {
        public LessEqualNode()
            : base(EBinocularOp.CmpLessEqual)
        {
            
        }
    }
    #endregion

    #region Bool
    public partial class BoolNode : Binocular
    {
        public BoolNode(EBinocularOp op)
            : base(op)
        {
            LeftType = Rtti.UTypeDescGetter<bool>.TypeDesc;
        }
        public override System.Type GetOutPinType(PinOut pin)
        {
            return typeof(bool);
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as INodeExpr;
            if (nodeExpr == null)
                return true;
            if (nodeExpr.GetOutPinType(oPin) != typeof(bool))
            {
                return false;
            }
            return true;
        }
    }
    public partial class AndNode : BoolNode
    {
        public AndNode()
            : base(EBinocularOp.And)
        {
        }
    }
    public partial class OrNode : BoolNode
    {
        public OrNode()
            : base(EBinocularOp.Or)
        {
        }
    }
    #endregion
}
