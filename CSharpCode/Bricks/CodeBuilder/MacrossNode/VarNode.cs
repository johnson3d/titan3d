using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls.PropertyGrid;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public partial class VarNode : UNodeBase
    {
        public UVariableDeclaration Var;
        public Rtti.UTypeDesc VarType;
        public PinIn SetPin { get; set; } = new PinIn();
        public PinOut GetPin { get; set; } = new PinOut();
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
        public VarNode()
        {
            //AddPinIn(BeforeExec);
            //AddPinOut(AfterExec);

            SetPin.Name = "Set";
            GetPin.Name = "Get";
            SetPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            GetPin.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
            SetPin.LinkDesc.CanLinks.Add("Value");
            GetPin.LinkDesc.CanLinks.Add("Value");

            //AddPinIn(SetPin);
            //AddPinOut(GetPin);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (VarType == null)
                return;
            if (stayPin == GetPin || stayPin == SetPin)
                EGui.Controls.CtrlUtility.DrawHelper($"VarType:{VarType.ToString()}");
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            if (iPin == SetPin)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                return UCodeGeneratorBase.CanConvert(testType, VarType);
            }
            return true;
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    Var.VarName = this.Name;
        //    Var.DefType = VarType.FullName;
        //    if (string.IsNullOrEmpty(Var.InitValue))
        //    {
        //        if (SetPin.EditValue != null)
        //        {
        //            if (SetPin.EditValue.Value != null)
        //                Var.InitValue = SetPin.EditValue.Value.ToString();
        //            else
        //                Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
        //        }
        //        else
        //        {
        //            Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
        //        }
        //    }

        //    if (this.Var.IsLocalVar)
        //    {
        //        funGraph.Function.AddLocalVar(this.Var);
        //    }

        //    return new OpUseDefinedVar(Var);
        //}
    }

    [ContextMenu("self,this,my,myself", "Self\\Self", UMacross.MacrossEditorKeyword)]
    public partial class SelfNode : UNodeBase
    {
        public PinOut OutPin { get; set; } = new PinOut();
        public SelfNode()
        {
            AddPinOut(OutPin);
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new USelfReferenceExpression();
        }

        //public override Type GetOutPinType(PinOut pin)
        //{
        //    return base.GetOutPinType(pin);
        //}
    }
    [ContextMenu("null", "Data\\POD\\null", UMacross.MacrossEditorKeyword)]
    public partial class NullNode : UNodeBase
    {
        public PinOut OutPin { get; set; } = new PinOut();
        public NullNode()
        {
            AddPinOut(OutPin);
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new UNullValueExpression();
        }

        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.UTypeDescGetter<object>.TypeDesc;
        }
    }

    public partial class MemberVar : VarNode, UEditableValue.IValueEditNotify, IAfterExecNode, IBeforeExecNode, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public PinOut AfterExec { get; set; } = new PinOut();
        public PinIn BeforeExec { get; set; } = new PinIn();

        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public string DefaultValue { get; set; } = null;
        }
        [Rtti.Meta(Order = 2)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                if (SetPin.EditValue != null)
                    tmp.DefaultValue = SetPin.EditValue.Value.ToString();
                return tmp;
            }
            set
            {
                if (SetPin.EditValue != null)
                {
                    SetPin.EditValue.Value = Support.TConvert.ToObject(VarType, value.DefaultValue);
                    OnValueChanged(SetPin.EditValue);
                }
            }
        }

        public static MemberVar NewMemberVar(UClassDeclaration kls, string varName, bool isGet)
        {
            var result = new MemberVar();
            result.Initialize(kls, varName, isGet);
            return result;
        }
        public MemberVar()
        {
            Icon = MacrossStyles.Instance.MemberIcon;
            TitleColor = MacrossStyles.Instance.VarTitleColor;
            BackColor = MacrossStyles.Instance.VarBGColor;
        }
        private void Initialize(UClassDeclaration kls, string varName, bool isGet)
        {
            mDefClass = kls;
            IsGet = isGet;
            if(MemberName != varName)
                MemberName = varName;

            //VarType = kls.TryGetTypeDesc();

            if (isGet)
                AddPinOut(GetPin);
            else
            {
                BeforeExec.Name = " >>";
                AfterExec.Name = ">> ";
                BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
                AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
                AddPinIn(BeforeExec);
                AddPinOut(AfterExec);

                if(Var != null)
                    SetPin.EditValue = UEditableValue.CreateEditableValue(this, Var.VariableType.TypeDesc, SetPin);

                AddPinIn(SetPin);
                AddPinOut(GetPin);
            }
            OnPositionChanged();
        }
        public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            base.OnPreRead(tagObject, hostObject, fromXml);
            var klsGraph = hostObject as UMacrossMethodGraph;
            if (klsGraph == null)
                return;
            mDefClass = klsGraph.MacrossEditor.DefClass;
        }
        private UClassDeclaration mDefClass;
        [Rtti.Meta(Order = 1)]
        public string MemberName
        {
            get
            {
                if (Var == null)
                    return null;
                return Var.VariableName;
            }
            protected set
            {
                Var = mDefClass.FindMember(value);
                if (Var != null)
                {
                    VarType = Var.VariableType.TypeDesc;
                    Name = Var.VariableName;
                }
                else
                {
                    HasError = true;
                    CodeExcept = new GraphException(this, null, $"Member {value} not found");
                }
                Initialize(mDefClass, MemberName, IsGet);
            }
        }
        bool mIsGet = true;
        [Rtti.Meta(Order = 0)]
        public bool IsGet
        {
            get => mIsGet;
            set => mIsGet = value;
        }

        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            if (IsGet)
                return;

            if (!data.NodeGraph.PinHasLinker(SetPin))
                return;
            var assignSt = new UAssignOperatorStatement();
            var srcExp = data.NodeGraph.GetOppositePinExpression(SetPin, ref data);
            var oppoType = data.NodeGraph.GetOppositePinType(SetPin);
            var curType = GetInPinType(SetPin);
            if (oppoType != curType)
            {
                srcExp = new UCastExpression()
                {
                    TargetType = new UTypeReference(curType),
                    SourceType = new UTypeReference(oppoType),
                    Expression = srcExp,
                };
            }
            assignSt.From = srcExp;
            assignSt.To = new UVariableReferenceExpression()
            {
                VariableName = MemberName,
                IsProperty = true,
            };
            data.CurrentStatements.Add(assignSt);

            var oppoNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (oppoNode != null)
                oppoNode.BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            return new UVariableReferenceExpression()
            {
                VariableName = MemberName,
                IsProperty = true,
            };
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if(Var != null)
                return Var.VariableType.TypeDesc;
            return null;
        }
        public override Rtti.UTypeDesc GetInPinType(PinIn pin)
        {
            if (Var != null)
                return Var.VariableType.TypeDesc;
            return null;
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (Var == null)
                return;
            if(stayPin == SetPin || stayPin == GetPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper(Var.VariableType.TypeDesc.FullName);
            }
        }

        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            if (IsGet)
                return;

            var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
            proDesc.Name = Name;
            proDesc.DisplayName = Name;
            proDesc.PropertyType = VarType;
            //proDesc.CustomValueEditor = SetPin.EditValue;
            collection.Add(proDesc);
        }

        public object GetPropertyValue(string propertyName)
        {
            if (IsGet)
                return null;
            return SetPin.EditValue.Value;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            if (IsGet)
                return;
            SetPin.EditValue.Value = value;
        }

        public void OnValueChanged(UEditableValue ev)
        {
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }
    public partial class ClassPropertyVar : VarNode, UEditableValue.IValueEditNotify, IBeforeExecNode, IAfterExecNode, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public static ClassPropertyVar NewClassProperty(Rtti.UClassMeta.PropertyMeta meta, bool isGet)
        {
            var result = new ClassPropertyVar();
            result.Initialize(meta, isGet);
            return result;
        }
        public PinIn Self;

        bool mIsGet = true;
        [Rtti.Meta(Order = 0)]
        public bool IsGet
        {
            get => mIsGet;
            set => mIsGet = value;
        }
        public string mClassPropertyMeta;
        [Rtti.Meta(Order = 1)]
        public string ClassPropertyMeta
        {
            get => mClassPropertyMeta;
            set
            {
                mClassPropertyMeta = value;
                var segs = value.Split('#');
                if (segs.Length != 2)
                    return;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if(kls != null)
                {
                    var pro = kls.CurrentVersion.GetProperty(segs[1]);
                    if (pro != null)
                        Initialize(pro, mIsGet);
                }
            }
        }
        public Rtti.UClassMeta HostClass
        {
            get
            {
                var segs = mClassPropertyMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                return Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
            }
        }
        public Rtti.UClassMeta.PropertyMeta ClassProperty
        {
            get
            {
                var segs = mClassPropertyMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                    return kls.CurrentVersion.GetProperty(segs[1]);
                return null;
            }
        }

        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public string DefaultValue { get; set; } = null;
        }
        [Rtti.Meta(Order = 2)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                if(SetPin.EditValue != null)
                    tmp.DefaultValue = SetPin.EditValue.Value.ToString();
                return tmp;
            }
            set
            {
                if (SetPin.EditValue != null)
                {
                    SetPin.EditValue.Value = Support.TConvert.ToObject(VarType, value.DefaultValue);
                    OnValueChanged(SetPin.EditValue);
                }
            }
        }

        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();
        public ClassPropertyVar()
        {
            Icon = MacrossStyles.Instance.MemberIcon;
            TitleColor = MacrossStyles.Instance.VarTitleColor;
            BackColor = MacrossStyles.Instance.VarBGColor;
            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
        }
        private void Initialize(Rtti.UClassMeta.PropertyMeta pro, bool isGet)
        {
            if(string.IsNullOrEmpty(mClassPropertyMeta))
            {
                mClassPropertyMeta = Rtti.UTypeDesc.TypeStr(pro.PropInfo.DeclaringType) + "#" + pro.PropInfo.Name;
            }
            mIsGet = isGet;
            Name = (isGet ? "Get " : "Set ") + pro.PropertyName;
            VarType = pro.FieldType;
            System.Diagnostics.Debug.Assert(Rtti.UTypeDesc.TypeOf(pro.PropInfo.PropertyType) == pro.FieldType);

            if (isGet && pro.PropInfo.CanRead)
            {
                var getMethod = pro.PropInfo.GetGetMethod();
                if(getMethod.IsStatic == false)
                {
                    Self = new PinIn()
                    {
                        Name = "Self",
                    };
                    AddPinIn(Self);
                }

                GetPin = new PinOut();
                AddPinOut(GetPin);
            }
            else if(!isGet && pro.PropInfo.CanWrite)
            {
                AddPinIn(BeforeExec);
                AddPinOut(AfterExec);

                var setMethod = pro.PropInfo.GetSetMethod();
                if(setMethod.IsStatic == false)
                {
                    Self = new PinIn();
                    Self.LinkDesc = MacrossStyles.Instance.NewInOutPinDesc();
                    Self.LinkDesc.CanLinks.Add("Value");
                    Self.Name = "Self";
                    AddPinIn(Self);
                }

                SetPin = new PinIn();
                SetPin.EditValue = UEditableValue.CreateEditableValue(this, pro.PropInfo.PropertyType, SetPin);
                AddPinIn(SetPin);

                GetPin = new PinOut();
                AddPinOut(GetPin);
            }
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if(stayPin == Self)
            {
                EGui.Controls.CtrlUtility.DrawHelper(HostClass.ClassType.FullName);
            }
            else if(stayPin == SetPin || stayPin == GetPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper(ClassProperty.FieldType.FullName);
            }
        }
        UExpressionBase GetHostExpression(ref BuildCodeStatementsData data)
        {
            UExpressionBase hostExp = null;
            if(Self != null)
            {
                if (data.NodeGraph.PinHasLinker(Self))
                    hostExp = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                var hostClass = HostClass;
                // property is static
                if (hostClass != null)
                    hostExp = new UClassReferenceExpression() { Class = hostClass.ClassType };
            }
            return hostExp;
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            if (IsGet)
                return;

            var assignSt = new UAssignOperatorStatement();
            if (data.NodeGraph.PinHasLinker(SetPin))
            {
                var srcExp = data.NodeGraph.GetOppositePinExpression(SetPin, ref data);
                var oppoType = data.NodeGraph.GetOppositePinType(SetPin);
                var curType = GetInPinType(SetPin);
                if (oppoType != curType)
                {
                    srcExp = new UCastExpression()
                    {
                        TargetType = new UTypeReference(curType),
                        SourceType = new UTypeReference(oppoType),
                        Expression = srcExp,
                    };
                }
                assignSt.From = srcExp;
            }
            else
                assignSt.From = new UPrimitiveExpression(SetPin.EditValue.ValueType, SetPin.EditValue.Value);

            assignSt.To = new UVariableReferenceExpression()
            {
                Host = GetHostExpression(ref data),
                VariableName = ClassProperty.PropertyName,
                IsProperty = true,
            };
            data.CurrentStatements.Add(assignSt);

            var oppoNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (oppoNode != null)
                oppoNode.BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin != GetPin)
                return null;

            return new UVariableReferenceExpression()
            {
                Host = GetHostExpression(ref data),
                VariableName = ClassProperty.PropertyName,
                IsProperty = true,
            };
        }

        public void OnValueChanged(UEditableValue ev)
        {

        }

        public override Rtti.UTypeDesc GetInPinType(PinIn pin)
        {
            return ClassProperty.FieldType;
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return ClassProperty.FieldType;
        }

        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            if (IsGet)
                return;

            var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
            proDesc.Name = Name;
            proDesc.DisplayName = Name;
            proDesc.PropertyType = VarType;
            //proDesc.CustomValueEditor = SetPin.EditValue;
            collection.Add(proDesc);
        }

        public object GetPropertyValue(string propertyName)
        {
            if (IsGet)
                return null;
            return SetPin.EditValue.Value;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            if (IsGet)
                return;
            SetPin.EditValue.Value = value;
            OnValueChanged(SetPin.EditValue);
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }

    public partial class ClassFieldVar : VarNode, UEditableValue.IValueEditNotify, IBeforeExecNode, IAfterExecNode, IPropertyCustomization
    {   
        public static ClassFieldVar NewClassMemberVar(Rtti.UClassMeta.FieldMeta meta, bool isGet)
        {
            var result = new ClassFieldVar();
            result.Initialize(meta, isGet);
            return result;
        }
        public PinIn Self { get; set; }

        public Rtti.UClassMeta HostClass
        {
            get
            {
                var segs = mClassFieldMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                return Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
            }
        }

        public Rtti.UClassMeta.FieldMeta ClassField
        {
            get
            {
                var segs = mClassFieldMeta.Split('#');
                if (segs.Length != 2)
                    return null;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if (kls != null)
                    return kls.GetField(segs[1]);
                return null;
            }
        }
        bool mIsGet = true;
        [Rtti.Meta(Order = 0)]
        public bool IsGet
        {
            get => mIsGet;
            set => mIsGet = value;
        }
        public string mClassFieldMeta;
        [Rtti.Meta(Order = 1)]
        public string ClassFieldMeta
        {
            get => mClassFieldMeta;
            set
            {
                mClassFieldMeta = value;
                var segs = value.Split('#');
                if (segs.Length != 2)
                    return;
                var kls = Rtti.UClassMetaManager.Instance.GetMeta(segs[0]);
                if(kls != null)
                {
                    var mem = kls.GetField(segs[1]);
                    if (mem != null)
                        Initialize(mem, mIsGet);
                }
            }
        }

        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public string DefaultValue { get; set; } = null;
        }
        [Rtti.Meta(Order = 2)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                if (SetPin.EditValue != null)
                    tmp.DefaultValue = SetPin.EditValue.Value.ToString();
                return tmp;
            }
            set
            {
                if (SetPin.EditValue != null)
                {
                    SetPin.EditValue.Value = Support.TConvert.ToObject(VarType, value.DefaultValue);
                    OnValueChanged(SetPin.EditValue);
                }
            }
        }

        public PinIn BeforeExec { get; set; } = new PinIn();
        public PinOut AfterExec { get; set; } = new PinOut();
        public ClassFieldVar()
        {
            Icon = MacrossStyles.Instance.MemberIcon;
            TitleColor = MacrossStyles.Instance.VarTitleColor;
            BackColor = MacrossStyles.Instance.VarBGColor;
            BeforeExec.Name = " >>";
            AfterExec.Name = ">> ";
            BeforeExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
            AfterExec.LinkDesc = MacrossStyles.Instance.NewExecPinDesc();
        }
        private void Initialize(Rtti.UClassMeta.FieldMeta m, bool isGet)
        {
            if (string.IsNullOrEmpty(mClassFieldMeta))
                mClassFieldMeta = Rtti.UTypeDesc.TypeStr(m.Field.DeclaringType) + "#" + m.Field.Name;
            mIsGet = isGet;
            Name = (isGet ? "Get " : "Set ") + m.Field.Name;

            if(!m.Field.IsStatic)
            {
                Self = new PinIn()
                {
                    Name = "Self",
                };
                AddPinIn(Self);
            }

            VarType = Rtti.UTypeDesc.TypeOf(m.Field.FieldType);
            if (isGet)
            {
                GetPin = new PinOut();
                AddPinOut(GetPin);
            }
            else
            {
                AddPinIn(BeforeExec);
                AddPinOut(AfterExec);

                SetPin = new PinIn()
                {
                    EditValue = UEditableValue.CreateEditableValue(this, m.Field.FieldType, SetPin),
                };
                AddPinIn(SetPin);

                GetPin = new PinOut();
                AddPinOut(GetPin);
            }
        }
        //private void Initialize(DefineClass kls, string varName)
        //{
        //    mDefClass = kls;
        //    MemberName = varName;
        //}
        //private DefineClass mDefClass;
        //[Rtti.Meta(Order = 0)]
        //public DefineClass DefClass
        //{
        //    get => mDefClass;
        //    set => mDefClass = value;
        //}

        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            if (iPin == Self)
            {
                var nodeExpr = OutNode;
                if (nodeExpr == null)
                    return true;
                var testType = nodeExpr.GetOutPinType(oPin);
                var klsDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(HostClass.ClassType.FullName);
                if (klsDesc == null)
                    return false;
                return UCodeGeneratorBase.CanConvert(testType, klsDesc);
            }
            return true;
        }
        //public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
        //{
        //    Var.VarName = this.Name;
        //    Var.DefType = VarType.FullName;
        //    if (string.IsNullOrEmpty(Var.InitValue))
        //    {
        //        if (SetPin.EditValue != null)
        //        {
        //            if (SetPin.EditValue.Value != null)
        //                Var.InitValue = SetPin.EditValue.Value.ToString();
        //            else
        //                Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
        //        }
        //        else
        //        {
        //            Var.InitValue = cGen.GetDefaultValue(VarType.SystemType);
        //        }
        //    }

        //    var result =new OpUseDefinedVar(Var);
        //    var links = new List<UPinLinker>();
        //    funGraph.FindInLinker(Self, links);
        //    if (links.Count == 1)
        //    {
        //        var selfNode = links[0].OutNode as UNodeExpr;
        //        result.Self = selfNode.GetExpr(funGraph, cGen, true) as OpExpress;
        //    }
        //    else if (links.Count == 0)
        //    {
        //        if (funGraph.MacrossEditor.DefClass.GetFullName() != mDefClass.GetFullName())
        //            throw new GraphException(this, Self, $"Please link self:{Var.DefType}");
        //    }
        //    return result;
        //}
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (stayPin == Self)
            {
                EGui.Controls.CtrlUtility.DrawHelper(HostClass.ClassType.FullName);
            }
            else if (stayPin == SetPin || stayPin == GetPin)
            {
                EGui.Controls.CtrlUtility.DrawHelper(ClassField.Field.FieldType.FullName);
            }
        }

        UExpressionBase GetHostExpression(ref BuildCodeStatementsData data)
        {
            UExpressionBase hostExp = null;
            if (Self != null)
            {
                if (data.NodeGraph.PinHasLinker(Self))
                    hostExp = data.NodeGraph.GetOppositePinExpression(Self, ref data);
            }
            else
            {
                var hostClass = HostClass;
                // property is static
                if (hostClass != null)
                    hostExp = new UClassReferenceExpression() { Class = hostClass.ClassType };
            }
            return hostExp;
        }
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            if (IsGet)
                return;

            var assignSt = new UAssignOperatorStatement();
            if (data.NodeGraph.PinHasLinker(SetPin))
            {
                var srcExp = data.NodeGraph.GetOppositePinExpression(SetPin, ref data);
                var oppoType = data.NodeGraph.GetOppositePinType(SetPin);
                var curType = GetInPinType(SetPin);
                if (oppoType != curType)
                {
                    srcExp = new UCastExpression()
                    {
                        TargetType = new UTypeReference(curType),
                        SourceType = new UTypeReference(oppoType),
                        Expression = srcExp,
                    };
                }
                assignSt.From = srcExp;
            }
            else
                assignSt.From = new UPrimitiveExpression(SetPin.EditValue.ValueType, SetPin.EditValue.Value);

            assignSt.To = new UVariableReferenceExpression()
            {
                Host = GetHostExpression(ref data),
                VariableName = ClassField.Field.Name,
                IsProperty = false,
            };
            data.CurrentStatements.Add(assignSt);

            var oppoNode = data.NodeGraph.GetOppositePinNode(AfterExec);
            if (oppoNode != null)
                oppoNode.BuildStatements(ref data);
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin != GetPin)
                return null;

            return new UVariableReferenceExpression()
            {
                Host = GetHostExpression(ref data),
                VariableName = ClassField.Field.Name,
                IsProperty = false,
            };
        }

        public void OnValueChanged(UEditableValue ev)
        {

        }

        public override Rtti.UTypeDesc GetInPinType(PinIn pin)
        {
            return Rtti.UTypeDesc.TypeOf(ClassField.Field.FieldType);
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.UTypeDesc.TypeOf(ClassField.Field.FieldType);
        }

        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            if (IsGet)
                return;

            var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
            proDesc.Name = Name;
            proDesc.DisplayName = Name;
            proDesc.PropertyType = VarType;
            //proDesc.CustomValueEditor = SetPin.EditValue;
            collection.Add(proDesc);
        }

        public object GetPropertyValue(string propertyName)
        {
            if (IsGet)
                return null;
            return SetPin.EditValue.Value;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            if (IsGet)
                return;
            SetPin.EditValue.Value = value;
            OnValueChanged(SetPin.EditValue);
        }

        public void LightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = true;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.LightDebuggerLine();
            }
        }

        public void UnLightDebuggerLine()
        {
            var linker = ParentGraph.GetFirstLinker(BeforeExec);
            if (linker != null)
            {
                linker.InDebuggerLine = false;
                var node = ParentGraph.GetOppositePinNode(BeforeExec) as IBeforeExecNode;
                if (node != null)
                    node.UnLightDebuggerLine();
            }
        }
    }
    
    public partial class LocalVar : VarNode
    {
        protected UEditableValue EditValue;
        public LocalVar()
        {
            //Var = new UVariableDeclaration();
            //Var.VisitMode = EVisisMode.Local;
            //Var.VariableType = new UTypeReference(typeof(int));

            //Name = Var.VariableName;
            Icon = MacrossStyles.Instance.LocalVarIcon;
            TitleColor = MacrossStyles.Instance.VarTitleColor;
            BackColor = MacrossStyles.Instance.VarBGColor;

            //EditObject = new LVarEditObject();
            //EditObject.Host = this;
            AddPinOut(GetPin);
        }
        //[Rtti.Meta]
        //public UVariableDeclaration LVar
        //{
        //    get { return Var; }
        //    set
        //    {
        //        Var = value;
        //        Name = Var.VariableName;
        //        if (SetPin.EditValue != null)
        //        {
        //            SetPin.EditValue.Value = Var.InitValue;
        //        }
        //    }
        //}
        //LVarEditObject EditObject;
        //private class LVarEditObject
        //{
        //    public LocalVar Host;
        //    public string VarName
        //    {
        //        get { return Host.Name; }
        //        set => Host.Name = value;
        //    }
        //    public string Comment { get; set; }
        //    public string CurrentType
        //    {
        //        get
        //        {
        //            if (Host.VarType == null)
        //                return null;
        //            return Host.VarType.FullName;
        //        }
        //    }
        //}
        public override object GetPropertyEditObject()
        {
            return SetPin.EditValue;
        }
    }
    //[ContextMenu("AnyVar", "Data\\AnyVar@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class AnyVar : LocalVar, UEditableValue.IValueEditNotify
    {
        public AnyVar()
        {
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(System.Type), SetPin) as UTypeSelectorEValue;
            edtValue.Selector.BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(object).FullName);
            SetPin.EditValue = edtValue;
            Var.InitValue = new UNullValueExpression();
        }
        public void OnValueChanged(UEditableValue ev)
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
                var edtValue = EditValue as UTypeSelectorEValue;
                if (edtValue != null)
                {
                    edtValue.Selector.SelectedType = VarType;
                    edtValue.Value = VarType;
                    OnValueChanged(edtValue);
                }
            }
        }
    }
    [ContextMenu("Bool", "Data\\POD\\Bool@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class BoolLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        bool mValue;
        [Rtti.Meta]
        public bool Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public BoolLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(bool).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(bool), SetPin);
            edtValue.Value = false;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((SByte)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((SByte)ev.Value);
            mValue = (bool)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (bool)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("SByte", "Data\\POD\\SByte@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class SByteLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        SByte mValue;
        [Rtti.Meta]
        public SByte Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public SByteLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(sbyte).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(sbyte), SetPin);
            edtValue.Value = (sbyte)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((SByte)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((SByte)ev.Value);
            mValue = (SByte)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (SByte)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Int16", "Data\\POD\\Int16@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Int16LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Int16 mValue;
        [Rtti.Meta]
        public Int16 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Int16LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Int16).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(Int16), SetPin);
            edtValue.Value = (Int16)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Int16)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Int16)ev.Value);
            mValue = (Int16)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Int16)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Int32", "Data\\POD\\Int32@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Int32LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Int32 mValue;
        [Rtti.Meta]
        public Int32 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Int32LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Int32).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(Int32), SetPin);
            edtValue.Value = (Int32)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Int32)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Int32)ev.Value);
            mValue = (Int32)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Int32)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Int64", "Data\\POD\\Int64@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Int64LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Int64 mValue;
        [Rtti.Meta]
        public Int64 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Int64LVar()
        {
            VarType = Rtti.UTypeDescGetter<Int64>.TypeDesc;
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(Int64), SetPin);
            edtValue.Value = (Int64)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Int64)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Int64)ev.Value);
            mValue = (Int64)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Int64)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Byte", "Data\\POD\\Byte@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class ByteLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Byte mValue;
        [Rtti.Meta]
        public Byte Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public ByteLVar()
        {
            VarType = Rtti.UTypeDescGetter<byte>.TypeDesc;
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(byte), SetPin);
            edtValue.Value = (byte)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Byte)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Byte)ev.Value);
            mValue = (Byte)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Byte)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("UInt16", "Data\\POD\\UInt16@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class UInt16LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        UInt16 mValue;
        [Rtti.Meta]
        public UInt16 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public UInt16LVar()
        {
            VarType = Rtti.UTypeDescGetter<UInt16>.TypeDesc;
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(UInt16), SetPin);
            edtValue.Value = (UInt16)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((UInt16)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((UInt16)ev.Value);
            mValue = (UInt16)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (UInt16)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("UInt32", "Data\\POD\\UInt32@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class UInt32LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        UInt32 mValue;
        [Rtti.Meta]
        public UInt32 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public UInt32LVar()
        {
            VarType = Rtti.UTypeDescGetter<UInt32>.TypeDesc;
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(UInt32), SetPin);
            edtValue.Value = (UInt32)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((UInt32)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((UInt32)ev.Value);
            mValue = (UInt32)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (UInt32)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("UInt64", "Data\\POD\\UInt64@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class UInt64LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        UInt64 mValue;
        [Rtti.Meta]
        public UInt64 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public UInt64LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(UInt64).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(UInt64), SetPin);
            edtValue.Value = (UInt64)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((UInt64)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((UInt64)ev.Value);
            mValue = (UInt64)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (UInt64)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("float", "Data\\POD\\Float@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class FloatLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        float mValue;
        [Rtti.Meta]
        public float Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public FloatLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(float).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(float), SetPin);
            edtValue.Value = (float)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((float)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((float)ev.Value);
            mValue = (float)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (float)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("double", "Data\\POD\\Double@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class DoubleLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        double mValue;
        [Rtti.Meta]
        public double Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public DoubleLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(double).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(double), SetPin);
            edtValue.Value = (double)0;
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((double)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((double)ev.Value);
            mValue = (double)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (double)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("string", "Data\\POD\\String@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class StringLVar : LocalVar, UEditableValue.IValueEditNotify
    {
        string mValue;
        [Rtti.Meta]
        public string Value 
        {
            get => mValue;
            set
            {
                if(mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public StringLVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(string).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(string), SetPin);
            edtValue.Value = "";
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((string)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((string)ev.Value);
            mValue = (string)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (string)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            if(Value == null)
                EGui.Controls.CtrlUtility.DrawHelper("null");
            else
                EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Vector2", "Data\\POD\\BaseData\\Vector2@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Vector2LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Vector2 mValue;
        [Rtti.Meta]
        public Vector2 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Vector2LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector2).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(Vector2), SetPin);
            edtValue.Value = new Vector2(0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Vector2)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Vector2)ev.Value);
            mValue = (Vector2)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Vector2)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Vector3", "Data\\POD\\BaseData\\Vector3@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Vector3LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Vector3 mValue;
        [Rtti.Meta]
        public Vector3 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Vector3LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector3).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, typeof(Vector3), SetPin);
            edtValue.Value = new Vector3(0, 0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Vector3)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Vector3)ev.Value);
            mValue = (Vector3)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Vector3)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }
    [ContextMenu("Vector4", "Data\\POD\\BaseData\\Vector4@_serial@", UMacross.MacrossEditorKeyword)]
    public partial class Vector4LVar : LocalVar, UEditableValue.IValueEditNotify
    {
        Vector4 mValue;
        [Rtti.Meta]
        public Vector4 Value
        {
            get => mValue;
            set
            {
                if (mValue != value)
                    SetPin.EditValue.Value = value;
                mValue = value;
            }
        }

        public Vector4LVar()
        {
            VarType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Vector4).FullName);
            var edtValue = UEditableValue.CreateEditableValue(this, Rtti.UTypeDesc.TypeOf(typeof(Vector4)), SetPin);
            edtValue.Value = new Vector4(0, 0, 0, 0);
            SetPin.EditValue = edtValue;
        }
        public void OnValueChanged(UEditableValue ev)
        {
            //var exp = Var.InitValue as UPrimitiveExpression;
            //if (exp == null)
            //{
            //    exp = new UPrimitiveExpression((Vector4)ev.Value);
            //    Var.InitValue = exp;
            //}
            //else
            //    exp.SetValue((Vector4)ev.Value);
            mValue = (Vector4)ev.Value;
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            mValue = (Vector4)SetPin.EditValue.Value;
            return new UPrimitiveExpression(Value);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            EGui.Controls.CtrlUtility.DrawHelper(Value.ToString());
        }
    }

    #region VarSetNode
    //[MacrossContextMenu("VarSetNode", "Data\\VarSetNode@_serial@")]
    //public partial class VarSetNode : UNodeExpr
    //{
    //    public PinIn Left { get; set; } = new PinIn();
    //    public PinIn Right { get; set; } = new PinIn();
    //    public Rtti.UTypeDesc LeftType;
    //    public VarSetNode()
    //    {
    //        Name = " = ";

    //        Icon.Size = new Vector2(25, 25);
    //        Icon.Color = 0xFF00FF00;
    //        TitleColor = MacrossStyles.Instance.FlowControlTitleColor;
    //        BackColor = MacrossStyles.Instance.BGColor;

    //        Left.Name = " L";
    //        Right.Name = " R";

    //        Left.Link = MacrossStyles.Instance.NewInOutPinDesc();
    //        Right.Link = MacrossStyles.Instance.NewInOutPinDesc();

    //        Left.Link.CanLinks.Add("Value");
    //        Right.Link.CanLinks.Add("Value");

    //        AddPinIn(BeforeExec);
    //        AddPinOut(AfterExec);

    //        AddPinIn(Left);
    //        AddPinIn(Right);
    //    }
    //    public override void OnMouseStayPin(NodePin stayPin)
    //    {
    //        if (LeftType == null)
    //        {
    //            var linker = ParentGraph.FindInLinkerSingle(Left);
    //            if (linker != null)
    //            {
    //                var nodeExpr = linker.OutNode as UNodeExpr;
    //                if (nodeExpr == null)
    //                    return;
    //                LeftType = Rtti.UTypeDesc.TypeOf(nodeExpr.GetOutPinType(linker.OutPin));
    //            }
    //        }
    //        if (LeftType != null)
    //            EGui.Controls.CtrlUtility.DrawHelper($"SetType:{LeftType.ToString()}");
    //    }
    //    public override void OnLoadLinker(UPinLinker linker)
    //    {
    //        if (LeftType == null)
    //        {
    //            var nodeExpr = linker.OutNode as UNodeExpr;
    //            if (nodeExpr == null)
    //                return;
    //            LeftType = Rtti.UTypeDesc.TypeOf(nodeExpr.GetOutPinType(linker.OutPin));
    //        }
    //    }
    //    public override void OnRemoveLinker(UPinLinker linker)
    //    {
    //        if (linker.InPin == Left)
    //        {
    //            LeftType = null;
    //        }
    //    }
    //    public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
    //    {
    //        if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
    //            return false;

    //        if (iPin == Right)
    //        {
    //            var nodeExpr = OutNode as UNodeExpr;
    //            if (nodeExpr == null)
    //                return true;
    //            var testType = nodeExpr.GetOutPinType(oPin);
    //            return ICodeGen.CanConvert(testType, LeftType.SystemType);
    //        }
    //        else if (iPin == Left)
    //        {
    //            var nodeExpr = OutNode as VarNode;
    //            if (nodeExpr == null)
    //                return false;
    //        }
    //        return true;
    //    }
    //    public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
    //    {
    //        base.OnLinkedFrom(iPin, OutNode, oPin);

    //        if (iPin == Left)
    //        {
    //            var nodeExpr = OutNode as UNodeExpr;
    //            if (nodeExpr != null)
    //            {
    //                var newType = nodeExpr.GetOutPinType(oPin);
    //                if (LeftType.SystemType != newType)
    //                {//类型改变，所有输入输出都需要断开
    //                    this.ParentGraph.RemoveLinkedIn(this.Right);
    //                }
    //                LeftType = Rtti.UTypeDesc.TypeOf(newType);
    //                return;
    //            }
    //        }
    //    }
    //    public override IExpression GetExpr(UMacrossMethodGraph funGraph, ICodeGen cGen, bool bTakeResult)
    //    {
    //        var binOp = new AssignOp();
    //        var links = new List<UPinLinker>();
    //        funGraph.FindInLinker(Left, links);
    //        if (links.Count != 1)
    //        {
    //            throw new GraphException(this, Left, $"Left link error : {links.Count}");
    //        }
    //        var leftNode = links[0].OutNode as UNodeExpr;
    //        var leftExpr = leftNode.GetExpr(funGraph, cGen, true) as OpExpress;
    //        var leftType = leftNode.GetOutPinType(links[0].OutPin);
    //        binOp.Left = leftExpr;

    //        links.Clear();
    //        funGraph.FindInLinker(Right, links);
    //        if (links.Count != 1)
    //        {
    //            throw new GraphException(this, Left, $"Right link error : {links.Count}");
    //        }
    //        var rightNode = links[0].OutNode as UNodeExpr;
    //        var rightExpr = rightNode.GetExpr(funGraph, cGen, true) as OpExpress;
    //        var rightType = rightNode.GetOutPinType(links[0].OutPin);
    //        if (rightType != leftType)
    //        {
    //            if (!ICodeGen.CanConvert(rightType, leftType))
    //            {
    //                throw new GraphException(this, Right, $"Cant convert from {rightType.FullName} to {leftType.FullName}");
    //            }
    //            var cvtExpr = new ConvertTypeOp();
    //            cvtExpr.TargetType = cGen.GetTypeString(leftType);
    //            cvtExpr.ObjExpr = rightExpr;
    //            binOp.Right = cvtExpr;
    //        }
    //        else
    //        {
    //            binOp.Right = rightExpr;
    //        }

    //        binOp.NextExpr = this.GetNextExpr(funGraph, cGen);

    //        return binOp;
    //    }
    //}
    #endregion


}
