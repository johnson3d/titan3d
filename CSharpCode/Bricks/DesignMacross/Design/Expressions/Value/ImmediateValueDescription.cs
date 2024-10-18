using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.ComponentModel;

namespace EngineNS.DesignMacross.Design.Expressions
{
    public class TtImmediateValueDescription : TtExpressionDescription
    {
        [Rtti.Meta]
        public TtTypeDesc TypeDesc { get; set; } = TtTypeDesc.TypeOf<bool>();
        [Rtti.Meta, Category("Option"), DisplayName("Value")]
        public string StrValue { get; set; }
        public TtImmediateValueDescription()
        {
            
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {

            return null;
        }
    }

    [ContextMenu("Bool", "Data\\POD\\Bool", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtBoolValueDescription : TtImmediateValueDescription
    {
        public TtBoolValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<bool>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            bool value = false;
            bool.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(value);
        }
    }
    [ContextMenu("SByte", "Data\\POD\\SByte", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtSByteValueDescription : TtImmediateValueDescription
    {
        public TtSByteValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<SByte>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            SByte value = 0;
            SByte.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Int16", "Data\\POD\\Int16", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtInt16ValueDescription : TtImmediateValueDescription
    {
        public TtInt16ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Int16>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            Int16 value = 0;
            Int16.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Int32", "Data\\POD\\Int32", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtInt32ValueDescription : TtImmediateValueDescription
    {
        public TtInt32ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Int32>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            Int32 value = 0;
            Int32.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Int64", "Data\\POD\\Int64", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtInt64ValueDescription : TtImmediateValueDescription
    {
        public TtInt64ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Int64>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            Int64 value = 0;
            Int64.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Byte", "Data\\POD\\Byte", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtByteValueDescription : TtImmediateValueDescription
    {
        public TtByteValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Byte>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            Byte value = 0;
            Byte.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("UInt16", "Data\\POD\\UInt16", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtUInt16ValueDescription : TtImmediateValueDescription
    {
        public TtUInt16ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<UInt16>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            UInt16 value = 0;
            UInt16.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("UInt32", "Data\\POD\\UInt32", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtUInt32ValueDescription : TtImmediateValueDescription
    {
        public TtUInt32ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<UInt32>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            UInt32 value = 0;
            UInt32.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("UInt64", "Data\\POD\\UInt64", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtUInt64ValueDescription : TtImmediateValueDescription
    {
        public TtUInt64ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<UInt64>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            UInt64 value = 0;
            UInt64.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Float", "Data\\POD\\Float", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtFloatValueDescription : TtImmediateValueDescription
    {
        public TtFloatValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<float>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            float value = 0;
            float.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("Double", "Data\\POD\\Double", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtDoubleValueDescription : TtImmediateValueDescription
    {
        public TtDoubleValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<double>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            double value = 0;
            double.TryParse(StrValue, out value);
            return new TtPrimitiveExpression(TypeDesc, value);
        }
    }
    [ContextMenu("String", "Data\\POD\\String", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtStringValueDescription : TtImmediateValueDescription
    {
        public TtStringValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<string>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, StrValue);
        }
    }
    [ContextMenu("Vector2", "Data\\POD\\Vector2", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtVector2ValueDescription : TtImmediateValueDescription
    {
        public TtVector2ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Vector2>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, Vector2.FromString(StrValue));
        }
    }
    [ContextMenu("Vector3", "Data\\POD\\Vector3", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtVector3ValueDescription : TtImmediateValueDescription
    {
        public TtVector3ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Vector3>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, Vector3.FromString(StrValue));
        }
    }
    [ContextMenu("Vector4", "Data\\POD\\Vector4", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtVector4ValueDescription : TtImmediateValueDescription
    {
        public TtVector4ValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Vector4>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, Vector4.FromString(StrValue));
        }
    }
    [ContextMenu("Color3f", "Data\\POD\\Color3f", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtColor3fValueDescription : TtImmediateValueDescription
    {
        public TtColor3fValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Color3f>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, Color3f.FromString(StrValue));
        }
    }
    [ContextMenu("Color4f", "Data\\POD\\Color4f", UDesignMacross.MacrossScriptEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_ImmediateValue))]
    public class TtColor4fValueDescription : TtImmediateValueDescription
    {
        public TtColor4fValueDescription()
        {
            TypeDesc = TtTypeDesc.TypeOf<Color4f>();
            Name = TypeDesc.Name;
            AddDataOutPin(new() { Name = "", TypeDesc = TypeDesc });
        }
        public override TtExpressionBase BuildExpression(ref FExpressionBuildContext expressionBuildContext)
        {
            return new TtPrimitiveExpression(TypeDesc, Color4f.FromString(StrValue));
        }
    }
}
