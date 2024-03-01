using EngineNS.UI.Bind;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Template
{
    public partial class TtTemplateSimpleValue : TtBindablePropertyValueBase
    {
        public object Value;
        public TtBindableProperty Property;
        public UInt64 PropertyNameHash;

        public TtTemplateSimpleValue() { }
        public TtTemplateSimpleValue(object value, TtBindableProperty property)
        {
            Value = value;
            Property = property;
            PropertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(property.Name);
        }

        public override void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            Value = value;
        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            return (T)Value;
        }
    }
    public abstract class TtTemplateBindingValue : TtBindablePropertyValueBase
    {
        public string PropertyName;
        public string TemplatePropertyName;
        public EBindingMode Mode = EBindingMode.Default;
        public TtTemplateBindingValue()
        {

        }
        public TtTemplateBindingValue(string propertyName, string templatePropertyName, EBindingMode mode = EBindingMode.Default)
        {
            PropertyName = propertyName;
            TemplatePropertyName = templatePropertyName;
            Mode = mode;
        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            throw new NotImplementedException();
        }
        public override void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            throw new NotImplementedException();
        }
        public virtual void BindingTemplate(in IBindableObject target, in IBindableObject source)
        {
            throw new NotImplementedException();
        }
    }
    public class TtTemplateBindingValue<TTag, TSrc> : TtTemplateBindingValue
    {
        public TtTemplateBindingValue(string propertyName, string templatePropertyName, EBindingMode mode = EBindingMode.Default)
            : base(propertyName, templatePropertyName, mode)
        {

        }
        public override void BindingTemplate(in IBindableObject target, in IBindableObject source)
        {
            TtBindingOperations.SetBinding<TTag, TSrc>(target, PropertyName, source, TemplatePropertyName, Mode);
        }
    }
}
