using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UUniformVar : UNodeBase
    {
        public Rtti.UTypeDesc VarType;
        [Browsable(false)]
        public PinOut Out { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutXY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutX { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutY { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutZ { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut OutW { get; set; } = new PinOut();
        public UUniformVar()
        {
            VarType = Rtti.UTypeDescGetter<float>.TypeDesc;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF80FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;

            //Name = $"{Value}";

            Out.Name = "v";
            Out.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(Out);

            OutXY.Name = "xy";
            OutXY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXY);

            OutXYZ.Name = "xyz";
            OutXYZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutXYZ);
            OutX.Name = "x";
            OutX.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutX);
            OutY.Name = "y";
            OutY.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutY);
            OutZ.Name = "z";
            OutZ.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutZ);
            OutW.Name = "w";
            OutW.LinkDesc = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(OutW);
        }
        public override Rtti.UTypeDesc GetOutPinType(PinOut pin)
        {
            if(pin == OutXY)
            {
                return Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Vector2));
            }
            else if (pin == OutXYZ)
            {
                return Rtti.UTypeDesc.TypeOf(typeof(EngineNS.Vector3));
            }
            else if((pin == OutX) ||
                    (pin == OutY) ||
                    (pin == OutZ) ||
                    (pin == OutW))
            {
                return Rtti.UTypeDesc.TypeOf(typeof(float));
            }
            
            return VarType;
        }
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

            return true;
        }
        public override bool CanLinkTo(PinOut oPin, UNodeBase InNode, PinIn iPin)
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
        //public virtual IExpression GetVarExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    DefineVar Var = new DefineVar();
        //    Var.IsLocalVar = false;
        //    Var.VarName = this.Name;
        //    Var.DefType = Type2HLSLType(VarType.SystemType, false);

        //    return new OpUseDefinedVar(Var);
        //}
        //public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, PinOut oPin, bool bTakeResult)
        //{
        //    var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
        //    if (oPin == Out)
        //    {
        //        return expr;
        //    }
        //    return null;
        //}

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin == OutXY)
            {
                return new UVariableReferenceExpression("xy", new UVariableReferenceExpression(Name));
            }
            else if (pin == OutXYZ)
            {
                return new UVariableReferenceExpression("xyz", new UVariableReferenceExpression(Name));
            }
            else if (pin == OutX)
            {
                return new UVariableReferenceExpression("x", new UVariableReferenceExpression(Name));
            }
            else if (pin == OutY)
            {
                return new UVariableReferenceExpression("y", new UVariableReferenceExpression(Name));
            }
            else if (pin == OutZ)
            {
                return new UVariableReferenceExpression("z", new UVariableReferenceExpression(Name));
            }
            else if (pin == OutW)
            {
                return new UVariableReferenceExpression("w", new UVariableReferenceExpression(Name));
            }
            return new UVariableReferenceExpression(Name);
        }
    }
}
