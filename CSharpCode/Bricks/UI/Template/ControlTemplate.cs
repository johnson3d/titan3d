using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Template
{
    public class TtControlTemplate : TtUITemplate
    {
        protected UTypeDesc mTargetType;
        public UTypeDesc TargetType
        {
            get => mTargetType;
            set
            {
                mTargetType = value;
            }
        }

        protected override void CheckTemplateParentValid(TtUIElement templateParent)
        {
            if (templateParent == null)
                throw new ArgumentNullException("templateParent");

            if (mTargetType != null && !mTargetType.IsInstanceOfType(templateParent))
                throw new ArgumentException($"Target type({mTargetType.FullName}) is mismatch with templateParent type{templateParent.GetType().FullName}");

            if (templateParent.TemplateInternal != this)
                throw new ArgumentException($"templateParent's template is not this template");
        }
    }
}
