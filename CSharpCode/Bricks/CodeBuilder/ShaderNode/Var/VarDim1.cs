using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;

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
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            var material = data.UserData as UMaterial;
            var varNode = this;
            if (varNode.IsUniform && (material.FindVar(Name) == null))
            {
                var type = this.GetType();
                var valueProp = type.GetProperty("Value");
                var value = valueProp.GetValue(this, null);
                var tmp = new Graphics.Pipeline.Shader.UMaterial.NameValuePair();
                tmp.VarType = data.CodeGen.GetTypeString(varNode.VarType);
                tmp.Name = this.Name;
                tmp.Value = value.ToString();
                material.UsedUniformVars.Add(tmp);
            }
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
        public Rtti.UTypeDesc VarType;
        public override void OnMouseStayPin(NodePin stayPin)
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
        public static string Type2HLSLType(Rtti.UTypeDesc type, bool IsHalfPrecision)
        {
            if (type.IsEqual(typeof(float)))
                return IsHalfPrecision ? "half" : "float";
            else if (type.IsEqual(typeof(Vector2)))
                return IsHalfPrecision ? "half2" : "float2";
            else if (type.IsEqual(typeof(Vector3)))
                return IsHalfPrecision ? "half3" : "float3";
            else if (type.IsEqual(typeof(Vector4)))
                return IsHalfPrecision ? "half4" : "float4";
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
    [ContextMenu("f1,float1", "Data\\float@_serial@", UMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF1 : VarNode
    {
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
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
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InX { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutX { get; set; } = new PinOut();
        public VarDimF1()
        {
            VarType = Rtti.UTypeDescGetter<float>.TypeDesc;

            //Name = $"{Value}";

            InX.Name = "x";
            InX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);

            OutX.Name = "x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
        }
        //public override void PreGenExpr()
        //{
        //    Executed = false;
        //}
        //bool Executed = false;
        //public override IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    DefineVar Var = new DefineVar();
        //    Var.IsLocalVar = !IsUniform;
        //    Var.VarName = this.Name;
        //    Var.DefType = Type2HLSLType(VarType, IsHalfPrecision);

        //    Var.InitValue = GetDefaultValue();

        //    if (Var.IsLocalVar)
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
        //    }
        //    else
        //    {
        //        funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
        //    }
        //    if (Executed)
        //    {
        //        return new OpUseDefinedVar(Var);
        //    }
        //    Executed = true;
        //    var seq = new ExecuteSequence();
        //    var linkX = funGraph.FindInLinkerSingle(InX);
        //    if (linkX != null)
        //    {
        //        var exprNode = linkX.OutNode as IBaseNode;
        //        var xyExpr = exprNode.GetExpr(funGraph, cGen, linkX.OutPin, true) as OpExpress;
        //        var setExpr = new AssignOp();
        //        setExpr.Left = new OpUseDefinedVar(Var);
        //        setExpr.Right = xyExpr;

        //        seq.Lines.Add(setExpr);
        //    }
        //    return new OpExecuteAndUseDefinedVar(seq, Var);
        //}        
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
        //    if (oPin == OutX)
        //    {
        //        return expr;
        //    }
        //    return null;
        //}
        public override void BuildStatements(ref BuildCodeStatementsData data)
        {
            base.BuildStatements(ref data);
            if(data.NodeGraph.PinHasLinker(InX))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InX);
                var opPin = data.NodeGraph.GetOppositePin(InX);
                pinNode.BuildStatements(ref data);

                var assignStatement = new UAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new UVariableReferenceExpression(Name),
                };
                if(!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }

            if(!data.MethodDec.HasLocalVariable(Name))
            {
                var val = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = Name,
                    InitValue = new UPrimitiveExpression(Value),
                };
                if (IsUniform)
                {
                    var graph = data.NodeGraph as UMaterialGraph;
                    graph.ShaderEditor.MaterialOutput.UniformVars.Add(val);
                }
                else
                    data.MethodDec.AddLocalVar(val);
            }
        }
        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == OutX)
                return new UVariableReferenceExpression(Name);
            return null;
        }
    }
}
