using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class VarNode : INodeExpr
    {
        public DefineVar Var;
        public Rtti.UTypeDesc VarType;
        public EGui.Controls.NodeGraph.PinIn SetPin { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public EGui.Controls.NodeGraph.PinOut GetPin { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
        public VarNode()
        {
            //AddPinIn(BeforeExec);
            //AddPinOut(AfterExec);

            SetPin.Name = "Set";
            GetPin.Name = "Get";
            SetPin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            GetPin.Link = MacrossStyles.Instance.NewInOutPinDesc();
            SetPin.Link.CanLinks.Add("Dummy");
            GetPin.Link.CanLinks.Add("Value");

            AddPinIn(SetPin);
            AddPinOut(GetPin);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (VarType == null)
                return;
            EGui.Controls.CtrlUtility.DrawHelper($"VarType:{VarType.ToString()}");
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            if (iPin == SetPin)
            {
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return ICodeGen.CanConvert(testType, VarType.SystemType);
            }
            return true;
        }
        public override IExpression GetExpr(FunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            Var.VarName = this.Name;
            Var.DefType = VarType.FullName;
            if (string.IsNullOrEmpty(Var.InitValue))
            {
                if (SetPin.EditValue != null)
                {
                    if (SetPin.EditValue.Value != null)
                        Var.InitValue = SetPin.EditValue.Value.ToString();
                    else
                        Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
                }
                else
                {
                    Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
                }
            }

            if (this.Var.IsLocalVar)
            {
                funGraph.Function.AddLocalVar(this.Var);
            }

            return new OpUseDefinedVar(Var);
        }
    }

    public class MemberVar : VarNode
    {
        public static MemberVar NewMemberVar(DefineClass kls, string varName)
        {
            var result = new MemberVar();
            result.Initialize(kls, varName);
            return result;
        }
        public MemberVar()
        {
            Icon = MacrossStyles.Instance.MemberIcon;
            TitleImage.Color = MacrossStyles.Instance.VarTitleColor;
            Background.Color = MacrossStyles.Instance.VarBGColor;
        }
        private void Initialize(DefineClass kls, string varName)
        {
            mDefClass = kls;
            MemberName = varName;
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var klsGraph = tagObject as ClassGraph;
            if (klsGraph == null)
                return;

            mDefClass = klsGraph.DefClass;
        }
        private DefineClass mDefClass;
        [Rtti.Meta]
        public string MemberName
        {
            get
            {
                if (Var == null)
                    return null;
                return Var.VarName;
            }
            protected set
            {
                Var = mDefClass.FindVar(value);
                if (Var != null)
                {
                    VarType = Rtti.UTypeDesc.TypeOfFullName(Var.DefType);
                    Name = Var.VarName;
                }
            }
        }
    }

    public class ClassMemberVar : VarNode
    {   
        public static ClassMemberVar NewClassMemberVar(DefineClass kls, string varName)
        {
            var result = new ClassMemberVar();
            result.Initialize(kls, varName);
            return result;
        }
        public EGui.Controls.NodeGraph.PinIn Self { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public ClassMemberVar()
        {
            Icon = MacrossStyles.Instance.MemberIcon;
            TitleImage.Color = MacrossStyles.Instance.VarTitleColor;
            Background.Color = MacrossStyles.Instance.VarBGColor;

            Self.Name = "Self";
            Self.Link = MacrossStyles.Instance.NewInOutPinDesc();
            Self.Link.CanLinks.Add("Value");

            RemovePinIn(SetPin);
            AddPinIn(Self);
        }
        private void Initialize(DefineClass kls, string varName)
        {
            mDefClass = kls;
            MemberName = varName;
        }
        private DefineClass mDefClass;
        [Rtti.Meta(Order = 0)]
        public DefineClass DefClass
        {
            get => mDefClass;
            set => mDefClass = value;
        }
        [Rtti.Meta(Order = 1)]
        public string MemberName
        {
            get
            {
                if (Var == null)
                    return null;
                return Var.VarName;
            }
            protected set
            {
                Var = mDefClass.FindVar(value);
                if (Var != null)
                {
                    VarType = Rtti.UTypeDesc.TypeOfFullName(Var.DefType);
                    Name = Var.VarName;
                }
            }
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            if (iPin == Self)
            {
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                var klsDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(mDefClass.GetFullName());
                if (klsDesc == null)
                    return false;
                return ICodeGen.CanConvert(testType, klsDesc.SystemType);
            }
            return true;
        }
        public override IExpression GetExpr(FunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            Var.VarName = this.Name;
            Var.DefType = VarType.FullName;
            if (string.IsNullOrEmpty(Var.InitValue))
            {
                if (SetPin.EditValue != null)
                {
                    if (SetPin.EditValue.Value != null)
                        Var.InitValue = SetPin.EditValue.Value.ToString();
                    else
                        Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
                }
                else
                {
                    Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
                }
            }

            var result =new OpUseDefinedVar(Var);
            var links = new List<EGui.Controls.NodeGraph.PinLinker>();
            funGraph.FindInLinker(Self, links);
            if (links.Count == 1)
            {
                var selfNode = links[0].OutNode as INodeExpr;
                result.Self = selfNode.GetExpr(funGraph, cGen, true) as OpExpress;
            }
            else if (links.Count == 0)
            {
                if (funGraph.MacrossEditor.DefClass.GetFullName() != mDefClass.GetFullName())
                    throw new GraphException(this, Self, $"Please link self:{Var.DefType}");
            }
            return result;
        }
    }
    
    public class LocalVar : VarNode
    {
        public LocalVar()
        {
            Var = new DefineVar();
            Var.VisitMode = EVisitMode.Local;
            Var.IsLocalVar = true;
            Var.DefType = typeof(int).FullName;

            Name = Var.VarName;
            Icon = MacrossStyles.Instance.LocalVarIcon;
            TitleImage.Color = MacrossStyles.Instance.VarTitleColor;
            Background.Color = MacrossStyles.Instance.VarBGColor;

            EditObject = new LVarEditObject();
            EditObject.Host = this;
        }
        [Rtti.Meta]
        public DefineVar LVar
        {
            get { return Var; }
            set
            {
                Var = value;
                Name = Var.VarName;
                if (SetPin.EditValue != null)
                {
                    SetPin.EditValue.Value = Var.InitValue;
                }
            }
        }
        LVarEditObject EditObject;
        private class LVarEditObject
        {
            public LocalVar Host;
            public string VarName
            {
                get { return Host.Name; }
                set => Host.Name = value;
            }
            public string Comment { get; set; }
            public string CurrentType
            {
                get
                {
                    if (Host.VarType == null)
                        return null;
                    return Host.VarType.FullName;
                }
            }
        }
        protected override object GetPropertyEditObject()
        {
            return EditObject;
        }
    }
    public class AnyVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public AnyVar()
        {
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(System.Type), SetPin) as EGui.Controls.NodeGraph.TypeSelectorEValue;
            edtValue.Selector.BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(object).FullName);
            SetPin.EditValue = edtValue;
            Var.InitValue = "null";
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            VarType = ev.Value as Rtti.UTypeDesc;
        }
        [Rtti.Meta]
        public string VarTypeString
        {
            get
            {
                if (VarType != null)
                    return Rtti.UTypeDesc.TypeStr(VarType);
                return "Unknown";
            }
            set
            {
                VarType = Rtti.UTypeDesc.TypeOf(value);
                var edtValue = SetPin.EditValue as EGui.Controls.NodeGraph.TypeSelectorEValue;
                if (edtValue != null)
                {
                    edtValue.Selector.SelectedType = VarType;
                    edtValue.Value = VarType;
                    OnValueChanged(edtValue);
                }
            }
        }
    }
    public class SByteLVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public SByteLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(sbyte).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(sbyte), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Int16LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Int16LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Int16).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Int16), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Int32LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Int32LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Int32).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Int32), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Int64LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Int64LVar()
        {
            VarType = Rtti.UTypeDescGetter<Int64>.TypeDesc;
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Int64), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class ByteLVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public ByteLVar()
        {
            VarType = Rtti.UTypeDescGetter<byte>.TypeDesc;
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(byte), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class UInt16LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public UInt16LVar()
        {
            VarType = Rtti.UTypeDescGetter<UInt16>.TypeDesc;
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(UInt16), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class UInt32LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public UInt32LVar()
        {
            VarType = Rtti.UTypeDescGetter<UInt32>.TypeDesc;
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(UInt32), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class UInt64LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public UInt64LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(UInt64).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(UInt64), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class FloatLVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public FloatLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(float).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(float), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class DoubleLVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public DoubleLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(double).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(double), SetPin);
            edtValue.Value = 0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class StringLVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public StringLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(string).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(string), SetPin);
            edtValue.Value = "";
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Vector2LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Vector2LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector2).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Vector2), SetPin);
            edtValue.Value = new Vector2(0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Vector3LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Vector3LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector3).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Vector3), SetPin);
            edtValue.Value = new Vector3(0, 0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }
    public class Vector4LVar : LocalVar, EGui.Controls.NodeGraph.EditableValue.IValueEditNotify
    {
        public Vector4LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector4).FullName);
            var edtValue = EGui.Controls.NodeGraph.EditableValue.CreateEditableValue(this, typeof(Vector4), SetPin);
            edtValue.Value = new Vector4(0, 0, 0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(EGui.Controls.NodeGraph.EditableValue ev)
        {
            Var.InitValue = ev.Value.ToString();
        }
    }

    #region VarSetNode
    public class VarSetNode : INodeExpr
    {
        public EGui.Controls.NodeGraph.PinIn Left { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public EGui.Controls.NodeGraph.PinIn Right { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        public Rtti.UTypeDesc LeftType;
        public VarSetNode()
        {
            Name = " = ";

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleImage.Color = MacrossStyles.Instance.FlowControlTitleColor;
            Background.Color = MacrossStyles.Instance.BGColor;

            Left.Name = " L";
            Right.Name = " R";

            Left.Link = MacrossStyles.Instance.NewInOutPinDesc();
            Right.Link = MacrossStyles.Instance.NewInOutPinDesc();

            Left.Link.CanLinks.Add("Value");
            Right.Link.CanLinks.Add("Value");

            AddPinIn(BeforeExec);
            AddPinOut(AfterExec);

            AddPinIn(Left);
            AddPinIn(Right);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (LeftType == null)
            {
                var linker = ParentGraph.FindInLinkerSingle(Left);
                if (linker != null)
                {
                    var nodeExpr = linker.OutNode as INodeExpr;
                    if (nodeExpr == null)
                        return;
                    LeftType = Rtti.UTypeDesc.TypeOf(nodeExpr.GetOutPinType(linker.Out));
                }
            }
            if (LeftType != null)
                EGui.Controls.CtrlUtility.DrawHelper($"SetType:{LeftType.ToString()}");
        }
        public override void OnLoadLinker(PinLinker linker)
        {
            if (LeftType == null)
            {
                var nodeExpr = linker.OutNode as INodeExpr;
                if (nodeExpr == null)
                    return;
                LeftType = Rtti.UTypeDesc.TypeOf(nodeExpr.GetOutPinType(linker.Out));
            }
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
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return ICodeGen.CanConvert(testType, LeftType.SystemType);
            }
            else if (iPin == Left)
            {
                var nodeExpr = OutNode as VarNode;
                if (nodeExpr == null)
                    return false;
            }
            return true;
        }
        public override void OnLinkedFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == Left)
            {
                var nodeExpr = OutNode as INodeExpr;
                if (nodeExpr != null)
                {
                    var newType = nodeExpr.GetOutPinType(oPin);
                    if (LeftType.SystemType != newType)
                    {//类型改变，所有输入输出都需要断开
                        this.ParentGraph.RemoveLinkedIn(this.Right);
                    }
                    LeftType = Rtti.UTypeDesc.TypeOf(newType);
                    return;
                }
            }
        }
        public override IExpression GetExpr(FunctionGraph funGraph, ICodeGen cGen, bool bTakeResult)
        {
            var binOp = new AssignOp();
            var links = new List<EGui.Controls.NodeGraph.PinLinker>();
            funGraph.FindInLinker(Left, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Left link error : {links.Count}");
            }
            var leftNode = links[0].OutNode as INodeExpr;
            var leftExpr = leftNode.GetExpr(funGraph, cGen, true) as OpExpress;
            var leftType = leftNode.GetOutPinType(links[0].Out);
            binOp.Left = leftExpr;

            links.Clear();
            funGraph.FindInLinker(Right, links);
            if (links.Count != 1)
            {
                throw new GraphException(this, Left, $"Right link error : {links.Count}");
            }
            var rightNode = links[0].OutNode as INodeExpr;
            var rightExpr = rightNode.GetExpr(funGraph, cGen, true) as OpExpress;
            var rightType = rightNode.GetOutPinType(links[0].Out);
            if (rightType != leftType)
            {
                if (!ICodeGen.CanConvert(rightType, leftType))
                {
                    throw new GraphException(this, Right, $"Cant convert from {rightType.FullName} to {leftType.FullName}");
                }
                var cvtExpr = new ConvertTypeOp();
                cvtExpr.TargetType = cGen.GetTypeString(leftType);
                cvtExpr.ObjExpr = rightExpr;
                binOp.Right = cvtExpr;
            }
            else
            {
                binOp.Right = rightExpr;
            }

            binOp.NextExpr = this.GetNextExpr(funGraph, cGen);

            return binOp;
        }
    }
    #endregion

    
}
