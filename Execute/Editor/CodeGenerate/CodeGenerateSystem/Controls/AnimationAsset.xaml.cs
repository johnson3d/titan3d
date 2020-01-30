using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeGenerateSystem.Controls
{
    public partial class AnimationAsset
    {
        public AnimationAsset()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }

    }
}
