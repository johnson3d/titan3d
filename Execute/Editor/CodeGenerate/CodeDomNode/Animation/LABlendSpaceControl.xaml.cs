using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode.Animation
{
    public  partial class LABlendSpaceControl
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mXLinkHandle = XValueHandle;
            mYLinkHandle = YValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mXLinkHandle.MultiLink = false;
            mYLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;
        }
    }
}
