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
}
