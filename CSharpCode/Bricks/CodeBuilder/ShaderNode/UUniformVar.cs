using System;
using System.Collections.Generic;
using EngineNS.EGui.Controls.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UUniformVar : IBaseNode
    {
        public Rtti.UTypeDesc VarType;
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public EGui.Controls.NodeGraph.PinOut Out { get; set; } = new EGui.Controls.NodeGraph.PinOut();
        public UUniformVar()
        {
            VarType = Rtti.UTypeDescGetter<float>.TypeDesc;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF80FF40;
            TitleImage.Color = 0xFF804020;
            Background.Color = 0x80808080;

            //Name = $"{Value}";

            Out.Name = "v";
            Out.Link = UShaderEditorStyles.Instance.NewInOutPinDesc();
            this.AddPinOut(Out);
        }
        public override System.Type GetOutPinType(EGui.Controls.NodeGraph.PinOut pin)
        {
            return VarType.SystemType;
        }
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
            Var.IsLocalVar = false;
            Var.VarName = this.Name;
            Var.DefType = Type2HLSLType(VarType.SystemType, false);

            return new OpUseDefinedVar(Var);
        }
        public override IExpression GetExpr(UMaterialGraph funGraph, ICodeGen cGen, EGui.Controls.NodeGraph.PinOut oPin, bool bTakeResult)
        {
            var expr = GetVarExpr(funGraph, cGen, oPin, bTakeResult);
            if (oPin == Out)
            {
                return expr;
            }
            return null;
        }
    }
}
