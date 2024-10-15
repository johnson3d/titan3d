using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class VarNode : UNodeBase
    {
        public VarNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;
        }
        private bool mIsUniform = false;
        [Rtti.Meta]
        public bool IsUniform 
        {
            get => mIsUniform;
            set
            {
                mIsUniform = value;
                OnAsUniform(mIsUniform);
            }
        }
        public string VarName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        //public override void OnMaterialEditorGenCode(UMaterial Material)
        //{
        //    var varNode = this;
        //    if (varNode.IsUniform)
        //    {
        //        var type = this.GetType();
        //        var valueProp = type.GetProperty("Value");
        //        var value = valueProp.GetValue(this, null);
        //        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameValuePair();
        //        tmp.VarType = gen.GetTypeString(varNode.VarType);
        //        tmp.Name = this.Name;
        //        tmp.Value = value.ToString();
        //        Material.UsedUniformVars.Add(tmp);
        //    }
        //}
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var varNode = this;
            if (varNode.IsUniform && (material.FindVar(Name) == null))
            {
                var type = this.GetType();
                var valueProp = type.GetProperty("Value");
                var value = valueProp.GetValue(this, null);
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameValuePair();
                tmp.VarType = data.CodeGen.GetTypeString(varNode.VarType);
                tmp.Name = this.Name;
                tmp.Value = value.ToString();
                material.UsedUniformVars.Add(tmp);
            }
        }
        protected virtual void OnAddLocalVar(TtVariableDeclaration val, ref BuildCodeStatementsData data)
        {
            if (IsUniform)
            {
                var graph = data.NodeGraph as TtMaterialGraph;
                graph.UniformVars.Add(val);
            }
            else
                data.MethodDec.AddLocalVar(val);
        }
        protected virtual void OnAsUniform(bool isUniform)
        {
            if (isUniform)
            {
                foreach (var i in this.Inputs)
                {
                    this.ParentGraph.RemoveLinkedIn(i);
                }
                TitleColor = 0xFF2040af;
            }
            else
            {
                TitleColor = 0xFF804020;
            }
        }
        [Rtti.Meta]
        public bool IsHalfPrecision { get; set; } = false;
        public Rtti.TtTypeDesc VarType;
        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
        {
            if (VarType == null)
                return;
            EGui.Controls.CtrlUtility.DrawHelper($"VarType:{VarType.ToString()}");
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            if (IsUniform)
                return false;

            return true;
        }
        public override bool CanLinkTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
        {
            if (base.CanLinkTo(oPin, InNode, iPin) == false)
                return false;

            return true;
        }
        public static string Type2HLSLType(Rtti.TtTypeDesc type, bool IsHalfPrecision)
        {
            if (type.IsEqual(typeof(float)))
                return IsHalfPrecision ? "half" : "float";
            else if (type.IsEqual(typeof(Vector2)))
                return IsHalfPrecision ? "half2" : "float2";
            else if (type.IsEqual(typeof(Vector3)))
                return IsHalfPrecision ? "half3" : "float3";
            else if (type.IsEqual(typeof(Vector4)))
                return IsHalfPrecision ? "half4" : "float4";
            else if (type.IsEqual(typeof(int)))
                return IsHalfPrecision ? "min16int" : "int";
            else if (type.IsEqual(typeof(Vector2i)))
                return IsHalfPrecision ? "min16int2" : "int2";
            else if (type.IsEqual(typeof(Vector3i)))
                return IsHalfPrecision ? "min16int3" : "int3";
            else if (type.IsEqual(typeof(Vector4i)))
                return IsHalfPrecision ? "min16int4" : "int4";
            return type.FullName;
        }
        //public virtual IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    DefineVar Var = new DefineVar();
        //    Var.IsLocalVar = !IsUniform;
        //    Var.VarName = this.Name;
        //    Var.DefType = Type2HLSLType(VarType.SystemType, IsHalfPrecision);

        //    Var.InitValue = GetDefaultValue();

        //    if (Var.IsLocalVar)
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
        //    }
        //    else
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
        //    }
        //    return new OpUseDefinedVar(Var);
        //}
        protected virtual string GetDefaultValue()
        {
            return "0";
        }
    }
    [ContextMenu("f1,float1", "Data\\float@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF1 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InX)
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            return Value.ToString();
        }
        [Rtti.Meta]
        public float Value { get; set; } = 0;
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        public VarDimF1()
        {
            VarType = Rtti.TtTypeDescGetter<float>.TypeDesc;

            //Name = $"{Value}";

            InX.Name = "x";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);

            OutX.Name = "x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if(data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new TtPrimitiveExpression(Value),
                };
                OnAddLocalVar(val, ref data);
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == OutX)
                return new TtVariableReferenceExpression(Name);
            return null;
        }
    }

    [ContextMenu("i1,int1", "Data\\int@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimI1 : VarNode
    {
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return VarType;
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as UNodeBase;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InX)
            {
                if (!type.IsEqual(typeof(int)))
                    return false;
            }
            return true;
        }
        protected override string GetDefaultValue()
        {
            return Value.ToString();
        }
        [Rtti.Meta]
        public float Value { get; set; } = 0;
        [Browsable(false)]
        public PinIn InX { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        public VarDimI1()
        {
            VarType = Rtti.TtTypeDescGetter<int>.TypeDesc;

            //Name = $"{Value}";

            InX.Name = "x";
            InX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);

            OutX.Name = "x";
            OutX.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutX.MultiLinks = true;
            this.AddPinOut(OutX);
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if (data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(opPin, ref data);

                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }

            if (!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new TtPrimitiveExpression(Value),
                };
                OnAddLocalVar(val, ref data);
            }
        }
        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutX)
                return new TtVariableReferenceExpression(Name);
            return null;
        }
    }
}
