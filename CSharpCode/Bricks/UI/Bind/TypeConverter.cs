using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace EngineNS.UI.Bind
{
    public sealed class PropertyPathConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public abstract class TtBindTypeConvertBase
    {
        public abstract bool CanConvertTo<TTag, TSrc>();
        public abstract bool CanConvertFrom<TTag, TSrc>();
        public abstract TTag ConvertTo<TTag, TSrc>(TtBindingExpressionBase bindingExp, TSrc value);
        public abstract TSrc ConvertFrom<TTag, TSrc>(TtBindingExpressionBase bindingExp, TTag value);
    }
}
