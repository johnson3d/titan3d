using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode.Animation
{
    public partial class LATransitionNodeControl
    {
        public LATransitionNodeControl()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
            var param = CSParam as LATransitionNodeControlConstructionParams;
            this.Width = 40;
            this.Height = 10;
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;

            var param = CSParam as LATransitionNodeControlConstructionParams;
            this.Width = param.Width;
            this.Height = param.Height;
        }

    }
}
