using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls.Containers
{
    public partial class TtHeaderedContentsControl : TtContainer
    {
        TtTemplateContainer mHeader = new TtTemplateContainer();
        [Bind.BindProperty, Rtti.Meta, Browsable(false)]
        public TtTemplateContainer Header
        {
            get => mHeader;
            set
            {
                OnValueChange(in value, in mHeader);
                mHeader = value;
            }
        }

        [Bind.BindProperty, Browsable(false)]
        public bool HasHeader
        {
            get { return GetValue<bool>(); }
        }

        TtUITemplate mHeaderTemplate;
        [BindProperty, Browsable(false)]
        public TtUITemplate HeaderTemplate
        {
            get => mHeaderTemplate;
            set
            {
                OnValueChange(value, mHeaderTemplate);
                mHeaderTemplate = value;
                if (mHeaderTemplate != null)
                    mHeaderTemplate.Seal();
            }
        }

        TtTemplateContainer mContent = new TtTemplateContainer();
        [Bind.BindProperty, Rtti.Meta, Browsable(false)]
        public TtTemplateContainer Content
        {
            get => mContent;
            set
            {
                OnValueChange(in value, in mContent);
                mContent = value;
            }
        }

        public override bool CanAddChild(Rtti.UTypeDesc childType)
        {
            return false;
        }
    }
}
