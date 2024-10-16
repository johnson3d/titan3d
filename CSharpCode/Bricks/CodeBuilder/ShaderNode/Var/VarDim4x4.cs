using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Rtti;
using Jither.OpenEXR.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    [ContextMenu("matrixf,f4x4,float4x4", "Data\\float4x4@_serial@", TtMaterialGraph.MaterialEditorKeyword)]
    public class VarDimF4x4 : VarNode
    {
        Matrix mValue = Matrix.Identity;
        [Rtti.Meta]
        public Matrix Value { get => mValue; set => mValue = value; }
        [Browsable(false)]
        public PinIn InValue { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In11 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In12 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In13 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In14 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In21 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In22 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In23 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In24 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In31 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In32 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In33 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In34 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In41 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In42 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In43 { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn In44 { get; set; } = new PinIn();

        [Browsable(false)]
        public PinOut OutValue { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out11 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out12 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out13 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out14 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out21 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out22 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out23 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out24 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out31 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out32 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out33 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out34 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out41 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out42 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out43 { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut Out44 { get; set; } = new PinOut();

        public VarDimF4x4()
        {
            VarType = Rtti.TtTypeDescGetter<Matrix>.TypeDesc;

            InValue.Name = "matrix ";
            InValue.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(InValue);
            In11.Name = "11 ";
            In11.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In11);
            In12.Name = "12 ";
            In12.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In12);
            In13.Name = "13 ";
            In13.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In13);
            In14.Name = "14 ";
            In14.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In14);
            In21.Name = "21 ";
            In21.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In21);
            In22.Name = "22 ";
            In22.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In22);
            In23.Name = "23 ";
            In23.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In23);
            In24.Name = "24 ";
            In24.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In24);
            In31.Name = "31 ";
            In31.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In31);
            In32.Name = "32 ";
            In32.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In32);
            In33.Name = "33 ";
            In33.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In33);
            In34.Name = "34 ";
            In34.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In34);
            In41.Name = "41 ";
            In41.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In41);
            In42.Name = "42 ";
            In42.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In42);
            In43.Name = "43 ";
            In43.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In43);
            In44.Name = "44 ";
            In44.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            AddPinIn(In44);

            OutValue.Name = " matrix";
            OutValue.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutValue.MultiLinks = true;
            AddPinOut(OutValue);
            Out11.Name = " 11";
            Out11.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out11.MultiLinks = true;
            AddPinOut(Out11);
            Out12.Name = " 12";
            Out12.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out12.MultiLinks = true;
            AddPinOut(Out12);
            Out13.Name = " 13";
            Out13.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out13.MultiLinks = true;
            AddPinOut(Out13);
            Out14.Name = " 14";
            Out14.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out14.MultiLinks = true;
            AddPinOut(Out14);
            Out21.Name = " 21";
            Out21.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out21.MultiLinks = true;
            AddPinOut(Out21);
            Out22.Name = " 22";
            Out22.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out22.MultiLinks = true;
            AddPinOut(Out22);
            Out23.Name = " 23";
            Out23.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out23.MultiLinks = true;
            AddPinOut(Out23);
            Out24.Name = " 24";
            Out24.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out24.MultiLinks = true;
            AddPinOut(Out24);
            Out31.Name = " 31";
            Out31.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out31.MultiLinks = true;
            AddPinOut(Out31);
            Out32.Name = " 32";
            Out32.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out32.MultiLinks = true;
            AddPinOut(Out32);
            Out33.Name = " 33";
            Out33.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out33.MultiLinks = true;
            AddPinOut(Out33);
            Out34.Name = " 34";
            Out34.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out34.MultiLinks = true;
            AddPinOut(Out34);
            Out41.Name = " 41";
            Out41.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out41.MultiLinks = true;
            AddPinOut(Out41);
            Out42.Name = " 42";
            Out42.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out42.MultiLinks = true;
            AddPinOut(Out42);
            Out43.Name = " 43";
            Out43.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out43.MultiLinks = true;
            AddPinOut(Out43);
            Out44.Name = " 44";
            Out44.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            Out44.MultiLinks = true;
            AddPinOut(Out44);
        }

        public override TtTypeDesc GetOutPinType(PinOut pin)
        {
            if (pin == OutValue)
                return Rtti.TtTypeDesc.TypeOf(typeof(Matrix));
            else if (
                (pin == Out11) ||
                (pin == Out12) ||
                (pin == Out13) ||
                (pin == Out14) ||
                (pin == Out21) ||
                (pin == Out22) ||
                (pin == Out23) ||
                (pin == Out24) ||
                (pin == Out31) ||
                (pin == Out32) ||
                (pin == Out33) ||
                (pin == Out34) ||
                (pin == Out41) ||
                (pin == Out42) ||
                (pin == Out43) ||
                (pin == Out44) )
                return Rtti.TtTypeDesc.TypeOf(typeof(float));
            return null;
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            var type = OutNode.GetOutPinType(oPin);
            if(iPin == InValue)
            {
                if (!type.IsEqual(typeof(Matrix)))
                    return false;
            }
            else if(
                (iPin == In11) ||
                (iPin == In12) ||
                (iPin == In13) ||
                (iPin == In14) ||
                (iPin == In21) ||
                (iPin == In22) ||
                (iPin == In23) ||
                (iPin == In24) ||
                (iPin == In31) ||
                (iPin == In32) ||
                (iPin == In33) ||
                (iPin == In34) ||
                (iPin == In41) ||
                (iPin == In42) ||
                (iPin == In43) ||
                (iPin == In44) )
            {
                if (!type.IsEqual(typeof(float)))
                    return false;
            }

            return true;
        }
        protected override string GetDefaultValue()
        {
            if (IsHalfPrecision)
                throw new InvalidOperationException("half matrix not support!");
            else
                return $"float4x4({Value.M11},{Value.M12},{Value.M13},{Value.M14}," +
                                $"{Value.M21},{Value.M22},{Value.M23},{Value.M24}," +
                                $"{Value.M31},{Value.M32},{Value.M33},{Value.M34}," +
                                $"{Value.M41},{Value.M42},{Value.M43},{Value.M44})";
        }
        void BuildStatementWithPin(PinIn pin, string varName, ref BuildCodeStatementsData data)
        {
            var pinNode = data.NodeGraph.GetOppositePinNode(pin);
            var opPin = data.NodeGraph.GetOppositePin(pin);
            pinNode.BuildStatements(opPin, ref data);

            var assignStatment = new TtAssignOperatorStatement()
            {
                From = pinNode.GetExpression(opPin, ref data),
                To = new TtVariableReferenceExpression(varName, new TtVariableReferenceExpression(Name)),
            };
            if (!data.CurrentStatements.Contains(assignStatment))
                data.CurrentStatements.Add(assignStatment);
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            base.BuildStatements(pin, ref data);
            if(data.NodeGraph.PinHasLinker(InValue))
            {
                var pinNode = data.NodeGraph.GetOppositePinNode(InValue);
                var opPin = data.NodeGraph.GetOppositePin(InValue);
                pinNode.BuildStatements(opPin, ref data);
                var assignStatement = new TtAssignOperatorStatement()
                {
                    From = pinNode.GetExpression(opPin, ref data),
                    To = new TtVariableReferenceExpression(Name),
                };
                if (!data.CurrentStatements.Contains(assignStatement))
                    data.CurrentStatements.Add(assignStatement);
            }
            if (data.NodeGraph.PinHasLinker(In11))
                BuildStatementWithPin(In11, "_11", ref data);
            if (data.NodeGraph.PinHasLinker(In12))
                BuildStatementWithPin(In12, "_12", ref data);
            if (data.NodeGraph.PinHasLinker(In13))
                BuildStatementWithPin(In13, "_13", ref data);
            if (data.NodeGraph.PinHasLinker(In14))
                BuildStatementWithPin(In14, "_14", ref data);

            if (data.NodeGraph.PinHasLinker(In21))
                BuildStatementWithPin(In21, "_21", ref data);
            if (data.NodeGraph.PinHasLinker(In22))
                BuildStatementWithPin(In22, "_22", ref data);
            if (data.NodeGraph.PinHasLinker(In23))
                BuildStatementWithPin(In23, "_23", ref data);
            if (data.NodeGraph.PinHasLinker(In24))
                BuildStatementWithPin(In24, "_24", ref data);

            if (data.NodeGraph.PinHasLinker(In31))
                BuildStatementWithPin(In31, "_31", ref data);
            if (data.NodeGraph.PinHasLinker(In32))
                BuildStatementWithPin(In32, "_32", ref data);
            if (data.NodeGraph.PinHasLinker(In33))
                BuildStatementWithPin(In33, "_33", ref data);
            if (data.NodeGraph.PinHasLinker(In34))
                BuildStatementWithPin(In34, "_34", ref data);

            if (data.NodeGraph.PinHasLinker(In41))
                BuildStatementWithPin(In41, "_41", ref data);
            if (data.NodeGraph.PinHasLinker(In42))
                BuildStatementWithPin(In42, "_42", ref data);
            if (data.NodeGraph.PinHasLinker(In43))
                BuildStatementWithPin(In43, "_43", ref data);
            if (data.NodeGraph.PinHasLinker(In44))
                BuildStatementWithPin(In44, "_44", ref data);

            if(!data.MethodDec.HasLocalVariable(Name))
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
            if (pin == OutValue)
                return new TtVariableReferenceExpression(Name);

            else if (pin == Out11)
                return new TtVariableReferenceExpression("_11", new TtVariableReferenceExpression(Name));
            else if (pin == Out12)
                return new TtVariableReferenceExpression("_12", new TtVariableReferenceExpression(Name));
            else if (pin == Out13)
                return new TtVariableReferenceExpression("_13", new TtVariableReferenceExpression(Name));
            else if (pin == Out14)
                return new TtVariableReferenceExpression("_14", new TtVariableReferenceExpression(Name));

            else if (pin == Out21)
                return new TtVariableReferenceExpression("_21", new TtVariableReferenceExpression(Name));
            else if (pin == Out22)
                return new TtVariableReferenceExpression("_22", new TtVariableReferenceExpression(Name));
            else if (pin == Out23)
                return new TtVariableReferenceExpression("_23", new TtVariableReferenceExpression(Name));
            else if (pin == Out24)
                return new TtVariableReferenceExpression("_24", new TtVariableReferenceExpression(Name));

            else if (pin == Out31)
                return new TtVariableReferenceExpression("_31", new TtVariableReferenceExpression(Name));
            else if (pin == Out32)
                return new TtVariableReferenceExpression("_32", new TtVariableReferenceExpression(Name));
            else if (pin == Out33)
                return new TtVariableReferenceExpression("_33", new TtVariableReferenceExpression(Name));
            else if (pin == Out34)
                return new TtVariableReferenceExpression("_34", new TtVariableReferenceExpression(Name));

            else if (pin == Out41)
                return new TtVariableReferenceExpression("_41", new TtVariableReferenceExpression(Name));
            else if (pin == Out42)
                return new TtVariableReferenceExpression("_42", new TtVariableReferenceExpression(Name));
            else if (pin == Out43)
                return new TtVariableReferenceExpression("_43", new TtVariableReferenceExpression(Name));
            else if (pin == Out44)
                return new TtVariableReferenceExpression("_44", new TtVariableReferenceExpression(Name));
            return null;
        }
    }
}
