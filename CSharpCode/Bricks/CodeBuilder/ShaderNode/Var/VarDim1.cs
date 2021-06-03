using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    public class VarNode : IBaseNode
    {
        public VarNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF40FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;
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
        protected virtual void OnAsUniform(bool isUniform)
        {
            if (isUniform)
            {
                foreach (var i in this.Inputs)
                {
                    this.ParentGraph.RemoveLinkedIn(i);
                }
                TitleImage.Color = 0xFF2040af;
            }
            else
            {
                TitleImage.Color = 0xFF804020;
            }
        }
        [Rtti.Meta]
        public bool IsHalfPrecision { get; set; } = false;
        public Rtti.UTypeDesc VarType;
        public override void OnMouseStayPin(EGui.Controls.NodeGraph.NodePin stayPin)
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

            if (IsUniform)
                return false;

            return true;
        }
        public override bool CanLinkTo(PinOut oPin, NodeBase InNode, PinIn iPin)
        {
            if (base.CanLinkTo(oPin, InNode, iPin) == false)
                return false;

            return true;
        }
        public static string Type2HLSLType(Type type, bool IsHalfPrecision)
        {
            if (type == typeof(float))
                return IsHalfPrecision ? "half" : "float";
            else if (type == typeof(Vector2))
                return IsHalfPrecision ? "half2" : "float2";
            else if (type == typeof(Vector3))
                return IsHalfPrecision ? "half3" : "float3";
            else if (type == typeof(Vector4))
                return IsHalfPrecision ? "half4" : "float4";
            return type.FullName;
        }
        public virtual IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            DefineVar Var = new DefineVar();
            Var.IsLocalVar = !IsUniform;
            Var.VarName = this.Name;
            Var.DefType = Type2HLSLType(VarType.SystemType, IsHalfPrecision);

            Var.InitValue = GetDefaultValue();

            if (Var.IsLocalVar)
            {
                funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
            }
            else
            {
                funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
            }
            return new OpUseDefinedVar(Var);
        }
        protected virtual string GetDefaultValue()
        {
            return "0";
        }
    }
    
    public class VarDimF1 : VarNode
    {
        public override string GetTitleName()
        {
            return $"{Name}={Value}";
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
        public override bool CanLinkFrom(EGui.Controls.NodeGraph.PinIn iPin, EGui.Controls.NodeGraph.NodeBase OutNode, EGui.Controls.NodeGraph.PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var nodeExpr = OutNode as IBaseNode;
            var type = nodeExpr.GetOutPinType(oPin);

            if (iPin == InX)
            {
                if (type != typeof(float))
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
        public EGui.Controls.NodeGraph.PinIn InX { get; set; } = new EGui.Controls.NodeGraph.PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut OutX { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public VarDimF1()
        {
            VarType = Rtti.UTypeDescGetter<float>.TypeDesc;

            //Name = $"{Value}";

            InX.Name = "x";
            InX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinIn(InX);

            OutX.Name = "x";
            OutX.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
        }
        public override void PreGenExpr()
        {
            Executed = false;
        }
        bool Executed = false;
        public override IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            DefineVar Var = new DefineVar();
            Var.IsLocalVar = !IsUniform;
            Var.VarName = this.Name;
            Var.DefType = Type2HLSLType(VarType.SystemType, IsHalfPrecision);

            Var.InitValue = GetDefaultValue();

            if (Var.IsLocalVar)
            {
                funGraph.ShaderEditor.MaterialOutput.Function.AddLocalVar(Var);
            }
            else
            {
                funGraph.ShaderEditor.MaterialOutput.UniformVars.Add(Var);
            }
            if (Executed)
            {
                return new OpUseDefinedVar(Var);
            }
            Executed = true;
            var seq = new ExecuteSequence();
            var linkX = funGraph.FindInLinkerSingle(InX);
            if (linkX != null)
            {
                var exprNode = linkX.OutNode as IBaseNode;
                var xyExpr = exprNode.GetExpr(funGraph, cGen, linkX.Out, true) as OpExpress;
                var setExpr = new AssignOp();
                setExpr.Left = new OpUseDefinedVar(Var);
                setExpr.Right = xyExpr;

                seq.Lines.Add(setExpr);
            }
            return new OpExecuteAndUseDefinedVar(seq, Var);
        }        
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == OutX)
            {
                return expr;
            }
            return null;
        }
    }
}
