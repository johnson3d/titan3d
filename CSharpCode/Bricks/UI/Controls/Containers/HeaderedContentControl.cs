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
        TtTemplateContainer mHeader;
        [Bind.BindProperty]
        public TtTemplateContainer Header
        {
            get => mHeader;
            set
            {
                OnValueChange(in value, in mHeader);
                mHeader = value;
            }
        }

        [Bind.BindProperty]
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

        TtTemplateContainer mContent;
        [Bind.BindProperty]
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
