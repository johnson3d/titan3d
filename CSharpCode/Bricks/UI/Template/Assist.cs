using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Template
{
    internal enum enTemplateValueType : byte
    {
        Simple,
        Trigger,
        PropertyTriggerResource,
        DataTrigger,
        DataTriggerResource,
        TemplateBinding,
        Resource
    }
    internal interface ITemplateValue
    {

    }
    internal struct TemplateValue<T> : ITemplateValue
    {
        public enTemplateValueType Type;
        public Bind.TtBindableProperty Property;
        public T Value;
    }
}
