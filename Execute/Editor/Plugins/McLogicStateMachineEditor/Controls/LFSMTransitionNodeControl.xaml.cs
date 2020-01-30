using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace McLogicStateMachineEditor.Controls
{
    public partial class LFSMTransitionNodeControl
    {
        public LFSMTransitionNodeControl()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
            var param = CSParam as LFSMTransitionNodeControlConstructionParams;
            this.Width = 40;
            this.Height = 10;
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;

            var param = CSParam as LFSMTransitionNodeControlConstructionParams;
            this.Width = param.Width;
            this.Height = param.Height;
        }

    }
}
