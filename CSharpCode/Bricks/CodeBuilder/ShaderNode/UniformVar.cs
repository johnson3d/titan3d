using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public partial class UUniformVar : TtShadeBaseNode
    {
        Rtti.TtTypeDesc mVarType;
        [Rtti.Meta("")]
        public Rtti.TtTypeDesc VarType 
        {
            get => mVarType;
            set
            {
                mVarType = value;
                BuildOutPins();
            }
        }
        public void BuildOutPins()
        {
            Outputs.Clear();
            Swizzles.Clear();
            if (VarType.SystemType == typeof(float))
            {
                var Out = new PinOut();
                Out.Name = "v";
                Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                Out.MultiLinks = true;
                Out.Tag = typeof(float);
                this.AddPinOut(Out);
                Swizzles.Add(Out);
            }
            else if (VarType.SystemType == typeof(uint))
            {
                var Out = new PinOut();
                Out.Name = "v";
                Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                Out.MultiLinks = true;
                Out.Tag = typeof(uint);
                this.AddPinOut(Out);
                Swizzles.Add(Out);
            }
            else if (VarType.SystemType == typeof(Vector2))
            {
                var Out = new PinOut();
                Out.Name = "v";
                Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                Out.MultiLinks = true;
                Out.Tag = typeof(Vector2);
                this.AddPinOut(Out);
                Swizzles.Add(Out);
            }
            else if (VarType.SystemType == typeof(Vector3))
            {
                var Out = new PinOut();
                Out.Name = "v";
                Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                Out.MultiLinks = true;
                Out.Tag = typeof(Vector3);
                this.AddPinOut(Out);
                Swizzles.Add(Out);
            }
            else if (VarType.SystemType == typeof(Vector4))
            {
                var Out = new PinOut();
                Out.Name = "v";
                Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                Out.MultiLinks = true;
                Out.Tag = typeof(Vector4);
                this.AddPinOut(Out);
                Swizzles.Add(Out);
            }
            else if (VarType.IsValueType)
            {
                {
                    var Out = new PinOut();
                    Out.Name = "Self";
                    Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                    Out.MultiLinks = true;
                    Out.Tag = VarType.SystemType;
                    this.AddPinOut(Out);
                    Swizzles.Add(Out);
                }
                var members = VarType.SystemType.GetFields();
                foreach (var i in members)
                {
                    var attrs = i.GetCustomAttributes(typeof(Editor.ShaderCompiler.TtShaderDefineAttribute), false);
                    if (attrs.Length == 0)
                        continue;
                    var attr = attrs[0] as Editor.ShaderCompiler.TtShaderDefineAttribute;
                    var Out = new PinOut();
                    Out.Name = attr.ShaderName;
                    Out.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
                    Out.MultiLinks = true;
                    Out.Tag = i.FieldType;
                    this.AddPinOut(Out);
                    Swizzles.Add(Out);
                }
            }
        }
        public List<PinOut> Swizzles { get; set; } = new List<PinOut>();
        /// <summary>
        /// out pin 名字直接来自 TtShaderDefineAttribute.ShaderName (见 BuildOutPins), 而旧资产里
        /// UPinLinker.TSaveData 存的是当时的 pin 名字符串。结构体字段一旦改名, 旧连线就会
        /// FindPinOut 拿不到 pin, 被 Linker.SaveData 静默 Remove 掉 —— 图看上去只是少了根线,
        /// 不报任何错。这里按 LegacyShaderName 回退到改名后的 pin, 资产重存一次后
        /// SaveData 会写回新 pin 名, 自然完成迁移。
        /// </summary>
        public override PinOut FindPinOut(string name)
        {
            var pin = base.FindPinOut(name);
            if (pin != null)
                return pin;

            if (VarType == null || VarType.SystemType == null || VarType.IsValueType == false)
                return null;
            foreach (var i in VarType.SystemType.GetFields())
            {
                var attrs = i.GetCustomAttributes(typeof(Editor.ShaderCompiler.TtShaderDefineAttribute), false);
                if (attrs.Length == 0)
                    continue;
                var attr = attrs[0] as Editor.ShaderCompiler.TtShaderDefineAttribute;
                if (string.IsNullOrEmpty(attr.LegacyShaderName) == false && attr.LegacyShaderName == name)
                    return base.FindPinOut(attr.ShaderName);
            }
            return null;
        }
        public UUniformVar()
        {
            VarType = Rtti.TtTypeDescGetter<float>.TypeDesc;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF80FF40;
            TitleColor = 0xFF804020;
            BackColor = 0x80808080;
        }
        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.TtTypeDesc.TypeOf(pin.Tag as System.Type);
        }
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            var pinType = stayPin.Tag as System.Type;
            if (VarType == null || pinType == null)
                return;
            EGui.Controls.CtrlUtility.DrawHelper($"VarType:{pinType.ToString()}");
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;

            if (VarType == null)
                return true;

            return true;
        }
        public override bool CanLinkTo(PinOut oPin, TtNodeBase InNode, PinIn iPin)
        {
            if (base.CanLinkTo(oPin, InNode, iPin) == false)
                return false;

            return true;
        }
        
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
        }

        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (VarType.SystemType == typeof(float))
            {
                return new TtVariableReferenceExpression(Name);
            }
            else if (VarType.SystemType == typeof(uint))
            {
                return new TtVariableReferenceExpression(Name);
            }
            else if (VarType.SystemType == typeof(Vector2))
            {
                return new TtVariableReferenceExpression("xy", new TtVariableReferenceExpression(Name));
            }
            else if (VarType.SystemType == typeof(Vector3))
            {
                return new TtVariableReferenceExpression("xyz", new TtVariableReferenceExpression(Name));
            }
            else if (VarType.SystemType == typeof(Vector4))
            {
                return new TtVariableReferenceExpression("xyzw", new TtVariableReferenceExpression(Name));
            }
            else 
            {
                if(pin.Name == "Self")
                {
                    return new TtVariableReferenceExpression(Name);
                }
                else
                {
                    var outType = pin.Tag as System.Type;
                    return new TtVariableReferenceExpression(pin.Name, new TtVariableReferenceExpression(Name));
                }
            }
        }
    }
}
